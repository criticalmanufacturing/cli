#!/bin/bash
set -e

# Optional: Import test library bundled with the devcontainer CLI
# Provides the 'check' and 'reportResults' commands.
source dev-container-features-test-lib

# Feature-specific tests
# The 'check' command comes from the dev-container-features-test-lib.
check "cmf" cmf --version | grep "5.2.0"

# Report result
# If any of the checks above exited with a non-zero exit code, the test will fail.
reportResults
