#!/usr/bin/env node

/*
 * Validates minimum package age for NuGet dependencies in .csproj files.
 *
 * Env vars:
 * - NUGET_MIN_RELEASE_AGE_DAYS: minimum age in days (default: 7)
 * - NUGET_REGISTRATION_BASE: registration API base url
 *   (default: https://api.nuget.org/v3/registration5-gz-semver2)
 */

const fs = require("fs");
const path = require("path");
const https = require("https");
const zlib = require("zlib");

const repoRoot = process.cwd();
const minAgeDays = Number(process.env.NUGET_MIN_RELEASE_AGE_DAYS || 3);
const registrationBase = process.env.NUGET_REGISTRATION_BASE || "https://api.nuget.org/v3/registration5-gz-semver2";

if (!Number.isFinite(minAgeDays) || minAgeDays < 0) {
  console.error("Invalid NUGET_MIN_RELEASE_AGE_DAYS. Expected a non-negative number.");
  process.exit(2);
}

const ignoredPathSegments = [
  `node_modules${path.sep}`,
  `.git${path.sep}`,
  `bin${path.sep}`,
  `obj${path.sep}`,
  `cmf-cli${path.sep}resources${path.sep}template_feed${path.sep}`,
  `tests${path.sep}Fixtures${path.sep}`,
];

function walkCsprojFiles(dir, acc) {
  const entries = fs.readdirSync(dir, { withFileTypes: true });

  for (const entry of entries) {
    const fullPath = path.join(dir, entry.name);
    const relativePath = path.relative(repoRoot, fullPath);
    const normalized = relativePath.split(path.sep).join(path.sep);

    if (ignoredPathSegments.some((segment) => normalized.includes(segment))) {
      continue;
    }

    if (entry.isDirectory()) {
      walkCsprojFiles(fullPath, acc);
      continue;
    }

    if (entry.isFile() && entry.name.endsWith(".csproj")) {
      acc.push(fullPath);
    }
  }
}

function extractPackageReferences(csprojPath) {
  const content = fs.readFileSync(csprojPath, "utf8");
  const refs = [];

  const inlinePattern = /<PackageReference\s+[^>]*Include="([^"]+)"[^>]*Version="([^"]+)"[^>]*\/?\s*>/g;
  let match;

  while ((match = inlinePattern.exec(content)) !== null) {
    refs.push({ packageId: match[1], version: match[2], sourceFile: csprojPath });
  }

  const blockPattern = /<PackageReference\s+[^>]*Include="([^"]+)"[^>]*>([\s\S]*?)<\/PackageReference>/g;
  while ((match = blockPattern.exec(content)) !== null) {
    const packageId = match[1];
    const body = match[2];
    const versionMatch = body.match(/<Version>([^<]+)<\/Version>/);

    if (versionMatch) {
      refs.push({ packageId, version: versionMatch[1], sourceFile: csprojPath });
    }
  }

  return refs;
}

function isExactVersion(version) {
  if (!version) {
    return false;
  }

  const disallowed = ["*", ",", "[", "]", "(", ")", "$(", "%", " "];
  return !disallowed.some((token) => version.includes(token));
}

function getJson(url) {
  return new Promise((resolve, reject) => {
    https
      .get(url, { timeout: 30000 }, (res) => {
        const { statusCode } = res;
        const chunks = [];

        res.on("data", (chunk) => {
          chunks.push(chunk);
        });

        res.on("end", () => {
          let payload = Buffer.concat(chunks);

          if (payload.length >= 2 && payload[0] === 0x1f && payload[1] === 0x8b) {
            try {
              payload = zlib.gunzipSync(payload);
            } catch (err) {
              reject(new Error(`Failed to decompress gzip response from ${url}: ${err.message}`));
              return;
            }
          }

          const data = payload.toString("utf8");

          if (statusCode && statusCode >= 200 && statusCode < 300) {
            try {
              resolve(JSON.parse(data));
            } catch (err) {
              reject(new Error(`Failed to parse JSON from ${url}: ${err.message}`));
            }
            return;
          }

          if (statusCode === 404) {
            resolve(null);
            return;
          }

          reject(new Error(`HTTP ${statusCode || "unknown"} for ${url}`));
        });
        res.on("error", (err) => {
          reject(new Error(`Failed to read response from ${url}: ${err.message}`));
        });
      })
      .on("error", (err) => {
        reject(err);
      });
  });
}

async function getPublishedDate(packageId, version) {
  const id = packageId.toLowerCase();
  const ver = version.toLowerCase();
  const url = `${registrationBase}/${encodeURIComponent(id)}/${encodeURIComponent(ver)}.json`;
  const payload = await getJson(url);

  if (!payload) {
    return null;
  }

  const published =
    payload.published ||
    (payload.catalogEntry && typeof payload.catalogEntry === "object" ? payload.catalogEntry.published : null);
  if (!published) {
    return null;
  }

  const parsed = new Date(published);
  if (Number.isNaN(parsed.getTime())) {
    return null;
  }

  return parsed;
}

function uniqueKey(packageId, version) {
  return `${packageId.toLowerCase()}@${version.toLowerCase()}`;
}

function toDays(ageMs) {
  return ageMs / (1000 * 60 * 60 * 24);
}

async function main() {
  const csprojFiles = [];
  walkCsprojFiles(repoRoot, csprojFiles);

  const refs = [];
  for (const file of csprojFiles) {
    refs.push(...extractPackageReferences(file));
  }

  const exactRefs = refs.filter((r) => isExactVersion(r.version));

  const deduped = new Map();
  for (const ref of exactRefs) {
    const key = uniqueKey(ref.packageId, ref.version);
    if (!deduped.has(key)) {
      deduped.set(key, { ...ref, allSources: [ref.sourceFile] });
    } else {
      deduped.get(key).allSources.push(ref.sourceFile);
    }
  }

  const now = Date.now();
  const failures = [];
  const warnings = [];

  const items = Array.from(deduped.values());
  for (const item of items) {
    try {
      const publishedAt = await getPublishedDate(item.packageId, item.version);
      if (!publishedAt) {
        warnings.push(
          `${item.packageId}@${item.version}: publish date not available from registration API (skipped)`
        );
        continue;
      }

      const ageDays = toDays(now - publishedAt.getTime());
      if (ageDays < minAgeDays) {
        failures.push({
          packageId: item.packageId,
          version: item.version,
          ageDays,
          publishedAt,
          sourceFile: item.sourceFile,
        });
      }
    } catch (err) {
      warnings.push(`${item.packageId}@${item.version}: ${err.message} (skipped)`);
    }
  }

  console.log(`Checked ${items.length} unique PackageReference versions in ${csprojFiles.length} .csproj files.`);
  console.log(`Minimum required package age: ${minAgeDays} day(s).`);

  if (warnings.length > 0) {
    console.log("\nWarnings:");
    for (const warning of warnings) {
      console.log(`- ${warning}`);
    }
  }

  if (failures.length > 0) {
    console.error("\nNuGet minimum-age check failed for:");
    for (const failure of failures) {
      console.error(
        `- ${failure.packageId}@${failure.version} published ${failure.publishedAt.toISOString()} (${failure.ageDays.toFixed(2)} days old) in ${path.relative(repoRoot, failure.sourceFile)}`
      );
    }
    process.exit(1);
  }

  console.log("NuGet minimum-age check passed.");
}

main().catch((err) => {
  console.error(`Unexpected error: ${err.message}`);
  process.exit(2);
});
