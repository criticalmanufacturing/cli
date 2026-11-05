import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, readFile, writeFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { createServer } from 'node:http';
import { artifacts, publish, verify } from './release-artifacts.mjs';

const bytes = Buffer.from('PK synthetic release archive');
const json = value => Response.json(value);
const metadata = { versions: [{ version: 'latest' }] };

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

test('uploads with Bearer auth, exact legacy package/filename and storage version latest', async t => {
  const files = await fixture(t);
  const calls = [];
  await publish(files, 'synthetic-token', async (url, options) => {
    calls.push({ url, options });
    assert.equal(options.headers.Authorization, 'Bearer synthetic-token');
    assert.equal(options.redirect, 'error');
    if (options.method === 'POST') {
      assert.equal(options.body.get('packageName'), files[0].name);
      assert.equal(options.body.get('packageVersion'), 'latest');
      assert.equal(options.body.get('packageFiles').name, files[0].name);
      assert.deepEqual(Buffer.from(await options.body.get('packageFiles').arrayBuffer()), bytes);
      return json([{ packageFileName: files[0].name, isSucceeded: true }]);
    }
    if (url.includes('/api/universal/')) return new Response(bytes);
    return new Response('', { status: 404 });
  });
  assert.equal(calls.length, 3);
  assert.ok(calls[1].url.endsWith('/packages/single'));
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
    const uploaded = [];
    await publish(files, 'synthetic-token', async (url, options) => {
      if (options.method === 'POST') {
        const name = options.body.get('packageName');
        assert.ok(name.includes(version));
        assert.equal(options.body.get('packageFiles').name, name);
        assert.equal(options.body.get('packageVersion'), 'latest');
        uploaded.push(name);
        return json([{ packageFileName: name, isSucceeded: true }]);
      }
      if (url.includes('/api/universal/')) {
        assert.ok(url.includes(version));
        assert.ok(url.includes('/latest/'));
        return new Response(bytes);
      }
      return new Response('', { status: 404 });
    });
    assert.deepEqual(uploaded.sort(), files.map(file => file.name).sort());
    const downloads = [];
    await verify(files, { attempts: 1, fetcher: async url => {
      assert.ok(url.includes(version));
      downloads.push(url);
      return new Response(bytes);
    } });
    assert.equal(downloads.length, files.length * 3);
  }
});

test('checks all packages before writing and refuses HTTP lookup errors', async t => {
  const files = await fixture(t, 2);
  let writes = 0;
  await assert.rejects(publish(files, 'token', async (url, options) => {
    if (options.method === 'POST') writes++;
    return url.endsWith(files[0].name) ? new Response('', { status: 404 }) : new Response('', { status: 401 });
  }), /lookup failed.*401/);
  assert.equal(writes, 0);
});

test('skips existing packages only after verifying bytes directly in the registry', async t => {
  const files = await fixture(t);
  let writes = 0;
  await publish(files, 'token', async (url, options) => {
    if (options.method === 'POST') writes++;
    return url.includes('/api/universal/') ? new Response(bytes) : json(metadata);
  });
  assert.equal(writes, 0);
});

test('refuses overwriting different existing bytes', async t => {
  const files = await fixture(t);
  await assert.rejects(publish(files, 'token', async url =>
    url.includes('/api/universal/') ? new Response('different') : json(metadata)), /refusing to overwrite/);
});

test('fails closed on malformed or unexpected package versions', async t => {
  const files = await fixture(t);
  for (const value of [{}, { versions: [{ version: '1' }] }, { versions: ['latest'] }]) {
    await assert.rejects(publish(files, 'token', async () => json(value)), /metadata|storage versions/);
  }
});

test('checks per-file upload failures even when HTTP status is 200', async t => {
  const files = await fixture(t);
  for (const result of [[], {}, [{ packageFileName: files[0].name, isSucceeded: false }],
    [{ packageFileName: 'wrong.zip', isSucceeded: true }]]) {
    await assert.rejects(publish(files, 'token', async (url, options) =>
      options.method === 'POST' ? json(result) : new Response('', { status: 404 })), /did not confirm/);
  }
});

