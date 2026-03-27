#!/bin/bash
set -e

# Usage: build-and-pack.sh [mode]
#   mode: 'bundle' - build to npm/dist/ (for npm packaging, no zip)
#         'prod'   - build to dist/ and create zip artifacts (default)
MODE=${1:-prod}

platforms=("win-x64" "linux-x64" "osx-x64")

for platform in "${platforms[@]}"; do
  if [ "$MODE" = "bundle" ]; then
    output_dir="npm/dist/$platform"
  else
    output_dir="dist/$platform"
  fi

  echo "Building for $platform..."
  dotnet publish ./cmf-cli/cmf.csproj -c Release -r "$platform" -o "$output_dir" --self-contained /p:IncludeSourceRevisionInInformationalVersion=false

  if [ "$MODE" = "prod" ]; then
    echo "Packing $platform..."
    if [ ! -d "$output_dir" ]; then
      echo "Error: output directory '$output_dir' not found after build." >&2
      exit 1
    fi
    (cd "$output_dir" && zip -X -r "../cmf-cli.$platform.zip" .)
  fi
done
