using System;
using System.Resources;
using System.Windows.Forms;

namespace SharpCvsAddIn
{
	public class UIShell : IUIShell
	{
		private IController controller_;

		public UIShell( IController controller )
		{
			controller_ = controller;
		}

		#region IUIShell Members

		public IController Controller
		{
			get
			{
				// TODO:  Add UIShell.Context getter implementation
				return controller_;
			}
		}

		public void ExclamationMessage(Form owner, string msgResource )
		{
			ResourceManager rm = controller_.Model.ResourceManager;
			MessageBox.Show( owner,
				rm.GetString(msgResource),
				rm.GetString( "APPLICATION_TITLE" ),
				MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation );
		}

		public void ExclamationMessage(string msgResource )
		{
			ResourceManager rm = controller_.Model.ResourceManager;
			
			MessageBox.Show( controller_.HostWindow,
				rm.GetString(msgResource),
				rm.GetString( "APPLICATION_TITLE" ),
				MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation );
		}

		public void ExceptionMessage(string message )
		{
			ResourceManager rm = controller_.Model.ResourceManager;
			
			MessageBox.Show( controller_.HostWindow,
				message,
				rm.GetString( "APPLICATION_TITLE" ),
				MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation );
		}

		public void ExceptionMessage(Form frm, string message )
		{
			ResourceManager rm = controller_.Model.ResourceManager;
			
			MessageBox.Show( frm,
				message,
				rm.GetString( "APPLICATION_TITLE" ),
				MessageBoxButtons.OK,
				MessageBoxIcon.Exclamation );
		}

		public System.Windows.Forms.DialogResult QueryWhetherAddinShouldLoad()
		{
			// TODO:  Add UIShell.QueryWhetherAddinShouldLoad implementation
			return new System.Windows.Forms.DialogResult ();
		}

		public void ShowRepositoryExplorer(bool show)
		{
			// TODO:  Add UIShell.ShowRepositoryExplorer implementation
		}

		public void SetRepositoryExplorerSelection(object[] selection)
		{
			// TODO:  Add UIShell.SetRepositoryExplorerSelection implementation
		}

		public System.Windows.Forms.DialogResult ShowMessageBox(string text, string caption, System.Windows.Forms.MessageBoxButtons buttons)
		{
			// TODO:  Add UIShell.ShowMessageBox implementation
			return new System.Windows.Forms.DialogResult ();
		}

		System.Windows.Forms.DialogResult SharpCvsAddIn.IUIShell.ShowMessageBox(string text, string caption, System.Windows.Forms.MessageBoxButtons buttons, System.Windows.Forms.MessageBoxIcon icon)
		{
			// TODO:  Add UIShell.SharpCvsAddIn.IUIShell.ShowMessageBox implementation
			return new System.Windows.Forms.DialogResult ();
		}

		#endregion

	}

}