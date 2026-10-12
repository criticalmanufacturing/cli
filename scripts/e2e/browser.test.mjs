import test from 'node:test';
import assert from 'node:assert/strict';
import { createServer } from 'node:http';
import { mkdtemp, readFile, rm } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { verifyHomepage } from './browser.mjs';

async function scenario({ ci = false, reset = true, rejectScope = false, missingLabel = false }) {
  const state = await mkdtemp(path.join(os.tmpdir(), 'cmf-browser-'));
  let needsReset = reset, blank = false, logins = 0, resets = 0, rootRequests = 0;
  const server = createServer((req, res) => {
    res.setHeader('Content-Type', 'text/html');
    if (req.method === 'POST') {
      if (req.url === '/reset') { resets++; needsReset = false; blank = true; }
      else logins++;
      res.writeHead(302, { Location: req.url === '/reset' ? '/' : rejectScope ? '/scope-error' : needsReset ? '/reset' : '/home' });
      res.end();
    } else if (req.url === '/reset') res.end('<form method="POST"><input name="password"><input name="password_confirmation"><button>Reset</button></form>');
    else if (req.url === '/home') res.end('<input name="username" style="display:none"><div class="menu-sub-menu" style="display:none">Hidden</div>' +
      '<div class="menu-sub-menu"><span>Administration</span>' + (missingLabel ? '' : '<span>Business Data</span>') + '</div>');
    else {
      if (req.url === '/') rootRequests++;
      if (blank) { blank = false; res.end('<html><body></body></html>'); }
      else res.end('<form method="POST"><input name="username"><input name="password"><button>Login</button></form>' +
        (req.url === '/scope-error' ? "<div>The user doesn't have all the required scopes.</div>" : ''));
    }
  });
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  const report = {};
  let error;
  try {
    await verifyHomepage({ config: { uiUrl: `http://127.0.0.1:${server.address().port}` },
      state, report, ci, uiTimeout: 2000 });
  } catch (caught) { error = caught; }
  await new Promise(resolve => server.close(resolve));
  return { state, report, error, logins, resets, rootRequests };
}

test('local password reset recovers a blank root once and ignores hidden matches', async () => {
  const result = await scenario({});
  try {
    assert.ifError(result.error);
    assert.equal(result.logins, 2); assert.equal(result.resets, 1); assert.equal(result.rootRequests, 3);
    assert.equal(result.report.uiRecovery.automated, true);
    await readFile(path.join(result.state, 'homepage.png'));
    await readFile(path.join(result.state, 'ui-progress.json'));
  } finally { await rm(result.state, { recursive: true, force: true }); }
});

test('CI fails the blank reset page without retry and saves diagnostics', async () => {
  const result = await scenario({ ci: true });
  try {
    assert.match(result.error?.message, /recovery is disabled in CI/);
    assert.equal(result.logins, 1); assert.equal(result.rootRequests, 2);
    assert.equal(result.report.uiRecovery, undefined);
    await readFile(path.join(result.state, 'ui-failure.png'));
    await assert.rejects(readFile(path.join(result.state, 'homepage.png')), { code: 'ENOENT' });
  } finally { await rm(result.state, { recursive: true, force: true }); }
});

test('scope rejection fails without recovery', async () => {
  const result = await scenario({ reset: false, rejectScope: true });
  try { assert.match(result.error?.message, /MES rejected login:.*required scopes/); assert.equal(result.logins, 1); }
  finally { await rm(result.state, { recursive: true, force: true }); }
});

test('a sidebar missing Business Data fails verification', async () => {
  const result = await scenario({ reset: false, missingLabel: true });
  try { assert.equal(result.error?.name, 'TimeoutError'); await readFile(path.join(result.state, 'ui-failure.png')); }
  finally { await rm(result.state, { recursive: true, force: true }); }
});
