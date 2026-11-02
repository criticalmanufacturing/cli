// Generated Compose layers use two-space service keys, with nested settings
// indented further. Ignore other top-level sections such as volumes/configs.
export function layerServices(compose) {
  const section = compose.match(/^services:\s*\n([\s\S]*?)(?=^\S|$(?![\s\S]))/m)?.[1] || '';
  const services = [...section.matchAll(/^  ([a-zA-Z0-9_-]+):\s*$/gm)].map(match => match[1]);
  if (!services.length) throw new Error('No services found in generated stack layer');
  return services;
}

export async function waitForLayerServices(services, inspect, { timeoutMs, intervalMs = 5000,
  check = () => {}, onStatus = () => {} }) {
  const deadline = Date.now() + timeoutMs;
  let last = '', consecutive = 0;
  while (Date.now() < deadline) {
    check();
    const output = await inspect();
    const states = output.trim().split(/\r?\n/).map(line => line.split('|'));
    const pending = services.filter(service => {
      const state = states.find(row => row[0] === service);
      return !state || state[1] !== 'running' || /unhealthy|health: starting/i.test(state[2]);
    });
    last = pending.length ? `Waiting for layer services: ${pending.join(', ')}` : 'All layer services running (healthy where a healthcheck exists)';
    onStatus(last);
    consecutive = pending.length ? 0 : consecutive + 1;
    if (consecutive >= 2) return;
    await new Promise(resolve => setTimeout(resolve, Math.min(intervalMs, Math.max(0, deadline - Date.now()))));
  }
  throw new Error(`Timed out checking stack layer; ${last}`);
}
