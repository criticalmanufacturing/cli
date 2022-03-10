using System;
using Xunit.Sdk;

namespace tests
{
    /// <summary>
    /// Extra Asserts that support custom reason
    /// </summary>
    public class AssertWithMessage : Xunit.Assert
    {
        public static void Equal<T>(T expected, T actual, string message)
        {
            try
            {
                Equal(expected, actual);
            }
            catch (EqualException)
            {
                throw new EqualWithMessageException(expected, actual, message);
            }
        }

        public static void Null(object expected, string message)
        {
            try
            {
                Null(expected);
            }
            catch (NullException)
            {
                throw new NullWithMessageException(expected, message);
            }
        }
        
        public static void NotNull(object @object, string message)
        {
            try
            {
                NotNull(@object);
            }
            catch (NotNullException)
            {
                throw new NotNullWithMessageException(message);
            }
        }
    }

    public class EqualWithMessageException : EqualException
    {
        public EqualWithMessageException(object expected, object actual, string message) : base(expected, actual)
        {
            this.Message = message;
        }

        /// <summary>
        /// <inheritdoc cref="EqualException.Message"/>
        /// </summary>
        public override string Message { get; }
    }
    
    class NullWithMessageException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NullException"/> class.
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public NullWithMessageException(object actual, string message)
            : base(null, actual, message)
        { }
    }
    
    class NotNullWithMessageException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NotNullException"/> class.
        /// </summary>
        public NotNullWithMessageException(string message)
            : base(message)
        { }
    }
}