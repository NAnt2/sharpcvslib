/********************************************************************************************
 *	This file contains classes that wrap cvs status information
 * 	
 * *******************************************************************************************/
using System;
using System.Collections;
using System.Reflection;
using EnvDTE;
using System.Diagnostics;
using Microsoft.Office.Core;
using System.Runtime.InteropServices;
using log4net;

namespace SharpCvsAddIn
{
    /// <summary>
    /// Responsible for registering the ICommand implementations in this assembly.
    /// </summary>
    public class CommandMap : DictionaryBase
    {
		private static readonly ILog log_ = LogManager.GetLogger(typeof(CommandMap));
        /// <summary>
        /// Private constructor to avoid instantiation.
        /// </summary>
        private  CommandMap()
        {			
        }

        /// <summary>
        /// Returns the ICommand object corresponding to the name.
        /// </summary>
        public ICommand this[ string name ]
        {
            get{ return (ICommand)this.Dictionary[name]; }
        }


        /// <summary>
        /// Registers all commands present in this DLL.
        /// </summary>
        /// <param name="dte">TODO: what to do, what to do?</param>
        public static CommandMap LoadCommands( IController context, bool register )
        {
            CreateSubMenu( context );

            CommandMap commands = new CommandMap();

            // find all the ICommand subclasses in all modules
            foreach( System.Reflection.Module module in 
                typeof( CommandMap ).Assembly.GetModules( false ) )
            {
                foreach( Type type in module.FindTypes( 
                    new TypeFilter( CommandMap.CommandTypeFilter ), null ) )
                {  
                    // is this a VS.NET command?
                    VSNetCommandAttribute[] vsattrs = (VSNetCommandAttribute[])(
                        type.GetCustomAttributes(typeof(VSNetCommandAttribute), false) );
                    if ( vsattrs.Length > 0 )
                    {
                        // put it in the dict
                        ICommand cmd = (ICommand)Activator.CreateInstance( type );
                        commands.Dictionary[ context.AddIn.ProgID + "." + vsattrs[0].Name ] = cmd;

                        // do we want to register it?
                        if ( register )
                            RegisterVSNetCommand( vsattrs[0], cmd,  context );
                    }
                }
            }

            return commands;            
        }

        /// <summary>
        /// Get rid of any old commands hanging around.
        /// </summary>
        public static void DeleteCommands( IController controller )
        {
           EnvDTE.Commands cmds = controller.DTE.Commands;

            if ( cmds != null )
            {
                // we only want to delete our own commands
                foreach( Command cmd in cmds )
                {
                    try
                    {
						if ( cmd.Name != null && cmd.Name.StartsWith( controller.AddIn.ProgID ) )
						{
							Debug.WriteLine( string.Format( "Deleting command {0}", cmd.Name ) );
							cmd.Delete();
						}
                    }
                    catch( System.IO.FileNotFoundException )
                    {
                        // swallow
                        // HACK: find out why FileNotFoundException is thrown
                    }
                    catch( COMException )
                    {
                        // swallow
                        // HACK: ditto
                    }
                }
            }
        }
            

        /// <summary>
        /// Callback used to filter the type list.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool CommandTypeFilter( Type type, object obj )
        {
            return typeof(ICommand).IsAssignableFrom(type) && !type.IsAbstract;
        }

        /// <summary>
        /// Register a command.
        /// </summary>
        /// <param name="type">A Type object representing the command to register.</param>
        /// <param name="commands">A Commands collection in which to put the command.</param>
        private static void RegisterVSNetCommand( VSNetCommandAttribute attr, 
            ICommand cmd, IController controller )
        {
            // register the command with the environment
            object []contextGuids = new object[] { };
			
			string menuText = controller.GetLocalizedString( attr.TextResource );
			string toolTip = controller.GetLocalizedString( attr.ToolTipResource );

			log_.Debug( string.Format( "Adding menu item {0} resource name is {1}", menuText, attr.TextResource ));

            cmd.Command = controller.DTE.Commands.AddNamedCommand( controller.AddIn, attr.Name, menuText,
				toolTip, true,
                attr.Bitmap, ref contextGuids, (int)vsCommandStatus.vsCommandStatusUnsupported );
			Debug.WriteLine( string.Format("Adding command {0}", cmd.Command.Name ));

            RegisterControl( cmd, controller );     
        }

        /// <summary>
        /// Registers a commandbar. 
        /// </summary>
        /// <param name="command">The ICommand to attach the command bar to.</param>
        /// <param name="type">The type that handles the command.</param>
        private static void RegisterControl( ICommand cmd, IController controller )
        {
            // register the command bars
            foreach( VSNetControlAttribute control in cmd.GetType().GetCustomAttributes( 
                typeof(VSNetControlAttribute), false) ) 
            {
                // get the actual name of the command
                string name = ((VSNetCommandAttribute)cmd.GetType().GetCustomAttributes(
                    typeof(VSNetCommandAttribute), false )[0]).Name;

                control.AddControl( cmd, controller, control.CommandBar + "." + name );
            }
        }        

        private static void CreateReposExplorerPopup( IController context )
        {
			/*
            context.RepositoryExplorer.CommandBar = (CommandBar)
                context.DTE.Commands.AddCommandBar( "ReposExplorer", vsCommandBarType.vsCommandBarTypePopup,
                    null, 1 );
					*/
        }

        /// <summary>
        /// Creates a submenu on the Tools menu.
        /// </summary>
        /// <param name="context"></param>
        private static void CreateSubMenu( IController context )
        {
            CommandBar toolMenu = (CommandBar)
                context.DTE.CommandBars[ "Tools" ];
/*
            context.DTE.Commands.AddCommandBar( "AnkhSVN", 
                vsCommandBarType.vsCommandBarTypeMenu,
                toolMenu, 1 );
*/
        }
    }
}
