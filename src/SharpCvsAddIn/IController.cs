/********************************************************************************************
 *
 * *******************************************************************************************/

using System.Windows.Forms;
using System;
using EnvDTE;


namespace SharpCvsAddIn
{
    public interface IController
    {
        /// <summary>
        /// Raised when addin is unloading.
        /// </summary>
        event EventHandler Unloading;

        /// <summary>
        /// The top level automation object.
        /// </summary>
        EnvDTE._DTE DTE { get; }

        /// <summary>
        /// The UI shell.
        /// </summary>
        IUIShell UIShell { get; }

		Model Model { get; }


        /// <summary>
        /// The addin object.
        /// </summary>
        EnvDTE.AddIn AddIn { get; }

		void OpenSolution( string solutionPath );
		void SolutionCleanup();
		void HandleSolutionOpenEvent();

        /// <summary>
        /// The SolutionExplorer object.
        /// </summary>
        ISolutionExplorer SolutionExplorer { get; }

        /// <summary>
        /// The output pane.
        /// </summary>
        OutputPaneWriter OutputPane { get; }

        /// <summary>
        /// Whether a solution is open.
        /// </summary>
        bool SolutionIsOpen { get; }

        /// <summary>
        /// Whether Addin is loaded for the current solution.
        /// </summary>
        bool AddinLoadedForSolution { get;  }

        /// <summary>
        /// Reloads the current solution.
        /// </summary>
        /// <returns>True if the solution has been reloaded.</returns>
        bool ReloadSolutionIfNecessary();

        /// <summary>
        /// The Ankh configuration.
        /// </summary>
        // todo: get persistence information here ??
        // Ankh.Config.Config Config { get; }

        /// <summary>
        /// The error handler.
        /// </summary>
        IErrorHandler ErrorHandler { get; }

        /// <summary>
        /// The configloader.
        /// </summary>
        //Ankh.Config.ConfigLoader ConfigLoader { get; }

        /// <summary>
        /// The status cache.
        /// </summary>
        // TODO: implement statuscache
      //  StatusCache StatusCache { get; }

        /// <summary>
        /// Whether an operation is currently running.
        /// </summary>
        bool OperationRunning { get; }

        /// <summary>
        /// An IWin32Window to be used for parenting dialogs.
        /// </summary>
        IWin32Window HostWindow { get; }

        /// <summary>
        /// Manage issues related to conflicts.
        /// </summary>
        // TODO: implement conflict manager
        //ConflictManager ConflictManager { get; }

        /// <summary>
        /// Watches the project files.
        /// </summary>
        // TODO: implement filewatcher
        //FileWatcher ProjectFileWatcher{ get; }

        /// <summary>
        /// Event handler for the SolutionOpened event. Can also be called at
        /// addin load time, or if addin is enabled for a solution.
        /// </summary>
        void SolutionOpened();

        /// <summary>
        /// Called when a solution is closed.
        /// </summary>
        void SolutionClosing();

        /// <summary>
        /// Should be called before starting any lengthy operation
        /// </summary>
        void StartOperation( string description );

        /// <summary>
        ///  called at the end of any lengthy operation
        /// </summary>
        void EndOperation();

        /// <summary>
        /// Miscellaneous cleanup stuff goes here.
        /// </summary>
        void Shutdown();

		/// <summary>
		/// Gets a localized string from satellite dll
		/// </summary>
		/// <param name="resourceId">Tag that identifies the localized string </param>
		/// <returns>The string.</returns>
		string GetLocalizedString( string resourceId );
    }
}
