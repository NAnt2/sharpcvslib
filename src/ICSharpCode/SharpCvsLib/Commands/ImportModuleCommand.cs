#region Copyright
// ImportModuleCommand.cs 
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
//    <author>Mike Krueger</author>
//    <author>Clayton Harbour</author>
#endregion

using System;
using System.Collections;
using System.IO;

using ICSharpCode.SharpCvsLib.Requests;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.FileSystem;

using log4net;

namespace ICSharpCode.SharpCvsLib.Commands { 
    
    /// <summary>
    /// Import a module into the cvs repository
    /// </summary>
    public class ImportModuleCommand : ICommand
    {
        private WorkingDirectory workingdirectory;
        private string  logmessage;
        private string  vendor  = "vendor";
        private string  release = "release";

        private readonly ILog LOGGER = 
            LogManager.GetLogger (typeof (ImportModuleCommand));
        
        /// <summary>
        /// The log message returned by the cvs server.
        /// </summary>
        public string LogMessage {
            get {
                return logmessage;
            }
            set {
                logmessage = value;
            }
        }
        
        /// <summary>
        /// Vendor string.
        /// </summary>
        public string VendorString {
            get {
                return vendor;
            }
            set {
                vendor = value;
            }
        }
        
        /// <summary>
        /// Release string
        /// </summary>
        public string ReleaseString {
            get {
                return release;
            }
            set {
                release = value;
            }
        }
        
        /// <summary>
        /// Constructor for the import module command.
        /// </summary>
        /// <param name="workingdirectory"></param>
        /// <param name="logmessage"></param>
        public ImportModuleCommand(WorkingDirectory workingdirectory, string logmessage)
        {
            this.logmessage = logmessage;
            this.workingdirectory = workingdirectory;
        }

        /// <summary>
        /// Do the dirty work.
        /// </summary>
        /// <param name="connection"></param>
        public void Execute(ICommandConnection connection)
        {
            connection.SubmitRequest(new CaseRequest());
            connection.SubmitRequest(new ArgumentRequest("-b"));
            connection.SubmitRequest(new ArgumentRequest("1.1.1"));
            connection.SubmitRequest(new ArgumentRequest("-m"));
            connection.SubmitRequest(new ArgumentRequest(logmessage));
            connection.SubmitRequest(new ArgumentRequest(workingdirectory.WorkingDirectoryName));
            connection.SubmitRequest(new ArgumentRequest(vendor));
            connection.SubmitRequest(new ArgumentRequest(release));

            System.Console.WriteLine("IMPORT START");
            
            foreach (DictionaryEntry folder in workingdirectory.Folders) {
                foreach (Entry entry  in ((Folder)folder.Value).Entries) {
                    DateTime old = entry.TimeStamp;
                    entry.TimeStamp = entry.TimeStamp.ToUniversalTime();
                    
                    string path = workingdirectory.CvsRoot.CvsRepository +  "/" + workingdirectory.WorkingDirectoryName + folder.Key.ToString();
                    string modulepath;
                    
                    if (folder.Key.ToString().Length < 1) {
                        modulepath = ".";
                    } else {
                        modulepath = folder.Key.ToString().Substring(1);
                    }
                    
                    connection.SubmitRequest(new DirectoryRequest(modulepath, path));
                    connection.SubmitRequest(new ModifiedRequest(entry.Name));
                    
                    path = Path.Combine (workingdirectory.CvsRoot.CvsRepository, 
                                         folder.Key.ToString());
                    
                                        
                    string fileName = Path.Combine (path, entry.Name);
                    connection.SendFile(fileName, entry.IsBinaryFile);
                    
                    entry.TimeStamp = old;
                }
            }
            
            connection.SubmitRequest(new DirectoryRequest(".", workingdirectory.CvsRoot.CvsRepository + "/" + workingdirectory.WorkingDirectoryName));
            connection.SubmitRequest(new ImportRequest());
            if (LOGGER.IsDebugEnabled) {
                LOGGER.Debug ("IMPORT END");
            }
        }
    }
}
