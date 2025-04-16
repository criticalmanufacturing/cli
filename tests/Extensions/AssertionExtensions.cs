using FluentAssertions;
using FluentAssertions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tests.Extensions
{
    public static class AssertionExtensions
    {
        public static AndConstraint<StringAssertions> BeEquivalentToIgnoringNewLines(this StringAssertions assertion, string expected)
        {
            var expectedNormalized = expected
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace("\r", "\n", StringComparison.Ordinal);

            return assertion.Subject
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace("\r", "\n", StringComparison.Ordinal)
                .Should()
                .BeEquivalentTo(expectedNormalized);
        }
    }
}
