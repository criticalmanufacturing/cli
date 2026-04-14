// Manual CJS mock for http-proxy-agent v9+
// Needed because http-proxy-agent v9+ uses ES modules which Jest (CommonJS mode) cannot auto-mock.
// jest.mock('http-proxy-agent') in test files will use this file instead of the real module.
const HttpProxyAgent = jest.fn().mockImplementation(function (proxy) {
    return { proxy };
});

module.exports = HttpProxyAgent;
