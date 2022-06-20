using Cmf.CLI.Core.Objects;

namespace tests.Mocks;

public class MockVersionService : IVersionService
{
    public string PackageId => "test";
    public string CurrentVersion => "1.0.0";
}