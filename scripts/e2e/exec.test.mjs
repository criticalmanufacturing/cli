import test from 'node:test';
import assert from 'node:assert/strict';
import { spawn } from 'node:child_process';
import { mkdtemp, readFile, rm } from 'node:fs/promises';
import path from 'node:path';
import os from 'node:os';

test('remote timeout fails even if the command handles termination with exit zero', { timeout: 10000 }, async () => {
  const root = await mkdtemp(path.join(os.tmpdir(), 'cmf-exec-'));
  try {
    const command = [process.execPath, '-e',
      "process.on('SIGTERM',()=>process.exit(0)); setInterval(()=>{},1000)"];
    let output = '';
    const code = await new Promise(resolve => {
      const child = spawn(process.execPath, [path.join(import.meta.dirname, 'exec.mjs'), root,
        '0.5', JSON.stringify(command), '', path.join(root, 'command.pid')]);
      child.stderr.on('data', chunk => { output += chunk; });
      child.once('close', resolve);
    });
    assert.equal(code, 1);
    assert.match(output, /Command timed out/);
    await assert.rejects(readFile(path.join(root, 'command.pid')), { code: 'ENOENT' });
  } finally { await rm(root, { recursive: true, force: true }); }
});


test('cancellation stops both concurrent remote command groups', { timeout: 10000 }, async () => {
  const root = await mkdtemp(path.join(os.tmpdir(), 'cmf-exec-parallel-'));
  const children = [];
  try {
    const exits = [];
    for (const name of ['one', 'two']) {
      const pidFile = path.join(root, `command-${name}.pid`);
      const command = [process.execPath, '-e', "console.log('ready'); setInterval(()=>{},1000)"];
      const child = spawn(process.execPath, [path.join(import.meta.dirname, 'exec.mjs'),
        root, '30', JSON.stringify(command), '', pidFile]);
      children.push(child);
      exits.push(new Promise(resolve => child.once('close', resolve)));
      await new Promise((resolve, reject) => {
        child.stdout.once('data', resolve); child.once('error', reject);
      });
      assert.ok(Number(await readFile(pidFile, 'utf8')) > 1);
    }
    const stop = spawn(process.execPath, [path.join(import.meta.dirname, 'serve.mjs'), 'stop-command', root]);
    assert.equal(await new Promise(resolve => stop.once('close', resolve)), 0);
    assert.deepEqual(await Promise.all(exits), [1, 1]);
    for (const name of ['one', 'two'])
      await assert.rejects(readFile(path.join(root, `command-${name}.pid`)), { code: 'ENOENT' });
  } finally {
    for (const child of children) child.kill('SIGTERM');
    await rm(root, { recursive: true, force: true });
  }
});
