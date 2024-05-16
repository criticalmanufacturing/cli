using Cmf.CLI.Builders;
using Cmf.CLI.Core.Commands;
using Cmf.CLI.Core.Objects;
using System;
using System.CommandLine;
using System.IO.Abstractions;

namespace tests.Mocks;

public class MockProcessCommand : ProcessCommand
{
    public new IDirectoryInfo WorkingDirectory { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public Func<bool> ConditionForExecute = () => { return true; };

    public ProcessBuildStep[] Steps = Array.Empty<ProcessBuildStep>();
    public override bool Condition()
    {
        return this.ConditionForExecute();
    }

    public override ProcessBuildStep[] GetSteps()
    {
        return Steps;
    }
}

public class MockBaseCommand : IBaseCommand
{
    public void Configure(Command cmd)
    {
        throw new NotImplementedException();
    }
}
