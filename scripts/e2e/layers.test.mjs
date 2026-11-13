import test from 'node:test';
import assert from 'node:assert/strict';
import { layerServices, waitForLayerServices } from './layers.mjs';

test('generated layer services exclude nested keys and other top-level sections', () => {
  assert.deepEqual(layerServices('services:\n  envmanager:\n    environment:\n      ENABLED: true\n  librechat:\n    ports: []\nvolumes:\n  ferret-data:\n'),
    ['envmanager', 'librechat']);
  assert.deepEqual(layerServices('services:\n  host:\n    ports: []\n'), ['host']);
  assert.throws(() => layerServices('volumes:\n  data:\n'), /No services/);
});

test('layer readiness waits for missing, stopped, unhealthy, and starting services', async () => {
  const snapshots = ['', 'librechat|exited|Exited (1)', 'librechat|running|Up (unhealthy)',
    'librechat|running|Up (health: starting)', 'librechat|running|Up (healthy)', 'librechat|running|Up (healthy)'];
  let calls = 0;
  await waitForLayerServices(['librechat'], async () => snapshots[calls++], { timeoutMs: 1000, intervalMs: 1 });
  assert.equal(calls, 6);
});

test('running services without healthchecks pass; unrelated services cannot satisfy a layer', async () => {
  await waitForLayerServices(['core-rag'], async () => 'core-rag|running|Up 2 seconds', { timeoutMs: 1000, intervalMs: 1 });
  await assert.rejects(waitForLayerServices(['librechat'], async () => 'other|running|Up',
    { timeoutMs: 10, intervalMs: 1 }), /librechat/);
});

test('layer readiness observes cancellation', async () => {
  await assert.rejects(waitForLayerServices(['librechat'], async () => '', {
    timeoutMs: 1000, check: () => { throw new Error('cancelled'); },
  }), /cancelled/);
});
