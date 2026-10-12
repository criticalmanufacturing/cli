import { cp, writeFile } from 'node:fs/promises';
import { randomUUID } from 'node:crypto';
import path from 'node:path';

function parseLine(output, predicate) {
  for (const line of output.split(/\r?\n/)) {
    try { const value = JSON.parse(line); if (predicate(value)) return value; } catch {}
  }
}

export function createContainer(session, commands) {
  const folder = session.config.devcontainerWorkspace || session.workspace;
  const override = path.join(folder, '.e2e/devcontainer.json');
  const labels = ['--id-label', `devcontainer.local_folder=${folder}`, '--id-label', `cmf.e2e.run=${session.report.runId}`];
  const remotePath = relative => path.posix.join(session.containerWorkspace, relative);

  function inside(step, command, argv = [], cwd = session.containerWorkspace, label = command, options = {}) {
    return commands.run(step, 'devcontainer', ['exec', '--workspace-folder', folder, '--override-config', override,
      ...labels, 'node', remotePath('.e2e/exec.mjs'), cwd, String(session.config.commandTimeoutSeconds),
      JSON.stringify([command, ...argv]), remotePath('.e2e/cli'), remotePath(`.e2e/command-${randomUUID()}.pid`)],
      { label, timeout: session.config.commandTimeoutSeconds + 60,
        display: { command, argv, cwd, context: 'devcontainer' }, ...options });
  }
  const cli = (step, argv, directory = session.containerWorkspace, label = argv.join('-')) =>
    inside(step, remotePath('.e2e/cli/cmf'), argv, directory, label);
  const serve = (step, action, options = {}) => inside(step, 'node', [remotePath('.e2e/serve.mjs'), action,
    remotePath('.e2e'), remotePath(path.relative(session.workspace, session.directories.html))],
    session.containerWorkspace, `serve-${action}`, options);

  async function start(step) {
    // Copy the complete candidate and refresh helpers even when resuming.
    await cp(session.bundle, path.join(session.state, 'cli'), { recursive: true });
    for (const file of ['exec.mjs', 'serve.mjs'])
      await cp(path.join(session.scriptDirectory, file), path.join(session.state, file));
    const output = await commands.run(step, 'devcontainer', ['read-configuration', '--workspace-folder', folder],
      { label: 'devcontainer-configuration' });
    const parsed = parseLine(output, value => value?.configuration);
    if (!parsed) throw new Error('Could not read generated devcontainer configuration');
    const config = { ...parsed.configuration, ...parsed.workspace };
    delete config.configFilePath;
    config.runArgs = [...(config.runArgs || [])];
    const published = new Set();
    for (const [url, port] of [[session.config.uiUrl || 'http://localhost:80', 80],
      [session.config.deploymentUrl || 'http://localhost:80/deployment/status', 80],
      [session.config.serveUrl || 'http://localhost:7000', 7000]]) {
      const address = new URL(url);
      if (!['localhost', '127.0.0.1'].includes(address.hostname)) continue;
      const mapping = `127.0.0.1:${address.port || (address.protocol === 'https:' ? '443' : '80')}:${port}`;
      if (!published.has(mapping)) { config.runArgs.push('--publish', mapping); published.add(mapping); }
    }
    await writeFile(path.join(session.state, 'devcontainer.json'), JSON.stringify(config, null, 2));
    const up = await commands.run(step, 'devcontainer', ['up', '--workspace-folder', folder,
      '--override-config', override, ...labels], { label: 'devcontainer-up' });
    const result = parseLine(up, value => value?.outcome === 'success' && value?.remoteWorkspaceFolder);
    if (!result) throw new Error('devcontainer up did not report a successful workspace');
    session.containerWorkspace = result.remoteWorkspaceFolder;
    session.report.containerId = result.containerId;
    const version = (await cli(step, ['--version'], session.containerWorkspace, 'container-cli-version')).trim();
    if (!version.endsWith(session.report.cliVersion)) throw new Error('Candidate CLI versions differ between containers');
    await cli(step, ['dev', 'local', 'stack', '--help'], session.containerWorkspace, 'plugin-check');
  }

  async function stopCommands() {
    if (!session.containerWorkspace) return;
    await commands.run('cleanup', 'devcontainer', ['exec', '--workspace-folder', folder, '--override-config', override,
      ...labels, 'node', remotePath('.e2e/serve.mjs'), 'stop-command', remotePath('.e2e')],
      { label: 'stop-command', timeout: 30, allowCancelled: true });
  }
  return { start, remotePath, inside, cli, serve, stopCommands };
}
