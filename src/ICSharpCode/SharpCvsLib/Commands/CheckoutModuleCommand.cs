#region "Copyright"
// CheckoutModuleCommand.cs 
// Copyright (C) 2001 Mike Krueger
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.
//
//    Author:     Mike Krueger, 
//                Clayton Harbour  {claytonharbour@sporadicism.com}
#endregion

using System;
using System.IO;

using ICSharpCode.SharpCvsLib.Requests;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.FileSystem;

using log4net;

namespace ICSharpCode.SharpCvsLib.Commands { 

    /// <summary>
    /// Checkout module command
    /// </summary>
    public class CheckoutModuleCommand : ICommand
    {
        private WorkingDirectory workingDirectory;

        private ILog LOGGER = LogManager.GetLogger (typeof (CheckoutModuleCommand));
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="workingDirectory"></param>
        public CheckoutModuleCommand(WorkingDirectory workingDirectory)
        {
            this.workingDirectory    = workingDirectory;
        }

        /// <summary>
        /// Execute checkout module command.
        /// </summary>
        /// <param name="connection">Server connection</param>
        public void Execute(ICommandConnection connection)
        {
            workingDirectory.Clear();
            
            connection.SubmitRequest(new CaseRequest());
            connection.SubmitRequest(new ArgumentRequest(workingDirectory.ModuleName));

            connection.SubmitRequest(new DirectoryRequest(".", 
                            workingDirectory.CvsRoot.CvsRepository + 
                            "/" + workingDirectory.ModuleName));

            connection.SubmitRequest(new ExpandModulesRequest());

            connection.SubmitRequest(
                new ArgumentRequest(ArgumentRequest.Options.MODULE_NAME));
            
            if (workingDirectory.HasRevision) {
                connection.SubmitRequest (new ArgumentRequest (ArgumentRequest.Options.REVISION));
                connection.SubmitRequest(new ArgumentRequest(workingDirectory.Revision));
            }
            if (workingDirectory.HasOverrideDirectory) {
                connection.SubmitRequest (
                    new ArgumentRequest (ArgumentRequest.Options.OVERRIDE_DIRECTORY));
                connection.SubmitRequest (
                    new ArgumentRequest (workingDirectory.OverrideDirectory));
            }
            
            connection.SubmitRequest(new ArgumentRequest(workingDirectory.ModuleName));

            connection.SubmitRequest(new DirectoryRequest(".", 
                            workingDirectory.CvsRoot.CvsRepository + 
                            "/" + workingDirectory.ModuleName));

            connection.SubmitRequest(new CheckoutRequest());
            Manager manager = new Manager ();
            if (LOGGER.IsDebugEnabled) {
                LOGGER.Debug ("looking for directories to add to the " + 
                              "entries file in=[" +
                             this.workingDirectory.WorkingPath + "]");
            }
            manager.AddDirectories (this.workingDirectory.WorkingPath);

        }
    }
}
