#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."

registry_mode=${1:-dev}
case "$registry_mode" in
  dev)
    npm_registry=https://dev.criticalmanufacturing.io/repository/npm-public/
    nuget_registry=https://dev.criticalmanufacturing.io/repository/nuget-hosted/index.json
    credentials=(--secret SCAFFOLDING_REGISTRY_TOKEN=)
    ;;
  prod)
    npm_registry=https://criticalmanufacturing.io/repository/npm/
    nuget_registry=https://criticalmanufacturing.io/repository/nuget-hosted/index.json
    # Preserve the existing local token variable; act prompts if neither is set.
    export SCAFFOLDING_REGISTRY_TOKEN=${SCAFFOLDING_REGISTRY_TOKEN:-${CRITICALMANUFACTURING_IO_TOKEN:-}}
    credentials=(--var "CRITICALMANUFACTURING_IO_USER=${CRITICALMANUFACTURING_IO_USER:-tfsproductbuild}"
      --secret SCAFFOLDING_REGISTRY_TOKEN)
    ;;
  *)
    echo "Usage: $0 [dev|prod] [act options...]" >&2
    exit 1
    ;;
esac

# Keep artifacts between runs, outside the checkout and separated by registry.
artifact_dir="${XDG_CACHE_HOME:-$HOME/.cache}/cmf-cli/act-artifacts/$registry_mode"
mkdir -p "$artifact_dir"

# Exercise the main-push path locally; act copies this checkout instead of fetching it.
event_file=$(mktemp -t cli-act-event-XXXXXX.json)
trap 'rm -f "$event_file"' EXIT
printf '{"ref":"refs/heads/main","after":"%s"}\n' "$(git rev-parse HEAD)" > "$event_file"

shift $(( $# > 0 ? 1 : 0 ))

act push \
  --eventpath "$event_file" \
  --workflows .github/workflows/scaffolding-validation.yml \
  --platform ubuntu-latest=catthehacker/ubuntu:act-24.04 \
  --pull=false \
  --artifact-server-path "$artifact_dir" \
  --rm \
  --env-file /dev/null \
  --secret-file /dev/null \
  --var "CRITICALMANUFACTURING_NPM_REGISTRY=$npm_registry" \
  --var "CRITICALMANUFACTURING_NUGET_REGISTRY=$nuget_registry" \
  "${credentials[@]}" "$@"