test('does not replay failed or ambiguous POST uploads', async t => {
  const files = await fixture(t);
  for (const failure of ['http', 'lost-response']) {
    let writes = 0;
    await assert.rejects(publish(files, 'token', async (url, options) => {
      if (options.method !== 'POST') return new Response('', { status: 404 });
      writes++;
      if (failure === 'lost-response') throw new Error('connection reset');
      return new Response('', { status: 503 });
    }));
    assert.equal(writes, 1);
  }
});

test('rejects upload success followed by wrong stored bytes', async t => {
  const files = await fixture(t);
  await assert.rejects(publish(files, 'token', async (url, options) => {
    if (options.method === 'POST') return json([{ packageFileName: files[0].name, isSucceeded: true }]);
    if (url.includes('/api/universal/')) return new Response('HTML login page');
    return new Response('', { status: 404 });
  }), /differs after upload/);
});

test('requires a valid token before any network request', async t => {
  const files = await fixture(t);
  for (const token of ['', undefined, 'bad\nheader']) {
    await assert.rejects(publish(files, token, () => assert.fail('unexpected network request')), /CRITICALMANUFACTURING_IO_TOKEN/);
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

test('fails even if GitHub works but a mirror returns 404 or HTML with 200', async t => {
  const files = await fixture(t);
  for (const status of [200, 404]) {
    await assert.rejects(verify(files, { attempts: 1, fetcher: async url =>
      url.includes('com.cn') ? new Response('<html>error</html>', { status }) : new Response(bytes),
    }), /com.cn/);
  }
});

test('reports all failed sources instead of stopping at the first failure', async t => {
  const files = await fixture(t);
  await assert.rejects(verify(files, { attempts: 1, fetcher: async () => new Response('', { status: 404 }) }), error => {
    assert.match(error.message, /github.com/);
    assert.match(error.message, /criticalmanufacturing.io/);
    assert.match(error.message, /com.cn/);
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

test('real HTTP publication retries after a mirror failure without overwriting original bytes', async t => {
  const files = await fixture(t, 1, '1.2.3-beta.2');
  let stored = false;
  let writes = 0;
  const server = createServer(async (req, res) => {
    const body = [];
    for await (const chunk of req) body.push(chunk);
    if (req.url.startsWith('/api/')) assert.equal(req.headers.authorization, 'Bearer synthetic-token');
    else assert.equal(req.headers.authorization, undefined);
    if (req.method === 'POST') {
      writes++;
      const form = await new Request('http://localhost', {
        method: 'POST', headers: req.headers, body: Buffer.concat(body),
      }).formData();
      assert.equal(form.get('packageName'), files[0].name);
      assert.deepEqual(Buffer.from(await form.get('packageFiles').arrayBuffer()), bytes);
      stored = true;
      res.setHeader('Content-Type', 'application/json');
      res.end(JSON.stringify([{ packageFileName: files[0].name, isSucceeded: true }]));
    } else if (req.url.includes('/packages/')) {
      res.statusCode = stored ? 200 : 404;
      res.end(JSON.stringify(metadata));
    } else {
      res.write(bytes.subarray(0, 8));
      res.end(bytes.subarray(8));
    }
  });
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  t.after(() => { server.closeAllConnections(); return new Promise(resolve => server.close(resolve)); });
  const fetcher = (url, options) => fetch(`http://127.0.0.1:${server.address().port}${new URL(url).pathname}`, options);
  await publish(files, 'synthetic-token', fetcher);
  await assert.rejects(verify(files, { attempts: 1, fetcher: (url, options) =>
    url.includes('com.cn') ? new Response('', { status: 404 }) : fetcher(url, options),
  }), /com.cn/);
  // The failed job downloads the original release assets again instead of rebuilding.
  await writeFile(files[0].path, bytes);
  await publish(files, 'synthetic-token', fetcher);
  await verify(files, { fetcher, attempts: 1 });
  assert.equal(writes, 1);
});
