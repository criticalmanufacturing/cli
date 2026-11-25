import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdir, readFile, rm, writeFile } from 'node:fs/promises';
import { mkdtemp } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { createDashboard, MAX_OUTPUT } from './dashboard.mjs';

test('dashboard streams branded live state and writes an offline report', async () => {
  const root = await mkdtemp(path.join(os.tmpdir(), 'cmf-dashboard-'));
  const state = path.join(root, '.e2e');
  await mkdir(state);
  const report = { runId: 'run-123', status: 'running', mesVersion: '11.0.0', workspace: root,
    steps: [{ name: 'build-html', status: 'running', startedAt: new Date().toISOString() }],
    commands: [], cleanup: [] };
  const session = { report, state, workspaceReady: true, config: { portalPat: 'secret-pat', portalToken: 'secret-token' } };
  const dashboard = createDashboard(session, { port: 0 });
  try {
    const url = await dashboard.start();
    assert.match(url, /^http:\/\/127\.0\.0\.1:\d+$/);
    const page = await (await fetch(url)).text();
    assert.match(page, /#007ac9/);
    assert.match(page, /#14a9fa/);
    assert.match(page, /Execution flow/);
    assert.match(page, /id="setup-steps"/);
    assert.doesNotMatch(page, /__INITIAL__|__LIVE__/);
    assert.match(await (await fetch(`${url}/dashboard.js`)).text(), /Following active step/);

    const record = { step: 'build-html', context: 'devcontainer', command: 'cmf build', status: 'running' };
    report.commands.push(record);
    dashboard.commandStarted(record);
    dashboard.commandOutput(record, `output secret-token ${'x'.repeat(MAX_OUTPUT + 20)}`);
    const snapshot = await (await fetch(`${url}/state`)).json();
    assert.equal(Object.values(snapshot.outputs)[0].includes('secret-token'), false);
    assert.ok(Object.values(snapshot.outputs)[0].length <= MAX_OUTPUT);

    await writeFile(path.join(state, 'ui-progress.png'), Buffer.from('png-fixture'));
    await dashboard.writeFinal();
    const finalPage = await readFile(path.join(state, 'dashboard.html'), 'utf8');
    assert.match(finalPage, /Final report/);
    assert.match(finalPage, /data:image\/png;base64/);
  } finally {
    await dashboard.close();
    await rm(root, { recursive: true, force: true });
  }
});
