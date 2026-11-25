import test from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, mkdir, writeFile, readFile, rm } from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { createContainer } from './container.mjs';

for (const image of [undefined, 'registry.invalid/devcontainer:candidate']) {
  test(`container configuration ${image ? 'selects candidate image' : 'keeps generated build'}`, async () => {
    const root = await mkdtemp(path.join(os.tmpdir(), 'cmf-e2e-container-'));
    try {
      const state = path.join(root, '.e2e');
      const bundle = path.join(root, 'bundle');
      await mkdir(state);
      await mkdir(bundle);
      await writeFile(path.join(bundle, 'cmf'), 'candidate');
      const generated = {
        build: { dockerfile: 'Dockerfile' }, dockerFile: 'Dockerfile', context: '..',
        postCreateCommand: 'setup', runArgs: ['--privileged'],
      };
      const session = {
        config: { devcontainerImage: image }, workspace: root, state, bundle,
        scriptDirectory: import.meta.dirname, report: { runId: 'test', cliVersion: '1.0.0' },
      };
      const commands = { run: async (_step, _command, argv) => {
        if (argv[0] === 'read-configuration') return JSON.stringify({ configuration: generated });
        if (argv[0] === 'up') return JSON.stringify({ outcome: 'success', remoteWorkspaceFolder: '/workspace', containerId: 'test' });
        return '1.0.0';
      } };
      await createContainer(session, commands).start('devcontainer');
      const config = JSON.parse(await readFile(path.join(state, 'devcontainer.json'), 'utf8'));
      assert.equal(config.postCreateCommand, 'setup');
      assert.equal(config.runArgs[0], '--privileged');
      assert.ok(config.runArgs.includes('127.0.0.1:80:80'));
      if (image) {
        assert.equal(config.image, image);
        for (const key of ['build', 'dockerFile', 'context']) assert.equal(key in config, false);
      } else {
        assert.deepEqual(config.build, generated.build);
        assert.equal(config.image, undefined);
      }
    } finally {
      await rm(root, { recursive: true, force: true });
    }
  });
}
