using System;
using System.Windows.Forms;
using EnvDTE;
using SharpCvsAddIn.UI;
using System.IO;



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
		private SolutionExplorer solutionExplorer_ = null;
		private Events.ProjectFileEvents fileEvents_ = null;
		private JobQueue jobs_ = null;


		public Controller(EnvDTE._DTE dte, EnvDTE.AddIn addin,IErrorHandler errorHandler)
		{
			application_ = dte;
			addIn_ = addin;
			model_= new Model( addin.SatelliteDllPath );
			shell_ = new UIShell( this );
			solutionExplorer_ = new SolutionExplorer(this);
			errorHandler_ = errorHandler;

			outputWriter_ = new OutputPaneWriter( application_, this.GetLocalizedString("OUTPUT_WINDOW_PANE") );

			fileEvents_ = new Events.ProjectFileEvents( this );
			jobs_ = new JobQueue( this);

			//solutionExplorer_.Initialize();
		}


		#region IController Members


		public JobQueue Jobs
		{
			get
			{
				return jobs_;
			}
		}

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
		public void HandleSolutionOpenEvent()
		{
			// start processing cvs commands

			addInLoadedForSolution_ = true;
			// synch user interface with loaded project
			this.SolutionExplorer.Refresh();
			// wire up events so we know when things change
			fileEvents_.AddHandlers();

			//statusCache_ = new FileStatusCache( application_ );
			
		}

		public void SolutionCleanup()
		{
			solutionOpen_ = false;
			fileEvents_.RemoveHandlers();
			this.SolutionExplorer.Cleanup();

		}
		/// <summary>
		/// Used to store information about the cvs repository that
		/// we are currently connected to
		/// </summary>
		public Persistance.Connection CurrentConnection
		{ 
			get
			{
				return model_.Storage.CurrentConnection;
			}
			set
			{
				model_.Storage.CurrentConnection = value;
			}
		}

		public Persistance.Module CurrentModule
		{
			get
			{
				return model_.Storage.CurrentModule;
			}
			set
			{
				model_.Storage.CurrentModule = value;
			}
		}

		public string CurrentTag
		{
			get
			{
				return model_.Storage.CurrentTag;
			}
			set
			{
				model_.Storage.CurrentTag = value;
			}
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
			solutionOpen_ = true;
		}

		public void SolutionClosing()
		{
			solutionOpen_ = false;
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

		public bool SolutionInCVS
		{
			get
			{
				// if there is a CVS subdirectory the solution is under source control
				string solutionPath = Path.GetDirectoryName(this.DTE.Solution.FileName);
				string cvsPath = Path.Combine( solutionPath, "CVS" );
				FileAttributes attr = File.GetAttributes( cvsPath );
				return (int)attr != -1;
			}
		}

		#endregion
	}
}
