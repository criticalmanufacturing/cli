import { createServer } from 'node:http';
import { readFile, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { randomUUID } from 'node:crypto';

export const MAX_OUTPUT = 64 * 1024;
export const flow = [
  { title: 'Setup', steps: ['preflight', 'login', 'init', 'devcontainer', 'container-login-sync'] },
  { title: 'Packages', steps: ['scaffold-business', 'scaffold-html', 'scaffold-help', 'scaffold-data', 'scaffold-test', 'scaffold-iot',
    'build-business', 'pack-business', 'build-html', 'pack-html', 'build-help', 'pack-help', 'build-data', 'pack-data',
    'build-test', 'pack-test', 'build-iot', 'pack-iot'] },
  { title: 'Installation', steps: ['stack-init', 'stack-up', 'installation', 'generate-lbos'] },
  { title: 'Browser check', steps: ['serve', 'ui-login', 'stop-serve'] },
  { title: 'Teardown', steps: ['stack-down'] },
];

export const labels = {
  preflight: 'Preflight checks', login: 'Portal login', init: 'Initialize project', devcontainer: 'Start devcontainer',
  'container-login-sync': 'Sync container login', 'stack-init': 'Initialize local stack', 'stack-up': 'Start local stack',
  installation: 'Wait for installation', 'generate-lbos': 'Generate LBOs', serve: 'Serve HTML package',
  'ui-login': 'Verify UI login', 'stop-serve': 'Stop serve process', 'stack-down': 'Remove stack and volumes',
};
for (const type of ['business', 'html', 'help', 'data', 'test', 'iot']) {
  labels[`scaffold-${type}`] = `Scaffold ${type} package`;
  labels[`build-${type}`] = `Build ${type} package`;
  labels[`pack-${type}`] = `Pack ${type} package`;
}

const templatePath = path.join(path.dirname(fileURLToPath(import.meta.url)), 'dashboard-template.html');
const jsonForScript = value => JSON.stringify(value).replaceAll('<', '\\u003c');

function latestSteps(report) {
  const latest = new Map();
  for (const step of report.steps || []) latest.set(step.name, step);
  return [...latest.values()];
}

export function createDashboard(session, { enabled = true, port = 8787 } = {}) {
  let server;
  let address;
  const clients = new Set();
  const outputs = new Map();
  const commandIds = new WeakMap();
  const notices = [];
  let broadcastTimer;
  let lastBroadcastAt = 0;

  const snapshot = () => {
    const report = session.report || { status: 'starting', steps: [], commands: [], cleanup: [] };
    const commands = report.commands.map(record => ({ ...record, dashboardId: commandIds.get(record) }));
    return { report: { ...report, commands }, flow, labels, steps: latestSteps(report), outputs: Object.fromEntries(outputs),
      notices: notices.slice(-100), updatedAt: new Date().toISOString() };
  };
  const broadcast = () => {
    if (!clients.size) return;
    const message = `data: ${jsonForScript(snapshot())}\n\n`;
    for (const client of clients) client.write(message);
  };
  const notify = () => {
    if (!server || !clients.size || broadcastTimer) return;
    const delay = Math.max(0, 200 - (Date.now() - lastBroadcastAt));
    broadcastTimer = setTimeout(() => {
      broadcastTimer = null;
      lastBroadcastAt = Date.now();
      broadcast();
    }, delay);
  };
  const commandKey = record => {
    if (!commandIds.has(record)) commandIds.set(record, `${session.report.commands.indexOf(record)}-${randomUUID()}`);
    return commandIds.get(record);
  };
  const commandStarted = record => {
    const key = commandKey(record);
    outputs.set(key, '');
    notify();
  };
  const commandOutput = (record, chunk) => {
    const key = commandKey(record);
    const secrets = [session.config.portalPat, session.config.portalToken, process.env.CMF_E2E_UI_PASSWORD].filter(Boolean);
    const safeChunk = secrets.reduce((text, secret) => text.split(secret).join('[REDACTED]'), String(chunk));
    outputs.set(key, `${outputs.get(key) || ''}${safeChunk}`.slice(-MAX_OUTPUT));
    notify();
  };
  const log = (step, message) => {
    notices.push({ step, message: String(message), at: new Date().toISOString() });
    notify();
  };
  const progress = (step, message, details = {}) => {
    session.report.uiProgress = { step, message, ...details, at: new Date().toISOString() };
    log(step, message);
  };
  async function asset(file) {
    if (!session.workspaceReady) return null;
    try { return await readFile(path.join(session.state, file)); }
    catch (error) { if (error.code === 'ENOENT') return null; throw error; }
  }
  async function markup(live) {
    const screenshot = await asset('ui-progress.png') || await asset('ui-failure.png') || await asset('homepage.png');
    let html = await readFile(templatePath, 'utf8');
    const client = await readFile(path.join(path.dirname(templatePath), 'dashboard-client.js'), 'utf8');
    if (!live) html = html.replace('<script src="/dashboard.js"></script>', `<script>${client}</script>`);
    const initial = jsonForScript(snapshot());
    const image = jsonForScript(screenshot ? `data:image/png;base64,${screenshot.toString('base64')}` : '');
    return html.replaceAll('__LIVE__', () => String(live)).replaceAll('__INITIAL__', () => initial)
      .replaceAll('__SCREENSHOT__', () => image)
      .replaceAll('__LIVE_LABEL__', () => live ? 'Live updates' : 'Final report');
  }
  function send(response, status, type, body) {
    response.writeHead(status, { 'content-type': type, 'cache-control': 'no-store', 'x-content-type-options': 'nosniff' });
    response.end(body);
  }
  async function handleRequest(request, response) {
    const url = new URL(request.url, 'http://localhost');
    if (url.pathname === '/') return send(response, 200, 'text/html; charset=utf-8', await markup(true));
    if (url.pathname === '/state') return send(response, 200, 'application/json; charset=utf-8', jsonForScript(snapshot()));
    if (url.pathname === '/dashboard.js') {
      return send(response, 200, 'text/javascript; charset=utf-8', await readFile(path.join(path.dirname(templatePath), 'dashboard-client.js'), 'utf8'));
    }
    if (url.pathname === '/events') {
      response.writeHead(200, { 'content-type': 'text/event-stream', 'cache-control': 'no-cache', connection: 'keep-alive' });
      response.write(`data: ${jsonForScript(snapshot())}\n\n`);
      lastBroadcastAt = Date.now();
      clients.add(response);
      response.on('close', () => clients.delete(response));
      return;
    }
    if (url.pathname === '/asset/browser.png') {
      const data = await asset('ui-progress.png') || await asset('ui-failure.png') || await asset('homepage.png');
      if (!data) return send(response, 404, 'text/plain; charset=utf-8', 'No browser capture available yet.');
      response.writeHead(200, { 'content-type': 'image/png', 'cache-control': 'no-store', 'x-content-type-options': 'nosniff' });
      return response.end(data);
    }
    send(response, 404, 'text/plain; charset=utf-8', 'Not found');
  }
  async function start() {
    if (!enabled) return null;
    server = createServer((request, response) => { void handleRequest(request, response).catch(error =>
      send(response, 500, 'text/plain; charset=utf-8', `Dashboard error: ${error.message}`)); });
    await new Promise(resolve => {
      const listen = selectedPort => {
        server.once('error', error => {
          if (error.code === 'EADDRINUSE' && selectedPort !== 0) return listen(0);
          console.warn(`Dashboard unavailable: ${error.message}`); server = null; resolve();
        });
        server.listen(selectedPort, '127.0.0.1', () => { address = server.address(); resolve(); });
      };
      const selectedPort = port === undefined || port === null ? 8787 : Number(port);
      listen(Number.isInteger(selectedPort) && selectedPort >= 0 ? selectedPort : 8787);
    });
    if (address) console.log(`Dashboard: http://127.0.0.1:${address.port}`);
    return address ? `http://127.0.0.1:${address.port}` : null;
  }
  async function writeFinal() {
    if (session.workspaceReady) await writeFile(path.join(session.state, 'dashboard.html'), await markup(false), { mode: 0o600 });
  }
  async function close() {
    clearTimeout(broadcastTimer);
    broadcastTimer = null;
    for (const client of clients) client.end();
    clients.clear();
    if (!server) return;
    await new Promise(resolve => server.close(resolve));
    server = null; address = null;
  }
  return { start, close, writeFinal, notify, commandStarted, commandOutput, log, progress,
    get url() { return address ? `http://127.0.0.1:${address.port}` : null; } };
}
