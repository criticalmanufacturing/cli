using FluentAssertions;

namespace tests
{
    /// <summary>
    /// Extra Asserts that support custom reason
    /// </summary>
    public class AssertWithMessage : Xunit.Assert
    {
        public static void Equal<T>(T expected, T actual, string message)
        {
            expected.Should().Be(actual, message);
        }

        public static void Null(object expected, string message)
        {
            expected.Should().BeNull(message);
        }
        
        public static void NotNull(object @object, string message)
        {
            @object.Should().NotBeNull(message);
        }
    }
}