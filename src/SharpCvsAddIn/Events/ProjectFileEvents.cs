
using System;
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using log4net;
using SharpCvsAddIn.UI;

namespace SharpCvsAddIn.Events
{
	public class ProjectFileEvents : AbstractEvents
	{
		FileSystemWatcher watcher_ = new FileSystemWatcher();
		DateTime lastChange_ = DateTime.MinValue;
		TimeSpan granularity_ = new TimeSpan(10000000); 
		string lastChangeFileName_ = string.Empty;

		private static readonly ILog log_ = LogManager.GetLogger( typeof(ProjectFileEvents) );

		public ProjectFileEvents( Controller cont )
			:base( cont )
		{
		}

		public override void AddHandlers()
		{
			log_.Debug("Adding file watcher handlers");
			watcher_.EnableRaisingEvents = false;
			watcher_.Path = System.IO.Path.GetDirectoryName(controller_.DTE.Solution.FullName);
			watcher_.Filter = "*.*";
			watcher_.IncludeSubdirectories = true;
			watcher_.NotifyFilter = NotifyFilters.LastWrite;
			watcher_.Changed += new FileSystemEventHandler(watcher__Changed);
			watcher_.EnableRaisingEvents = true;
		}

		public override void RemoveHandlers()
		{
			log_.Debug("Removing file watcher handlers");
			watcher_.EnableRaisingEvents = false;
			watcher_.Changed -= new FileSystemEventHandler(watcher__Changed);
		}

		private void ProcessChange(string changedFile)
		{
			log_.Debug(string.Format("Processing change on {0}", changedFile));
			lastChangeFileName_ = changedFile;
			lastChange_ = DateTime.Now;

			IStatusNode root = controller_.SolutionExplorer.Root;
			IStatusNode child = (IStatusNode)root.Get( changedFile );
			if( child != null )
			{
				child.OnChangeEvent();
			}

		}

		private void watcher__Changed(object sender, FileSystemEventArgs e)
		{
			log_.Debug( string.Format("file changed event {0}", e.Name));
			TimeSpan delta = DateTime.Now - lastChange_;

			if( delta > granularity_ )
			{
				ProcessChange(e.Name);
				return;
			}
			else
			{
				if( e.Name != lastChangeFileName_ )
				{
					ProcessChange(e.Name);
					return;
				}
			}

			log_.Debug(string.Format("not processing change on {0}", e.Name));



		}
	}

}