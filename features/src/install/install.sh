#!/bin/sh
set -e

echo "Activating feature '@criticalmanufacturing/cli/install'"
echo "The requested @criticalmanufacturing/cli version is: ${VERSION}"

npm install --global @criticalmanufacturing/cli@$VERSION
