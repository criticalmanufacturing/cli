// Keep the timeout inside the devcontainer so descendants cannot outlive exec.
import { spawn } from 'node:child_process';
import { writeFileSync, unlinkSync } from 'node:fs';
const [cwd, seconds, commandJson, bundlePath = '', pidFile] = process.argv.slice(2);
const [command, ...args] = JSON.parse(commandJson);
const env = { ...process.env, NG_CLI_ANALYTICS: 'false' };
if (bundlePath) env.PATH = `${bundlePath}:${env.PATH}`;
const child = spawn(command, args, { cwd, env, detached: true, stdio: 'inherit' });
let killTimer;
let stopped = false;
function stop() {
  stopped = true;
  try { process.kill(-child.pid, 'SIGTERM'); } catch { /* Already exited. */ }
  killTimer = setTimeout(() => {
    try { process.kill(-child.pid, 'SIGKILL'); } catch { /* Already exited. */ }
  }, 5000);
}
const timer = setTimeout(() => { console.error('Command timed out'); stop(); }, Number(seconds) * 1000);
child.once('spawn', () => writeFileSync(pidFile, String(child.pid), { mode: 0o600 }));
process.once('SIGINT', stop);
process.once('SIGTERM', stop);
child.once('error', error => { clearTimeout(timer); console.error(error.message); process.exitCode = 1; });
child.once('exit', (code, signal) => {
  clearTimeout(timer);
  if (!stopped) clearTimeout(killTimer);
  try { unlinkSync(pidFile); } catch { /* Spawn may have failed. */ }
  process.exitCode = signal || stopped ? 1 : code;
});
