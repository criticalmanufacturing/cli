using Cmf.CLI;
using Cmf.CLI.Utilities;
using Xunit;


namespace tests.Specs
{
    public class ProgramTests
    {
        [Theory]
        [InlineData(10)]
        [InlineData(11)]
        public void Should_NotThrow_For_Valid_Versions(int version)
        {
            Program.ValidateMesVersion(version);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(9)]
        public void Should_Throw_For_Invalid_Versions(int version)
        {
            Assert.Throws<CliException>(() => Program.ValidateMesVersion(version));
        }

        [Fact]
        public void Should_NotThrow_When_Version_Is_Null()
        {
            Program.ValidateMesVersion(null);
        }
    }
}
