using System;

namespace ICSharpCode.SharpCvsLib.Misc
{
	/// <summary>
	/// An invalid cvsroot exception is thrown if the client attempts to send in
	///     a root that is not understood by the server.  
	/// </summary>
	public class CvsRootParseException : Exception{
        /// <summary>
        /// Indicate that an invalid cvsroot has been passed into the library.
        /// </summary>
        /// <param name="msg">A useful message that will help a developer debug
        ///     the problem that has occurred.</param>
		public CvsRootParseException(String msg) : base (msg) {
		}

        /// <summary>
        /// Indicate that an invalid cvsroot has been passed into the library.
        /// </summary>
        /// <param name="msg">A useful message that will help a developer debug
        ///     the problem that has occurred.</param>
        /// <param name="e"></param>
        public CvsRootParseException (String msg, Exception e) : base (msg, e) {
        }
	}
}
