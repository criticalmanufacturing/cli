using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace tests.Specs;

public class ConcatStreams
{
    [Fact]
    public void PartialAndZeroLengthReads_DoNotSkipUnreadSources()
    {
        using var stream = new Cmf.CLI.Core.Objects.ConcatStreams([
            new MemoryStream(new byte[] { 1, 2, 3 }),
            new MemoryStream(new byte[] { 4, 5 })
        ]);
        var buffer = new byte[20];

        stream.Read(buffer, 2, 0).Should().Be(0);
        stream.Position.Should().Be(0);
        stream.Read(buffer, 2, 2).Should().Be(2);
        buffer[2..4].Should().Equal(1, 2);
        stream.Read(buffer, 2, 0).Should().Be(0);
        stream.Read(buffer, 2, 2).Should().Be(2);
        buffer[2..4].Should().Equal(3, 4);
        stream.Read(buffer, 2, 2).Should().Be(1);
        buffer[2].Should().Be(5);
        stream.Read(buffer, 2, 2).Should().Be(0);
    }

    [Fact]
    public void InvalidRead_DoesNotAdvanceStream()
    {
        using var stream = new Cmf.CLI.Core.Objects.ConcatStreams([new MemoryStream(new byte[] { 1 })]);
        Action read = () => { _ = stream.Read(new byte[2], 1, 2); };
        read.Should().Throw<ArgumentException>();
        stream.Position.Should().Be(0);
        stream.ReadByte().Should().Be(1);
    }
}
