using System;

namespace ICSharpCode.SharpCvsLib.Options
{
	/// <summary>
	/// The unsupported option exception is thrown when an option that is not supported
	///     by the client library is used.
	/// </summary>
	public class UnsupportedOptionException : Exception{
        /// <summary>
        /// Indicate that an unknown option has been used.
        /// </summary>
        /// <param name="msg">A useful message that will help a developer debug
        ///     the problem that has occurred.</param>
		public UnsupportedOptionException(String msg) : base (msg) {
		}

        /// <summary>
        /// Indicate that an unsupported option has been specified.
        /// </summary>
        /// <param name="msg">A useful message that will help a developer debug
        ///     the problem that has occurred.</param>
        /// <param name="e"></param>
        public UnsupportedOptionException (String msg, Exception e) : base (msg, e) {

        }
	}
}
