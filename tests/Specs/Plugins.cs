using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cmf.CLI.Core.Constants;
using Cmf.CLI.Core.Objects;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Xunit;

namespace tests.Specs;

public class Plugins
{
    [Fact]
    public void PluginsSuccess()
    {
        var content = @"{""objects"":[{""package"":{""name"":""@criticalmanufacturing/portal"",""scope"":""criticalmanufacturing"",""version"":""1.6.0"",""description"":""Critical Manufacturing Portal CLI"",""keywords"":[""cmf"",""cmf-cli-plugin""],""date"":""2022-09-27T09:12:30.603Z"",""links"":{""npm"":""https://www.npmjs.com/package/%40criticalmanufacturing%2Fportal"",""homepage"":""https://github.com/criticalmanufacturing/portal-sdk#readme"",""repository"":""https://github.com/criticalmanufacturing/portal-sdk"",""bugs"":""https://github.com/criticalmanufacturing/portal-sdk/issues""},""author"":{""name"":""Critical Manufacturing, SA""},""publisher"":{""username"":""x1"",""email"":""x1@example.com""},""maintainers"":[{""username"":""x2"",""email"":""x2@example.com""},{""username"":""x3"",""email"":""x3@example.com""},{""username"":""x4"",""email"":""x4@example.com""},{""username"":""x5"",""email"":""x5@example.com""},{""username"":""x6"",""email"":""x6@example.com""},{""username"":""x7"",""email"":""x7@example.com""},{""username"":""x8"",""email"":""x8@example.com""},{""username"":""x9"",""email"":""x9@example.com""}]},""score"":{""final"":0.22965059277610983,""detail"":{""quality"":0.36747990161041577,""popularity"":0.007828444646624137,""maintenance"":0.3333333333333333}},""searchScore"":2.6164612e-10}],""total"":1,""time"":""Tue Oct 25 2022 15:43:09 GMT+0000 (Coordinated Universal Time)""}";
        var search = $"{CoreConstants.NpmJsUrl.TrimEnd('/')}/-/v1/search?text=+keywords:cmf-cli-plugin";

        // mock HttpClient
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.AbsoluteUri == search),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();

                response.Content = new StringContent(content);
                response.StatusCode = HttpStatusCode.OK;

                return response;
            });
        var httpClient = new HttpClient(httpMessageHandler.Object);

        var npmClient = new NPMClient(httpClient);

        var plugins = npmClient.FindPlugins(null);

        plugins.Should().ContainSingle();
        plugins[0].IsOfficial.Should().BeTrue();
        plugins[0].Name = "@criticalmanufacturing/portal";
        plugins[0].Registry = "https://registry.npmjs.com/";
        plugins[0].Link.AbsoluteUri.Should().Be("https://www.npmjs.com/package/%40criticalmanufacturing%2Fportal");
    }

    [Fact]
    public void PluginsError()
    {
        var search = $"{CoreConstants.NpmJsUrl.TrimEnd('/')}/-/v1/search?text=+keywords:cmf-cli-plugin";

        // mock HttpClient
        var httpMessageHandler = new Mock<HttpMessageHandler>();
        httpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(message => message.RequestUri.AbsoluteUri == search),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                
                response.StatusCode = HttpStatusCode.InternalServerError;

                return response;
            });
        var httpClient = new HttpClient(httpMessageHandler.Object);

        var npmClient = new NPMClient(httpClient);

        var logWriter = (new Logging()).GetLogStringWriter();
        
        var plugins = npmClient.FindPlugins(null);

        plugins.Should().BeEmpty();
        logWriter.ToString().Should().Contain($"Search request to {new Uri(CoreConstants.NpmJsUrl).AbsoluteUri} failed: {HttpStatusCode.InternalServerError}");
    }
}