 namespace SharpCvsAddIn
{
	using System;
	using Microsoft.Office.Core;
	using System.Windows.Forms;
	using Microsoft.Win32;
	using Extensibility;
	using System.Runtime.InteropServices;
	using EnvDTE;
	using System.Diagnostics;
	using System.Text.RegularExpressions;
	using log4net;
	using log4net.Config;
	using System.IO;

	#region Read me for Add-in installation and setup information.
	// When run, the Add-in wizard prepared the registry for the Add-in.
	// At a later time, if the Add-in becomes unavailable for reasons such as:
	//   1) You moved this project to a computer other than which is was originally created on.
	//   2) You chose 'Yes' when presented with a message asking if you wish to remove the Add-in.
	//   3) Registry corruption.
	// you will need to re-register the Add-in by building the MyAddin21Setup project 
	// by right clicking the project in the Solution Explorer, then choosing install.
	#endregion

	public class SafeCursor : IDisposable
	{
		Cursor saved_ = null;

		public SafeCursor(Cursor newCursor)
		{
			saved_ = Cursor.Current;
			Cursor.Current = newCursor;
		}
		
		#region IDisposable Members

		public void Dispose()
		{
			Cursor.Current = saved_;
		}

		#endregion
	}


	
	/// <summary>
	///   The object for implementing an Add-in.
	/// </summary>
	/// <seealso class='IDTExtensibility2' />
	[GuidAttribute("81E0C568-E96D-4F12-922B-08F9FC26F335"), ProgId("SharpCvsAddIn.Connect")]
	public class Connect : Object, Extensibility.IDTExtensibility2, IDTCommandTarget, IWin32Window
	{
		private Controller controller_ = null;
		private CommandMap commands_ = null;
		private Events.SolutionEvents solutionEvents_ = null;

		private static readonly ILog logger_ = LogManager.GetLogger( typeof(Connect) );
		/// <summary>
		///		Implements the constructor for the Add-in object.
		///		Place your initialization code within this method.
		/// </summary>
		public Connect()
		{
			// load up logging configuration
			string codeBase = this.GetType().Assembly.CodeBase;
			Uri uri = new Uri( codeBase );
			string configPath = Path.Combine( Path.GetDirectoryName( uri.LocalPath ), "sharpcvsaddin-log.config" );
			XmlConfigurator.Configure( new FileInfo( configPath ) );		
		}
		/// <summary>
		/// We provide this hook into the com registration process so we menu and
		/// user interface elements are recreated every time we rebuild and reregister
		/// the add-in
		/// </summary>
		/// <param name="t"></param>
		[ComRegisterFunctionAttribute]
		public static void RegsiterFunction(Type t )
		{
			string progId = string.Empty;

		}

		/// <summary>
		///      Implements the OnConnection method of the IDTExtensibility2 interface.
		///      Receives notification that the Add-in is being loaded.
		/// </summary>
		/// <param term='application'>
		///      Root object of the host application.
		/// </param>
		/// <param term='connectMode'>
		///      Describes how the Add-in is being loaded.
		/// </param>
		/// <param term='addInInst'>
		///      Object representing this Add-in.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, Extensibility.ext_ConnectMode connectMode, object addInInst, ref System.Array custom)
		{
			logger_.Debug( "Connection starting" );
			// bail on commandline builds
			// we don't want to load on command line builds.
			if ( Regex.IsMatch( Environment.CommandLine, "/build" ) )
				return;

			controller_ = new Controller( (_DTE)application, (AddIn)addInInst, new ErrorHandler() );

			solutionEvents_ = new Events.SolutionEvents( controller_ );
			solutionEvents_.AddHandlers();


#if ALWAYS_REGISTER
			bool register = true;
#else
			bool register = (connectMode == Extensibility.ext_ConnectMode.ext_cm_UISetup);
#endif
			// get rid of old menu commands
			if(register)
			{
				CommandMap.DeleteCommands( controller_ );
			}

			// load new menu commands
			commands_ = CommandMap.LoadCommands( controller_, register );

			logger_.Debug( "Connection finished" );

		
		}

		/// <summary>
		///     Implements the OnDisconnection method of the IDTExtensibility2 interface.
		///     Receives notification that the Add-in is being unloaded.
		/// </summary>
		/// <param term='disconnectMode'>
		///      Describes how the Add-in is being unloaded.
		/// </param>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(Extensibility.ext_DisconnectMode disconnectMode, ref System.Array custom)
		{
			logger_.Debug( "Disconnecting" );
			solutionEvents_.RemoveHandlers();
		}

		/// <summary>
		///      Implements the OnAddInsUpdate method of the IDTExtensibility2 interface.
		///      Receives notification that the collection of Add-ins has changed.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnAddInsUpdate(ref System.Array custom)
		{
		}

		/// <summary>
		///      Implements the OnStartupComplete method of the IDTExtensibility2 interface.
		///      Receives notification that the host application has completed loading.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref System.Array custom)
		{
			logger_.Debug("Startup complete");
		}

		/// <summary>
		///      Implements the OnBeginShutdown method of the IDTExtensibility2 interface.
		///      Receives notification that the host application is being unloaded.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref System.Array custom)
		{
			logger_.Debug("begin shutdown");
		}
		
		/// <summary>
		///      Implements the QueryStatus method of the IDTCommandTarget interface.
		///      This is called when the command's availability is updated
		/// </summary>
		/// <param term='commandName'>
		///		The name of the command to determine state for.
		/// </param>
		/// <param term='neededText'>
		///		Text that is needed for the command.
		/// </param>
		/// <param term='status'>
		///		The state of the command in the user interface.
		/// </param>
		/// <param term='commandText'>
		///		Text requested by the neededText parameter.
		/// </param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, EnvDTE.vsCommandStatusTextWanted neededText, ref EnvDTE.vsCommandStatus status, ref object commandText)
		{
			logger_.Debug("Query status" );
			if(neededText == EnvDTE.vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
				ICommand cmd = (ICommand)commands_[commandName];
				if( cmd != null )
				{
					status = cmd.QueryStatus( controller_ );
				}
			}
		}

		/// <summary>
		///      Implements the Exec method of the IDTCommandTarget interface.
		///      This is called when the command is invoked.
		/// </summary>
		/// <param term='commandName'>
		///		The name of the command to execute.
		/// </param>
		/// <param term='executeOption'>
		///		Describes how the command should be run.
		/// </param>
		/// <param term='varIn'>
		///		Parameters passed from the caller to the command handler.
		/// </param>
		/// <param term='varOut'>
		///		Parameters passed from the command handler to the caller.
		/// </param>
		/// <param term='handled'>
		///		Informs the caller if the command was handled or not.
		/// </param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, EnvDTE.vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			logger_.Debug("exec called");
			handled = false;
			Debug.WriteLine( string.Format("command exec {0}", commandName ) );

			if(executeOption == EnvDTE.vsCommandExecOption.vsCommandExecOptionDoDefault)
			{
				ICommand cmd = (ICommand)commands_[commandName];
				if( cmd != null )
				{
					logger_.Debug(string.Format("executing command {0}", commandName));
					string args = (string)varIn;
					handled = true;
					cmd.Execute( controller_, args );
				}
			}

		}

		//Implementation of the IWin32Window.Handle property:
		public System.IntPtr Handle
		{
			get
			{
				return new System.IntPtr (controller_.DTE.MainWindow.HWnd);
			}
		}

		
	}
}