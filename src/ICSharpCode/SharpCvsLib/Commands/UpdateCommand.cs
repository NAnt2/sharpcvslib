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
//    Author:     Mike Krueger, 
//                Clayton Harbour  {claytonharbour@sporadicism.com}
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
		    if (LOGGER.IsDebugEnabled) {
		        String msg = "In execute, looking for working folders.  " +
		            "count of working folders=[" + 
		                workingdirectory.Folders.Count + "]";
		        LOGGER.Debug (msg);
		    }
		    
		    Folder[] _foldersToUpdate = 
		        (Folder[])workingdirectory.FoldersToUpdate.Clone ();
			foreach (Folder folder in _foldersToUpdate) {
			    this.SetDirectory (connection, folder);
			    
			    // TODO: Move this somewhere else when I get the fileset
			    //    system working.  This just grabs the tag file at the
			    //    root folder.
			    Tag tag = this.FetchTag (connection);
			    if (null != tag) {
			        connection.SubmitRequest (new StickyRequest (tag.FileContents));
			    }
			    foreach (Entry entry  in folder.Entries) {
    				if (!entry.IsDirectory) {
//    					String path = workingdirectory.CvsRoot.CvsRepository + 
//    					                   "/" +
//    					                   folder.Repos.FileContents;

    					//string path = workingdirectory.CvsRoot.CvsRepository +  
    					//			"/" + workingdirectory.WorkingDirectoryName;/* + 
    					//			folder.Key.ToString();
    					
//    					if (LOGGER.IsDebugEnabled) {
//    					    String msg = "Before submit directory request.  " +
//    					        "path=[" + path + "]";
//    					    LOGGER.Debug (msg);
//    					}
    					
//    					try {
//        					connection.SubmitRequest(new DirectoryRequest(".", path));
//    					}
//    					catch (Exception e) {
//    					    String msg = "Exception while submitting directory request.  " +
//    					        "path=[" + path + "]";
//                            LOGGER.Error (e);
//    					}
    					
//    					path = workingdirectory.CvsRoot.CvsRepository + 
//    							folder.Repos.FileContents;
    					    					
//    				    string fileName =
//    				        this.getFileNameAndPath (
//    				                                 Path.GetDirectoryName (workingdirectory.LocalDirectory),
//    				                                 path.Substring (workingdirectory.CvsRoot.CvsRepository.Length),
//    				                                 entry.Name);
    		
    				    //this.FetchFile (connection, entry);

    				}
                    connection.SubmitRequest(new UpdateRequest());
			    }
			}

		}


		private void SetDirectory (CVSServerConnection connection, 
		                           Folder folder) {
            String absoluteDir = 
                connection.Repository.CvsRoot.CvsRepository + "/" +
                        folder.Repos.FileContents;
            
    		try {
    			connection.SubmitRequest(new DirectoryRequest(".",
    			                                              absoluteDir));
    		}
    		catch (Exception e) {
    		    String msg = "Exception while submitting directory request.  " +
    		        "path=[" + folder.Repos.FileContents + "]";
                LOGGER.Error (e);
    		}
		}
		
		private Tag FetchTag (CVSServerConnection connection) {
		    Manager manager = new Manager ();
		    
		    try {
    		    return manager.FetchTag (connection.Repository.WorkingDirectoryName);
		    } catch (FileNotFoundException) {
		        return null;
		    }
		}
		
		private void FetchFile (CVSServerConnection connection,
		                        Entry entry) {
			bool fileExists;
			DateTime old = entry.TimeStamp;
			entry.TimeStamp = entry.TimeStamp;
			try {
			    fileExists = File.Exists (entry.Filename);
			}
			catch (Exception e) {
			    LOGGER.Error (e);
			    fileExists = false;
			}
			
			if (!fileExists) {
			    connection.SubmitRequest (new EntryRequest (entry));
			} else if (File.GetLastAccessTime(entry.Filename) != 
			           entry.TimeStamp.ToUniversalTime ()) {
				connection.SubmitRequest(new ModifiedRequest(entry.Name));
				
				if (entry.IsBinaryFile) {
					connection.UncompressedFileHandler.SendBinaryFile(connection.OutputStream, 
					                                                  entry.Filename);
				} else {
					connection.UncompressedFileHandler.SendTextFile(connection.OutputStream, 
					                                                entry.Filename);
				}
			} else {
				connection.SubmitRequest(new EntryRequest(entry));
				connection.SubmitRequest(new UnchangedRequest(entry.Name));
			}
			
			entry.TimeStamp = old;
		}
		
		/// <summary>Returns the local filename and path.
		/// </summary>
		/// <param name="baseDir">The base working dir.</param>
		/// <param name="reposDir">The remote repository relative path.</param>
		/// <param name="entryName">The name of the file to look at.</param>
    	private String getFileNameAndPath (    string baseDir, 
    	                                       string reposDir, 
    	                                       string entryName) {
           if (LOGGER.IsDebugEnabled) {
               String msg = "baseDir=[" + baseDir + "]" +
                    "reposDir=[" + reposDir + "]" +
                    "entryName=[" + entryName + "]";
               LOGGER.Debug (msg);
           }
            string _baseDir;
            if (baseDir.EndsWith (Path.DirectorySeparatorChar.ToString ())) {
                _baseDir = baseDir.Replace (Path.DirectorySeparatorChar.ToString (), "");
            }
            else {
                _baseDir = baseDir;
            }           
            
            //string _reposDir = reposDir.Replace ('/', Path.DirectorySeparatorChar) + 
            //                    Path.DirectorySeparatorChar;
            //string _entryName = entryName.Replace ('/', Path.DirectorySeparatorChar);

            String path = Path.Combine (_baseDir, reposDir);
            String fileNameAndPath = Path.Combine (path, entryName);
    	                                           
            fileNameAndPath.Replace ('/', Path.DirectorySeparatorChar);

            if (LOGGER.IsDebugEnabled) {
                String msg = "fileNameAndPath=[" + fileNameAndPath + "]";
                LOGGER.Debug (msg);
            }
            
            return fileNameAndPath;
    	}
	}
}
