using System;
using EnvDTE;


namespace SharpCvsAddIn.Commands
{
	[VSNetCommandAttribute("CVS_UPDATE_COMMAND", TextResource="CVS_UPDATE_COMMAND", ToolTipResource="CVS_UPDATE_COMMAND_DESC"),
	 VSNetControl("Item", Position = 2),
	 VSNetControl("Project", Position = 1),
	 VSNetControl("Solution", Position = 2),
	 VSNetControl("Folder", Position = 2) ]
	public class UpdateCommand : CommandBase 
	{
		override public vsCommandStatus QueryStatus( IController cont )
		{
			return vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
		}

		override public void Execute( IController cont, string parameters )
		{
		}
	}

}

