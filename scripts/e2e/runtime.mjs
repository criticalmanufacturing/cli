import { access } from 'node:fs/promises';
import { constants } from 'node:fs';
import { createInterface } from 'node:readline';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import os from 'node:os';
import { createReporter } from './report.mjs';
import { createDashboard } from './dashboard.mjs';
import { createCommandRunner } from './commands.mjs';
import { createContainer } from './container.mjs';

export async function createRuntime(config, options) {
  const workspace = path.resolve(config.workspace), bundle = path.resolve(config.cliBundle);
  await access(path.join(bundle, 'cmf'), constants.X_OK);
  const session = { config, ...options, workspace, bundle, state: path.join(workspace, '.e2e'),
    scriptDirectory: path.dirname(fileURLToPath(import.meta.url)), directories: {},
    dockerConfig: process.env.DOCKER_CONFIG || path.join(os.homedir(), '.config/cmf-cli-e2e/docker'),
    serveStarted: false };
  const reporter = await createReporter(session);
  const dashboard = createDashboard(session, { enabled: options.dashboard !== false,
    port: options.dashboardPort ?? config.dashboardPort });
  session.dashboard = dashboard;
  await dashboard.start();
  let cancelled = false, browser, cancellation, finishing = false;
  const commands = createCommandRunner(session, reporter, () => cancelled);
  const container = createContainer(session, commands);
  const checkCancelled = () => { if (cancelled) throw new Error('Run interrupted'); };
  function setBrowser(value) {
    browser = value;
    if (cancelled) browser?.close().catch(() => {});
  }
  function cancel() {
    if (cancellation) return cancellation;
    cancelled = true;
    commands.cancel();
    browser?.close().catch(() => {});
    cancellation = container.stopCommands().catch(error => { session.report.cleanup.push(error.message); });
    return cancellation;
  }
  const onSignal = () => { if (!finishing) void cancel(); };
  process.on('SIGINT', onSignal); process.on('SIGTERM', onSignal);

  // Each step receives its own command scope. There is no shared active-step state.
  function step(name, action) {
    checkCancelled();
    return reporter.step(name, () => action({
      run: (command, argv, options) => commands.run(name, command, argv, options),
      cli: (argv, directory, label) => container.cli(name, argv, directory, label),
      serve: action => container.serve(name, action),
      startContainer: () => container.start(name),
    }));
  }
  async function finish() {
    finishing = true; // Further interrupts must not terminate cleanup commands.
    if (cancellation) await cancellation;
    if (session.serveStarted && session.containerWorkspace) {
      try {
        await container.serve('cleanup', 'stop', { allowCancelled: true });
        session.serveStarted = false;
        session.report.cleanup.push('Serve process stopped');
      } catch (error) {
        session.report.cleanup.push(error.message); session.report.status = 'failed';
      }
    }
    session.report.finishedAt = new Date().toISOString();
    await reporter.save();
    await dashboard.writeFinal().catch(error => console.warn(`Could not write dashboard report: ${error.message}`));
    console.log(`\nResult: ${session.report.status}.` + (session.workspaceReady ? ` Report: ${path.join(session.state, 'report.json')}` :
      ' No workspace was created; fix the prerequisites and retry.'));
    if (session.workspaceReady) console.log(session.report.status === 'passed' ?
      'Project and artifacts retained; local stack and volumes removed.' :
      'Project and artifacts retained for inspection; any started containers remain running.');
    if (session.report.status === 'failed' && !session.ci && dashboard.url && process.stdin.isTTY && process.stdout.isTTY) {
      const url = dashboard.url;
      console.log(`Dashboard remains available at ${url}. Press Enter or Ctrl+C to close it.`);
      const prompt = createInterface({ input: process.stdin, output: process.stdout });
      await new Promise(resolve => {
        prompt.once('line', resolve);
        prompt.once('SIGINT', resolve);
      });
      prompt.close();
    }
    await dashboard.close();
    process.off('SIGINT', onSignal); process.off('SIGTERM', onSignal);
  }
  return { session, step, reporter, container, cancel, checkCancelled, setBrowser, finish, dashboard };
}
