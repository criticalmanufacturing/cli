import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, readFile, writeFile, rm } from 'node:fs/promises';
import path from 'node:path';
import os from 'node:os';
import { createCommandRunner } from './commands.mjs';

test('cancellation settles concurrent runner processes with explicit step attribution', { timeout: 10000 }, async () => {
  const root = await mkdtemp(path.join(os.tmpdir(), 'cmf-commands-'));
  const report = { commands: [] };
  let cancelled = false;
  const session = { workspace: root, state: root, bundle: root, scriptDirectory: root,
    dockerConfig: root, workspaceReady: true, report, config: { commandTimeoutSeconds: 5 } };
  const commands = createCommandRunner(session, { save: async () => {} }, () => cancelled);
  try {
    const script = path.join(root, 'wait.cjs');
    await writeFile(script, 'require("node:fs").writeFileSync(process.argv[2],String(process.pid));setInterval(()=>{},1000);');
    const running = ['one', 'two'].map(name => commands.run(name, process.execPath,
      [script, path.join(root, `${name}.pid`)], { label: name }));
    const settled = Promise.allSettled(running);
    const pids = [];
    for (const name of ['one', 'two']) {
      for (let i = 0; i < 100; i++) {
        try { pids.push(Number(await readFile(path.join(root, `${name}.pid`), 'utf8'))); break; }
        catch (error) { if (error.code !== 'ENOENT') throw error; await new Promise(resolve => setTimeout(resolve, 20)); }
      }
    }
    assert.equal(pids.length, 2);
    cancelled = true; commands.cancel();
    const outcomes = await settled;
    assert.ok(outcomes.every(result => result.status === 'rejected'));
    assert.deepEqual(report.commands.map(record => record.step), ['one', 'two']);
    assert.ok(report.commands.every(record => record.status === 'failed'));
    for (const pid of pids) assert.throws(() => process.kill(pid, 0), { code: 'ESRCH' });
  } finally { commands.cancel(); await rm(root, { recursive: true, force: true }); }
});
