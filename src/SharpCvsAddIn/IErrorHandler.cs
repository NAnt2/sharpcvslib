/********************************************************************************************
 *
 * *******************************************************************************************/
using System;

namespace SharpCvsAddIn
{
	/// <summary>
	/// Represents a class that handles an Ankh error.
	/// </summary>
	public interface IErrorHandler
	{
		void Handle( Exception ex );

        /// <summary>
        /// Send an report about a non-specific error.
        /// </summary>
        void SendReport();

        /// <summary>
        /// Display an error in the output pane.
        /// </summary>
        void Write( string message, Exception ex, System.IO.TextWriter writer );
	}
}