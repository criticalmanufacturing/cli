using Cmf.CLI.Core.Services;
using tests.Mocks;
using Xunit;

namespace tests.Specs;

public class FeaturesServiceTests
{
    private const string EnvPrefix = "cmf_cli_test_features";
    private const string RepositoryClientsFeature = "use_repository_clients";
    private const string StreamingPublishFeature = "use_streaming_publish";

    [Fact]
    public void UsesFeatureDefaultsWhenEnvironmentVariablesAreNotSet()
    {
        using var env = new ScopedFeatureEnvironment();

        var sut = new FeaturesService(EnvPrefix);

        Assert.True(sut.UseRepositoryClients);
        Assert.True(sut.UseStreamingPublish);
    }

    [Theory]
    [InlineData("false")]
    [InlineData("0")]
    public void DisablesRepositoryClientsWhenEnvironmentVariableIsFalseOrZero(string value)
    {
        using var env = new ScopedFeatureEnvironment();
        env.Set(RepositoryClientsFeature, value);

        var sut = new FeaturesService(EnvPrefix);

        Assert.False(sut.UseRepositoryClients);
    }

    [Theory]
    [InlineData("false")]
    [InlineData("0")]
    public void DisablesStreamingPublishWhenEnvironmentVariableIsFalseOrZero(string value)
    {
        using var env = new ScopedFeatureEnvironment();
        env.Set(StreamingPublishFeature, value);

        var sut = new FeaturesService(EnvPrefix);

        Assert.False(sut.UseStreamingPublish);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("1")]
    [InlineData(" TRUE ")]
    public void EnablesFeaturesWhenEnvironmentVariablesAreTrueOrOne(string value)
    {
        using var env = new ScopedFeatureEnvironment();
        env.Set(RepositoryClientsFeature, value);
        env.Set(StreamingPublishFeature, value);

        var sut = new FeaturesService(EnvPrefix);

        Assert.True(sut.UseRepositoryClients);
        Assert.True(sut.UseStreamingPublish);
    }

    [Theory]
    [InlineData("maybe")]
    [InlineData("2")]
    [InlineData("yes")]
    public void FallsBackToDefaultWhenEnvironmentVariableHasInvalidValue(string value)
    {
        using var env = new ScopedFeatureEnvironment();
        env.Set(RepositoryClientsFeature, value);
        env.Set(StreamingPublishFeature, value);

        var sut = new FeaturesService(EnvPrefix);

        Assert.True(sut.UseRepositoryClients);
        Assert.True(sut.UseStreamingPublish);
    }

    private sealed class ScopedFeatureEnvironment : System.IDisposable
    {
        private readonly MockEnvironment environment = new();

        public ScopedFeatureEnvironment()
        {
            Set(RepositoryClientsFeature, null);
            Set(StreamingPublishFeature, null);
        }

        public void Set(string feature, string value)
        {
            environment.SetEnvironmentVariable(GetEnvironmentVariableName(feature), value);
        }

        public void Dispose()
        {
            environment.Restore();
        }

        private static string GetEnvironmentVariableName(string feature)
        {
            return $"{EnvPrefix}_feature__{feature}";
        }
    }
}
