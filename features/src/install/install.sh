#!/bin/sh
set -e

package=@criticalmanufacturing/cli

echo "Activating feature '${package}/install'"
echo "The requested ${package} version is: ${VERSION}"

# Allow the CLI's postinstall without changing npm config or enabling other scripts.
# Older npm releases do not recognize --allow-scripts.
set -- --global "${package}@${VERSION}"
npm_config=$(npm config list --long)
if printf '%s\n' "$npm_config" | grep -Eq '^(; )?allow-scripts[[:space:]]*='; then
    set -- "$@" "--allow-scripts=${package}"
fi

npm install "$@"
