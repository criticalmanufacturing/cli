import { spawn } from 'node:child_process';
import { cp, mkdir, readFile, readdir, writeFile, access, appendFile } from 'node:fs/promises';
import { constants, createWriteStream } from 'node:fs';
import path from 'node:path';
import os from 'node:os';
import { randomUUID } from 'node:crypto';
import { fileURLToPath } from 'node:url';
import { createInterface } from 'node:readline/promises';
import { packageDirectory, nonemptyFile, waitForHttp } from './checks.mjs';
import { formatCommand } from './command-log.mjs';

async function main() {
  const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
  if (process.argv.includes('--help')) {
    console.log('Usage: node scripts/e2e/run.mjs --config <file.json> [--resume]\n' +
      'Runs login, init, devcontainer, scaffolding, local stack, build, UI login, and pack.\n' +
      'CMF_E2E_LICENSE_ID overrides licenseId. See README.md for prerequisites.');
    process.exit(0);
  }
  if (Number(process.versions.node.split('.')[0]) < 20)
    throw new Error(`The helper requires Node 20 or later (running ${process.versions.node}). ` +
      'Inside the CLI devcontainer, run "nvm use 20", then retry. The MES project can still use its own Node version.');
  const resume = process.argv.includes('--resume');
  const args = process.argv.slice(2).filter(arg => arg !== '--resume');
  if (args.length !== 2 || args[0] !== '--config') {
    console.error('Expected --config <file.json>; use --help for usage.');
    process.exit(1);
  }
  const config = JSON.parse(await readFile(path.resolve(args[1]), 'utf8'));
  for (const field of ['mesVersion', 'ciRepo', 'workspace', 'cliBundle']) {
    if (typeof config[field] !== 'string' || !config[field].trim()) throw new Error(`Missing ${field}`);
  }
  if (!/^\d+\.\d+\.\d+(?:[.-][\w.-]+)?$/.test(config.mesVersion)) throw new Error('Use a full MES version');
  if (!Array.isArray(config.releaseRepos) || config.releaseRepos.length === 0)
    throw new Error('releaseRepos must be a nonempty array of repository URLs');
  for (const [name, repos] of [['ciRepo', [config.ciRepo]], ['releaseRepos', config.releaseRepos]]) {
    for (const repo of repos) {
      if (typeof repo !== 'string' || !/^https?:$/.test(new URL(repo).protocol))
        throw new Error(`${name} must contain HTTP(S) repository URLs`);
    }
  }
  const major = config.mesVersion.split('.')[0];
  let licenseId = process.env.CMF_E2E_LICENSE_ID || config.licenseId;
  if (!licenseId && process.stdin.isTTY) {
    const prompt = createInterface({ input: process.stdin, output: process.stdout });
    try { licenseId = await prompt.question(`License ID for MES ${major}: `); } finally { prompt.close(); }
  }
  if (!/^\d+$/.test(String(licenseId || ''))) throw new Error(`Set licenseId or CMF_E2E_LICENSE_ID for MES ${config.mesVersion}`);
  const workspace = path.resolve(config.workspace);
  const bundle = path.resolve(config.cliBundle);
  await access(path.join(bundle, 'cmf'), constants.X_OK);
  const seconds = (name, fallback) => {
    const value = config[name] ?? fallback;
    if (!Number.isFinite(value) || value <= 0) throw new Error(`${name} must be a positive number`);
    return value;
  };
  const commandTimeout = seconds('commandTimeoutSeconds', 1800);
  const installationTimeout = seconds('installationTimeoutSeconds', 3600) * 1000;
  const uiTimeout = seconds('uiTimeoutSeconds', 600) * 1000;
  // Resume only a retained failed run; a normal run always needs a fresh directory.
  let previous;
  if (resume) {
    previous = JSON.parse(await readFile(path.join(workspace, '.e2e/report.json'), 'utf8'));
    if (previous.status !== 'failed' || previous.mesVersion !== config.mesVersion || previous.workspace !== workspace)
      throw new Error('Resume requires a failed report for this workspace and MES version');
    if (!previous.runId || !Array.isArray(previous.steps) || !Array.isArray(previous.commands))
      throw new Error('This report predates resume support; choose a fresh workspace');
  }
  if (!resume) try {
    await access(workspace);
    throw new Error(`Workspace already exists: ${workspace}. Choose a fresh workspace in the configuration ` +
      'or move the previous run aside before retrying.');
  } catch (error) { if (error.code !== 'ENOENT') throw error; }
  const state = path.join(workspace, '.e2e');
  const dockerConfig = process.env.DOCKER_CONFIG || path.join(os.homedir(), '.config', 'cmf-cli-e2e', 'docker');
  const overrideConfig = path.join(state, 'devcontainer.json');
  const devcontainerFolder = config.devcontainerWorkspace || workspace;
  const overridePath = path.join(devcontainerFolder, '.e2e', 'devcontainer.json');
  const runId = previous?.runId || randomUUID();
  const containerLabels = ['--id-label', `devcontainer.local_folder=${devcontainerFolder}`,
    '--id-label', `cmf.e2e.run=${runId}`];
  let workspaceReady = resume;
  const types = ['business', 'html', 'help', 'data', 'test', 'iot'];
  const report = previous || { runId, mesVersion: config.mesVersion, workspace, startedAt: new Date().toISOString(),
    status: 'running', steps: [], commands: [], cleanup: [] };
  if (resume) {
    report.status = 'running';
    delete report.error;
    delete report.finishedAt;
    (report.resumedAt ||= []).push(new Date().toISOString());
  }
  let activeStep;
  let containerWorkspace;
  let serveStarted = false;
  let cancelled = false;
  let active;
  let activeBrowser;
  const onSignal = () => {
    cancelled = true; active?.kill('SIGTERM');
    activeBrowser?.close().catch(() => {});
  };
  process.on('SIGINT', onSignal);
  process.on('SIGTERM', onSignal);

  async function saveReport() {
    if (!workspaceReady) return;
    await writeFile(path.join(state, 'report.json'), JSON.stringify(report, null, 2), { mode: 0o600 });
  }

  async function step(name, action) {
    if (cancelled) throw new Error('Run interrupted');
    const completed = previous?.steps.findLast(item => item.name === name)?.status === 'passed';
    if (completed && !['preflight', 'login', 'devcontainer', 'container-login-sync', 'installation', 'serve', 'ui-login', 'stop-serve'].includes(name)) {
      console.log(`\n[${name}] already passed; resuming`);
      return;
    }
    const result = { name, status: 'running', startedAt: new Date().toISOString() };
    activeStep = name;
    report.steps.push(result);
    console.log(`\n[${name}]`);
    await saveReport();
    try { await action(); result.status = 'passed'; }
    catch (error) { result.status = 'failed'; result.error = error.message; throw error; }
    finally { result.finishedAt = new Date().toISOString(); await saveReport(); }
  }

  async function run(command, argv, { cwd = workspaceReady ? workspace : process.cwd(), interactive = false, label = command,
    timeout = commandTimeout, display = { command, argv, cwd, context: 'runner' } } = {}) {
    const logFile = path.join(state, `${label.replace(/[^a-z0-9-]/gi, '-')}.log`);
    const log = interactive || !workspaceReady ? null : createWriteStream(logFile, { flags: 'a', mode: 0o600 });
    const printableCommand = formatCommand(display.command, display.argv, [
      process.env.CMF_E2E_PORTAL_TOKEN, process.env.CMF_E2E_UI_PASSWORD,
    ]);
    const header = `[${display.context}] cwd: ${display.cwd}\n$ ${printableCommand}\n`;
    console.log(header.trimEnd());
    log?.write(header);
    if (workspaceReady) await appendFile(path.join(state, 'commands.log'), header, { mode: 0o600 });
    const record = { step: activeStep, context: display.context, workingDirectory: display.cwd,
      command: printableCommand, startedAt: new Date().toISOString(), status: 'running' };
    report.commands.push(record);
    await saveReport();
    let output = '';
    let timedOut = false;
    try {
      await new Promise((resolve, reject) => {
        const child = spawn(command, argv, { cwd, stdio: interactive ? 'inherit' : ['ignore', 'pipe', 'pipe'],
          env: { ...process.env, NG_CLI_ANALYTICS: 'false',
            DOCKER_CONFIG: dockerConfig,
            PATH: `${bundle}:${process.env.PATH}:${path.join(scriptDirectory, 'node_modules', '.bin')}` } });
        active = child;
        const timer = setTimeout(() => { timedOut = true; child.kill('SIGTERM'); }, timeout * 1000);
        const forceTimer = setTimeout(() => child.kill('SIGKILL'), (timeout + 10) * 1000);
        for (const stream of [child.stdout, child.stderr].filter(Boolean)) stream.on('data', chunk => {
          output = (output + chunk).slice(-1024 * 1024);
          log?.write(chunk);
          process.stdout.write(chunk);
        });
        function done(error) {
          clearTimeout(timer); clearTimeout(forceTimer); active = null; log?.end();
          error ? reject(error) : resolve();
        }
        child.once('error', error => done(error.code === 'ENOENT' ? new Error(
          `${command} is unavailable. ` + (command === 'devcontainer' ?
            'Run "npm --prefix scripts/e2e ci" inside the CLI devcontainer to install the helper tools.' :
            'Install it inside the runner container or add it to PATH.')) : error));
        child.once('close', (code, signal) => done(code === 0 && !timedOut && !cancelled ? null :
          new Error(`${label} failed (${timedOut ? 'timeout' : signal || `exit ${code}`})` +
            (workspaceReady ? `; see ${logFile}` : '; see the output above'))));
      });
      record.status = 'passed';
    }
    catch (error) { record.status = 'failed'; throw error; }
    finally { record.finishedAt = new Date().toISOString(); await saveReport(); }
    return output;
  }

  const remotePath = relative => path.posix.join(containerWorkspace, relative);
  async function inside(command, argv = [], cwd = containerWorkspace, label = command) {
    return run('devcontainer', ['exec', '--workspace-folder', devcontainerFolder, '--override-config', overridePath,
      ...containerLabels,
      'node', remotePath('.e2e/exec.mjs'), cwd, String(commandTimeout),
      JSON.stringify([command, ...argv]), remotePath('.e2e/cli'), remotePath('.e2e/command.pid')],
      { label, timeout: commandTimeout + 60, display: { command, argv, cwd, context: 'devcontainer' } });
  }
  async function cli(argv, directory = containerWorkspace, label = argv.join('-')) {
    return inside(remotePath('.e2e/cli/cmf'), argv, directory, label);
  }
  const serve = action => inside('node', [remotePath('.e2e/serve.mjs'), action,
    remotePath('.e2e'), remotePath(path.relative(workspace, directories.html))], containerWorkspace, `serve-${action}`);
  const directories = {};

  try {
    await step('preflight', async () => {
      // Keep headless Docker authentication separate from VS Code's IPC credential helper.
      await mkdir(dockerConfig, { recursive: true, mode: 0o700 });
      await run('devcontainer', ['--version'], { label: 'devcontainer-version' });
      await run('docker', ['info', '--format', '{{.ServerVersion}}'], { label: 'docker-info' });
      const version = (await run(path.join(bundle, 'cmf'), ['--version'], { label: 'cli-version' })).trim();
      if (resume && version !== report.cliVersion) throw new Error('Resume requires the same candidate CLI version');
      report.cliVersion = version;
      const { chromium } = await import('playwright');
      let browser;
      try {
        browser = await chromium.launch({ headless: true, channel: 'chromium' });
      } catch (error) {
        const missingLibrary = error.message.match(/error while loading shared libraries: ([^:\s]+)/)?.[1];
        const reason = missingLibrary ? `Chromium is missing the Linux library ${missingLibrary}.` : error.message;
        throw new Error(`${reason}\nInstall Chromium and its system dependencies inside the runner's devcontainer:\nnpm --prefix scripts/e2e run browser:install`, { cause: error });
      }
      activeBrowser = browser;
      await browser.close();
      activeBrowser = null;
    });
    // Create artifacts only after prerequisites pass, so setup failures are retryable.
    if (!resume) {
      await mkdir(path.dirname(workspace), { recursive: true });
      try { await mkdir(workspace); }
      catch (error) {
        if (error.code === 'EEXIST') throw new Error(`Workspace already exists: ${workspace}. Choose a fresh workspace.`);
        throw error;
      }
      await mkdir(state, { mode: 0o700 });
      workspaceReady = true;
      await writeFile(path.join(state, 'commands.log'), report.commands.map(record =>
        `[${record.context}] cwd: ${record.workingDirectory}\n$ ${record.command}\n`).join(''), { mode: 0o600 });
    }
    await saveReport();
    await step('login', () => {
      const token = process.env.CMF_E2E_PORTAL_TOKEN;
      const argv = token ? ['login', 'Portal', '--auth-type', 'Bearer', '--token', token, '--no-prompt'] : ['login'];
      return run(path.join(bundle, 'cmf'), argv, { interactive: true, label: 'login' });
    });
    await step('init', async () => {
      await run(path.join(bundle, 'cmf'), ['init', config.projectName || 'test', '--tenant', config.tenant || 'test',
        '--baseVersion', config.mesVersion, '--ciRepo', config.ciRepo, '--releaseRepos', ...config.releaseRepos,
        ...(config.initArgs || [])], { label: 'init', interactive: config.interactiveInit === true });
      await nonemptyFile(path.join(workspace, 'cmfpackage.json'));
      await nonemptyFile(path.join(workspace, '.devcontainer', 'devcontainer.json'));
      await nonemptyFile(path.join(workspace, 'repositories.json'));
    });
    await step('devcontainer', async () => {
      // The generated container runs the exact self-contained candidate, including its templates.
      await cp(bundle, path.join(state, 'cli'), { recursive: true });
      for (const file of ['exec.mjs', 'serve.mjs']) await cp(path.join(scriptDirectory, file), path.join(state, file));
      const configurationOutput = await run('devcontainer', ['read-configuration', '--workspace-folder', devcontainerFolder],
        { label: 'devcontainer-configuration' });
      const parsed = configurationOutput.split(/\r?\n/).map(line => {
        try { return JSON.parse(line); } catch { return null; }
      }).find(value => value?.configuration);
      if (!parsed) throw new Error('Could not read generated devcontainer configuration');
      const containerConfig = { ...parsed.configuration };
      delete containerConfig.configFilePath;
      // VS Code forwardPorts requires its UI. Publish the browser ports for the headless runner.
      containerConfig.runArgs = [...(containerConfig.runArgs || [])];
      const published = new Set();
      for (const [url, containerPort] of [[config.uiUrl || 'http://localhost:80', 80],
        [config.deploymentUrl || 'http://localhost:80/deployment/status', 80],
        [config.serveUrl || 'http://localhost:7000', 7000]]) {
        const address = new URL(url);
        if (!['localhost', '127.0.0.1'].includes(address.hostname)) continue;
        const hostPort = address.port || (address.protocol === 'https:' ? '443' : '80');
        const mapping = `127.0.0.1:${hostPort}:${containerPort}`;
        if (!published.has(mapping)) { containerConfig.runArgs.push('--publish', mapping); published.add(mapping); }
      }
      if (parsed.workspace) Object.assign(containerConfig, parsed.workspace);
      await writeFile(overrideConfig, JSON.stringify(containerConfig, null, 2));
      const output = await run('devcontainer', ['up', '--workspace-folder', devcontainerFolder, '--override-config', overridePath,
        ...containerLabels],
        { label: 'devcontainer-up' });
      const result = output.split(/\r?\n/).map(line => {
        try { return JSON.parse(line); } catch { return null; }
      }).find(value => value?.outcome === 'success' && value?.remoteWorkspaceFolder);
      if (!result) throw new Error('devcontainer up did not report a successful workspace');
      containerWorkspace = result.remoteWorkspaceFolder;
      report.containerId = result.containerId;
      const version = (await cli(['--version'], containerWorkspace, 'container-cli-version')).trim();
      if (!version.endsWith(report.cliVersion)) throw new Error('Candidate CLI versions differ between containers');
      await cli(['dev', 'local', 'stack', '--help'], containerWorkspace, 'plugin-check');
    });
    await step('container-login-sync', () => cli(['login', 'sync'], containerWorkspace, 'container-login-sync'));
    for (const type of types) {
      await step(`scaffold-${type}`, async () => {
        const extra = type === 'data' ? ['--businessPackage', remotePath(path.relative(workspace, directories.business))] :
          type === 'iot' ? ['--htmlPackageLocation', remotePath(path.relative(workspace, directories.html))] : [];
        const scaffoldAlreadyRan = resume && report.commands.some(record =>
          record.step === `scaffold-${type}` && record.status === 'passed');
        if (!scaffoldAlreadyRan) await cli(['new', type, ...extra]);
        directories[type] = await packageDirectory(workspace, type);
      });
      directories[type] = await packageDirectory(workspace, type);
    }
    await step('stack-init', async () => {
      await cli(['dev', 'local', 'stack', 'init']);
      await nonemptyFile(path.join(workspace, 'compose.local.yaml'));
    });
    await step('stack-up', () => cli(['dev', 'local', 'stack', 'up', '--licenseId', String(licenseId), '--nobackup']));
    await step('installation', () => waitForHttp(config.deploymentUrl || 'http://localhost:80/deployment/status', {
      timeoutMs: installationTimeout, deployment: true,
      check: async () => { if (cancelled) throw new Error('Run interrupted'); },
      onStatus: status => console.log(status),
    }));
    await step('generate-lbos', async () => {
      await cli(['dev', 'local', 'lbos']);
      await nonemptyFile(path.join(workspace, 'Libs/LBOs/NetStandard/Cmf.LightBusinessObjects.dll'));
      await nonemptyFile(path.join(workspace, 'Libs/LBOs/TypeScript/package.json'));
    });
    for (const type of types) await step(`build-${type}`, () =>
      cli(['build'], remotePath(path.relative(workspace, directories[type])), `build-${type}`));
    await step('serve', async () => {
      // Refuse to accidentally validate a previously running development server.
      try {
        await fetch(config.serveUrl || 'http://localhost:7000', { signal: AbortSignal.timeout(2000) });
        throw new Error('serveUrl already responds; stop the existing development server first');
      } catch (error) { if (error.message.startsWith('serveUrl')) throw error; }
      serveStarted = true;
      await serve('start');
      await waitForHttp(config.serveUrl || 'http://localhost:7000', {
        timeoutMs: uiTimeout, check: async () => {
          if (cancelled) throw new Error('Run interrupted');
          await serve('check');
        },
      });
    });
    await step('ui-login', async () => {
      const { chromium } = await import('playwright');
      const browser = await chromium.launch({ headless: true, channel: 'chromium' });
      activeBrowser = browser;
      let page;
      try {
        page = await browser.newPage();
        page.setDefaultTimeout(uiTimeout);
        page.on('pageerror', error => console.log(`Browser error: ${error.message}`));
        const response = await page.goto(config.uiUrl || 'http://localhost:80', { timeout: uiTimeout });
        if (!response?.ok()) throw new Error(`UI returned HTTP ${response?.status()}`);
        const login = config.login || {};
        // SecurityPortal submits full-page forms. Inspect the new document, not
        // the login form that remains visible while its request is in flight.
        const submitForm = async selector => {
          await Promise.all([
            page.waitForEvent('framenavigated', { predicate: frame => frame === page.mainFrame() }),
            page.locator(selector).click(),
          ]);
          await page.waitForLoadState('domcontentloaded');
        };
        console.log('UI: login page opened');
        await page.locator(login.usernameSelector || "input[name='username']").fill(process.env.CMF_E2E_UI_USER || 'admin');
        await page.locator(login.passwordSelector || "input[name='password']").fill(process.env.CMF_E2E_UI_PASSWORD || 'admin');
        await submitForm(login.submitSelector || "button:has-text('Login')");
        console.log('UI: credentials submitted');
        const sidebar = page.locator(login.sidebarSelector || '.menu-sub-menu');
        const username = page.locator(login.usernameSelector || "input[name='username']");
        // Fresh local MES backups can require an initial password confirmation.
        const confirmation = page.locator(login.resetConfirmationSelector || "input[name='password_confirmation']");
        await sidebar.or(confirmation).or(username).first().waitFor({ state: 'visible' });
        if (await confirmation.isVisible()) {
          await page.locator(login.resetPasswordSelector || "input[name='password']")
            .fill(process.env.CMF_E2E_UI_PASSWORD || 'admin');
          await confirmation.fill(process.env.CMF_E2E_UI_PASSWORD || 'admin');
          await submitForm(login.resetSubmitSelector || "button:has-text('Reset')");
          console.log('UI: initial password confirmation completed');
        }
        await sidebar.or(username).first().waitFor({ state: 'visible' });
        if (await username.isVisible()) {
          const scopeError = "The user doesn't have all the required scopes.";
          if (await page.getByText(scopeError, { exact: true }).isVisible())
            throw new Error(`MES rejected login: ${scopeError} Check the local stack's OAuth scope roles.`);
          await username.fill(process.env.CMF_E2E_UI_USER || 'admin');
          await page.locator(login.passwordSelector || "input[name='password']")
            .fill(process.env.CMF_E2E_UI_PASSWORD || 'admin');
          await submitForm(login.submitSelector || "button:has-text('Login')");
          console.log('UI: additional sign-in submitted');
          await sidebar.or(username).first().waitFor({ state: 'visible' });
          if (await username.isVisible()) throw new Error('Login returned to the login form after the additional sign-in');
        }
        for (const text of ['Administration', 'Business Data']) await sidebar.getByText(text, { exact: true }).first()
          .waitFor({ state: 'visible' });
        console.log('UI: homepage sidebar verified');
        await page.screenshot({ path: path.join(state, 'homepage.png'), fullPage: true });
      } catch (error) {
        if (page) await page.screenshot({ path: path.join(state, 'ui-failure.png'), fullPage: true }).catch(() => {});
        throw error;
      } finally { await browser.close(); activeBrowser = null; }
    });
    await step('stop-serve', async () => { await serve('stop'); serveStarted = false; });
    for (const type of types) await step(`pack-${type}`, async () => {
      const output = remotePath(`.e2e/packages/${type}`);
      await cli(['pack', '--outputDir', output], remotePath(path.relative(workspace, directories[type])), `pack-${type}`);
      const files = await readdir(path.join(state, 'packages', type));
      const archives = files.filter(file => file.endsWith('.zip'));
      if (!archives.length) throw new Error(`No archive produced for ${type}`);
      for (const archive of archives) await nonemptyFile(path.join(state, 'packages', type, archive));
    });
    report.status = 'passed';
  } catch (error) {
    report.status = 'failed'; report.error = error.message; process.exitCode = 1;
    console.error(error.message);
  } finally {
    // Cancellation fails the flow, but must not reject successful cleanup commands.
    const interrupted = cancelled;
    cancelled = false;
    if (interrupted && containerWorkspace) {
      try {
        await run('devcontainer', ['exec', '--workspace-folder', devcontainerFolder, '--override-config', overridePath,
          ...containerLabels,
          'node', remotePath('.e2e/serve.mjs'), 'stop-command', remotePath('.e2e')], { label: 'stop-command', timeout: 30 });
      } catch (error) { report.cleanup.push(error.message); }
    }
    if (serveStarted && containerWorkspace) {
      try { await serve('stop'); report.cleanup.push('Serve process stopped'); }
      catch (error) { report.cleanup.push(error.message); report.status = 'failed'; process.exitCode = 1; }
    }
    report.finishedAt = new Date().toISOString();
    await saveReport();
    console.log(`\nResult: ${report.status}.` + (workspaceReady ? ` Report: ${path.join(state, 'report.json')}` :
      ' No workspace was created; fix the prerequisites and retry.'));
    if (workspaceReady) console.log('Project and artifacts retained for inspection; any started containers remain running.');
    process.off('SIGINT', onSignal); process.off('SIGTERM', onSignal);
  }
}

main().catch(error => { console.error(error.message); process.exitCode = 1; });
