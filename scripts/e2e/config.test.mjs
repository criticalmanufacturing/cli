import test from 'node:test';
import assert from 'node:assert/strict';
import { resolveConfig, parseArguments, loadConfig } from './config.mjs';
import { mkdtemp, writeFile, rm } from 'node:fs/promises';
import path from 'node:path';
import os from 'node:os';

test('one config selects licenses and workspace by MES version', () => {
  const input = { mesVersion: '11.3.6', licenseIds: { '10': '100', '11': '110' }, portalPat: 'pat' };
  const v11 = resolveConfig(input), v10 = resolveConfig(input, '10.2.16');
  assert.equal(v11.licenseId, '110');
  assert.equal(v10.licenseId, '100');
  assert.notEqual(v10.workspace, v11.workspace);
  assert.deepEqual(v10.releaseRepos, [v10.ciRepo]);
  assert.equal(v10.cliBundle, './npm/dist/linux-x64');
  assert.equal(v10.portalPat, 'pat');
});

test('CI flag and environment prohibit resume and reject ambiguous arguments', () => {
  assert.equal(parseArguments(['--config', 'v11.json', '--ci'], {}).ci, true);
  assert.equal(parseArguments(['--config', 'v11.json'], { CI: 'true' }).ci, true);
  assert.equal(parseArguments(['--config', 'v11.json'], {}).ci, false);
  assert.throws(() => parseArguments(['--config', 'v11.json', '--resume'], { CI: 'true' }), /fresh run/);
  assert.throws(() => parseArguments(['--config', '--ci'], {}), /Expected a value/);
  assert.throws(() => parseArguments(['--config', 'v11.json', '--unknown'], {}), /Unknown argument/);
});

test('dashboard options select a port or disable the live viewer', () => {
  const options = parseArguments(['--config', 'v11.json', '--dashboard-port', '9123', '--no-dashboard'], {});
  assert.equal(options.dashboard, false);
  assert.equal(options.dashboardPort, 9123);
  assert.throws(() => parseArguments(['--config', 'v11.json', '--dashboard-port', '70000'], {}), /valid TCP port/);
});

test('CI requires unattended credentials, a license, and noninteractive initialization', async () => {
  const root = await mkdtemp(path.join(os.tmpdir(), 'cmf-e2e-config-'));
  const saved = Object.fromEntries(['CMF_E2E_PORTAL_PAT', 'CMF_E2E_PORTAL_TOKEN', 'CMF_E2E_LICENSE_ID']
    .map(key => [key, process.env[key]]));
  for (const key of Object.keys(saved)) delete process.env[key];
  try {
    const file = path.join(root, 'config.json');
    await writeFile(file, JSON.stringify({ mesVersion: '11.3.6', licenseId: '123' }));
    await assert.rejects(loadConfig({ file, ci: true }), /CI requires/);
    await writeFile(file, JSON.stringify({ mesVersion: '11.3.6', portalPat: 'pat' }));
    await assert.rejects(loadConfig({ file, ci: true }), /licenseId/);
    await writeFile(file, JSON.stringify({ mesVersion: '11.3.6', portalPat: 'pat', licenseId: '123', interactiveInit: true }));
    await assert.rejects(loadConfig({ file, ci: true }), /interactiveInit/);
    await writeFile(file, JSON.stringify({ mesVersion: '11.3.6', portalPat: 'pat', licenseId: '123' }));
    const config = await loadConfig({ file, ci: true });
    assert.equal(config.licenseId, '123');
    assert.equal(config.uiTimeoutSeconds, 600);
  } finally {
    for (const [key, value] of Object.entries(saved)) {
      if (value === undefined) delete process.env[key]; else process.env[key] = value;
    }
    await rm(root, { recursive: true, force: true });
  }
});

test('explicit settings remain compatible and invalid versions fail', () => {
  const input = { mesVersion: '10.2.16', ciRepo: 'https://example.invalid',
    releaseRepos: ['https://releases.invalid'], workspace: '/tmp/project', cliBundle: '/tmp/bundle', licenseId: '123' };
  assert.deepEqual(resolveConfig(input), input);
  assert.throws(() => resolveConfig(input, '10'), /full MES version/);
});

test('default workspace names are Compose-compatible for stable and prerelease versions', () => {
  for (const mesVersion of ['11.3.6', '12.0.0-beta.2', '12.0.0-BETA.2']) {
    const config = resolveConfig({ mesVersion });
    const folder = path.basename(config.workspace);
    assert.match(folder, /^[a-z0-9][a-z0-9_-]*$/);
    assert.equal(config.mesVersion, mesVersion);
  }
  assert.equal(path.basename(resolveConfig({ mesVersion: '12.0.0-beta.2' }).workspace), 'mes-12_0_0-beta_2');
  assert.notEqual(resolveConfig({ mesVersion: '12.0.0-beta.2' }).workspace,
    resolveConfig({ mesVersion: '12.0.0-beta.3' }).workspace);
});
