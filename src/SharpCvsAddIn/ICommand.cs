/********************************************************************************************
 *	Contains interface to menu commands
 * ******************************************************************************************/
using EnvDTE;

namespace SharpCvsAddIn
{
    /// <summary>
    /// Represents an Ankh command.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Get the status of the command
        /// </summary>
        vsCommandStatus QueryStatus( IController context );

        /// <summary>
        /// Execute the command
        /// </summary>
        void Execute(IController context, string parameters);

        /// <summary>
        /// The EnvDTE.Command instance corresponding to this command.
        /// </summary>
        EnvDTE.Command Command
        {
            get;
            set;
        }

    }
}



