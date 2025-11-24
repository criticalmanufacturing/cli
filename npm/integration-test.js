#!/usr/bin/env node

/**
 * Integration test script for testing actual downloads from repositories
 * 
 * This script tests the real download functionality by attempting to download
 * from each repository URL without mocking. Run this manually when needed.
 * 
 * Usage:
 *   node integration-test.js [platform] [arch]
 * 
 * Example:
 *   node integration-test.js linux x64
 *   node integration-test.js win x64
 *   node integration-test.js osx x64
 */

"use strict";

const axios = require('axios');
const path = require('path');

const PLATFORM_MAPPING = {
    "darwin": "osx",
    "linux": "linux",
    "win32": "win"
};

const ARCH_MAPPING = {
    "ia32": "x86",
    "x64": "x64",
};

async function testUrl(url, description) {
    console.log(`\n📡 Testing ${description}...`);
    console.log(`   URL: ${url}`);
    
    try {
        const response = await axios({
            url: url,
            method: 'HEAD', // Just check if the file exists, don't download
            timeout: 10000
        });
        
        console.log(`   ✅ SUCCESS - Status: ${response.status}`);
        if (response.headers['content-length']) {
            const sizeMB = (parseInt(response.headers['content-length']) / 1024 / 1024).toFixed(2);
            console.log(`   📦 Size: ${sizeMB} MB`);
        }
        return true;
    } catch (error) {
        if (error.response) {
            console.log(`   ❌ FAILED - Status: ${error.response.status} ${error.response.statusText}`);
        } else if (error.code === 'ECONNABORTED') {
            console.log(`   ❌ FAILED - Timeout`);
        } else {
            console.log(`   ❌ FAILED - ${error.message}`);
        }
        return false;
    }
}

async function runIntegrationTests() {
    console.log('🧪 CMF CLI Repository Integration Tests\n');
    console.log('=========================================\n');
    
    // Get platform and arch from args or use current system
    const platform = process.argv[2] || process.platform;
    const arch = process.argv[3] || process.arch;
    
    // Map to RID format
    const mappedPlatform = PLATFORM_MAPPING[platform] || platform;
    const mappedArch = ARCH_MAPPING[arch] || arch;
    
    console.log(`Testing for platform: ${platform} (${mappedPlatform})`);
    console.log(`Testing for architecture: ${arch} (${mappedArch})`);
    
    // Use a known version for testing (update this to a real version)
    const version = '5.7.0';
    
    // Define all URLs to test
    const urls = [
        {
            url: `https://github.com/criticalmanufacturing/cli/releases/download/${version}/cmf-cli.${mappedPlatform}-${mappedArch}.zip`,
            description: 'GitHub Releases (Primary)'
        },
        {
            url: `https://criticalmanufacturing.io/repository/tools/cmf-cli.${mappedPlatform}-${mappedArch}.zip`,
            description: 'Critical Manufacturing IO (Fallback 1)'
        },
        {
            url: `https://repository.criticalmanufacturing.com.cn/repository/tools/cmf-cli.${mappedPlatform}-${mappedArch}.zip`,
            description: 'Critical Manufacturing CN (Fallback 2)'
        }
    ];
    
    // Test each URL
    const results = [];
    for (const { url, description } of urls) {
        const success = await testUrl(url, description);
        results.push({ url, description, success });
    }
    
    // Summary
    console.log('\n\n📊 Test Summary');
    console.log('===============\n');
    
    const successCount = results.filter(r => r.success).length;
    const totalCount = results.length;
    
    results.forEach(({ description, success }) => {
        console.log(`${success ? '✅' : '❌'} ${description}`);
    });
    
    console.log(`\n${successCount}/${totalCount} repositories accessible`);
    
    if (successCount === 0) {
        console.log('\n⚠️  WARNING: No repositories are accessible!');
        console.log('This could indicate:');
        console.log('  - Network connectivity issues');
        console.log('  - Version does not exist yet');
        console.log('  - Artifacts not published');
        process.exit(1);
    } else if (successCount < totalCount) {
        console.log('\n⚠️  Some repositories are not accessible.');
        console.log('Users in regions where accessible repos fail may have installation issues.');
        process.exit(0);
    } else {
        console.log('\n✨ All repositories are accessible!');
        process.exit(0);
    }
}

// Run the tests
runIntegrationTests().catch(error => {
    console.error('\n❌ Integration test failed:', error.message);
    process.exit(1);
});
