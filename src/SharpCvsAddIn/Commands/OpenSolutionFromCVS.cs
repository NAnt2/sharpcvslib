/********************************************************************************************
 *	Implementation of Open Solution From CVS menu command
 * 	
 * *******************************************************************************************/
using System;
using System.Windows.Forms;
using System.IO;

using EnvDTE;

using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Commands;

using SharpCvsAddIn.Jobs;


namespace SharpCvsAddIn.Commands
{
	[VSNetCommand("OPEN_SOLUTION_FROM_CVS", TextResource="OPEN_SOLUTION_FROM_CVS", 
		 ToolTipResource="OPEN_SOLUTION_FROM_CVS_DESC" ),
	VSNetControl( "File", Position = 5 ) ]
	public class OpenSolutionFromCVS : CommandBase
	{
		/// <summary>
		/// Get the status of the command
		/// </summary>
		override public vsCommandStatus QueryStatus( IController cont )
		{
			return vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
		}

		private void OnCheckoutComplete( object sender, object jobData )
		{
			CvsCheckoutJob job = (CvsCheckoutJob)jobData ;
			job.FinishCheckout();
		}

		/// <summary>
		/// Do all the stuff to open a solution
		/// </summary>
		override public void Execute( IController cont, string parameters )
		{
			// give user a chance to save current solution before we open another one
			if( cont.DTE.ItemOperations.PromptToSave !=
				vsPromptResult.vsPromptResultCancelled )
			{
				// it's possible not to have assigned a repository yet, if that is the case
				// don't let the user open solution, prompt them instead
				ConnectionString connectionString = cont.Model.CurrentConnection;

				if( connectionString != null )
				{
					// get the password if the user hasn't already supplied it
					if( connectionString.Password == string.Empty )
					{
						FormGetPassword passwordDlg = new FormGetPassword(cont);
						if( DialogResult.OK == passwordDlg.ShowDialog( cont.HostWindow ) )
						{
							connectionString.Password = passwordDlg.passwordTextBox.Text;
						}
						else
						{
							// user canceled, bail
							return;
						}
					}

					// got a password and a connection, lets get the module, and find out where
					// the user wants to put the solution
					FormOpenSolutionFromCvs openDlg = new FormOpenSolutionFromCvs( cont,
						connectionString.ToString(), connectionString.WorkingDirectory );
					if( DialogResult.OK == openDlg.ShowDialog(cont.HostWindow) )
					{
						using( new SafeCursor( Cursors.WaitCursor ) )
						{
							string slnLocation = openDlg.destPathTextBox.Text;
							string moduleName = openDlg.cvsModuleDropDown.Text;
							string tag = openDlg.cvsTagDropDown.Text;


							CvsCheckoutJob job = new CvsCheckoutJob( cont, 
								connectionString.ToString(),
								moduleName, 
								slnLocation,
								connectionString.Password,
								openDlg.SolutionPath,
								tag );
                            
							// cvs operation will be handled in job queue thread, the remainder of solution 
							// open operation will be done once job is done in OnCheckoutComplete
							cont.Jobs.AddJob( job, new JobCompletionHandler( this.OnCheckoutComplete ) );
						}
						// everything is successful
					}
				}
				else
				{
					cont.UIShell.ExclamationMessage( "MSGBOX_CVS_ROOT_MISSING" );
				}
			}

		}
	}
}
