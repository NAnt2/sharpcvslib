using System;
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using log4net;


namespace SharpCvsAddIn.Events
{
	/// <summary>
	/// Summary description for SolutionEvents.
	/// </summary>
	public class SolutionEvents : AbstractEvents
	{
		private EnvDTE.SolutionEvents solutionEvents_ = null;
		private EnvDTE.ProjectItemsEvents projectItemEvents_ = null;
		private EnvDTE.ProjectItemsEvents miscFileEvents_ = null;

		private static readonly ILog log_ = LogManager.GetLogger(typeof(SolutionEvents));

		public SolutionEvents(Controller controller)
			:base( controller )
		{
			solutionEvents_ = controller_.DTE.Events.SolutionEvents;
			projectItemEvents_ = controller_.DTE.Events.SolutionItemsEvents;
			miscFileEvents_ = controller_.DTE.Events.MiscFilesEvents;

		}

		public override void AddHandlers()
		{
			log_.Debug( "Adding solution event handlers");
			solutionEvents_.AfterClosing +=new _dispSolutionEvents_AfterClosingEventHandler(events__AfterClosing);
			solutionEvents_.BeforeClosing +=new _dispSolutionEvents_BeforeClosingEventHandler(events__BeforeClosing);
			solutionEvents_.Opened +=new _dispSolutionEvents_OpenedEventHandler(events__Opened);
			solutionEvents_.ProjectAdded +=new _dispSolutionEvents_ProjectAddedEventHandler(events__ProjectAdded);
			solutionEvents_.ProjectRemoved +=new _dispSolutionEvents_ProjectRemovedEventHandler(events__ProjectRemoved);
			solutionEvents_.ProjectRenamed +=new _dispSolutionEvents_ProjectRenamedEventHandler(events__ProjectRenamed);
			solutionEvents_.QueryCloseSolution +=new _dispSolutionEvents_QueryCloseSolutionEventHandler(events__QueryCloseSolution);
			solutionEvents_.Renamed +=new _dispSolutionEvents_RenamedEventHandler(events__Renamed);

			projectItemEvents_.ItemAdded +=new _dispProjectItemsEvents_ItemAddedEventHandler(projectItemEvents__ItemAdded);
			projectItemEvents_.ItemRemoved +=new _dispProjectItemsEvents_ItemRemovedEventHandler(projectItemEvents__ItemRemoved);
			projectItemEvents_.ItemRenamed +=new _dispProjectItemsEvents_ItemRenamedEventHandler(projectItemEvents__ItemRenamed);

			miscFileEvents_.ItemAdded +=new _dispProjectItemsEvents_ItemAddedEventHandler(miscFileEvents__ItemAdded);
			miscFileEvents_.ItemRemoved +=new _dispProjectItemsEvents_ItemRemovedEventHandler(miscFileEvents__ItemRemoved);
			miscFileEvents_.ItemRenamed +=new _dispProjectItemsEvents_ItemRenamedEventHandler(miscFileEvents__ItemRenamed);

		}

