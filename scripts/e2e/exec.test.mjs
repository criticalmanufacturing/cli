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
