using System;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Commands;
using log4net;
using System.Windows.Forms;
using System.IO;


namespace SharpCvsAddIn.Jobs
{
	public class CvsUpdateFolderJob : IJob
	{
		private static readonly ILog log_ = LogManager.GetLogger( typeof(CvsUpdateFolderJob));

		WorkingDirectory workingDirectory_ = null;
		string password_ = string.Empty;
		IController controller_ = null;
		// if an exception occurs store it here and pass it back to the calling thread.
		Exception exception_ = null;

		public CvsUpdateFolderJob( IController cont, WorkingDirectory wd, string password )
		{
			controller_ = cont;
			workingDirectory_ = wd;
			password_ = password;
		}

		public void FinishJob()
		{
			log_.Debug( "called finish job" );

			if( exception_ == null )
			{
				controller_.SolutionExplorer.Refresh();
			}
			else
			{
				log_.Error( "Exception in FinishJob", exception_ );
				controller_.UIShell.ExceptionMessage( 
					string.Format(controller_.GetLocalizedString("MSGBOX_EXCEPTION_CVS_UPDATE"), 
					exception_.Message ));
			}
		}

		#region IJob Members

		public string Name
		{
			get
			{
				return "CVS Update Job";
			}
		}

		public object DoWork()
		{
			log_.Debug(">>>>>>>>>>>> Starting CVS update >>>>>>>>>>>");
			CVSServerConnection cvsConnection = null;
			try
			{
				UpdateCommand2 cmd = new UpdateCommand2( workingDirectory_ );
				cvsConnection = new CVSServerConnection( workingDirectory_ );
				controller_.OutputPane.RegisterCvsConnection( cvsConnection );
				cvsConnection.Connect( workingDirectory_, password_ );
				cmd.Execute( cvsConnection );
				log_.Debug(">>>>>>>>>>>>>> CVS update complete >>>>>>>>>>>");
			}
			catch(Exception e )
			{
				this.exception_ = e;
				log_.Error( "An error occurred during cvs update job.", e );
			}

			if(cvsConnection != null) cvsConnection.Close();
		
			return this;
		}

		#endregion

		#region ICloneable Members

		public object Clone()
		{
			// TODO:  Add CvsUpdateJob.Clone implementation
			return this;
		}

		#endregion

	}

}