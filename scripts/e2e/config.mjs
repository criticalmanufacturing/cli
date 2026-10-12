import path from 'node:path';
import { readFile } from 'node:fs/promises';
import { createInterface } from 'node:readline/promises';

export const usage = 'Usage: node scripts/e2e/run.mjs --config <file.json> [--mes-version <version>] [--resume] [--ci] [--dashboard-port <port>] [--no-dashboard]';

export function parseArguments(argv, env = process.env) {
  const options = { resume: false, ci: env.CI === 'true', dashboard: true };
  for (let i = 0; i < argv.length; i++) {
    const arg = argv[i];
    if (arg === '--resume') options.resume = true;
    else if (arg === '--ci') options.ci = true;
    else if (arg === '--no-dashboard') options.dashboard = false;
    else if (arg === '--dashboard-port') {
      const value = argv[++i];
      if (!value || value.startsWith('--') || !/^\d+$/.test(value) || Number(value) > 65535)
        throw new Error('Expected a valid TCP port for --dashboard-port');
      options.dashboardPort = Number(value);
    }
    else if (arg === '--config' || arg === '--mes-version') {
      const value = argv[++i];
      if (!value || value.startsWith('--')) throw new Error(`Expected a value for ${arg}`);
      options[arg === '--config' ? 'file' : 'mesVersion'] = value;
    } else throw new Error(`Unknown argument: ${arg}\n${usage}`);
  }
  if (!options.file) throw new Error(usage);
  if (options.ci && options.resume) throw new Error('CI requires a fresh run; --resume is only available locally');
  return options;
}

export function resolveConfig(input, mesVersion = input.mesVersion) {
  if (typeof mesVersion !== 'string' || !/^\d+\.\d+\.\d+(?:[.-][\w.-]+)?$/.test(mesVersion))
    throw new Error('Use a full MES version');
  const ciRepo = input.ciRepo ?? 'https://dev.criticalmanufacturing.io';
  const versionFolder = mesVersion.toLowerCase().replace(/[^a-z0-9_-]/g, '_');
  return {
    ...input, mesVersion, ciRepo,
    releaseRepos: input.releaseRepos ?? [ciRepo],
    workspace: input.workspace ?? path.join('artifacts', 'user-flow', `mes-${versionFolder}`),
    cliBundle: input.cliBundle ?? './npm/dist/linux-x64',
    licenseId: input.licenseId || input.licenseIds?.[mesVersion.split('.')[0]],
  };
}

export async function loadConfig(options) {
  const config = resolveConfig(JSON.parse(await readFile(path.resolve(options.file), 'utf8')), options.mesVersion);
  for (const field of ['ciRepo', 'workspace', 'cliBundle'])
    if (typeof config[field] !== 'string' || !config[field].trim()) throw new Error(`Missing ${field}`);
  if (!Array.isArray(config.releaseRepos) || !config.releaseRepos.length)
    throw new Error('releaseRepos must be a nonempty array of repository URLs');
  for (const [name, repos] of [['ciRepo', [config.ciRepo]], ['releaseRepos', config.releaseRepos]]) {
    for (const repo of repos) {
      let valid = false;
      try { valid = typeof repo === 'string' && /^https?:$/.test(new URL(repo).protocol); } catch {}
      if (!valid) throw new Error(`${name} must contain HTTP(S) repository URLs`);
    }
  }
  config.portalPat = process.env.CMF_E2E_PORTAL_PAT || config.portalPat;
  config.portalToken = process.env.CMF_E2E_PORTAL_TOKEN;
  config.licenseId = process.env.CMF_E2E_LICENSE_ID || config.licenseId;
  if (options.ci && !config.portalPat && !config.portalToken)
    throw new Error('CI requires CMF_E2E_PORTAL_PAT, portalPat, or CMF_E2E_PORTAL_TOKEN');
  if (options.ci && config.interactiveInit)
    throw new Error('CI cannot use interactiveInit');
  if (!config.licenseId && !options.ci && process.stdin.isTTY) {
    const prompt = createInterface({ input: process.stdin, output: process.stdout });
    try { config.licenseId = await prompt.question(`License ID for MES ${config.mesVersion.split('.')[0]}: `); }
    finally { prompt.close(); }
  }
  if (!/^\d+$/.test(String(config.licenseId || '')))
    throw new Error(`Set licenseId or CMF_E2E_LICENSE_ID for MES ${config.mesVersion}`);
  for (const [name, fallback] of [['commandTimeoutSeconds', 1800], ['installationTimeoutSeconds', 3600], ['uiTimeoutSeconds', 600]]) {
    config[name] ??= fallback;
    if (!Number.isFinite(config[name]) || config[name] <= 0) throw new Error(`${name} must be a positive number`);
  }
  if (config.dashboardPort !== undefined && (!Number.isInteger(config.dashboardPort) || config.dashboardPort < 0 || config.dashboardPort > 65535))
    throw new Error('dashboardPort must be a TCP port between 0 and 65535');
  return config;
}
