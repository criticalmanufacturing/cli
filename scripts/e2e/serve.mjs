// Runs inside the generated devcontainer. A detached process survives exec's exit.
import { spawn } from 'node:child_process';
import { openSync, readFileSync, writeFileSync, existsSync, unlinkSync, writeSync, readdirSync, readlinkSync } from 'node:fs';
import path from 'node:path';

const [action, stateDirectory, htmlDirectory] = process.argv.slice(2);
const pidFile = path.join(stateDirectory, 'serve.pid');
if (action === 'stop-command') {
  for (const file of readdirSync(stateDirectory).filter(file => /^command(?:-[\w-]+)?\.pid$/.test(file))) {
    const commandPidFile = path.join(stateDirectory, file);
    const pid = Number(readFileSync(commandPidFile, 'utf8'));
    if (!Number.isInteger(pid) || pid <= 1) throw new Error('Invalid command PID');
    try {
      // Retained PID files can refer to a reused process after a failed run.
      const cwd = readlinkSync(`/proc/${pid}/cwd`);
      const workspace = path.dirname(stateDirectory);
      if (cwd !== workspace && !cwd.startsWith(workspace + path.sep))
        throw new Error('Recorded command PID belongs to another workspace');
      process.kill(-pid, 'SIGKILL');
    } catch (error) { if (!['ESRCH', 'ENOENT'].includes(error.code)) throw error; }
    try { unlinkSync(commandPidFile); } catch (error) { if (error.code !== 'ENOENT') throw error; }
  }
} else if (action === 'start') {
  if (existsSync(pidFile)) throw new Error('Serve process already recorded');
  const log = openSync(path.join(stateDirectory, 'serve.log'), 'a', 0o600);
  const header = `[devcontainer] cwd: ${htmlDirectory}\n$ npm run serve (background)\n`;
  console.log(header.trimEnd());
  writeSync(log, header);
  const child = spawn('npm', ['run', 'serve'], {
    cwd: htmlDirectory, detached: true, stdio: ['ignore', log, log],
  });
  child.once('error', error => { console.error(error.message); process.exitCode = 1; });
  child.once('spawn', () => {
    writeFileSync(pidFile, String(child.pid), { mode: 0o600 });
    child.unref();
  });
} else if (action === 'check' || action === 'stop') {
  const pid = Number(readFileSync(pidFile, 'utf8'));
  if (!Number.isInteger(pid) || pid <= 1) throw new Error('Invalid serve PID');
  if (action === 'check') process.kill(pid, 0);
  else {
    try {
      if (readlinkSync(`/proc/${pid}/cwd`) !== htmlDirectory)
        throw new Error('Recorded serve PID belongs to another directory; refusing to stop it');
    } catch (error) {
      if (error.code !== 'ENOENT') throw error;
      unlinkSync(pidFile);
      process.exit(0);
    }
    try { process.kill(-pid, 'SIGTERM'); } catch (error) { if (error.code !== 'ESRCH') throw error; }
    await new Promise(resolve => setTimeout(resolve, 2000));
    try { process.kill(-pid, 'SIGKILL'); } catch (error) { if (error.code !== 'ESRCH') throw error; }
    unlinkSync(pidFile);
  }
} else throw new Error(`Unknown action: ${action}`);
