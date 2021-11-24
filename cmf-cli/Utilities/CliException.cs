using System;

namespace Cmf.Common.Cli.Utilities
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class CliException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CliException"/> class.
        /// </summary>
        public CliException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CliException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CliException(string message) : base(message)
        {
            Log.Error(message);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CliException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public CliException(string message, Exception innerException) : base(message, innerException)
        {
            Log.Error($"{message} | InnerException: {innerException.Message}");
        }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client.
        /// <summary>
        /// Initializes a new instance of the <see cref="CliException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected CliException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
