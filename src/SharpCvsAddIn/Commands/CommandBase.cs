
using System;
using EnvDTE;
using Microsoft.Office.Core;


namespace SharpCvsAddIn.Commands
{
    /// <summary>
    /// Base class for ICommand instances
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        /// <summary>
        /// Get the status of the command
        /// </summary>
        public abstract vsCommandStatus QueryStatus( IController cont );

        /// <summary>
        /// Execute the command
        /// </summary>
        public abstract void Execute( IController cont, string parameters );

        /// <summary>
        /// The EnvDTE.Command instance corresponding to this command.
        /// </summary>
        public EnvDTE.Command Command
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return this.command; 
            }
            [System.Diagnostics.DebuggerStepThrough]
            set
            {
                this.command = value;
            }
        }

        /// <summary>
        /// Whether the Shift key is down.
        /// </summary>
        protected static bool Shift
        {
            get
            { 
				return false;
				// TODO: implement code that detects if shift key is down
				/*
                return Utils.Win32.Win32.GetAsyncKeyState( 
                    (int)System.Windows.Forms.Keys.ShiftKey ) != 0;
				*/
            }
        }

        protected void SaveAllDirtyDocuments( IController cont )
        {
            cont.DTE.ExecuteCommand( "File.SaveAll", "" );
        }

		/*
        protected static PathSelector GetPathSelector( string text )
        {
            PathSelector p = new PathSelector();
            p.Caption = text;
            p.GetPathInfo += new GetPathInfoDelegate(GetPathInfo);
            return p;
        }
		*/

        protected const vsCommandStatus Enabled = 
            vsCommandStatus.vsCommandStatusEnabled |
            vsCommandStatus.vsCommandStatusSupported;

        protected const vsCommandStatus Disabled = 
            vsCommandStatus.vsCommandStatusSupported;

        protected CommandBarControl GetControl(IController cont, string barName, string name )
        {
            // TODO: either preload this or find a better way to map to 
            // the commandbarcontrols for a command
            CommandBar bar = VSNetControlAttribute.GetCommandBar( barName, cont );           
            CommandBarControl cntl = bar.FindControl( Type.Missing, Type.Missing, 
                barName + "." + name, Type.Missing, Type.Missing );
            return cntl;
        }


        /// <summary>
        /// A ResourceFilterCallback method that filters for modified items.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected static bool ModifiedFilter( IProjectItem item )
        {
            return item.IsModified;
        }

        /// <summary>
        /// A ResourceFilterCallback that filters for versioned directories.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected static bool DirectoryFilter( IProjectItem item )
        {
            return item.IsVersioned && item.IsDirectory;
        }

        protected static bool VersionedFilter( IProjectItem item )
        {
            return item.IsVersioned;
        }

        protected static bool UnversionedFilter( IProjectItem item )
        {
            return !item.IsVersioned;
        }

        protected static bool UnmodifiedSingleFileFilter( IProjectItem item )
        {
            return item.IsVersioned && !item.IsModified && item.IsFile;
        }

        protected static bool UnmodifiedItemFilter( IProjectItem item )
        {
            return item.IsVersioned && !item.IsModified;
        }

		/*
        protected static void GetPathInfo(object sender, GetPathInfoEventArgs args)
        {
            IProjectItem item = (IProjectItem)args.Item;
            args.IsDirectory = item.IsDirectory;
            args.Path = item.Path;
        }
		*/

        private EnvDTE.Command command;

        
    }
}
