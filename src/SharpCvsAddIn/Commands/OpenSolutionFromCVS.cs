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
	
							// see if we can get the solution from cvs, if we do,
							// write user changes to isolated storage
							CVSServerConnection serverConnection = null;
							try
							{			
								CvsRoot root = new CvsRoot(connectionString.ToString());
								WorkingDirectory wd = new WorkingDirectory( root, slnLocation, moduleName );
								CheckoutModuleCommand cmd = new CheckoutModuleCommand( wd );
								serverConnection = new CVSServerConnection( wd );
								cont.OutputPane.RegisterCvsConnection( serverConnection );
								serverConnection.Connect( wd, connectionString.Password );
								cmd.Execute( serverConnection );
							}
							catch(Exception e )
							{
								using( new SafeCursor( Cursors.Default ) )
								{
									cont.UIShell.ExceptionMessage( e.Message );
								}						
							}

							if( serverConnection != null ) serverConnection.Close();

							try
							{

								// find solution to open
								DirectoryInfo di = new DirectoryInfo( openDlg.SolutionPath );

								FileInfo[] files = di.GetFiles( "*.sln" );
								string solutionName = string.Empty;
							
								// no solution file to open
								if( files.Length == 0 )
								{
									cont.UIShell.ExclamationMessage( "MSGBOX_MISSING_SOLUTION_FILE"  );
									return;
								}

								// two or more solution files, pick one
								if( files.Length > 1 )
								{
									using( new SafeCursor( Cursors.Default ) )
									{
										FormPickSolution fps = new FormPickSolution( cont, files );
										if( fps.ShowDialog( cont.HostWindow ) == DialogResult.OK )
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
								cont.OpenSolution(Path.Combine(openDlg.SolutionPath, solutionName )); 
								// save user actions
								Module m = cont.Model.Roots.CurrentRoot.AddModule( moduleName );
								if( tag != string.Empty )
								{
									m.AddTag( tag );
								}

								cont.Model.Save();
							}
							catch(Exception e)
							{
								using( new SafeCursor( Cursors.Default ) )
								{
									cont.UIShell.ExceptionMessage(e.Message);
								}
							}
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
