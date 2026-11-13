import test from 'node:test';
import assert from 'node:assert/strict';
import { createSteps } from './steps.mjs';
import { mkdtemp, mkdir, writeFile, readFile, rm } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';

for (const status of [200, 400]) {
  test(`internal installation status ${status} retains deployment validation`, async () => {
    const calls = [];
    const runtime = {
      session: { config: { deploymentStatusContainer: 'envmanager', installationTimeoutSeconds: 1 },
        directories: {}, containerWorkspace: '/workspace' },
      container: { inside: async (...args) => {
        calls.push(args);
        return `${JSON.stringify({ message: status === 200 ? 'Deployment finished successfully' : 'Failed' })}\n${status}`;
      } },
      checkCancelled() {},
    };
    const result = createSteps(runtime).installation();
    if (status === 200) await result;
    else await assert.rejects(result, /EnvManager installation failed/);
    assert.equal(calls[0][0], 'installation');
    assert.equal(calls[0][1], 'docker');
    assert.deepEqual(calls[0][2].slice(0, 3), ['exec', 'envmanager', 'curl']);
    assert.equal(calls[0][2].at(-1), 'http://127.0.0.1:8080/deployment/status');
  });
}

for (const mesVersion of ['11.0.0', '12.0.0']) {
  test(`LBO validation accepts the generated .NET output for MES ${mesVersion}`, async () => {
    const workspace = await mkdtemp(path.join(os.tmpdir(), 'cmf-e2e-lbos-'));
    try {
      const libs = path.join(workspace, 'Libs/LBOs');
      await mkdir(path.join(libs, 'NetStandard/Packages'), { recursive: true });
      await mkdir(path.join(libs, 'TypeScript'));
      const netFile = mesVersion.startsWith('12.')
        ? 'NetStandard/Packages/Cmf.LightBusinessObjects.12.0.0.nupkg'
        : 'NetStandard/Cmf.LightBusinessObjects.dll';
      await writeFile(path.join(libs, netFile), 'fixture');
      await writeFile(path.join(libs, 'TypeScript/package.json'), '{}');
      const runtime = { session: { config: { mesVersion }, workspace, directories: {} }, container: {} };
      await createSteps(runtime).generateLbos({ cli: async () => {} });
      if (mesVersion.startsWith('12.')) {
        const previous = path.join(libs, 'NetStandard/Packages/Cmf.LightBusinessObjects.12.0.0-previous.nupkg');
        await writeFile(previous, 'previous run');
        await createSteps(runtime).generateLbos({ cli: async () => {} });
        await rm(previous);
        await rm(path.join(libs, netFile));
        await assert.rejects(createSteps(runtime).generateLbos({ cli: async () => {} }), /Expected a generated/);
      }
    } finally {
      await rm(workspace, { recursive: true, force: true });
    }
  });
}

test('layer startup is cumulative and teardown includes all configured layers', async () => {
  const workspace = await mkdtemp(path.join(os.tmpdir(), 'cmf-e2e-layer-up-'));
  try {
    for (const layer of ['extended', 'genai']) {
      await mkdir(path.join(workspace, '.localenv', layer), { recursive: true });
      await writeFile(path.join(workspace, '.localenv', layer, 'compose.local.yaml'), 'services:\n  host:\n');
    }
    const calls = [];
    const commands = { cli: async argv => { calls.push(argv); } };
    const actions = createSteps({ session: { workspace, directories: {},
      config: { licenseId: '123', stackLayers: ['extended', 'genai'] } }, container: {} });
    await actions.stackLayerUp(commands, 'extended');
    await actions.stackLayerUp(commands, 'genai');
    await actions.stackDown(commands);
    assert.deepEqual(calls, [
      ['dev', 'local', 'stack', 'up', '--licenseId', '123', '--nobackup', '--components', '.localenv/extended'],
      ['dev', 'local', 'stack', 'up', '--licenseId', '123', '--nobackup', '--components', '.localenv/extended', '--components', '.localenv/genai'],
      ['dev', 'local', 'stack', 'down', '--remove-volumes', '--components', '.localenv/extended', '--components', '.localenv/genai'],
    ]);
  } finally {
    await rm(workspace, { recursive: true, force: true });
  }
});

test('project-help check mounts built content without replacing the candidate nginx config', async () => {
  const workspace = await mkdtemp(path.join(os.tmpdir(), 'cmf-e2e-help-'));
  try {
    const help = path.join(workspace, 'Cmf.Custom.Help');
    await mkdir(path.join(help, 'site'), { recursive: true });
    await writeFile(path.join(help, 'cmfpackage.json'), JSON.stringify({ packageId: 'help', packageType: 'help', version: '1.0.0' }));
    await writeFile(path.join(help, 'site/index.html'), 'built help');
    await mkdir(path.join(workspace, '.localenv/extended'), { recursive: true });
    await writeFile(path.join(workspace, '.localenv/extended/compose.local.yaml'), 'services:\n  host:\n');
    const calls = [];
    const actions = createSteps({ session: { workspace, directories: {}, containerWorkspace: '/workspace',
      config: { testProjectHelp: true, licenseId: '123', stackLayers: ['extended'] } },
      container: { inside: async () => '200' } });
    await actions.prepareProjectHelp();
    const compose = await readFile(path.join(workspace, '.localenv/e2e-project-help/compose.local.yaml'), 'utf8');
    assert.match(compose, /Cmf.Custom.Help\/site/);
    assert.match(compose, /read_only: true/);
    assert.doesNotMatch(compose, /nginx|configs:/);
    await actions.stackLayerUp({ cli: async argv => { calls.push(argv); } }, 'extended');
    assert.deepEqual(calls[0].slice(-4), ['--components', '.localenv/extended', '--components', '.localenv/e2e-project-help']);
    await actions.projectHelpReady();
  } finally {
    await rm(workspace, { recursive: true, force: true });
  }
});
