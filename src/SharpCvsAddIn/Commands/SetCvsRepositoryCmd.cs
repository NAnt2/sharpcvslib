/*******************************************************************************
 * Implementation of Set CVS Repository menu command
 * 
 * *******************************************************************************/
using System;
using System.Windows.Forms;
using EnvDTE;

namespace SharpCvsAddIn.Commands
{
	[VSNetCommandAttribute( "SET_CVS_REPOSITORY", TextResource="SET_CVS_REPOSITORY", ToolTipResource="SET_CVS_REPOSITORY_DESC" ),
	VSNetControl( "File", Position = 6 ) ]
	public class SetCvsRepositoryCmd : CommandBase
	{
		/// <summary>   
		/// Get the status of the command
		/// </summary>
		override public vsCommandStatus QueryStatus( IController cont )
		{
			return vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
		}

		/// <summary>
		/// Shows dialog box that allows user to select the current repository that
		/// will be used for things like opening a solution from cvs
		/// </summary>
		override public void Execute( IController cont, string parameters )
		{
			FormSetCvsConnection frm = new FormSetCvsConnection( cont, cont.Model.CurrentConnection );

			if( frm.ShowDialog(cont.HostWindow) == DialogResult.OK )
			{
				using(new SafeCursor(Cursors.WaitCursor))
				{
					FormSetCvsConnection.Protocol p = (FormSetCvsConnection.Protocol)frm.protocolList.SelectedItem;

					cont.Model.Roots.CurrentConnection = new ConnectionString(frm.workingDirTextBox.Text,
						frm.userNameTextBox.Text,
						frm.cvsHostTxtBox.Text,
						int.Parse( frm.cvsPortTextBox.Text ),
						p != null ? p.Name : string.Empty,
						frm.cvsRootTextBox.Text,
						frm.cvsPasswordTxtBox.Text );

					cont.Model.Save();
				}
			}

		}
	}
}