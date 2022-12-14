using System;

namespace ICSharpCode.SharpCvsLib.Responses
{
	/// <summary>
	/// The unknown response exception is thrown when the cvs server returns 
	///     a response that the client does not know how to handle.
	/// </summary>
	public class UnsupportedResponseException : Exception{
        /// <summary>
        /// Indicate that an unknown response has been returned from the repository.
        /// </summary>
        /// <param name="msg">A useful message that will help a developer debug
        ///     the problem that has occurred.</param>
		public UnsupportedResponseException(String msg) : base (msg) {
		}

        /// <summary>
        /// Indicate that an unknown response has been returned from the repository.        /// </summary>
        /// <param name="msg">A useful message that will help a developer debug
        ///     the problem that has occurred.</param>
        /// <param name="e"></param>
        public UnsupportedResponseException (String msg, Exception e) : base (msg, e) {

        }
	}
}
