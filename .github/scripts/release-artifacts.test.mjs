import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, readFile, writeFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { artifacts, verify } from './release-artifacts.mjs';

const bytes = Buffer.from('PK synthetic release archive');

async function fixture(t, count = 1, version = '1.0.0') {
  const directory = await mkdtemp(join(tmpdir(), 'release-artifacts-'));
  t.after(() => rm(directory, { recursive: true, force: true }));
  const files = [];
  for (let index = 0; index < count; index++) {
    const name = `release-${index}-${version}.zip`;
    const path = join(directory, name);
    await writeFile(path, bytes);
    files.push({ name, path, github: `https://github.com/example/tool/releases/download/${version}/${name}` });
  }
  return files;
}

test('builds exact versioned mirror URLs and rejects invalid release tags', () => {
  const config = { repository: 'criticalmanufacturing/cli', files: [
    { local: 'cmf-cli.win-x64.zip', github: 'cmf-cli.win-x64.zip', mirror: 'cmf-cli.win-x64-{version}.zip' },
  ] };
  const [file] = artifacts(config, '6.0.1-0', 'dist');
  assert.equal(file.name, 'cmf-cli.win-x64-6.0.1-0.zip');
  assert.equal(file.github, 'https://github.com/criticalmanufacturing/cli/releases/download/6.0.1-0/cmf-cli.win-x64.zip');
  for (const version of [undefined, 'latest', '../6.0.0', '6.0.0;echo', 'v6.0.0']) {
    assert.throws(() => artifacts(config, version, 'dist'));
  }
});

test('prereleases retain their full version in every package name and download URL', async t => {
  const config = JSON.parse(await readFile(new URL('../release-artifacts.json', import.meta.url), 'utf8'));
  const directory = await mkdtemp(join(tmpdir(), 'prerelease-artifacts-'));
  t.after(() => rm(directory, { recursive: true, force: true }));
  for (const version of ['1.2.3-0', '1.2.3-beta.2', '1.2.3-rc.1']) {
    const files = artifacts(config, version, directory);
    const stableNames = new Set(artifacts(config, '1.2.3', directory).map(file => file.name));
    for (const file of files) {
      await writeFile(file.path, bytes);
      assert.ok(file.name.includes(version));
      assert.ok(!stableNames.has(file.name));
      assert.ok(file.github.includes(`/releases/download/${version}/`));
    }
    const downloads = [];
    await verify(files, { attempts: 1, fetcher: async url => {
      assert.ok(url.includes(version));
      downloads.push(url);
      return new Response(bytes);
    } });
    assert.equal(downloads.length, files.length * 3);
  }
});

test('verifies every artifact at all three sources anonymously', async t => {
  const files = await fixture(t, 2);
  const urls = [];
  await verify(files, { attempts: 1, fetcher: async (url, options) => {
    urls.push(url);
    assert.equal(options.headers, undefined);
    return new Response(bytes);
  } });
  assert.equal(urls.length, 6);
  for (const host of ['github.com', 'criticalmanufacturing.io', 'repository.criticalmanufacturing.com.cn']) {
    assert.equal(urls.filter(url => new URL(url).hostname === host).length, 2);
  }
});

test('required sources still fail on missing or mismatched downloads', async t => {
  const files = await fixture(t);
  for (const host of ['github.com', 'criticalmanufacturing.io']) {
    for (const status of [200, 404]) {
      await assert.rejects(verify(files, { attempts: 1, fetcher: async url =>
        new URL(url).hostname === host ? new Response('<html>error</html>', { status }) : new Response(bytes),
      }), error => error.message.includes(host));
    }
  }
});

test('China failures warn and allow publication without waiting for retries', async t => {
  const files = await fixture(t);
  const warnings = [];
  t.mock.method(console, 'warn', message => warnings.push(message));
  for (const failure of [
    () => new Response('', { status: 404 }),
    () => new Response('<html>error</html>'),
    () => { throw new Error('connection failed'); },
  ]) {
    let chinaRequests = 0;
    await verify(files, { attempts: 2, delay: () => assert.fail('China must not delay publication'), fetcher: async url => {
      if (new URL(url).hostname === 'repository.criticalmanufacturing.com.cn') {
        chinaRequests++;
        return failure();
      }
      return new Response(bytes);
    } });
    assert.equal(chinaRequests, 1);
  }
  assert.equal(warnings.length, 3);
  for (const warning of warnings) {
    assert.match(warning, /^::warning::China mirror verification failed:/);
    assert.ok(warning.includes(files[0].name));
    assert.match(warning, /repository\.criticalmanufacturing\.com\.cn/);
  }
});

test('reports all failed required sources instead of stopping at the first failure', async t => {
  const files = await fixture(t);
  await assert.rejects(verify(files, { attempts: 1, fetcher: async () => new Response('', { status: 404 }) }), error => {
    assert.match(error.message, /github.com/);
    assert.match(error.message, /criticalmanufacturing.io/);
    assert.doesNotMatch(error.message, /com.cn/);
    return true;
  });
});

test('retries read-only downloads to accommodate replication lag', async t => {
  const files = await fixture(t);
  let calls = 0;
  let delays = 0;
  await verify(files, { bases: [], attempts: 2, delay: async () => delays++, fetcher: async () =>
    ++calls === 1 ? new Response('', { status: 404 }) : new Response(bytes),
  });
  assert.equal(calls, 2);
  assert.equal(delays, 1);
});
