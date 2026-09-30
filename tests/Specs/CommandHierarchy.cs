using System.Linq;
using System.Reflection;
using Cmf.CLI.Commands;
using Cmf.CLI.Core.Attributes;
using FluentAssertions;
using Xunit;

namespace tests.Specs;

public class CommandHierarchy
{
    private static CmfCommandAttribute[] GetCommandAttributes() =>
        typeof(UpgradeBaseCommand).Assembly.GetTypes()
            .Select(t => t.GetCustomAttribute<CmfCommandAttribute>(false))
            .Where(a => a != null)
            .ToArray();

    [Fact]
    public void UpgradeBase_ParentIsRegistered()
    {
        var upgrade = typeof(UpgradeCommand).GetCustomAttribute<CmfCommandAttribute>(false);
        var upgradeBase = typeof(UpgradeBaseCommand).GetCustomAttribute<CmfCommandAttribute>(false);

        upgrade.Should().NotBeNull();
        upgrade.Name.Should().Be("upgrade");
        upgrade.ParentId.Should().BeNullOrWhiteSpace();
        upgradeBase.ParentId.Should().Be(upgrade.Id);
    }

    [Fact]
    public void AllCommandsWithParentId_HaveAnExistingParent()
    {
        var attributes = GetCommandAttributes();
        var ids = attributes.Select(a => a.Id).Where(id => !string.IsNullOrWhiteSpace(id)).ToHashSet();

        var orphans = attributes
            .Where(a => !string.IsNullOrWhiteSpace(a.ParentId) && !ids.Contains(a.ParentId))
            .Select(a => $"{a.Id ?? a.Name} -> {a.ParentId}");

        orphans.Should().BeEmpty();
    }
}
