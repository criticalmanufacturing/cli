using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cmf.CLI.Core.Utilities
{
    /// <summary>
    /// Class used solely to wrap unhandled/unexpected exceptions and preserve their inner stack trace
    /// when rethrowing,
    /// for easier troubleshooting.
    /// </summary>
    public class WrappedException : Exception
    { 
        /// <summary>
        /// Cosntructor is protected, to ensure the exception is always created through the static Wrap method
        /// </summary>
        protected WrappedException(Exception innerException) : base("Unhandled exception", innerException) { }

        public static WrappedException Wrap(Exception innerException)
        {
            // No double wraps
            if (innerException is WrappedException wrappedException) 
            {
                return wrappedException;
            }
            else
            {
                return new WrappedException(innerException);
            }
        }

        /// <summary>
        /// If the exception is a WrappedException, return it's InnerException, otherwise return the root exception itself
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static Exception Unwrap(Exception exception)
        {
            // No double wraps
            if (exception is WrappedException wrappedException && wrappedException.InnerException != null)
            {
                return wrappedException.InnerException!;
            }
            else
            {
                return exception;
            }
        }
    }
}
