using System;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Commands;
using log4net;
using System.Windows.Forms;
using System.IO;

namespace SharpCvsAddIn.Jobs
{

	public class CvsCheckoutJob : IJob
	{
		const string name_ = "CVS Checkout Job";
		private IController controller_ = null;
		private string connectionString_  = string.Empty;
		private string moduleName_ = string.Empty;
		private string solutionLocation_ = string.Empty;
		private string password_ = string.Empty;
		private string solutionPath_ = string.Empty;
		private string tag_ = string.Empty;
		private Exception except_ = null;

		private static readonly ILog log_ = LogManager.GetLogger(typeof(CvsCheckoutJob));


		public CvsCheckoutJob( IController controller )
		{
			controller_ = controller;
			connectionString_ = controller.CurrentConnection.ConnectionString;
			moduleName_ = controller.CurrentModule.Name;
			solutionLocation_ = controller.CurrentConnection.WorkingDirectory;
			password_ = controller.CurrentConnection.Password;
			solutionPath_ = Path.Combine( controller.CurrentConnection.WorkingDirectory, controller.CurrentModule.Name);
			// TODO - implement tags
			//tag_ = tag;

		}

		#region IJob Members

		public string Name
		{
			get
			{
				return name_;
			}
		}

		public object DoWork()
		{
			log_.Debug(">>>>>>>>>>>>>>>>>>>>> started cvs checkout");
			CVSServerConnection serverConnection = null;	

			try
			{
				CvsRoot root = new CvsRoot(connectionString_);
				WorkingDirectory wd = new WorkingDirectory( root, solutionLocation_, moduleName_ );
				CheckoutModuleCommand cmd = new CheckoutModuleCommand( wd );
				serverConnection = new CVSServerConnection( wd );
				controller_.OutputPane.RegisterCvsConnection( serverConnection );
				serverConnection.Connect( wd, password_ );
				cmd.Execute( serverConnection );
				log_.Debug(">>>>>>>>>>>>>>>>>>>>> cvs checkout complete");
			}
			catch(Exception e)
			{
				// hang on to the exception for processing in calling thread
				this.except_ = e;
				log_.Error("An error occurred during cvs checkout operation", e);
			}
			finally
			{

				if( serverConnection != null ) serverConnection.Close();
			}

			return this;
		}

		public void FinishCheckout()
		{
			log_.Debug("finish checkout called");
			try
			{
				// first check for errors
				if( this.except_ != null )
				{
					controller_.UIShell.ExceptionMessage(
						string.Format(controller_.GetLocalizedString("MSGBOX_EXCEPTION_CVS_CHECKOUT"),
						this.except_.Message ));
					return;
				}

				using( new SafeCursor( Cursors.WaitCursor ) )
				{
					// find solution to open
					DirectoryInfo di = new DirectoryInfo( this.solutionPath_ );

					FileInfo[] files = di.GetFiles( "*.sln" );
					string solutionName = string.Empty;
							
					// no solution file to open
					if( files.Length == 0 )
					{
						using( new SafeCursor( Cursors.Default ) )
						{
							controller_.UIShell.ExclamationMessage( "MSGBOX_MISSING_SOLUTION_FILE"  );
						}
						return;
					}

					// two or more solution files, pick one
					if( files.Length > 1 )
					{
						using( new SafeCursor( Cursors.Default ) )
						{
							FormPickSolution fps = new FormPickSolution( controller_, files );
							if( fps.ShowDialog( controller_.HostWindow ) == DialogResult.OK )
							{
								solutionName = fps.SolutionFile;
							}
							else
							{
								return;
							}
						}
					}

					solutionName = files[0].Name;

					// ok, if we are this far, we checked out the solution from cvs, lets open it
					controller_.OpenSolution(Path.Combine(this.solutionPath_, solutionName )); 
					// everything worked, write user changes to modules path etc 
					// to persistant storage
					controller_.Model.Save();
				}
			}
			catch(Exception e)
			{
				controller_.UIShell.ExceptionMessage(
					string.Format(controller_.GetLocalizedString("MSGBOX_EXCEPTION_CVS_CHECKOUT"),
					e.Message ));
			}
		} 

		#endregion

		#region ICloneable Members

		public object Clone()
		{
			// since we don't really do anything with shared state,
			// just return this object			
			return this;
		}

		#endregion

	}

}