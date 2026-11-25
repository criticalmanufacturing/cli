import { spawn } from 'node:child_process';
import { appendFile } from 'node:fs/promises';
import { createWriteStream } from 'node:fs';
import path from 'node:path';
import { formatCommand } from './command-log.mjs';

export function createCommandRunner(session, reporter, isCancelled) {
  const active = new Map();
  function cancel() { for (const stop of active.values()) stop(); }

  async function run(step, command, argv, { cwd = session.workspaceReady ? session.workspace : process.cwd(),
    interactive = false, label = command, timeout = session.config.commandTimeoutSeconds,
    allowCancelled = false, display = { command, argv, cwd, context: 'runner' } } = {}) {
    if (isCancelled() && !allowCancelled) throw new Error('Run interrupted');
    const logFile = path.join(session.state, `${label.replace(/[^a-z0-9-]/gi, '-')}.log`);
    const log = interactive || !session.workspaceReady ? null : createWriteStream(logFile, { flags: 'a', mode: 0o600 });
    const printable = formatCommand(display.command, display.argv, [session.config.portalPat,
      session.config.portalToken, process.env.CMF_E2E_UI_PASSWORD]);
    const header = `[${display.context}] cwd: ${display.cwd}\n$ ${printable}\n`;
    console.log(header.trimEnd());
    log?.write(header);
    if (session.workspaceReady) await appendFile(path.join(session.state, 'commands.log'), header, { mode: 0o600 });
    const record = { step, context: display.context, workingDirectory: display.cwd, command: printable,
      startedAt: new Date().toISOString(), status: 'running' };
    session.report.commands.push(record);
    session.dashboard?.commandStarted(record);
    await reporter.save();
    let output = '', timedOut = false;
    try {
      if (isCancelled() && !allowCancelled) throw new Error('Run interrupted');
      await new Promise((resolve, reject) => {
        const child = spawn(command, argv, { cwd, detached: true,
          stdio: interactive ? 'inherit' : ['ignore', 'pipe', 'pipe'],
          env: { ...process.env, NG_CLI_ANALYTICS: 'false', DOCKER_CONFIG: session.dockerConfig,
            PATH: `${session.bundle}:${process.env.PATH}:${path.join(session.scriptDirectory, 'node_modules', '.bin')}` } });
        const kill = signal => {
          if (child.pid) try { process.kill(-child.pid, signal); } catch (error) { if (error.code !== 'ESRCH') throw error; }
        };
        let cancellationTimer;
        active.set(child, () => { kill('SIGTERM'); cancellationTimer ||= setTimeout(() => kill('SIGKILL'), 1000); });
        const timer = setTimeout(() => { timedOut = true; kill('SIGTERM'); }, timeout * 1000);
        const forceTimer = setTimeout(() => kill('SIGKILL'), (timeout + 10) * 1000);
        for (const stream of [child.stdout, child.stderr].filter(Boolean)) stream.on('data', chunk => {
          output = (output + chunk).slice(-1024 * 1024);
          log?.write(chunk); process.stdout.write(chunk);
          session.dashboard?.commandOutput(record, chunk);
        });
        let finished = false;
        function done(error) {
          if (finished) return;
          finished = true;
          clearTimeout(timer); clearTimeout(forceTimer); clearTimeout(cancellationTimer);
          active.delete(child); log?.end();
          error ? reject(error) : resolve();
        }
        child.once('error', error => done(error.code === 'ENOENT' ? new Error(`${command} is unavailable. ` +
          (command === 'devcontainer' ? 'Run "npm --prefix scripts/e2e ci" inside the CLI devcontainer.' : 'Install it inside the runner container or add it to PATH.')) : error));
        child.once('close', (code, signal) => done(code === 0 && !timedOut && (!isCancelled() || allowCancelled) ? null :
          new Error(`${label} failed (${timedOut ? 'timeout' : signal || `exit ${code}`})` +
            (session.workspaceReady ? `; see ${logFile}` : '; see the output above'))));
      });
      record.status = 'passed';
    } catch (error) { record.status = 'failed'; throw error; }
    finally { log?.end(); record.finishedAt = new Date().toISOString(); session.dashboard?.notify(); await reporter.save(); }
    return output;
  }
  return { run, cancel };
}
