"use strict";

const fs = require('fs');
const path = require('path');
const { spawnSync } = require('child_process');
const axios = require('axios');
const AdmZip = require('adm-zip');

jest.mock('axios');

describe('CLI installation on disk', () => {
    let workspace;
    let packagePath;
    let sourcePath;
    let installPath;
    let install;
    let platformDescriptor;
    let archDescriptor;

    function writeFile(filePath, contents) {
        fs.mkdirSync(path.dirname(filePath), { recursive: true });
        fs.writeFileSync(filePath, contents);
    }

    function expectPreviousInstallation() {
        expect(fs.readFileSync(path.join(installPath, 'cmf'), 'utf8')).toBe('previous binary');
        expect(fs.readFileSync(path.join(installPath, 'resources/template_feed/init/.project-config.json'), 'utf8'))
            .toContain('$CLI_PARAM_DBReplica1');
    }

    function expectNoStagingDirectories() {
        expect(fs.readdirSync(path.dirname(installPath)).sort()).toEqual(['cmf-cli', 'unrelated-tool']);
    }

    beforeEach(() => {
        // Keep the fixture below npm/ so the actual installer resolves its dependencies.
        workspace = fs.mkdtempSync(path.join(__dirname, '.installer-test-'));
        packagePath = path.join(workspace, 'node_modules/@criticalmanufacturing/cli');
        fs.mkdirSync(packagePath, { recursive: true });
        for (const file of ['postinstall.js', 'utils.js', 'package.json']) {
            fs.copyFileSync(path.join(__dirname, file), path.join(packagePath, file));
        }
        sourcePath = path.join(packagePath, 'dist/linux-x64');
        installPath = path.join(workspace, 'node_modules/.bin/cmf-cli');
        writeFile(path.join(installPath, 'cmf'), 'previous binary');
        writeFile(path.join(installPath, 'resources/template_feed/init/.project-config.json'),
            '{"DBReplica1":"<%= $CLI_PARAM_DBReplica1 %>"}');
        writeFile(path.join(installPath, 'obsolete.dll'), 'obsolete library');
        writeFile(path.join(path.dirname(installPath), 'unrelated-tool'), 'unrelated');
        writeFile(path.join(sourcePath, 'cmf'), 'new binary');
        fs.cpSync(path.join(__dirname, '../cmf-cli/resources/template_feed/init'),
            path.join(sourcePath, 'resources/template_feed/init'), { recursive: true });

        platformDescriptor = Object.getOwnPropertyDescriptor(process, 'platform');
        archDescriptor = Object.getOwnPropertyDescriptor(process, 'arch');
        Object.defineProperty(process, 'platform', { value: 'linux' });
        Object.defineProperty(process, 'arch', { value: 'x64' });
    });

    afterEach(() => {
        jest.restoreAllMocks();
        Object.defineProperty(process, 'platform', platformDescriptor);
        Object.defineProperty(process, 'arch', archDescriptor);
        axios.mockReset();
        fs.rmSync(workspace, { recursive: true, force: true });
    });

    function loadInstaller(platform = 'linux') {
        Object.defineProperty(process, 'platform', { value: platform });
        jest.spyOn(console, 'info').mockImplementation(() => {});
        ({ install } = require(path.join(packagePath, 'postinstall.js')));
    }

    test.each([
        ['linux', 'linux', 'cmf'],
        ['darwin', 'osx', 'cmf'],
        ['win32', 'win', 'cmf.exe']
    ])('replaces obsolete templates and libraries when upgrading on %s', async (platform, rid, binary) => {
        if (rid !== 'linux') {
            const platformSource = path.join(packagePath, `dist/${rid}-x64`);
            fs.renameSync(sourcePath, platformSource);
            if (binary !== 'cmf') fs.renameSync(path.join(platformSource, 'cmf'), path.join(platformSource, binary));
        }
        loadInstaller(platform);
        const callback = jest.fn();

        await install(callback);

        expect(callback).toHaveBeenCalledTimes(1);
        expect(callback).toHaveBeenCalledWith(null);
        expect(fs.readFileSync(path.join(installPath, binary), 'utf8')).toBe('new binary');
        const templatePath = path.join(installPath, 'resources/template_feed/init');
        expect(fs.existsSync(path.join(templatePath, '.project-config.json'))).toBe(false);
        expect(fs.existsSync(path.join(templatePath, '.project-config-v1.json'))).toBe(true);
        expect(fs.readFileSync(path.join(templatePath, '.project-config-v2.json'), 'utf8')).not.toContain('DBReplica1');
        expect(fs.existsSync(path.join(installPath, 'obsolete.dll'))).toBe(false);
        expect(fs.readFileSync(path.join(path.dirname(installPath), 'unrelated-tool'), 'utf8')).toBe('unrelated');
        if (platform !== 'win32') expect(fs.statSync(path.join(installPath, binary)).mode & 0o111).toBe(0o111);
        expect(axios).not.toHaveBeenCalled();
        expectNoStagingDirectories();

        // Reinstalling the same version must also succeed without merging files.
        await install(callback);
        expect(callback).toHaveBeenLastCalledWith(null);
        expectNoStagingDirectories();
    });

    test('preserves the installed version and reports a failed copy', async () => {
        loadInstaller();
        const failure = new Error('disk full');
        jest.spyOn(fs, 'cpSync').mockImplementation(() => { throw failure; });
        const callback = jest.fn();

        await install(callback);

        expect(callback).toHaveBeenCalledWith(failure);
        expectPreviousInstallation();
        expectNoStagingDirectories();
    });

    test('installs into an empty location through the npm script entry point', () => {
        fs.rmSync(installPath, { recursive: true });

        const result = spawnSync(process.execPath, [path.join(packagePath, 'postinstall.js'), 'install'], {
            cwd: workspace,
            encoding: 'utf8',
            timeout: 10000
        });

        expect(result.error).toBeUndefined();
        expect(result.stderr).toBe('');
        expect(result.status).toBe(0);
        expect(fs.readFileSync(path.join(installPath, 'cmf'), 'utf8')).toBe('new binary');
        expect(fs.existsSync(path.join(installPath, 'resources/template_feed/init/.project-config-v2.json'))).toBe(true);
        expectNoStagingDirectories();
    });

    test('restores the installed version if replacing it fails', async () => {
        loadInstaller();
        const failure = new Error('rename failed');
        const rename = fs.renameSync;
        jest.spyOn(fs, 'renameSync').mockImplementation((from, to) => {
            if (path.basename(from) === 'next') throw failure;
            return rename(from, to);
        });
        const callback = jest.fn();

        await install(callback);

        expect(callback).toHaveBeenCalledWith(failure);
        expectPreviousInstallation();
        expectNoStagingDirectories();
    });

    test('keeps the backup and reports its path if restoration also fails', async () => {
        loadInstaller();
        const rename = fs.renameSync;
        jest.spyOn(fs, 'renameSync').mockImplementation((from, to) => {
            if (['next', 'previous'].includes(path.basename(from))) throw new Error('rename failed');
            return rename(from, to);
        });
        const callback = jest.fn();

        await install(callback);

        const stagingDirectory = fs.readdirSync(path.dirname(installPath)).find(name => name.startsWith('.cmf-cli-'));
        const backupPath = path.join(path.dirname(installPath), stagingDirectory, 'previous');
        expect(fs.readFileSync(path.join(backupPath, 'cmf'), 'utf8')).toBe('previous binary');
        expect(callback.mock.calls[0][0].message).toContain(`Previous installation preserved at ${backupPath}`);
    });

    test('rejects a downloaded archive without the expected binary and preserves the installed version', async () => {
        fs.rmSync(sourcePath, { recursive: true });
        const archive = new AdmZip();
        archive.addFile('resources/template_feed/init/.project-config-v2.json', Buffer.from('{}'));
        axios.mockResolvedValue({ data: archive.toBuffer() });
        loadInstaller();
        const callback = jest.fn();

        await install(callback);

        expect(callback).toHaveBeenCalledWith(expect.objectContaining({
            message: expect.stringContaining('Downloaded binary does not contain the binary specified')
        }));
        expectPreviousInstallation();
        expectNoStagingDirectories();
    });
});
