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
#endregion

using System;
using System.Collections;
using System.IO;

using ICSharpCode.SharpCvsLib.Requests;
using ICSharpCode.SharpCvsLib.Misc;

using log4net;

namespace ICSharpCode.SharpCvsLib.Commands { 
	
    /// <summary>
    /// Command to refresh the working folder with the current sources
    ///     from the repository.
    /// </summary>
	public class UpdateCommand2 : ICommand
	{
	    private readonly ILog LOGGER = 
	        LogManager.GetLogger (typeof (UpdateCommand2));
	    
		private WorkingDirectory workingdirectory;
		private string  logmessage;
		private string  vendor  = "vendor";
		private string  release = "release";
		
        /// <summary>
        /// Log message.
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
        /// Release string.
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
        /// Constructor.
        /// </summary>
        /// <param name="workingdirectory"></param>
		public UpdateCommand2(WorkingDirectory workingdirectory)
		{
			this.workingdirectory = workingdirectory;
		}

		/// <summary>
		/// Perform the update.
		/// </summary>
		/// <param name="connection"></param>
		public void Execute(CVSServerConnection connection)
		{
			foreach (DictionaryEntry folder in workingdirectory.Folders) {
				foreach (Entry entry  in ((Folder)folder.Value).Entries)
				if (!entry.IsDirectory) {
					DateTime old = entry.TimeStamp;
					entry.TimeStamp = entry.TimeStamp;
					
					string path = workingdirectory.CvsRoot.CvsRepository +  "/" + workingdirectory.WorkingDirectoryName + folder.Key.ToString();
					
					connection.SubmitRequest(new DirectoryRequest(".", path));
					
					path = workingdirectory.CvsRoot.CvsRepository + folder.Key.ToString();
					
					string fileName = Path.GetDirectoryName(workingdirectory.LocalDirectory) + Path.DirectorySeparatorChar + Path.GetDirectoryName(path.Substring(workingdirectory.CvsRoot.CvsRepository.Length)) + "/" + entry.Name;
					
					fileName = fileName.Replace('/', Path.DirectorySeparatorChar);
		
		            if (LOGGER.IsDebugEnabled){
					    LOGGER.Debug("local name ? : "  + fileName);
		            }
					
					if (File.GetLastAccessTime(fileName) != entry.TimeStamp) {
						connection.SubmitRequest(new ModifiedRequest(entry.Name));
						
						if (entry.IsBinaryFile) {
							connection.UncompressedFileHandler.SendBinaryFile(connection.OutputStream, fileName);
						} else {
							connection.UncompressedFileHandler.SendTextFile(connection.OutputStream, fileName);
						}
					} else {
						connection.SubmitRequest(new EntryRequest(entry));
						connection.SubmitRequest(new UnchangedRequest(entry.Name));
					}
					
					entry.TimeStamp = old;
				}
			}
			connection.SubmitRequest(new UpdateRequest());
		}
	}
}
