using System;
using System.Collections.Generic;

namespace Cmf.CLI.Core.Services;

public interface IFeaturesService
{
    public bool UseRepositoryClients {
        get;
    }
    
    public bool UseStreamingPublish {
        get;
    }
}

public class FeaturesService : IFeaturesService
{
    private readonly string envvarprefix;
    private static readonly Dictionary<string, bool> FeatureDefaults = new()
    {
        ["use_repository_clients"] = true,
        ["use_streaming_publish"] = true,
    };
    
    public bool UseRepositoryClients { get; }
    public bool UseStreamingPublish { get; }

    public FeaturesService(string envvarprefix)
    {
        this.envvarprefix = envvarprefix;
        this.UseRepositoryClients = ResolveFeatureState("use_repository_clients");
        this.UseStreamingPublish = ResolveFeatureState("use_streaming_publish");
    }

    private bool ResolveFeatureState(string feature)
    {
        if (!FeatureDefaults.TryGetValue(feature, out var defaultValue))
        {
            Log.Warning($"Feature '{feature}' has no configured default, defaulting to disabled.");
            defaultValue = false;
        }

        return ResolveFeatureState(feature, defaultValue);
    }

    private bool ResolveFeatureState(string feature, bool defaultValue)
    {
        var featval = Environment.GetEnvironmentVariable($"{envvarprefix}_feature__{feature}");
        Log.Debug($"Feature '{feature}' got value '{featval}'");

        if (string.IsNullOrWhiteSpace(featval))
        {
            Log.Debug($"Feature '{feature}' is set to default value '{defaultValue}'");
            return defaultValue;
        }

        var parsedValue = ParseFeatureValue(featval);
        if (parsedValue is null)
        {
            Log.Warning($"Feature '{feature}' has invalid value '{featval}', using default '{defaultValue}'.");
            return defaultValue;
        }

        Log.Debug($"Feature '{feature}' resolved to '{parsedValue.Value}'");
        return parsedValue.Value;
    }

    private static bool? ParseFeatureValue(string featval)
    {
        var normalizedValue = featval.Trim().ToLowerInvariant();
        return normalizedValue switch
        {
            "true" or "1" => true,
            "false" or "0" => false,
            _ => null,
        };
    }
    
}
