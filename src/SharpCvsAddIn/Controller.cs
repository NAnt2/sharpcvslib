using System;
using System.Windows.Forms;
using EnvDTE;
using SharpCvsAddIn.UI;



namespace SharpCvsAddIn
{
	/// <summary>
	/// 
	/// </summary>
	public class Controller : IController
	{
		private AddIn addIn_;
		private _DTE application_;
		private IUIShell shell_;
		private IErrorHandler errorHandler_;
		private Model model_ = null; //new Model();
		private OutputPaneWriter outputWriter_;
		private bool solutionOpen_ = false;
		private bool addInLoadedForSolution_ = false;
		private FileStatusCache statusCache_ = null;
		private SolutionExplorer solutionExplorer_ = null;

		public Controller(EnvDTE._DTE dte, EnvDTE.AddIn addin,IErrorHandler errorHandler)
		{
			application_ = dte;
			addIn_ = addin;
			model_= new Model( addin.SatelliteDllPath );
			shell_ = new UIShell( this );
			solutionExplorer_ = new SolutionExplorer(this);
			errorHandler_ = errorHandler;

			outputWriter_ = new OutputPaneWriter( application_, this.GetLocalizedString("OUTPUT_WINDOW_PANE") );

			//solutionExplorer_.Initialize();
		}


		#region IController Members



        public event System.EventHandler Unloading;

		/// <summary>
		/// Opens a solution
		/// </summary>
		/// <param name="solutionPath">The full path to the solution file</param>
		public void OpenSolution( string solutionPath )
		{
			addInLoadedForSolution_ = true;
			application_.Solution.Open( solutionPath );
			solutionOpen_ = true;

		}

		/// <summary>
		/// Called when the solution is loaded. It will do the work of getting status for all of the 
		/// items under cvs control and updating the solution explorer
		/// </summary>
		public void CacheSolutionState()
		{
			addInLoadedForSolution_ = true;
			this.SolutionExplorer.Refresh();

			//statusCache_ = new FileStatusCache( application_ );
			
		}

		public void SolutionCleanup()
		{
			solutionOpen_ = false;
			this.SolutionExplorer.Cleanup();
		}

		public Model Model { get{ return model_; } }

		public _DTE DTE
		{
			get
			{
				return application_;
			}
		}

		public IUIShell UIShell
		{
			get
			{
				return shell_;
			}
		}

		public AddIn AddIn
		{
			get
			{
				return addIn_;
			}
		}

		public ISolutionExplorer SolutionExplorer
		{
			get
			{
				return solutionExplorer_;
			}
		}

		public OutputPaneWriter OutputPane
		{
			get
			{
				return outputWriter_;
			}
		}

		public bool SolutionIsOpen
		{
			get
			{
				// TODO:  Add AddInController.SolutionIsOpen getter implementation
				return solutionOpen_;
			}
		}

		public bool AddinLoadedForSolution
		{
			get
			{
				// TODO:  Add AddInController.AddinLoadedForSolution getter implementation
				return addInLoadedForSolution_;
			}

		}

		public bool ReloadSolutionIfNecessary()
		{
			// TODO:  Add AddInController.ReloadSolutionIfNecessary implementation
			return false;
		}

		public IErrorHandler ErrorHandler
		{
			get
			{
				// TODO:  Add AddInController.ErrorHandler getter implementation
				return null;
			}
		}

		public bool OperationRunning
		{
			get
			{
				// TODO:  Add AddInController.OperationRunning getter implementation
				return false;
			}
		}

		public System.Windows.Forms.IWin32Window HostWindow
		{
			get
			{
				// TODO:  Add AddInController.HostWindow getter implementation
				return null;
			}
		}

		public void SolutionOpened()
		{
			// TODO:  Add AddInController.SolutionOpened implementation
		}

		public void SolutionClosing()
		{
			// TODO:  Add AddInController.SolutionClosing implementation
		}

		public void StartOperation(string description)
		{
			// TODO:  Add AddInController.StartOperation implementation
		}

		public void EndOperation()
		{
			// TODO:  Add AddInController.EndOperation implementation
		}

		public void Shutdown()
		{
			// TODO:  Add AddInController.Shutdown implementation
		}

		public string GetLocalizedString( string resourceId )
		{
			return model_.ResourceManager.GetString( resourceId );
		}

		#endregion
	}
}
