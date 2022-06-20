using System.Threading.Tasks;
using Cmf.CLI.Core.Objects;

namespace tests.Mocks;

public class MockNPMClient : INPMClient
{
    public Task<string> GetLatestVersion(bool preRelease = false)
    {
        return Task.FromResult("");
    }
}