using System;
using System.Threading.Tasks;
using Cmf.CLI.Core.Objects;

namespace tests.Mocks;

public class MockNPMClientThrows : INPMClient
{
    public Task<string> GetLatestVersion(bool preRelease = false)
    {
        throw new Exception("GetLatestVersion should not have been called");
    }

    public IPackage[] FindPlugins(Uri[] registries)
    {
        throw new NotImplementedException();
    }
}

public class MockNPMClient : INPMClient
{
    public Task<string> GetLatestVersion(bool preRelease = false)
    {
        return Task.FromResult("");
    }

    public IPackage[] FindPlugins(Uri[] registries)
    {
        throw new NotImplementedException();
    }
}