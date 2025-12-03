const fs = require('fs');
const path = require('path');

jest.mock('fs');

const { parsePackageJson, PLATFORM_MAPPING, ARCH_MAPPING } = require('./utils');

describe('utils.js', () => {
    let originalPlatform;
    let originalArch;

    beforeEach(() => {
        originalPlatform = process.platform;
        originalArch = process.arch;
        jest.clearAllMocks();
    });

    afterEach(() => {
        Object.defineProperty(process, 'platform', {
            value: originalPlatform
        });
        Object.defineProperty(process, 'arch', {
            value: originalArch
        });
    });

    describe('PLATFORM_MAPPING', () => {
        test('should have correct platform mappings', () => {
            expect(PLATFORM_MAPPING).toEqual({
                darwin: 'osx',
                linux: 'linux',
                win32: 'win'
            });
        });
    });

    describe('ARCH_MAPPING', () => {
        test('should have correct architecture mappings', () => {
            expect(ARCH_MAPPING).toEqual({
                ia32: 'x86',
                x64: 'x64'
            });
        });
    });

    describe('parsePackageJson', () => {
        test('should return null for unsupported architecture', () => {
            Object.defineProperty(process, 'arch', {
                value: 'arm64',
                writable: true
            });

            const consoleSpy = jest.spyOn(console, 'error').mockImplementation();
            const result = parsePackageJson('.');

            expect(result).toBeUndefined();
            expect(consoleSpy).toHaveBeenCalledWith(
                expect.stringContaining('Installation is not supported for this architecture')
            );

            consoleSpy.mockRestore();
        });

        test('should return null for unsupported platform', () => {
            Object.defineProperty(process, 'platform', {
                value: 'freebsd',
                writable: true
            });

            const consoleSpy = jest.spyOn(console, 'error').mockImplementation();
            const result = parsePackageJson('.');

            expect(result).toBeUndefined();
            expect(consoleSpy).toHaveBeenCalledWith(
                expect.stringContaining('Installation is not supported for this platform')
            );

            consoleSpy.mockRestore();
        });

        test('should return null when package.json does not exist', () => {
            Object.defineProperty(process, 'platform', {
                value: 'linux',
                writable: true
            });
            Object.defineProperty(process, 'arch', {
                value: 'x64',
                writable: true
            });

            fs.existsSync.mockReturnValue(false);

            const consoleSpy = jest.spyOn(console, 'error').mockImplementation();
            const result = parsePackageJson('.');

            expect(result).toBeUndefined();
            expect(consoleSpy).toHaveBeenCalledWith(
                expect.stringContaining('Unable to find package.json')
            );

            consoleSpy.mockRestore();
        });

        test('should return null for invalid package.json', () => {
            Object.defineProperty(process, 'platform', {
                value: 'linux',
                writable: true
            });
            Object.defineProperty(process, 'arch', {
                value: 'x64',
                writable: true
            });

            fs.existsSync.mockReturnValue(true);
            fs.readFileSync.mockReturnValue(JSON.stringify({
                version: '1.0.0'
                // Missing goBinary
            }));

            const consoleSpy = jest.spyOn(console, 'error').mockImplementation();
            const result = parsePackageJson('.');

            expect(result).toBeUndefined();
            expect(consoleSpy).toHaveBeenCalledWith(
                expect.stringContaining('Invalid package.json')
            );

            consoleSpy.mockRestore();
        });

        test('should parse valid package.json for Linux x64', () => {
            Object.defineProperty(process, 'platform', {
                value: 'linux',
                writable: true
            });
            Object.defineProperty(process, 'arch', {
                value: 'x64',
                writable: true
            });

            fs.existsSync.mockReturnValue(true);
            fs.readFileSync.mockReturnValue(JSON.stringify({
                version: '5.7.0',
                goBinary: {
                    name: 'cmf',
                    path: './bin',
                    url: 'https://github.com/criticalmanufacturing/cli/releases/download/{{version}}/cmf-cli.{{platform}}-{{arch}}.zip'
                }
            }));

            const result = parsePackageJson('.');

            expect(result).toEqual({
                binName: 'cmf',
                binPath: './bin',
                binUrl: 'https://github.com/criticalmanufacturing/cli/releases/download/{{version}}/cmf-cli.{{platform}}-{{arch}}.zip',
                version: '5.7.0'
            });
        });

        test('should parse valid package.json for Windows x64 and add .exe extension', () => {
            Object.defineProperty(process, 'platform', {
                value: 'win32',
                writable: true
            });
            Object.defineProperty(process, 'arch', {
                value: 'x64',
                writable: true
            });

            fs.existsSync.mockReturnValue(true);
            fs.readFileSync.mockReturnValue(JSON.stringify({
                version: '5.7.0',
                goBinary: {
                    name: 'cmf',
                    path: './bin',
                    url: 'https://github.com/criticalmanufacturing/cli/releases/download/{{version}}/cmf-cli.{{platform}}-{{arch}}.zip'
                }
            }));

            const result = parsePackageJson('.');

            expect(result).toEqual({
                binName: 'cmf.exe',
                binPath: './bin',
                binUrl: 'https://github.com/criticalmanufacturing/cli/releases/download/{{version}}/cmf-cli.{{platform}}-{{arch}}.zip',
                version: '5.7.0'
            });
        });

        test('should parse valid package.json for macOS x64', () => {
            Object.defineProperty(process, 'platform', {
                value: 'darwin',
                writable: true
            });
            Object.defineProperty(process, 'arch', {
                value: 'x64',
                writable: true
            });

            fs.existsSync.mockReturnValue(true);
            fs.readFileSync.mockReturnValue(JSON.stringify({
                version: '5.7.0',
                goBinary: {
                    name: 'cmf',
                    path: './bin',
                    url: 'https://github.com/criticalmanufacturing/cli/releases/download/{{version}}/cmf-cli.{{platform}}-{{arch}}.zip'
                }
            }));

            const result = parsePackageJson('.');

            expect(result).toEqual({
                binName: 'cmf',
                binPath: './bin',
                binUrl: 'https://github.com/criticalmanufacturing/cli/releases/download/{{version}}/cmf-cli.{{platform}}-{{arch}}.zip',
                version: '5.7.0'
            });
        });

        test('should strip leading "v" from version', () => {
            Object.defineProperty(process, 'platform', {
                value: 'linux',
                writable: true
            });
            Object.defineProperty(process, 'arch', {
                value: 'x64',
                writable: true
            });

            fs.existsSync.mockReturnValue(true);
            fs.readFileSync.mockReturnValue(JSON.stringify({
                version: 'v5.7.0',
                goBinary: {
                    name: 'cmf',
                    path: './bin',
                    url: 'https://test.com/{{version}}/file.zip'
                }
            }));

            const result = parsePackageJson('.');

            expect(result.version).toBe('5.7.0');
        });

        test('should handle missing version property', () => {
            Object.defineProperty(process, 'platform', {
                value: 'linux',
                writable: true
            });
            Object.defineProperty(process, 'arch', {
                value: 'x64',
                writable: true
            });

            fs.existsSync.mockReturnValue(true);
            fs.readFileSync.mockReturnValue(JSON.stringify({
                goBinary: {
                    name: 'cmf',
                    path: './bin',
                    url: 'https://test.com/file.zip'
                }
            }));

            const consoleSpy = jest.spyOn(console, 'error').mockImplementation();
            const result = parsePackageJson('.');

            expect(result).toBeUndefined();
            expect(consoleSpy).toHaveBeenCalledWith(
                expect.stringContaining("'version' property must be specified")
            );

            consoleSpy.mockRestore();
        });

        test('should handle missing goBinary.name property', () => {
            Object.defineProperty(process, 'platform', {
                value: 'linux',
                writable: true
            });
            Object.defineProperty(process, 'arch', {
                value: 'x64',
                writable: true
            });

            fs.existsSync.mockReturnValue(true);
            fs.readFileSync.mockReturnValue(JSON.stringify({
                version: '5.7.0',
                goBinary: {
                    path: './bin',
                    url: 'https://test.com/file.zip'
                }
            }));

            const consoleSpy = jest.spyOn(console, 'error').mockImplementation();
            const result = parsePackageJson('.');

            expect(result).toBeUndefined();
            expect(consoleSpy).toHaveBeenCalledWith(
                expect.stringContaining("'name' property is necessary")
            );

            consoleSpy.mockRestore();
        });

        test('should handle missing goBinary.path property', () => {
            Object.defineProperty(process, 'platform', {
                value: 'linux',
                writable: true
            });
            Object.defineProperty(process, 'arch', {
                value: 'x64',
                writable: true
            });

            fs.existsSync.mockReturnValue(true);
            fs.readFileSync.mockReturnValue(JSON.stringify({
                version: '5.7.0',
                goBinary: {
                    name: 'cmf',
                    url: 'https://test.com/file.zip'
                }
            }));

            const consoleSpy = jest.spyOn(console, 'error').mockImplementation();
            const result = parsePackageJson('.');

            expect(result).toBeUndefined();
            expect(consoleSpy).toHaveBeenCalledWith(
                expect.stringContaining("'path' property is necessary")
            );

            consoleSpy.mockRestore();
        });
    });
});
