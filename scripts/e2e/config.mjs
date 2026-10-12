import path from 'node:path';

export function resolveConfig(input, mesVersion = input.mesVersion) {
  if (typeof mesVersion !== 'string' || !/^\d+\.\d+\.\d+(?:[.-][\w.-]+)?$/.test(mesVersion))
    throw new Error('Use a full MES version');
  const ciRepo = input.ciRepo ?? 'https://dev.criticalmanufacturing.io';
  return {
    ...input, mesVersion, ciRepo,
    releaseRepos: input.releaseRepos ?? [ciRepo],
    workspace: input.workspace ?? path.join('artifacts', 'user-flow', `mes-${mesVersion}`),
    cliBundle: input.cliBundle ?? './npm/dist/linux-x64',
    licenseId: input.licenseId || input.licenseIds?.[mesVersion.split('.')[0]],
  };
}
