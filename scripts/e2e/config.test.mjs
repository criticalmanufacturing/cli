import test from 'node:test';
import assert from 'node:assert/strict';
import { resolveConfig } from './config.mjs';

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

test('explicit settings remain compatible and invalid versions fail', () => {
  const input = { mesVersion: '10.2.16', ciRepo: 'https://example.invalid',
    releaseRepos: ['https://releases.invalid'], workspace: '/tmp/project', cliBundle: '/tmp/bundle', licenseId: '123' };
  assert.deepEqual(resolveConfig(input), input);
  assert.throws(() => resolveConfig(input, '10'), /full MES version/);
});
