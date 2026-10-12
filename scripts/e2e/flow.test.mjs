import test from 'node:test';
import assert from 'node:assert/strict';
import { runFlow } from './flow.mjs';

function gate() {
  let resolve;
  const promise = new Promise(yes => { resolve = yes; });
  return { promise, resolve };
}

test('login waits for HTML serve readiness independently of installation; tests wait for LBOs', async () => {
  const installation = gate(), libraries = gate(), html = gate(), business = gate();
  const htmlStarted = gate(), businessPacked = gate(), loginStarted = gate(), lbosStarted = gate();
  const serveStarted = gate(), serveReady = gate();
  const businessScaffoldStarted = gate(), businessScaffoldReady = gate(), businessBuildStarted = gate();
  const calls = [];
  const flow = runFlow({
    types: ['business', 'html', 'test'],
    scaffold: async type => {
      calls.push(`new-${type}`);
      if (type === 'business') { businessScaffoldStarted.resolve(); await businessScaffoldReady.promise; }
    },
    build: async type => {
      calls.push(`build-${type}`);
      if (type === 'business') { businessBuildStarted.resolve(); await business.promise; }
      if (type === 'html') { htmlStarted.resolve(); await html.promise; }
    },
    pack: async type => { calls.push(`pack-${type}`); if (type === 'business') businessPacked.resolve(); },
    install: async () => { calls.push('install'); await installation.promise; },
    generateLbos: async () => { lbosStarted.resolve(); await libraries.promise; calls.push('lbos'); },
    serve: async () => { calls.push('serve'); serveStarted.resolve(); await serveReady.promise; },
    login: async () => { calls.push('login'); loginStarted.resolve(); },
    stopServe: async () => { calls.push('stop-serve'); },
    cancel: async () => { assert.fail('unexpected cancellation'); },
  });
  await businessScaffoldStarted.promise;
  assert.ok(!calls.includes('install'), 'stack startup waits for business scaffolding');
  businessScaffoldReady.resolve();
  await businessBuildStarted.promise;
  assert.ok(!calls.includes('install'), 'stack startup waits for the business build to finish');
  business.resolve();
  await businessPacked.promise;
  assert.ok(calls.includes('install'));
  assert.ok(!calls.includes('login'));
  await htmlStarted.promise;
  assert.ok(!calls.includes('serve'), 'serve cannot start before the HTML build');
  html.resolve();
  await serveStarted.promise;
  assert.ok(!calls.includes('login'), 'login must wait for the HTML server to respond');
  serveReady.resolve();
  await loginStarted.promise;
  assert.ok(!calls.includes('lbos'), 'login can start while installation is still pending');
  assert.ok(!calls.includes('build-test'), 'test build waits for libraries');
  installation.resolve();
  await lbosStarted.promise;
  libraries.resolve();
  await flow;
  assert.ok(calls.indexOf('lbos') < calls.indexOf('build-test'));
  assert.ok(calls.indexOf('serve') < calls.indexOf('login'));
  assert.equal(calls.filter(call => call.startsWith('pack-')).length, 3);
});

test('a stack failure still fails and settles the flow after independent UI login', async () => {
  const packagesReady = gate(), installation = gate();
  const error = new Error('stack failed');
  let logins = 0, cancellations = 0;
  const loginStarted = gate();
  const flow = runFlow({
    types: ['business', 'html', 'test'], scaffold: async () => {}, build: async () => {},
    pack: async type => { if (type === 'html') packagesReady.resolve(); },
    install: async () => { await installation.promise; throw error; },
    generateLbos: async () => { assert.fail('must not generate LBOs'); },
    serve: async () => {}, login: async () => { logins++; loginStarted.resolve(); }, stopServe: async () => {},
    cancel: async () => { cancellations++; },
  });
  await packagesReady.promise;
  await loginStarted.promise;
  assert.equal(logins, 1);
  installation.resolve();
  await assert.rejects(flow, candidate => candidate === error);
  assert.equal(logins, 1);
  assert.equal(cancellations, 1);
});

test('LBO generation waits for scaffolding even when installation is already complete', async () => {
  const scaffold = gate(), installed = gate(), generated = gate();
  let lbosGenerated = false;
  const flow = runFlow({
    types: ['business', 'html', 'test'], scaffold: async type => { if (type !== 'business') await scaffold.promise; },
    build: async () => {}, pack: async () => {},
    install: async () => { installed.resolve(); },
    generateLbos: async () => { lbosGenerated = true; generated.resolve(); },
    serve: async () => {}, login: async () => {}, stopServe: async () => {},
    cancel: async () => { assert.fail('unexpected cancellation'); },
  });
  await installed.promise;
  assert.equal(lbosGenerated, false);
  scaffold.resolve();
  await generated.promise;
  await flow;
  assert.equal(lbosGenerated, true);
});

test('a failed business build prevents stack startup and cancels the flow', async () => {
  const error = new Error('business build failed');
  let installs = 0, cancellations = 0;
  await assert.rejects(runFlow({
    types: ['business', 'html', 'test'], scaffold: async () => {},
    build: async () => { throw error; }, pack: async () => { assert.fail('must not pack'); },
    install: async () => { installs++; }, generateLbos: async () => { assert.fail('must not generate LBOs'); },
    serve: async () => { assert.fail('must not serve'); }, login: async () => { assert.fail('must not login'); },
    stopServe: async () => {}, cancel: async () => { cancellations++; },
  }), candidate => candidate === error);
  assert.equal(installs, 0); assert.equal(cancellations, 1);
});
