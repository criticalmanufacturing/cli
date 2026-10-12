const HttpsProxyAgent = jest.fn().mockImplementation(function (proxy) {
    return { proxy };
});

module.exports = HttpsProxyAgent;
