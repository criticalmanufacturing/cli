import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, mkdir, writeFile, rm } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { packageDirectory, waitForHttp } from './checks.mjs';

test('installation waits through startup and in-progress before accepting success', async () => {
  const results = [new Error('Connection refused'), new Response('', { status: 503 }),
    new Response('{"CanStart":true}', { status: 202 }),
    new Response('<html>Proxy page</html>'),
    new Response('{"Message":"Deployment finished successfully"}')];
  let calls = 0;
  await waitForHttp('http://localhost/deployment/status', { deployment: true, timeoutMs: 1000,
    intervalMs: 1, fetchImpl: async () => {
      const result = results[calls++];
      if (result instanceof Error) throw result;
      return result;
    } });
  assert.equal(calls, 5);
});

test('installation fails immediately when EnvManager reports failure', async () => {
  await assert.rejects(waitForHttp('http://localhost/deployment/status', {
    deployment: true, timeoutMs: 1000,
    fetchImpl: async () => new Response('{"Message":"Deployment encountered an error."}', { status: 400 }),
  }), /EnvManager installation failed/);
});

test('an installation that never completes times out with the last status', async () => {
  await assert.rejects(waitForHttp('http://localhost/deployment/status', {
    deployment: true, timeoutMs: 20, intervalMs: 1,
    fetchImpl: async () => new Response('Installing', { status: 202 }),
  }), /Timed out.*HTTP 202: Installing/);
});

test('readiness detects a failed background serve process', async () => {
  await assert.rejects(waitForHttp('http://localhost:7000', {
    timeoutMs: 1000, check: async () => { throw new Error('Serve exited'); },
  }), /Serve exited/);
});

test('scaffolding resolves directory casing and rejects missing content', async () => {
  const workspace = await mkdtemp(path.join(os.tmpdir(), 'cmf-flow-'));
  try {
    const directory = path.join(workspace, 'Cmf.Custom.Html');
    await mkdir(directory);
    await writeFile(path.join(directory, 'cmfpackage.json'), JSON.stringify({
      packageId: 'Cmf.Custom.HTML', packageType: 'HTML', version: '1.0.0',
    }));
    await assert.rejects(packageDirectory(workspace, 'html'), /No scaffolded content/);
    await writeFile(path.join(directory, 'package.json'), '{}');
    assert.equal(await packageDirectory(workspace, 'html'), directory);
    await assert.rejects(packageDirectory(workspace, 'business'), /Expected one/);
    const tests = path.join(workspace, 'Cmf.Custom.Tests');
    await mkdir(tests);
    await writeFile(path.join(tests, 'cmfpackage.json'), JSON.stringify({
      packageId: 'Cmf.Custom.Tests', packageType: 'Tests', version: '1.0.0',
    }));
    await writeFile(path.join(tests, 'Tests.sln'), 'solution');
    assert.equal(await packageDirectory(workspace, 'test'), tests);
  } finally { await rm(workspace, { recursive: true, force: true }); }
});
