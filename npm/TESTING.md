# NPM Package Tests

This directory contains unit tests for the CMF CLI npm package installation scripts.

## Test Files

### `postinstall.test.js`
Tests for the postinstall script that handles downloading and installing the CLI binary.

**Test Coverage:**
- **downloadAndExtract function:**
  - Successfully downloads and extracts from a URL
  - Handles proxy configuration correctly
  - Throws appropriate errors on download failure

- **Install with fallback repositories:**
  - Tries primary URL (GitHub) first and succeeds
  - Falls back to criticalmanufacturing.io on primary failure
  - Falls back to criticalmanufacturing.com.cn on secondary failure
  - Fails gracefully when all repositories are unavailable
  - Uses correct URL format for each platform (win32, linux, darwin)

### `utils.test.js`
Tests for utility functions used across the npm package.

**Test Coverage:**
- **Platform and Architecture Mappings:**
  - Validates correct platform mappings (darwin→osx, linux→linux, win32→win)
  - Validates correct architecture mappings (ia32→x86, x64→x64)

- **parsePackageJson function:**
  - Handles unsupported architectures
  - Handles unsupported platforms
  - Handles missing package.json file
  - Validates package.json structure
  - Correctly parses valid configurations for all platforms
  - Adds .exe extension for Windows binaries
  - Strips leading "v" from version numbers
  - Validates all required properties (version, goBinary.name, goBinary.path)

## Running Tests

### Install Dependencies
```bash
cd npm
npm install
```

### Run All Tests
```bash
npm test
```

### Run Tests in Watch Mode
```bash
npm run test:watch
```

### Run Tests with Coverage
```bash
npm run test:coverage
```

Coverage reports will be generated in the `npm/coverage` directory.

### Run Integration Tests
```bash
npm run test:integration
```

Integration tests check actual repository availability without mocking. This is useful for:
- Verifying artifacts were published correctly after a release
- Testing network connectivity to all repository URLs
- Validating fallback repository configuration

You can test specific platforms:
```bash
node integration-test.js linux x64
node integration-test.js win x64
node integration-test.js osx x64
```

## Test Framework

These tests use **Jest**, a popular JavaScript testing framework. Key features:
- Mock all external dependencies (axios, file system, etc.)
- Test multiple platform/architecture combinations
- Verify fallback repository logic
- Ensure error handling works correctly

## Key Test Scenarios

### 1. Fallback Repository Chain
Tests verify that the installation tries repositories in this order:
1. GitHub releases (primary)
2. criticalmanufacturing.io (fallback 1)
3. criticalmanufacturing.com.cn (fallback 2)

### 2. Cross-Platform Support
Tests verify correct behavior on:
- Windows (win32/x64)
- Linux (linux/x64)
- macOS (darwin/x64)

### 3. Error Handling
Tests verify proper error messages for:
- Network failures
- Unsupported platforms
- Invalid package.json
- Missing required properties

## CI/CD Integration

These tests can be integrated into the GitHub Actions workflow by adding a test step before publishing:

```yaml
- name: Run npm package tests
  run: cd npm && npm install && npm test
  working-directory: .
```

## Extending Tests

To add new tests:
1. Add test cases to the appropriate `*.test.js` file
2. Follow the existing test structure and naming conventions
3. Mock external dependencies appropriately
4. Run tests locally before committing