		public override void RemoveHandlers()
		{
			log_.Debug( "Removing solution event handlers");
			solutionEvents_.AfterClosing -=new _dispSolutionEvents_AfterClosingEventHandler(events__AfterClosing);
			solutionEvents_.BeforeClosing -=new _dispSolutionEvents_BeforeClosingEventHandler(events__BeforeClosing);
			solutionEvents_.Opened -=new _dispSolutionEvents_OpenedEventHandler(events__Opened);
			solutionEvents_.ProjectAdded -=new _dispSolutionEvents_ProjectAddedEventHandler(events__ProjectAdded);
			solutionEvents_.ProjectRemoved -=new _dispSolutionEvents_ProjectRemovedEventHandler(events__ProjectRemoved);
			solutionEvents_.ProjectRenamed -=new _dispSolutionEvents_ProjectRenamedEventHandler(events__ProjectRenamed);
			solutionEvents_.QueryCloseSolution -=new _dispSolutionEvents_QueryCloseSolutionEventHandler(events__QueryCloseSolution);
			solutionEvents_.Renamed -=new _dispSolutionEvents_RenamedEventHandler(events__Renamed);

			projectItemEvents_.ItemAdded -=new _dispProjectItemsEvents_ItemAddedEventHandler(projectItemEvents__ItemAdded);
			projectItemEvents_.ItemRemoved -=new _dispProjectItemsEvents_ItemRemovedEventHandler(projectItemEvents__ItemRemoved);
			projectItemEvents_.ItemRenamed -=new _dispProjectItemsEvents_ItemRenamedEventHandler(projectItemEvents__ItemRenamed);

			miscFileEvents_.ItemAdded -=new _dispProjectItemsEvents_ItemAddedEventHandler(miscFileEvents__ItemAdded);
			miscFileEvents_.ItemRemoved -=new _dispProjectItemsEvents_ItemRemovedEventHandler(miscFileEvents__ItemRemoved);
			miscFileEvents_.ItemRenamed -=new _dispProjectItemsEvents_ItemRenamedEventHandler(miscFileEvents__ItemRenamed);

		}

		private void events__AfterClosing()
		{
			controller_.SolutionCleanup();
			log_.Debug("AfterClosing event");

		}

		private void events__BeforeClosing()
		{
			log_.Debug("BeforeClosing event");

		}

		private void events__Opened()
		{
			log_.Debug( "Solution opened event triggered" );
			// in cvs controlled solution we need to check to see if user 
			// wants add in to manage solution so we pop up a dialog box
			// to allow user to choose. If the solution was opened directly from cvs
			// this step is not needed
			string solutionPath = Path.GetDirectoryName(controller_.DTE.Solution.FileName);
			string cvsPath = Path.Combine( solutionPath, "CVS" );
			FileAttributes attr = File.GetAttributes( cvsPath );

			if( (int)attr != -1 ) // check for existance
			{
				if( !controller_.AddinLoadedForSolution )
				{
					if( MessageBox.Show( controller_.HostWindow,
						controller_.GetLocalizedString( "MSGBOX_QUERY_LOAD_ADDIN"),
						controller_.GetLocalizedString( "APPLICATION_TITLE"),
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question ) != DialogResult.Yes )
					{
						return;
					}
				}

				controller_.CacheSolutionState();


			}


		}

		private void events__ProjectAdded(Project Project)
		{
			log_.Debug("Project Added");
		}

		private void events__ProjectRemoved(Project Project)
		{
			log_.Debug("Project Removed");
		}

		private void events__ProjectRenamed(Project Project, string OldName)
		{
			log_.Debug("project renamed");
		}

		private void events__QueryCloseSolution(ref bool fCancel)
		{
			log_.Debug("queryclose solution");
		}

		private void events__Renamed(string OldName)
		{
			log_.Debug(string.Format("solution renamed old name {0}", OldName) );
		}

		private void projectItemEvents__ItemAdded(ProjectItem ProjectItem)
		{
			log_.Debug(string.Format("Project item {0} added", ProjectItem.Name ));
		}

		private void projectItemEvents__ItemRemoved(ProjectItem ProjectItem)
		{
			log_.Debug(string.Format("Project item {0} removed", ProjectItem.Name ));
		}

		private void projectItemEvents__ItemRenamed(ProjectItem ProjectItem, string OldName)
		{
			log_.Debug(string.Format("Project {0} name changed to {1}", OldName, ProjectItem.Name));
		}

		private void miscFileEvents__ItemAdded(ProjectItem ProjectItem)
		{
			log_.Debug(string.Format("Misc file {0} added", ProjectItem.Name ));
			}

		private void miscFileEvents__ItemRemoved(ProjectItem ProjectItem)
		{
			log_.Debug(string.Format("misc item {0} removed", ProjectItem.Name ));
		}

		private void miscFileEvents__ItemRenamed(ProjectItem ProjectItem, string OldName)
		{
			log_.Debug(string.Format("misc item renamed from {0} to {1}", OldName, ProjectItem.Name));
		}
	}
}
