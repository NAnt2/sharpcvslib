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
using SharpCvsAddIn.Persistance;

using System.Diagnostics;


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
			if( cont.CurrentConnection != null )
			{
				return vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
			}
			
			return vsCommandStatus.vsCommandStatusSupported;

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
				Debug.Assert( cont.CurrentConnection != null );

				// get the password if the user hasn't already supplied it
				if( cont.CurrentConnection.Password == string.Empty )
				{
					FormGetPassword passwordDlg = new FormGetPassword(cont);
					if( DialogResult.OK == passwordDlg.ShowDialog( cont.HostWindow ) )
					{
						cont.CurrentConnection.Password = passwordDlg.passwordTextBox.Text;
					}
					else
					{
						// user canceled, bail
						return;
					}
				}

				// got a password and a connection, lets get the module, and find out where
				// the user wants to put the solution
				FormOpenSolutionFromCvs openDlg = new FormOpenSolutionFromCvs( cont );

				if( DialogResult.OK == openDlg.ShowDialog(cont.HostWindow) )
				{
					using( new SafeCursor( Cursors.WaitCursor ) )
					{
						cont.CurrentConnection.WorkingDirectory = openDlg.WorkingDirectory;
						// update the current connection information with user choices						
						cont.CurrentModule = new Persistance.Module( openDlg.cvsModuleDropDown.Text );
                        cont.CurrentTag = openDlg.cvsTagDropDown.Text;

						CvsCheckoutJob job = new CvsCheckoutJob( cont );
                
						// cvs operation will be handled in job queue thread, the remainder of solution 
						// open operation will be done once job is done in OnCheckoutComplete
						cont.Jobs.AddJob( job, new JobCompletionHandler( this.OnCheckoutComplete ) );
					}
					// everything is successful
				}		

			}

		}
	}
}
