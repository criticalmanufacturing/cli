import { createHash } from 'node:crypto';
import { readFile } from 'node:fs/promises';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const admin = 'https://admin.criticalmanufacturing.io';
const packages = `${admin}/api/1/workspaces/repository/repositories/tools/packages`;
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
    // The existing proxy maps a flat filename to <package>/latest/<filename>.
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

export async function publish(files, token, fetcher = fetch) {
  if (!token || !/^[A-Za-z0-9._~+/-]+=*$/.test(token)) {
    throw new Error('Set the CRITICALMANUFACTURING_IO_TOKEN secret to a registry tools deploy token.');
  }
  const headers = { Authorization: `Bearer ${token}`, Accept: 'application/json' };
  const pending = [];
  // Inspect every package before starting any write. Re-runs skip only identical
  // content verified directly in the registry, independently of the public proxy/cache.
  for (const file of files) {
    file.digest = await expected(file);
    const result = await request(fetcher, `${packages}/${encodeURIComponent(file.name)}`, {
      headers, redirect: 'error',
    });
    if (result.status === 404) {
      pending.push(file);
      continue;
    }
    if (!result.ok) throw new Error(`registry lookup failed for ${file.name}: HTTP ${result.status}`);
    const metadata = await result.json();
    if (!Array.isArray(metadata?.versions)) throw new Error('Invalid registry package metadata.');
    if (!metadata.versions.length) {
      pending.push(file);
      continue;
    }
    if (metadata.versions.length !== 1 || metadata.versions[0].version !== 'latest') {
      throw new Error(`Unexpected storage versions for ${file.name}; inspect before retrying.`);
    }
    const url = `${admin}/api/universal/repository/tools/${encodeURIComponent(file.name)}/latest/${encodeURIComponent(file.name)}`;
    const digest = await fingerprint(await request(fetcher, url, { headers, redirect: 'error' }));
    if (digest !== file.digest) throw new Error(`Existing registry artifact differs: ${file.name}; refusing to overwrite.`);
    console.log(`registry already contains identical bytes: ${file.name}`);
  }
  for (const file of pending) {
    const form = new FormData();
    form.append('packageName', file.name);
    // file.name contains the full release version, including prerelease suffixes.
    // This fixed storage path matches installer URLs; npm next/latest tags are separate.
    form.append('packageVersion', 'latest');
    form.append('packageFiles', new Blob([await readFile(file.path)], { type: 'application/zip' }), file.name);
    // Never retry an upload automatically: a lost response may follow a committed write.
    const result = await request(fetcher, `${packages}/single`, {
      method: 'POST', headers, body: form, redirect: 'error',
    });
    if (!result.ok) throw new Error(`registry upload failed for ${file.name}: HTTP ${result.status}; inspect before retrying.`);
    const entries = await result.json();
    if (!Array.isArray(entries) || entries.length !== 1 ||
        entries[0]?.packageFileName !== file.name || entries[0]?.isSucceeded !== true) {
      throw new Error(`registry did not confirm ${file.name}; inspect before retrying.`);
    }
    const url = `${admin}/api/universal/repository/tools/${encodeURIComponent(file.name)}/latest/${encodeURIComponent(file.name)}`;
    if (await fingerprint(await request(fetcher, url, { headers, redirect: 'error' })) !== file.digest) {
      throw new Error(`registry download differs after upload: ${file.name}`);
    }
    console.log(`Published and verified in registry: ${file.name}`);
  }
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
    if (command === 'publish') await publish(files, process.env.CRITICALMANUFACTURING_IO_TOKEN);
    else if (command === 'verify') await verify(files);
    else throw new Error('Usage: node .github/scripts/release-artifacts.mjs publish|verify VERSION [DIRECTORY]');
  } catch (error) {
    console.error(error.message);
    process.exitCode = 1;
  }
}
