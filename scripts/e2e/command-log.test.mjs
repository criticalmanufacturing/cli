import test from 'node:test';
import assert from 'node:assert/strict';
import { formatCommand, redactArguments } from './command-log.mjs';

test('command logging retains options and quotes paths with spaces', () => {
  assert.equal(formatCommand('/tmp/cli candidate/cmf', ['new', 'data', '--businessPackage', '/tmp/business package']),
    "'/tmp/cli candidate/cmf' new data --businessPackage '/tmp/business package'");
});

test('command logging redacts credentials in options, nested commands, and repository URLs', () => {
  const input = ['login', '--token', 'portal-token', '--password=registry-password',
    'https://user:feed-password@example.invalid/npm/',
    JSON.stringify(['cmf', 'login', '-t', 'nested-token']), 'environment-token'];
  const output = formatCommand('cmf', input, ['environment-token']);
  for (const secret of ['portal-token', 'registry-password', 'feed-password', 'nested-token', 'environment-token'])
    assert.ok(!output.includes(secret), `Leaked ${secret}`);
  assert.ok(output.includes('--token'));
  assert.deepEqual(redactArguments(['--nugetRegistryPassword', 'password']),
    ['--nugetRegistryPassword', '[REDACTED]']);
});
