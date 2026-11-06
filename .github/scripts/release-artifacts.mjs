import { createHash } from 'node:crypto';
import { readFile } from 'node:fs/promises';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const mirrors = [
  'https://criticalmanufacturing.io/repository/tools',
  'https://repository.criticalmanufacturing.com.cn/repository/tools',
];

export function artifacts(config, version, directory) {
  if (!/^\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?$/.test(version)) {
    throw new Error('Expected a release tag such as 6.0.0 or 6.0.1-0.');
  }
  return config.files.map(file => ({
    path: resolve(directory, file.local.replaceAll('{version}', version)),
    github: `https://github.com/${config.repository}/releases/download/${version}/${file.github.replaceAll('{version}', version)}`,
    name: file.mirror.replaceAll('{version}', version),
  }));
}

export async function fingerprint(response) {
  if (!response.ok || !response.body) throw new Error(`HTTP ${response.status}`);
  const hash = createHash('sha256');
  let size = 0;
  for await (const chunk of response.body) {
    size += chunk.length;
    hash.update(chunk);
  }
  if (!size) throw new Error('Empty download');
  return `${size}:${hash.digest('hex')}`;
}

async function expected(file) {
  const bytes = await readFile(file.path);
  if (!bytes.length) throw new Error(`Empty artifact: ${file.path}`);
  return `${bytes.length}:${createHash('sha256').update(bytes).digest('hex')}`;
}

function request(fetcher, url, options = {}) {
  return fetcher(url, { signal: AbortSignal.timeout(180_000), ...options });
}

export async function verify(files, {
  fetcher = fetch, attempts = 10, delay = ms => new Promise(r => setTimeout(r, ms)),
  bases = mirrors,
} = {}) {
  const checks = files.flatMap(file => [
    { file, url: file.github },
    ...bases.map(base => ({ file, url: `${base}/${file.name}` })),
  ]);
  const results = await Promise.allSettled(checks.map(async ({ file, url }) => {
    const digest = await expected(file);
    for (let attempt = 1; attempt <= attempts; attempt++) {
      try {
        // Anonymous customer GET, no fallback and no publishing credentials.
        if (await fingerprint(await request(fetcher, url)) !== digest) throw new Error('Size/SHA-256 mismatch');
        console.log(`Verified ${url}`);
        return;
      } catch (error) {
        if (attempt === attempts) throw new Error(`${url}: ${error.message}`);
        await delay(30_000);
      }
    }
  }));
  const errors = results.filter(result => result.status === 'rejected').map(result => result.reason.message);
  if (errors.length) throw new Error(`Release artifacts unavailable or incorrect:\n${errors.join('\n')}`);
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  try {
    const [command, version, directory = 'dist'] = process.argv.slice(2);
    const config = JSON.parse(await readFile(resolve(dirname(fileURLToPath(import.meta.url)), '../release-artifacts.json'), 'utf8'));
    const files = artifacts(config, version, directory);
    if (command === 'verify') await verify(files);
    else throw new Error('Usage: node .github/scripts/release-artifacts.mjs verify VERSION [DIRECTORY]');
  } catch (error) {
    console.error(error.message);
    process.exitCode = 1;
  }
}
