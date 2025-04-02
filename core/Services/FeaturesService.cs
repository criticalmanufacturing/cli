using System;

namespace Cmf.CLI.Core.Services;

public interface IFeaturesService
{
    public bool UseRepositoryClients {
        get;
    }
}

public class FeaturesService : IFeaturesService
{
    private readonly string envvarprefix;
    
    public bool UseRepositoryClients { get; }

    public FeaturesService(string envvarprefix)
    {
        this.envvarprefix = envvarprefix;
        this.UseRepositoryClients = GetFeatureState("use_repository_clients");
    }

    private bool GetFeatureState(string feature)
    {
        var featval = Environment.GetEnvironmentVariable($"{envvarprefix}_feature__{feature}");
        
        return (featval?.ToLowerInvariant() == "true" || featval == "1");
    }
    
}