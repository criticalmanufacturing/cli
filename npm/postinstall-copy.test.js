"use strict";

const fs = require('fs');
const path = require('path');
const vm = require('vm');

// Exercise the real installer, with copy/network operations replaced at the boundary.
function installer(copyError = null) {
    const exec = jest.fn((command, callback) => callback(copyError, 'copy summary', ''));
    const context = {
        __dirname,
        console: { info: jest.fn(), warn: jest.fn(), error: jest.fn(), log: jest.fn() },
        process: { platform: 'win32', arch: 'x64', argv: [] },
        require: (name) => {
            if (name === 'child_process') return { exec };
            if (name === 'fs') return { existsSync: () => true };
            if (name === 'mkdirp') return async () => {};
            if (name === 'axios') return () => { throw new Error('Unexpected download'); };
            if (name === './utils') return {
                parsePackageJson: () => ({ binName: 'cmf.exe', version: '1.0.0' }),
                PLATFORM_MAPPING: { win32: 'win' },
                ARCH_MAPPING: { x64: 'x64' }
            };
            return require(name);
        }
    };
    vm.runInNewContext(fs.readFileSync(path.join(__dirname, 'postinstall.js'), 'utf8'), context);
    return { context, exec };
}

test('bundled Windows installation suppresses per-file output and bounds retries', async () => {
    const { context, exec } = installer();
    const callback = jest.fn();
    await context.install(callback);
    expect(exec).toHaveBeenCalledWith(expect.stringContaining('/nfl /ndl /np /r:2 /w:1'), expect.any(Function));
    expect(callback).toHaveBeenCalledWith(null);
});

test.each([1, 2, 3, 4, 5, 6, 7])('accepts successful Robocopy exit status %i', async (code) => {
    const { context } = installer(Object.assign(new Error('Robocopy status'), { code }));
    const callback = jest.fn();
    await context.install(callback);
    expect(callback).toHaveBeenCalledWith(null);
});

test.each([8, 16, 'ERR_CHILD_PROCESS_STDIO_MAXBUFFER'])('fails installation on copy error %s', async (code) => {
    const error = Object.assign(new Error('Copy failed'), { code });
    const { context } = installer(error);
    const callback = jest.fn();
    await expect(context.install(callback)).rejects.toBe(error);
    expect(callback).not.toHaveBeenCalled();
});
