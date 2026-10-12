import test from 'node:test';
import assert from 'node:assert/strict';
import { runFlow } from './flow.mjs';

function gate() {
  let resolve;
  const promise = new Promise(yes => { resolve = yes; });
  return { promise, resolve };
}

test('packages overlap installation; login waits for installation and HTML; tests wait for LBOs', async () => {
  const installation = gate(), libraries = gate(), html = gate(), business = gate();
  const htmlStarted = gate(), businessPacked = gate(), loginStarted = gate(), lbosStarted = gate();
  const calls = [];
  const flow = runFlow({
    types: ['business', 'html', 'test'],
    scaffold: async type => { calls.push(`new-${type}`); },
    build: async type => {
      calls.push(`build-${type}`);
      if (type === 'business') await business.promise;
      if (type === 'html') { htmlStarted.resolve(); await html.promise; }
    },
    pack: async type => { calls.push(`pack-${type}`); if (type === 'business') businessPacked.resolve(); },
    install: async () => { calls.push('install'); await installation.promise; },
    generateLbos: async () => { lbosStarted.resolve(); await libraries.promise; calls.push('lbos'); },
    serve: async () => { calls.push('serve'); },
    login: async () => { calls.push('login'); loginStarted.resolve(); },
    stopServe: async () => { calls.push('stop-serve'); },
    cancel: async () => { assert.fail('unexpected cancellation'); },
  });
  business.resolve();
  await businessPacked.promise;
  assert.ok(calls.includes('install'));
  assert.ok(!calls.includes('login'));
  await htmlStarted.promise;
  installation.resolve();
  await lbosStarted.promise;
  assert.ok(!calls.includes('login'), 'installation alone cannot start login');
  html.resolve();
  await loginStarted.promise;
  assert.ok(!calls.includes('build-test'), 'test build waits for libraries');
  libraries.resolve();
  await flow;
  assert.ok(calls.indexOf('lbos') < calls.indexOf('build-test'));
  assert.ok(calls.indexOf('serve') < calls.indexOf('login'));
  assert.equal(calls.filter(call => call.startsWith('pack-')).length, 3);
});

test('HTML alone cannot start login, and a stack failure cancels and settles other work', async () => {
  const packagesReady = gate(), installation = gate();
  const error = new Error('stack failed');
  let logins = 0, cancellations = 0;
  const flow = runFlow({
    types: ['html', 'test'], scaffold: async () => {}, build: async () => {},
    pack: async type => { if (type === 'html') packagesReady.resolve(); },
    install: async () => { await installation.promise; throw error; },
    generateLbos: async () => { assert.fail('must not generate LBOs'); },
    serve: async () => { logins++; }, login: async () => { logins++; }, stopServe: async () => {},
    cancel: async () => { cancellations++; },
  });
  await packagesReady.promise;
  assert.equal(logins, 0);
  installation.resolve();
  await assert.rejects(flow, candidate => candidate === error);
  assert.equal(logins, 0);
  assert.equal(cancellations, 1);
});

test('LBO generation waits for scaffolding even when installation is already complete', async () => {
  const scaffold = gate(), installed = gate(), generated = gate();
  let lbosGenerated = false;
  const flow = runFlow({
    types: ['html', 'test'], scaffold: async () => { await scaffold.promise; },
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
