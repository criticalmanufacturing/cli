using System;
using Cmf.CLI.Core;
using Cmf.CLI.Core.Enums;

namespace Cmf.CLI.Utilities
{
    /// <summary>
    ///
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class CliException : Exception
    {
        /// <summary>
        /// The output ErrorCode of the application
        /// </summary>
        public ErrorCode ErrorCode = ErrorCode.Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="CliException"/> class.
        /// </summary>
        public CliException(ErrorCode errorCode = ErrorCode.Default)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CliException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public CliException(string message, ErrorCode errorCode = ErrorCode.Default) : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CliException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public CliException(string message, Exception innerException, ErrorCode errorCode = ErrorCode.Default) : base(message, innerException)
        {
            ErrorCode = errorCode;
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
