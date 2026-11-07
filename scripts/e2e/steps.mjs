import { access, mkdir, readFile, readdir, writeFile } from 'node:fs/promises';
import path from 'node:path';
import os from 'node:os';
import { packageDirectory, nonemptyFile, waitForHttp, cliVersion } from './checks.mjs';
import { checkBrowser, verifyHomepage } from './browser.mjs';
import { layerServices, waitForLayerServices } from './layers.mjs';

export function createSteps(runtime) {
  const { session, container, checkCancelled, setBrowser } = runtime;
  const { config, workspace, state, bundle, report } = session;
  const remotePackage = type => container.remotePath(path.relative(workspace, session.directories[type]));
  const resolvePackage = async type => { session.directories[type] = await packageDirectory(workspace, type); };

  async function preflight(commands) {
    await mkdir(session.dockerConfig, { recursive: true, mode: 0o700 });
    await commands.run('devcontainer', ['--version'], { label: 'devcontainer-version' });
    await commands.run('docker', ['info', '--format', '{{.ServerVersion}}'], { label: 'docker-info' });
    const version = cliVersion(await commands.run(path.join(bundle, 'cmf'), ['--version'], { label: 'cli-version' }));
    if (session.resume && version !== cliVersion(report.cliVersion)) throw new Error('Resume requires the same candidate CLI version');
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
  const waitInstallation = (stepName = 'installation') => waitForHttp(config.deploymentUrl || 'http://localhost:80/deployment/status', {
    timeoutMs: config.installationTimeoutSeconds * 1000, deployment: true,
    ...(config.deploymentStatusContainer ? { fetchImpl: async () => {
      const output = await container.inside(stepName, 'docker', ['exec', config.deploymentStatusContainer,
        'curl', '--silent', '--show-error', '--max-time', '10', '--write-out', '\n%{http_code}',
        'http://127.0.0.1:8080/deployment/status'], session.containerWorkspace, 'deployment-status');
      const separator = output.lastIndexOf('\n');
      return new Response(output.slice(0, separator), { status: Number(output.slice(separator + 1).trim()) });
    } } : {}),
    check: checkCancelled, onStatus: status => { console.log(status); session.dashboard?.log(stepName, status); },
  });
  const installation = () => waitInstallation();
  const helpComponents = () => config.testProjectHelp ? ['.localenv/e2e-project-help'] : [];
  const layerComponents = layer => [...config.stackLayers.slice(0, config.stackLayers.indexOf(layer) + 1)
    .map(name => `.localenv/${name}`), ...helpComponents()];
  async function prepareProjectHelp() {
    await resolvePackage('help');
    await nonemptyFile(path.join(session.directories.help, 'site/index.html'));
    const directory = path.join(workspace, '.localenv/e2e-project-help');
    await mkdir(directory, { recursive: true });
    const site = `./${path.relative(workspace, session.directories.help).split(path.sep).join('/')}/site`;
    await writeFile(path.join(directory, 'compose.local.yaml'),
      `services:\n  project-help:\n    volumes:\n    - type: bind\n      source: ${JSON.stringify(site)}\n      target: /app\n      read_only: true\n`);
  }
  async function projectHelpReady() {
    const output = await container.inside('project-help-ready', 'docker', ['exec', 'project-help', 'curl',
      '--fail', '--silent', '--show-error', '--max-time', '15', '--output', '/dev/null', '--write-out', '%{http_code}',
      'http://127.0.0.1:8080/'], session.containerWorkspace, 'project-help-http');
    if (output.trim() !== '200') throw new Error(`Expected project-help HTTP 200, received ${output.trim()}`);
  }
  async function stackLayerUp(commands, layer) {
    for (const component of layerComponents(layer))
      await nonemptyFile(path.join(workspace, component, 'compose.local.yaml'));
    await commands.cli(['dev', 'local', 'stack', 'up', '--licenseId', String(config.licenseId), '--nobackup',
      ...layerComponents(layer).flatMap(component => ['--components', component])]);
  }
  async function stackLayerReady(_commands, layer) {
    const stepName = `stack-${layer}-ready`;
    await waitInstallation(stepName);
    const services = new Set();
    for (const component of layerComponents(layer))
      for (const service of layerServices(await readFile(path.join(workspace, component, 'compose.local.yaml'), 'utf8')))
        services.add(service);
    const project = `${path.basename(workspace)}-local-${config.mesVersion.replaceAll('.', '_')}`.toLowerCase();
    await waitForLayerServices([...services], () => container.inside(stepName, 'docker', ['ps', '--all',
      '--filter', `label=com.docker.compose.project=${project}`, '--format',
      '{{.Label "com.docker.compose.service"}}|{{.State}}|{{.Status}}'], session.containerWorkspace, `stack-${layer}-services`), {
      timeoutMs: config.stackLayerTimeoutSeconds * 1000, check: checkCancelled,
      onStatus: status => { console.log(status); session.dashboard?.log(stepName, status); },
    });
  }
  async function generateLbos(commands) {
    await commands.cli(['dev', 'local', 'lbos']);
    if (Number(config.mesVersion.split('.')[0]) >= 12) {
      const packages = path.join(workspace, 'Libs/LBOs/NetStandard/Packages');
      const files = (await readdir(packages)).filter(file => /^Cmf\.LightBusinessObjects\.[\d].*\.nupkg$/i.test(file));
      if (!files.length) throw new Error('Expected a generated Cmf.LightBusinessObjects NuGet package');
      for (const file of files) await nonemptyFile(path.join(packages, file));
    } else {
      await nonemptyFile(path.join(workspace, 'Libs/LBOs/NetStandard/Cmf.LightBusinessObjects.dll'));
    }
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
  const stackDown = commands => commands.cli(['dev', 'local', 'stack', 'down', '--remove-volumes',
    ...[...(config.stackLayers || []).map(layer => `.localenv/${layer}`), ...helpComponents()]
      .flatMap(component => ['--components', component])]);
  return { preflight, login, init, scaffold, resolvePackage, stackInit, stackUp, installation,
    stackLayerUp, stackLayerReady, prepareProjectHelp, projectHelpReady, generateLbos, build, pack, serve, uiLogin, stopServe, stackDown };
}
