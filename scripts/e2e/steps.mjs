import { access, mkdir, readFile, readdir } from 'node:fs/promises';
import path from 'node:path';
import os from 'node:os';
import { packageDirectory, nonemptyFile, waitForHttp } from './checks.mjs';
import { checkBrowser, verifyHomepage } from './browser.mjs';

export function createSteps(runtime) {
  const { session, container, checkCancelled, setBrowser } = runtime;
  const { config, workspace, state, bundle, report } = session;
  const remotePackage = type => container.remotePath(path.relative(workspace, session.directories[type]));
  const resolvePackage = async type => { session.directories[type] = await packageDirectory(workspace, type); };

  async function preflight(commands) {
    await mkdir(session.dockerConfig, { recursive: true, mode: 0o700 });
    await commands.run('devcontainer', ['--version'], { label: 'devcontainer-version' });
    await commands.run('docker', ['info', '--format', '{{.ServerVersion}}'], { label: 'docker-info' });
    const version = (await commands.run(path.join(bundle, 'cmf'), ['--version'], { label: 'cli-version' })).trim();
    if (session.resume && version !== report.cliVersion) throw new Error('Resume requires the same candidate CLI version');
    report.cliVersion = version;
    await checkBrowser(setBrowser);
  }
  async function login(commands) {
    if (config.portalPat) {
      await commands.run(path.join(bundle, 'cmf'), ['portal', 'login', '--pat', config.portalPat], { label: 'portal-login' });
      config.portalToken = (await readFile(path.join(process.env.XDG_CONFIG_HOME || path.join(os.homedir(), '.config'),
        'cmfportal/cmfportaltoken'), 'utf8')).trim();
      if (!config.portalToken) throw new Error('Portal login did not cache a token');
    }
    const argv = config.portalToken ? ['login', 'Portal', '--auth-type', 'Bearer', '--token', config.portalToken, '--no-prompt'] : ['login'];
    await commands.run(path.join(bundle, 'cmf'), argv, { label: 'login', interactive: !config.portalToken });
  }
  async function init(commands) {
    await commands.run(path.join(bundle, 'cmf'), ['init', config.projectName || 'test', '--tenant', config.tenant || 'test',
      '--baseVersion', config.mesVersion, '--ciRepo', config.ciRepo, '--releaseRepos', ...config.releaseRepos,
      ...(config.initArgs || [])], { label: 'init', interactive: config.interactiveInit === true });
    for (const file of ['cmfpackage.json', '.devcontainer/devcontainer.json', 'repositories.json'])
      await nonemptyFile(path.join(workspace, file));
  }
  async function scaffold(commands, type) {
    const extra = type === 'data' ? ['--businessPackage', remotePackage('business')] :
      type === 'iot' ? ['--htmlPackageLocation', remotePackage('html')] : [];
    const alreadyRan = session.resume && report.commands.some(record => record.step === `scaffold-${type}` && record.status === 'passed');
    if (!alreadyRan) await commands.cli(['new', type, ...extra]);
    await resolvePackage(type);
  }
  async function stackInit(commands) {
    await commands.cli(['dev', 'local', 'stack', 'init']);
    await nonemptyFile(path.join(workspace, 'compose.local.yaml'));
  }
  const stackUp = commands => commands.cli(['dev', 'local', 'stack', 'up', '--licenseId', String(config.licenseId), '--nobackup']);
  const installation = () => waitForHttp(config.deploymentUrl || 'http://localhost:80/deployment/status', {
    timeoutMs: config.installationTimeoutSeconds * 1000, deployment: true,
    check: checkCancelled, onStatus: status => { console.log(status); session.dashboard?.log('installation', status); },
  });
  async function generateLbos(commands) {
    await commands.cli(['dev', 'local', 'lbos']);
    await nonemptyFile(path.join(workspace, 'Libs/LBOs/NetStandard/Cmf.LightBusinessObjects.dll'));
    await nonemptyFile(path.join(workspace, 'Libs/LBOs/TypeScript/package.json'));
  }
  const build = (commands, type) => commands.cli(['build'], remotePackage(type), `build-${type}`);
  async function pack(commands, type) {
    await commands.cli(['pack', '--outputDir', container.remotePath(`.e2e/packages/${type}`)], remotePackage(type), `pack-${type}`);
    const files = await readdir(path.join(state, 'packages', type));
    const archives = files.filter(file => file.endsWith('.zip'));
    if (!archives.length) throw new Error(`No archive produced for ${type}`);
    for (const archive of archives) await nonemptyFile(path.join(state, 'packages', type, archive));
  }
  async function serve(commands) {
    if (session.resume) {
      try {
        await access(path.join(state, 'serve.pid'));
        console.log('Stopping the development server recorded by the previous attempt');
        await commands.serve('stop');
      } catch (error) { if (error.code !== 'ENOENT') throw error; }
    }
    try {
      await fetch(config.serveUrl || 'http://localhost:7000', { signal: AbortSignal.timeout(2000) });
      throw new Error('serveUrl already responds; stop the existing development server first');
    } catch (error) { if (error.message.startsWith('serveUrl')) throw error; }
    session.serveStarted = true;
    await commands.serve('start');
    await waitForHttp(config.serveUrl || 'http://localhost:7000', {
      timeoutMs: config.uiTimeoutSeconds * 1000, check: async () => { checkCancelled(); await commands.serve('check'); },
    });
  }
  const uiLogin = () => verifyHomepage({ config, state, report, ci: session.ci,
    uiTimeout: config.uiTimeoutSeconds * 1000, setBrowser,
    onProgress: (message, details) => session.dashboard?.progress('ui-login', message, details) });
  async function stopServe(commands) { await commands.serve('stop'); session.serveStarted = false; }
  const stackDown = commands => commands.cli(['dev', 'local', 'stack', 'down', '--remove-volumes']);
  return { preflight, login, init, scaffold, resolvePackage, stackInit, stackUp, installation,
    generateLbos, build, pack, serve, uiLogin, stopServe, stackDown };
}
