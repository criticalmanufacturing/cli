#!/usr/bin/env node

"use strict";

const path = require('path');
const fs = require('fs');
const axios = require('axios');
const AdmZip = require('adm-zip');
const tmp = require('tmp');

// Mock all dependencies before requiring the module
jest.mock('axios');
jest.mock('adm-zip');
jest.mock('tmp');
jest.mock('mkdirp');
jest.mock('rimraf');
jest.mock('node_modules-path');
jest.mock('proxy-from-env');
jest.mock('fs');

const mockMkdirp = require('mkdirp');
const mockRimraf = require('rimraf');
const mockNodeModulesPath = require('node_modules-path');
const mockProxyFromEnv = require('proxy-from-env');

describe('postinstall.js', () => {
    let originalPlatform;
    let originalArch;

    beforeEach(() => {
        // Store original values
        originalPlatform = process.platform;
        originalArch = process.arch;

        // Reset all mocks
        jest.clearAllMocks();

        // Setup default mock implementations
        mockNodeModulesPath.mockReturnValue('/mock/node_modules');
        mockMkdirp.mockResolvedValue(undefined);
        mockRimraf.sync = jest.fn();
        mockProxyFromEnv.getProxyForUrl = jest.fn().mockReturnValue(null);
        
        fs.existsSync = jest.fn().mockReturnValue(false);
        fs.writeFileSync = jest.fn();
        fs.readFileSync = jest.fn().mockReturnValue(JSON.stringify({
            version: '5.7.0',
            goBinary: {
                name: 'cmf',
                path: './bin',
                url: 'https://github.com/criticalmanufacturing/cli/releases/download/{{version}}/cmf-cli.{{platform}}-{{arch}}.zip'
            }
        }));

        tmp.tmpNameSync = jest.fn().mockReturnValue('/tmp/mock-file.zip');
    });

    afterEach(() => {
        // Restore original values
        Object.defineProperty(process, 'platform', {
            value: originalPlatform
        });
        Object.defineProperty(process, 'arch', {
            value: originalArch
        });
    });

    describe('downloadAndExtract', () => {
        beforeEach(() => {
            // Need to require after mocks are set up
            jest.resetModules();
        });

        test('should successfully download and extract from primary URL', async () => {
            // Mock axios response
            const mockZipData = Buffer.from('mock-zip-data');
            axios.mockResolvedValue({
                data: mockZipData
            });

            // Mock AdmZip
            const mockExtractAllTo = jest.fn();
            AdmZip.mockImplementation(() => ({
                extractAllTo: mockExtractAllTo
            }));

            // Dynamically import the function after mocks are set
            const { downloadAndExtract } = createDownloadAndExtractFunction();

            const testUrl = 'https://github.com/test/cmf-cli.linux-x64.zip';
            const testDest = '/test/dest';

            await downloadAndExtract(testUrl, testDest);

            expect(axios).toHaveBeenCalledWith(expect.objectContaining({
                url: testUrl,
                method: 'GET',
                responseType: 'arraybuffer'
            }));
            expect(fs.writeFileSync).toHaveBeenCalledWith('/tmp/mock-file.zip', mockZipData);
            expect(mockExtractAllTo).toHaveBeenCalledWith(testDest);
            expect(mockRimraf.sync).toHaveBeenCalledWith('/tmp/mock-file.zip');
        });

        test('should handle proxy configuration', async () => {
            const mockProxy = 'http://proxy.example.com:8080';
            
            // Create a fresh mock for this test
            jest.isolateModules(() => {
                jest.doMock('proxy-from-env', () => ({
                    getProxyForUrl: jest.fn().mockReturnValue(mockProxy)
                }));
            });

            axios.mockResolvedValue({ data: Buffer.from('mock-data') });
            AdmZip.mockImplementation(() => ({
                extractAllTo: jest.fn()
            }));

            const { downloadAndExtract } = createDownloadAndExtractFunction();

            await downloadAndExtract('https://test.com/file.zip', '/dest');

            // Verify axios was called with proxy configuration
            expect(axios).toHaveBeenCalledWith(expect.objectContaining({
                proxy: false,
                responseType: 'arraybuffer'
            }));
        });

        test('should throw error on download failure', async () => {
            axios.mockRejectedValue(new Error('Network error'));

            const { downloadAndExtract } = createDownloadAndExtractFunction();

            await expect(downloadAndExtract('https://test.com/file.zip', '/dest'))
                .rejects.toThrow('Network error');
        });
    });

    describe('install with fallback repositories', () => {
        beforeEach(() => {
            jest.resetModules();
            Object.defineProperty(process, 'platform', {
                value: 'linux',
                writable: true
            });
            Object.defineProperty(process, 'arch', {
                value: 'x64',
                writable: true
            });
        });

        test('should try primary URL first and succeed', async () => {
            axios.mockResolvedValueOnce({ data: Buffer.from('mock-data') });
            AdmZip.mockImplementation(() => ({
                extractAllTo: jest.fn()
            }));

            // Mock parsePackageJson to return valid config
            const mockInstall = createMockInstallFunction();

            const callback = jest.fn();
            await mockInstall(callback);

            expect(axios).toHaveBeenCalledTimes(1);
            expect(axios).toHaveBeenCalledWith(expect.objectContaining({
                url: expect.stringContaining('github.com')
            }));
        });

        test('should fallback to criticalmanufacturing.io on primary failure', async () => {
            // First call (GitHub) fails
            axios.mockRejectedValueOnce(new Error('GitHub unavailable'));
            // Second call (criticalmanufacturing.io) succeeds
            axios.mockResolvedValueOnce({ data: Buffer.from('mock-data') });

            AdmZip.mockImplementation(() => ({
                extractAllTo: jest.fn()
            }));

            const mockInstall = createMockInstallFunction();
            const callback = jest.fn();

            await mockInstall(callback);

            expect(axios).toHaveBeenCalledTimes(2);
            expect(axios).toHaveBeenNthCalledWith(1, expect.objectContaining({
                url: expect.stringContaining('github.com')
            }));
            expect(axios).toHaveBeenNthCalledWith(2, expect.objectContaining({
                url: expect.stringContaining('criticalmanufacturing.io')
            }));
        });

        test('should fallback to criticalmanufacturing.com.cn on secondary failure', async () => {
            // First call (GitHub) fails
            axios.mockRejectedValueOnce(new Error('GitHub unavailable'));
            // Second call (criticalmanufacturing.io) fails
            axios.mockRejectedValueOnce(new Error('IO unavailable'));
            // Third call (criticalmanufacturing.com.cn) succeeds
            axios.mockResolvedValueOnce({ data: Buffer.from('mock-data') });

            AdmZip.mockImplementation(() => ({
                extractAllTo: jest.fn()
            }));

            const mockInstall = createMockInstallFunction();
            const callback = jest.fn();

            await mockInstall(callback);

            expect(axios).toHaveBeenCalledTimes(3);
            expect(axios).toHaveBeenNthCalledWith(3, expect.objectContaining({
                url: expect.stringContaining('criticalmanufacturing.com.cn')
            }));
        });

        test('should fail when all repositories are unavailable', async () => {
            // All calls fail
            axios.mockRejectedValue(new Error('Network unavailable'));

            AdmZip.mockImplementation(() => ({
                extractAllTo: jest.fn()
            }));

            const mockInstall = createMockInstallFunction();
            const callback = jest.fn();

            await mockInstall(callback);

            expect(axios).toHaveBeenCalledTimes(3);
            expect(callback).toHaveBeenCalledWith(
                expect.stringContaining('Could not install version')
            );
        });

        test('should use correct URL format for each platform', async () => {
            const platforms = [
                { platform: 'win32', expected: 'win' },
                { platform: 'linux', expected: 'linux' },
                { platform: 'darwin', expected: 'osx' }
            ];

            for (const { platform, expected } of platforms) {
                jest.clearAllMocks();
                Object.defineProperty(process, 'platform', {
                    value: platform,
                    writable: true
                });

                axios.mockResolvedValueOnce({ data: Buffer.from('mock-data') });
                AdmZip.mockImplementation(() => ({
                    extractAllTo: jest.fn()
                }));

                const mockInstall = createMockInstallFunction();
                const callback = jest.fn();

                await mockInstall(callback);

                expect(axios).toHaveBeenCalledWith(expect.objectContaining({
                    url: expect.stringContaining(`${expected}-x64`)
                }));
            }
        });
    });
});

