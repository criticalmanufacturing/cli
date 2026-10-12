#!/bin/bash
set -euo pipefail

scaffold_project() {
  local projectVersion="${1:-1.0.0}"
  local mesVersion="${2:-12.0.0}"

  # Determine the workspace folder (parent directory of this script)
  local scriptDir
  scriptDir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  local workspaceFolder
  workspaceFolder="$(dirname "$scriptDir")"

  local cmfPath="${workspaceFolder}/cmf-cli/bin/Debug/cmf"

  if [ ! -x "$cmfPath" ]; then
    echo "cmf executable not found at $cmfPath"
    return 1
  fi

  # Create a fresh temp directory for this run
  local tmpDir
  tmpDir="$(mktemp -d -t cli-project-XXXXXX)"
  cd "$tmpDir"

  local deploymentBaseDir="$tmpDir/deployment"
  mkdir -p "$deploymentBaseDir"

  # Create infra.json
  cat > infra.json << 'EOF'
{
    "NPMRegistry": "https://dev.criticalmanufacturing.io/repository/npm-public/",
    "CmfPipelineRepository": "https://dev.criticalmanufacturing.io/repository/npm-public/",
    "NuGetRegistry": "https://dev.criticalmanufacturing.io/repository/nuget-hosted/index.json"
}
EOF

  # Create env.json
  cat > env.json << 'EOF'
{
    "SYSTEM_NAME": "cmftraining",
    "TENANT_NAME": "cmftraining",
    "APPLICATION_PUBLIC_HTTP_ADDRESS": "cmftraining.local",
    "APPLICATION_PUBLIC_HTTP_PORT": "80",
    "APPLICATION_PUBLIC_HTTP_TLS_ENABLED": "false",
    "DATABASE_ONLINE_MSSQL_ADDRESS": "db",
    "DATABASE_ONLINE_MSSQL_ADDRESS": "db",
    "SECURITY_PORTAL_STRATEGY_LOCAL_AD_DEFAULT_DOMAIN": "",
    "DATABASE_MSSQL_ALWAYS_ON_ENABLED": "false"
}
EOF

  # Run the cmf init command
  local cmd=(
    "$cmfPath" init ExampleProject
    --version "$projectVersion"
    --infra infra.json
    --config env.json
    --MESVersion "$mesVersion"
    --deploymentDir "$deploymentBaseDir"
  )
  echo "Executing: ${cmd[*]}"
  "${cmd[@]}"

  cd "$tmpDir"
  echo "Scaffolded project in: $tmpDir"
  echo "Working directory is now: $(pwd)"

  if [[ "${BASH_SOURCE[0]}" == "$0" ]] && [[ -t 0 ]]; then
    echo "Opening an interactive shell in the scaffolded project..."
    exec "${SHELL:-/bin/bash}" -i
  fi
}

scaffold_project "$@"
