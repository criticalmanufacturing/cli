using System;
using Xunit;

namespace Cmf.Custom.Common.UnitTests
{
    public class GenericTestsFailingTests
    {
        [Fact]
        public void GenericTestsFailingTest()
        {
            Assert.Equal(1, 2);
        }
    }
}