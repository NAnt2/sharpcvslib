/********************************************************************************************
 *
 * *******************************************************************************************/
using System;
using System.Windows.Forms;

namespace SharpCvsAddIn
{
	/// <summary>
	/// Represents the UI of the addin.
	/// </summary>
	public interface IUIShell
	{
       
        /// <summary>
        /// The repository explorer UI.
        /// </summary>
        /*
        RepositoryExplorerControl RepositoryExplorer
        {
            get; 
        }
		*/

		void ExclamationMessage(string msgResource );
		void ExclamationMessage(Form owner, string msgResource );
		void ExceptionMessage(string message );
		void ExceptionMessage(Form owner, string message );

        /// <summary>
        /// An IContext.
        /// </summary>
        IController Controller
        {
            get;
        }

        /// <summary>
        /// Ask the user whether Addin should load for a given solution.
        /// </summary>
        /// <returns></returns>
        DialogResult QueryWhetherAddinShouldLoad();

        /// <summary>
        /// Whether to show the repository explorer tool window.
        /// </summary>
        /// <param name="show"></param>
        void ShowRepositoryExplorer(bool show);

        /// <summary>
        /// Set the selection for the repository explorer.
        /// </summary>
        /// <param name="selection"></param>
        void SetRepositoryExplorerSelection( object[] selection );

		DialogResult ShowMessageBox( string messageResource, MessageBoxButtons buttons, MessageBoxIcon icons);

	}
}
