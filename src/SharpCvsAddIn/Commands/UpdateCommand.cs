using System;
using EnvDTE;
using log4net;

using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Commands;	

using SharpCvsAddIn.Jobs;		


namespace SharpCvsAddIn.Commands
{
	/// <summary>
	/// Menu command that triggers a CVS update on everything in the solution.
	/// </summary>
	[VSNetCommandAttribute("CVS_UPDATE_SOLUTION_COMMAND", TextResource="CVS_UPDATE_COMMAND", ToolTipResource="CVS_UPDATE_COMMAND_DESC"),
	 VSNetControl("Solution", Position = 2) ]
	public class UpdateSolutionCommand : CommandBase 
	{
		private static readonly ILog log_ = LogManager.GetLogger( typeof(UpdateSolutionCommand));

		override public vsCommandStatus QueryStatus( IController cont )
		{
			if( cont.SolutionIsOpen && cont.SolutionInCVS )
				return vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
			else
				return vsCommandStatus.vsCommandStatusSupported;
		}

		override public void Execute( IController cont, string parameters )
		{
			log_.Debug( "Update solution menu command is triggered");
			
			CvsRoot root = new CvsRoot( cont.CurrentConnection.ConnectionString );
            WorkingDirectory wd = new WorkingDirectory( root, cont.CurrentConnection.WorkingDirectory, 
				cont.CurrentModule.Name );
			CvsUpdateFolderJob job = new CvsUpdateFolderJob( cont, wd, cont.CurrentConnection.Password );
			
			cont.Jobs.AddJob( job, new JobCompletionHandler( this.OnUpdateComplete ) );
		}

		private void OnUpdateComplete( object sender, object job )
		{
			log_.Debug( string.Format("Job {0} is complete", ((IJob)job).Name ));
		}

	}

	/// <summary>
	/// Menu command that triggers a CVS update on everything in the selected project
	/// </summary>
	[VSNetCommandAttribute("Project.CVS_UPDATE_COMMAND", TextResource="CVS_UPDATE_COMMAND", ToolTipResource="CVS_UPDATE_COMMAND_DESC"),
	VSNetControl("Project", Position = 2) ]
	public class UpdateProjectCommand : CommandBase 
	{
		private static readonly ILog log_ = LogManager.GetLogger( typeof(UpdateProjectCommand));

		override public vsCommandStatus QueryStatus( IController cont )
		{
			if( cont.SolutionIsOpen && cont.SolutionInCVS )
				return vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
			else
				return vsCommandStatus.vsCommandStatusSupported;
		}

		override public void Execute( IController cont, string parameters )
		{
			foreach( EnvDTE.SelectedItem item in cont.DTE.SelectedItems )
			{
				//	if( typeof(EnvDTE.ProjectItem ).IsInstanceOfType( item. )
				log_.Debug( string.Format( "selected item {0}", item.Name ) );
			}
		}

	}

	/// <summary>
	/// Menu command that triggers a CVS update on everything in the selecte folder
	/// </summary>
	[VSNetCommandAttribute("Folder.CVS_UPDATE_COMMAND", TextResource="CVS_UPDATE_COMMAND", ToolTipResource="CVS_UPDATE_COMMAND_DESC"),
	VSNetControl("Folder", Position = 2) ]
	public class UpdateFolderCommand : CommandBase 
	{
		private static readonly ILog log_ = LogManager.GetLogger( typeof(UpdateFolderCommand));

		override public vsCommandStatus QueryStatus( IController cont )
		{
			if( cont.SolutionIsOpen && cont.SolutionInCVS )
				return vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
			else
				return vsCommandStatus.vsCommandStatusSupported;
		}

		override public void Execute( IController cont, string parameters )
		{
			foreach( EnvDTE.SelectedItem item in cont.DTE.SelectedItems )
			{
				//	if( typeof(EnvDTE.ProjectItem ).IsInstanceOfType( item. )
				log_.Debug( string.Format( "selected item {0}", item.Name ) );
			}
		}

	}


	/// <summary>
	/// Menu command that triggers a CVS update on selected items in the project
	/// </summary>
	[VSNetCommandAttribute("Item.CVS_UPDATE_COMMAND", TextResource="CVS_UPDATE_COMMAND", ToolTipResource="CVS_UPDATE_COMMAND_DESC"),
	VSNetControl("Item", Position = 2) ]
	public class UpdateItemCommand : CommandBase 
	{
		private static readonly ILog log_ = LogManager.GetLogger( typeof(UpdateItemCommand));

		override public vsCommandStatus QueryStatus( IController cont )
		{
			if( cont.SolutionIsOpen && cont.SolutionInCVS )
				return vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
			else
				return vsCommandStatus.vsCommandStatusSupported;
		}

		override public void Execute( IController cont, string parameters )
		{
		}

	}



}

