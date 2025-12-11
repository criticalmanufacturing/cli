#!/bin/bash

# Example script demonstrating how to use the Business Package Linter
# This script shows how to invoke the linter once the CLI is built

# Example 1: Lint all files in a Business package solution
echo "Example 1: Lint all files in a Business package"
echo "cmf build business lint ./tests/Fixtures/new-packages/Cmf.Custom.Business/Business.sln"
echo ""

# Example 2: Lint specific files
echo "Example 2: Lint specific files"
echo "cmf build business lint ./tests/Fixtures/new-packages/Cmf.Custom.Business/Business.sln MaterialOrchestration.cs"
echo ""

# Example 3: Expected output when Load() is found in foreach
echo "Example 3: Expected output when problematic patterns are found"
echo "Running 1 linting rule(s)..."
echo "Warning: [NoLoadInForeach] /path/to/MaterialOrchestration.cs:23 - Load() method should not be called inside foreach loops. Found in class 'MaterialOrchestration', method 'GetMaterialsWithLoadInLoop'."
echo "Warning: [NoLoadInForeach] /path/to/MaterialOrchestration.cs:24 - Load() method should not be called inside foreach loops. Found in class 'MaterialOrchestration', method 'GetMaterialsWithLoadInLoop'."
echo "Linting complete."
echo ""

# Example 4: Expected output when no issues are found
echo "Example 4: Expected output when no issues are found"
echo "Running 1 linting rule(s)..."
echo "Linting complete."
