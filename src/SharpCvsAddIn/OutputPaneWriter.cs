/********************************************************************************************
 *  Contains class that handles writing add-in ouput to ide output window	
 * 
 * *******************************************************************************************/
using System;
using System.IO;
using System.Text;
using EnvDTE;

using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Messages;


namespace SharpCvsAddIn
{
    /// <summary>
    /// A TextWriter backed by the VS.NET output window.
    /// </summary>
    public class OutputPaneWriter : TextWriter
    {
        public OutputPaneWriter( _DTE dte, string caption )
        {
            this.outputWindow = dte.Windows.Item( EnvDTE.Constants.vsWindowKindOutput );

            OutputWindow window = (OutputWindow)this.outputWindow.Object;
            this.outputPane = window.OutputWindowPanes.Add( caption );		
        }

        public override Encoding Encoding
        {
            [System.Diagnostics.DebuggerStepThrough]
            get{ return Encoding.Default; }
        }

        /// <summary>
        /// Activate the pane.
        /// </summary>
        public void Activate()
        {
            if ( !this.outputWindow.AutoHides )
            {
                this.outputWindow.Activate();
                this.outputPane.Activate();
            }
        }

        /// <summary>
        /// Clear the pane.
        /// </summary>
        public void Clear()
        {
            this.outputPane.Clear();
        }

        public override void Write( char c )
        {
            this.outputPane.OutputString( c.ToString() );
        }

        public override void Write( string s )
        {
            this.outputPane.OutputString( s );
        }

        public override void WriteLine( string s )
        {
            this.outputPane.OutputString( s + Environment.NewLine );
        }


        /// <summary>
        /// Writes Start text to outputpane.
        /// </summary>
        /// <param name="action">Action.</param>
        public void StartActionText( string action )
        {
            
            this.Activate();
            this.outputPane.OutputString( this.FormatMessage( action ) + Environment.NewLine + 
                Environment.NewLine );
        }

        /// <summary>
        /// Writes end text to outputpane.
        /// </summary>
        public void EndActionText()
        {
            this.outputPane.OutputString( this.FormatMessage( "Done" ) + Environment.NewLine + 
                Environment.NewLine );
        }
        

        /// <summary>
        /// Formats the text for output.
        /// </summary>
        /// <param name="action">action string.</param>
        /// <returns>Formatet text string</returns>
        private string FormatMessage( string action )
        {
            int left = (LINELENGTH / 2) - (action.Length / 2);
            int right = LINELENGTH - ( left + action.Length );
            return new string( '-', left ) + action + new string( '-', right );
        }

		/// <summary>
		/// Register cvs connection to send status messages to output window
		/// </summary>
		/// <param name="cvs">Connection to cvs</param>
		public void RegisterCvsConnection( CVSServerConnection cvs )
		{
			cvs.RequestMessageEvent +=new ICSharpCode.SharpCvsLib.Messages.MessageEventHandler(cvs_RequestMessageEvent);
			cvs.ResponseMessageEvent +=new ICSharpCode.SharpCvsLib.Messages.MessageEventHandler(cvs_ResponseMessageEvent);
			cvs.StartProcessEvent +=new ICSharpCode.SharpCvsLib.Messages.ProcessEventHandler(cvs_StartProcessEvent);
			cvs.StopProcessEvent +=new ICSharpCode.SharpCvsLib.Messages.ProcessEventHandler(cvs_StopProcessEvent);
		}

		private void cvs_RequestMessageEvent(object sender, MessageEventArgs e)
		{
			this.WriteLine( e.Message );
		}

		private void cvs_ResponseMessageEvent(object sender, MessageEventArgs e)
		{
			this.WriteLine( e.Message );
		}

		private void cvs_StartProcessEvent(object sender, ProcessEventArgs e)
		{
			this.StartActionText( "Processing CVS Command" );
		}

		private void cvs_StopProcessEvent(object sender, ProcessEventArgs e)
		{
			this.EndActionText();
		}

        private const int LINELENGTH = 70;
        private const char LINECHAR = '-';
        private OutputWindowPane outputPane;
        private Window outputWindow;
    }
}