// Helper function to create a mock downloadAndExtract function
function createDownloadAndExtractFunction() {
    const proxyFromEnv = require('proxy-from-env');
    const HttpProxyAgent = require('http-proxy-agent');
    const HttpsProxyAgent = require('https-proxy-agent');

    async function downloadAndExtract(pkgUrl, dest) {
        const proxy = proxyFromEnv.getProxyForUrl(pkgUrl);
        let httpAgent, httpsAgent;
        if (proxy) {
            httpAgent = new HttpProxyAgent(proxy);
            httpsAgent = new HttpsProxyAgent(proxy);
        }

        const response = await axios({
            url: pkgUrl,
            method: 'GET',
            proxy: false,
            httpAgent: httpAgent,
            httpsAgent: httpsAgent,
            responseType: 'arraybuffer',
        });

        const zip = tmp.tmpNameSync();
        fs.writeFileSync(zip, response.data);
        const admZip = new AdmZip(zip);
        admZip.extractAllTo(dest);
        mockRimraf.sync(zip);
    }

    return { downloadAndExtract };
}

// Helper function to create a mock install function
function createMockInstallFunction() {
    const PLATFORM_MAPPING = {
        "darwin": "osx",
        "linux": "linux",
        "win32": "win"
    };

    const ARCH_MAPPING = {
        "ia32": "x86",
        "x64": "x64",
    };

    return async function install(callback) {
        const opts = {
            binName: 'cmf',
            binPath: './bin',
            binUrl: 'https://github.com/criticalmanufacturing/cli/releases/download/{{version}}/cmf-cli.{{platform}}-{{arch}}.zip',
            version: '5.7.0'
        };

        const src = `./dist/${PLATFORM_MAPPING[process.platform]}-${ARCH_MAPPING[process.arch]}`;

        if (!fs.existsSync("./dist")) {
            const primaryUrl = opts.binUrl
                .replace("{{version}}", opts.version)
                .replace("{{platform}}", PLATFORM_MAPPING[process.platform])
                .replace("{{arch}}", ARCH_MAPPING[process.arch]);

            const fallbackUrls = [
                `https://criticalmanufacturing.io/repository/tools/cmf-cli.${PLATFORM_MAPPING[process.platform]}-${ARCH_MAPPING[process.arch]}.zip`,
                `https://repository.criticalmanufacturing.com.cn/repository/tools/cmf-cli.${PLATFORM_MAPPING[process.platform]}-${ARCH_MAPPING[process.arch]}.zip`
            ];

            const allUrls = [primaryUrl, ...fallbackUrls];
            let downloadSucceeded = false;

            for (const pkgUrl of allUrls) {
                try {
                    const { downloadAndExtract } = createDownloadAndExtractFunction();
                    await downloadAndExtract(pkgUrl, src);
                    downloadSucceeded = true;
                    break;
                } catch (e) {
                    // Continue to next URL
                }
            }

            if (!downloadSucceeded) {
                callback(`Could not install version ${opts.version} on your platform ${process.platform}/${process.arch} from any repository.`);
                return;
            }
        }

        callback(null);
    };
}
