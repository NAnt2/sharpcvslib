using System;

namespace SharpCvsAddIn
{
	public class ErrorHandler : IErrorHandler
	{
		#region IErrorHandler Members

		public void Handle(Exception ex)
		{
			// TODO:  Add ErrorHandler.Handle implementation
		}

		public void SendReport()
		{
			// TODO:  Add ErrorHandler.SendReport implementation
		}

		public void Write(string message, Exception ex, System.IO.TextWriter writer)
		{
			// TODO:  Add ErrorHandler.Write implementation
		}

		#endregion

	}

}