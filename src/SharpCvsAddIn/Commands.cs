using System;
using System.Windows.Forms;

using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Exceptions;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Messages;
using EnvDTE;

namespace SharpCvsAddIn
{
	public interface IHandleCommands
	{
		void Exec();
	}

	public abstract class AbstractCommand
	{
		protected Connect owner_;
		protected AbstractCommand( Connect owner )
		{
			owner_ = owner;
		}
		protected void RegisterListeners( CVSServerConnection conn )
		{
			conn.ResponseMessageEvents.ErrorResponseMessageEvent += 
				new MessageEventHandler(this.WriteErrorResponse);
			conn.ResponseMessageEvent += 
				new MessageEventHandler(this.WriteResponse);

			conn.ResponseMessageEvents.UpdatedResponseMessageEvent += 
				new MessageEventHandler(this.WriteUpdatedResponse);
		}

		public void WriteErrorResponse(object sender, MessageEventArgs e) 
		{
			owner_.WriteToOutputWindow( string.Format("{0} - {1}\n", e.Message, e.Prefix) );
		}

		/// <summary>
		/// Write the update message to the update message text box.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void WriteUpdatedResponse(object sender, MessageEventArgs e) 
		{
			owner_.WriteToOutputWindow( string.Format("{0} - {1}\n", e.Message, e.Prefix) );
		}

		public void WriteResponse(object sender, MessageEventArgs e) 
		{
			owner_.WriteToOutputWindow( string.Format("{0} - {1}\n", e.Message, e.Prefix) );
		}
	}

	/// <summary>
	/// Summary description for Commands.
	/// </summary>
	public class CommandFactory
	{
		private CommandFactory()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public static IHandleCommands GetCommand( string commandName, Connect owner )
		{
			switch( commandName )
			{
				case "SharpCvsAddIn.Connect.OPEN_SOLUTION_FROM_CVS" :
					return new CommandOpenSolutionFromCvs( owner );
				case "SharpCvsAddIn.Connect.SET_CVS_REPOSITORY" :
					return new CommandSetCvsRepository(owner);
			}

			return null;
		}
	}
}
