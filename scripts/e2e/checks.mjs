import { readdir, readFile, stat } from 'node:fs/promises';
import path from 'node:path';

export async function packageDirectory(workspace, type) {
  // Resolve actual casing (Cmf.Custom.HTML versus Cmf.Custom.Html) from disk.
  const entries = await readdir(workspace, { withFileTypes: true });
  const suffix = type === 'test' ? 'tests' : type;
  const matches = entries.filter(entry => entry.isDirectory() &&
    entry.name.toLowerCase() === `cmf.custom.${suffix}`.toLowerCase());
  if (matches.length !== 1) throw new Error(`Expected one Cmf.Custom.${suffix} directory`);
  const directory = path.join(workspace, matches[0].name);
  const manifest = JSON.parse(await readFile(path.join(directory, 'cmfpackage.json'), 'utf8'));
  if (!manifest.packageId || !manifest.packageType || !manifest.version)
    throw new Error(`Incomplete package manifest in ${directory}`);
  if ((await readdir(directory)).filter(name => name !== 'cmfpackage.json').length === 0)
    throw new Error(`No scaffolded content in ${directory}`);
  return directory;
}

export async function nonemptyFile(file) {
  if (!(await stat(file)).isFile() || (await stat(file)).size === 0)
    throw new Error(`Missing or empty file: ${file}`);
}

export async function waitForHttp(url, { timeoutMs, deployment = false, intervalMs = 5000,
  fetchImpl = fetch, onStatus = () => {}, check = async () => {} }) {
  const deadline = Date.now() + timeoutMs;
  let last = 'no response';
  while (Date.now() < deadline) {
    await check();
    let response;
    try {
      response = await fetchImpl(url, {
        redirect: 'manual', signal: AbortSignal.timeout(Math.min(10000, Math.max(1, deadline - Date.now()))),
      });
    } catch (error) { last = error.message; }
    if (response) {
      const body = await response.text();
      last = `HTTP ${response.status}: ${body.slice(0, 500)}`;
      onStatus(last);
      if (deployment && response.status === 400) throw new Error(`EnvManager installation failed: ${last}`);
      // A proxy's HTML landing page is not proof of EnvManager completion.
      if (deployment && response.status === 200) {
        let result;
        try { result = JSON.parse(body); } catch { /* Keep waiting for EnvManager. */ }
        if (result?.Message === 'Deployment finished successfully' ||
            result?.message === 'Deployment finished successfully') return;
      } else if (!deployment && response.status >= 200 && response.status < 400) return;
    }
    await new Promise(resolve => setTimeout(resolve, Math.min(intervalMs, Math.max(0, deadline - Date.now()))));
  }
  throw new Error(`Timed out waiting for ${url}; last response: ${last}`);
}
