#region "Copyright"
// Copyright (C) 2003 Clayton Harbour
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
//    Author: Clayton Harbour
//     claytonharbour@sporadicism.com
#endregion

using System;
using System.Collections;
using System.Text;
using System.IO;

using log4net;

using ICSharpCode.SharpCvsLib.Misc;

namespace ICSharpCode.SharpCvsLib.FileSystem {
    
    /// <summary>
    ///     Manages the addition and creation of cvs files such as
    ///         - Entries
    ///         - Repository
    ///         - Root
    /// </summary>
    public class Manager {

        private readonly ILog LOGGER = 
            LogManager.GetLogger (typeof (Manager));
		/// <summary>The cvs directory information.</summary>
		public string CVS {
		    get {return "CVS";}
		}
		
        /// <summary>Constructory</summary>        
        public Manager () {
        }
        
        /// <summary>
        ///     Recurse through the directory entries and add a cvs file 
        ///         entry for each directory found in the physical path.
        /// </summary>
        /// <param name="path">The path to look in for directory entries.</param>
        public void AddDirectories (String path) {
            foreach (String directory in Directory.GetDirectories (path)) {
                // Only add the directory if the folder already contains a
                //    cvs directory.
                if (this.HasCvsDir (directory)) {
                    Entry dirEntry = 
                        this.CreateDirectoryEntryFromPath (path);
                    this.Add (dirEntry);
                }
                this.AddDirectories (directory);
            }
        }
        
        public Folder[] FetchFilesToUpdate (String directory) {
            ArrayList folders = new ArrayList ();
            Folder folder = new Folder ();
            folder.Repos = (Repository)this.FetchSingle (directory, 
                                                         Repository.FILE_NAME);
            folder.Entries = new ArrayList (this.Fetch (directory, Entry.FILE_NAME));
            folders.Add (folder);
            this.FetchFilesToUpdateRecursive (folders, directory);
            
            return (Folder[])folders.ToArray (typeof (Folder));
        }
        
        private void FetchFilesToUpdateRecursive (ArrayList folders, 
                                                  String directory) {
            foreach (String subDir in Directory.GetDirectories (directory)) {
                Folder folder = new Folder ();
                folder.Repos = (Repository)this.FetchSingle (directory, 
                                                             Repository.FILE_NAME);
                folder.Entries = new ArrayList (this.Fetch (directory, Entry.FILE_NAME));
                folders.Add (folder);
                this.FetchFilesToUpdateRecursive (folders, subDir);
            }
        }
                
        public Entry CreateDirectoryEntry (String localPath) {
            return this.CreateDirectoryEntryFromPath (localPath);            
        }
        
        public Entry CreateDirectoryEntry (PathTranslator path) {
            return this.CreateDirectoryEntryFromPath (path.LocalPath);
        }
        
        /// <summary>
        ///     Get all of the folders in the local base directory.
        /// </summary>
        /// <param name="localBaseDir">The path to the local basedir.</param>
        public Hashtable getFolders (String localBaseDir) {
            Hashtable folders = new Hashtable ();
            this.getFolders (folders, localBaseDir);
            return folders;
        }
        
        private void getFolders (Hashtable folders, String path) {
            foreach (String directory in Directory.GetDirectories (path)) {
                ICvsFile[] entries = this.Fetch (directory, Entry.FILE_NAME);
                Entry dirEntry = 
                    this.CreateDirectoryEntryFromPath (path);
                ICvsFile repository = 
                    this.FetchSingle (path, Repository.FILE_NAME);
                
                if (!dirEntry.Name.Equals ("CVS")) {
                    ICSharpCode.SharpCvsLib.Misc.Folder folder = 
                        new ICSharpCode.SharpCvsLib.Misc.Folder ();
                    folder.Entries = new ArrayList (entries);
                    folders.Add (repository, folder);
                    this.getFolders (folders, directory);
                }
            }
        }
        
        /// <summary>
        ///     Create a directory entry from the given path.
        /// </summary>
        /// <param name="path">The path to use in creating the entry.</param>
        /// <returns>The directory entry.</returns>
        public Entry CreateDirectoryEntryFromPath (String path) {
            path = path.Replace ('\\', '/');
            string[] dirTokens = path.Split ('/');
            
            string dirToken = dirTokens[dirTokens.Length - 1];
            string dirEntry = "D/" + dirToken;
            
            // If there is some path information append empty slashes,
            //     otherwise just leave the entry as 'D/'.
            if (dirEntry.Length > 2) {
                int addSlashes = 6 - dirEntry.Split ('/').Length;
                for (int slashes = 0; slashes < addSlashes; slashes++) {
                    dirEntry = dirEntry + "/";
                }
            }
            if (LOGGER.IsDebugEnabled) {
                String msg = "Create directory entry from path.  " +
                    "dirEntry=[" + dirEntry + "]" +
                    "dirToken=[" + dirToken + "]" +
                    "path=[" + path + "]";
                LOGGER.Debug (msg);
            }
            
            String upPath = this.UpPath (path);
            Entry entry = new Entry (upPath, dirEntry);
            return entry;
        }
        
        private String UpPath (String path) {
            path = path.Replace ('\\', '/');
            String [] dirs = path.Split ('/');
            
            String upPath = path.Substring (0, 
                                            path.Length - dirs[dirs.Length - 1].Length);
            if (LOGGER.IsDebugEnabled) {
                String msg = "in uppath.  " + 
                    "path=[" + path + "]" +
                    "upPath=[" + upPath + "]";
                LOGGER.Debug (msg);
            }
            return upPath;
        }

        public void Add (ICvsFile[] cvsEntries) {
            foreach (ICvsFile entry in cvsEntries) {
                this.Add (entry);
            }
        }
        /// <summary>
        ///     Add the contents of the cvs file object to the respective
        ///         file.
        /// </summary>
        public void Add (ICvsFile newCvsEntry) {
            String cvsPath = this.CombineCvsDir (newCvsEntry.Path);
            
            ArrayList newCvsEntries = new ArrayList ();
            
            bool newEntry = true;
            
            try {
                ICollection cvsFiles = this.Fetch (cvsPath, newCvsEntry.Filename);
                foreach (ICvsFile currentCvsEntry in cvsFiles) {
                    if (currentCvsEntry.Equals (newCvsEntry)) {
                        newCvsEntries.Add (newCvsEntry);
                        newEntry = false;
                    }
                    else {
                        newCvsEntries.Add (currentCvsEntry);
                    }
                }
            }
            catch (FileNotFoundException e) {
                // If we can't find the file, chances are this is the first
                //    entry that we are adding.
                LOGGER.Debug (e);
                newEntry = true;
            }
            
            if (newEntry) {
                newCvsEntries.Add (newCvsEntry);
            }
            
            this.WriteToFile (
                              (ICvsFile[])newCvsEntries.ToArray 
                                  (typeof (ICvsFile)));
        }
        
        /// <summary>
        ///     Remove the file contents from the file.
        /// </summary>
        public void Remove (ICvsFile file) {
            String cvsPath = this.CombineCvsDir (file.Path);
            this.RemoveFromFile (cvsPath, file.Filename, file.FileContents);
        }
        
        private void RemoveFromFile (String path, String file, String line) {
            ICollection fileLines = 
                this.ReadFromFile (path, file);
            
            ArrayList newFileLines = new ArrayList ();
            foreach (String fileLine in fileLines) {
                if (!fileLine.Equals (line)) {
                    newFileLines.Add (line);
                }
            }
            this.WriteToFile (path, 
                              file, 
                              (String[])newFileLines.ToArray (typeof (String)));
        }
        
        /// <summary>
        ///     Adds a collection of lines to the cvs file.  The first
        ///         entry overwrites any file currently in the directory
        ///         and all other following entries are appended to the
        ///         file.
        /// </summary>
        /// <param name="path">The current working directory.</param>
        /// <param name="file">The cvs file to write to.</param>
        /// <param name="lines">A collection of String value lines to add to the cvs file.</param>
        private void WriteToFile (String path,
                                  String file, 
                                  String[] lines) {
            bool overWriteFile = true;
            foreach (String line in lines) {
                this.WriteToFile (path, file, line, overWriteFile);
                if (overWriteFile) {
                    overWriteFile = false;
                }
            }
        }
        
        /// <summary>
        ///     Adds a collection of lines to the cvs file.  The first
        ///         entry overwrites any file currently in the directory
        ///         and all other following entries are appended to the
        ///         file.
        /// </summary>
        /// <param name="path">The current working directory.</param>
        /// <param name="file">The cvs file to write to.</param>
        /// <param name="lines">A collection of String value lines to add to 
        ///     the cvs file.</param>        
        private void WriteToFile (ICvsFile[] entries) {
            bool append = false;
            
            foreach (ICvsFile entry in entries) {
                String cvsPath = this.CombineCvsDir (entry.Path);
                this.WriteToFile (cvsPath, 
                                  entry.Filename, 
                                  entry.FileContents, 
                                  append);
                if (!append) {
                    append = true;
                }
            }
        }
                
        /// <summary>
        ///     Write to the cvs file.
        /// </summary>
        /// <param name="path">The current working directory.</param>
        /// <param name="file">The cvs file to write to.</param>
        /// <param name="line">The line to enter into the file.</param>
        /// <param name="append">Whether or not to append to the file.</param>
        private void WriteToFile (    String path, 
                                      String file, 
                                      String line, 
                                      bool append) {
            string fileAndPath = Path.Combine (path, file.Replace ("/", "").Replace ("\\", ""));
            this.CreateCvsDir (path);
            line = line.Replace ("\\", "/");
            
            if (LOGGER.IsDebugEnabled) {
                String msg = "Writing to a cvs file.  " +
                    "path=[" + path + "]" +
                    "file=[" + file + "]" +
                    "line=[" + line + "]" + 
                    "append=[" + append + "]";
                LOGGER.Debug (msg);
            }
            
			StreamWriter sw = 
			    new StreamWriter(fileAndPath, append, Encoding.ASCII);
            sw.WriteLine (line);    
            
			sw.Close();            
        }
        
        /// <summary>
        ///     Checks if a cvs directory exists in the specified path,
        ///         if it does not then it is created.
        /// </summary>
        /// <param name="path">The full directory path of the
        ///     directory.</param>
        private void CreateCvsDir (String path) {
            String cvsDir = this.CombineCvsDir (path);
            if (LOGGER.IsDebugEnabled) {
                String msg = "Creating cvs directory if it does not exist.  " +
                    "path=[" + path + "]" +
                    "cvsDir=[" + cvsDir + "]";
                LOGGER.Debug (msg);
            }
            
			if (!Directory.Exists(cvsDir)) {
				Directory.CreateDirectory(cvsDir);
			}            
        }
        
        /// <summary>
        ///     Determines if the path ends with the <code>CVS</code> constant.
        /// </summary>
        /// <returns><code>true</code> if the path ends with the <code>CVS</code>
        ///     constant, <code>false</code> otherwise.</returns>
        private bool HasCvsDir (String path) {
            String[] dirs = path.Replace ('\\', '/').Split ('/');
            bool hasCvsDir = (dirs[dirs.Length - 1].Equals (this.CVS));
            
            if (LOGGER.IsDebugEnabled) {
                String msg = "Does this path have a cvs directory at end of file?  " +
                    "hasCvsDir=[" + hasCvsDir + "]" +
                    "path=[" + path + "]";
                LOGGER.Debug (msg);
            }
            return hasCvsDir;
        }
        
        /// <summary>
        ///     Add the <code>CVS</code> constant to the directory path if it
        ///         does not exist.
        /// </summary>
        /// <returns>The current path if it ends with the <code>CVS</code> 
        ///     constant, or the path plus the <code>CVS</code> constant if it
        ///     does not already contain this.</returns>
        private String CombineCvsDir (String path) {
            bool hasCvs = this.HasCvsDir (path);            
            String cvsDir;            
            if (hasCvs) {
                cvsDir = path;
            }
            else {
                cvsDir = Path.Combine (path, this.CVS);
            }
            
            if (LOGGER.IsDebugEnabled) {
                String msg = "Cvsdir=[" + cvsDir + "]";
                LOGGER.Debug (msg);
            }
            
            return cvsDir;
        }
        
        /// <summary>
        ///     Sends the cvs message to a logging target and/ or the
        ///         console.
        /// 
        ///     Messages typically include responses from the server
        ///         and are used to determine which files have been
        ///         updated.  An example of an update/ created message
        ///         would like like the following:
        /// 
        ///         cvs server: U sharpcvslib/Cvslib/Streams/CvsStream
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void SendCvsMessage (String message) {
            System.Console.WriteLine (message);
            LOGGER.Info (message);
        }
        
        /// <summary>
        ///     Fetch a single entry.  If more than one entry is found then an
        ///         exception is thrown.
        /// </summary>
        /// <param name="path">The path to the current working directory
        ///    or to the cvs directory.</param>
        /// <param name="filename">The name of the cvs file to fetch values.</param>
        /// <returns>A single <see cref="ICvsFile">Cvs file</see></returns>
        public ICvsFile FetchSingle (String path, String filename) {
            ICvsFile [] entries = this.Fetch (path, filename);
            
            if (entries.Length == 0) {
                String msg = "File not found.  " +
                    "path=[" + path + "]" + 
                    "filename=[" + filename + "]";
                throw new Exception (msg);
            }
            if (entries.Length > 1) {
                String msg = "Expecting maximum of 1 entry, found=[" + 
                    entries.Length + "]";
                throw new Exception (msg);
            }
            
            return entries[0];
        }
        
        /// <summary>
        ///     Fetch all of the entry objects for the specified cvs filename
        ///         in the specified path.
        /// </summary>
        /// <param name="path">The path to the current working directory
        ///    or to the cvs directory.</param>
        /// <param name="filename">The name of the cvs file to fetch values.</param>
        /// <returns>A collection of <see cref="ICvsFile">Cvs files</see></returns>
        public ICvsFile [] Fetch (String path, String filename) {
            String cvsDir = this.CombineCvsDir (path);
            ICollection lines = this.ReadFromFile (cvsDir, filename);    
            ArrayList entries = new ArrayList ();
            Factory factory = new Factory ();
            
            foreach (String line in lines) {
                entries.Add (factory.CreateCvsObject (cvsDir, 
                                                      filename, 
                                                      line));
            }
            
            return (ICvsFile[])entries.ToArray (typeof (ICvsFile));
        }
        
        
        /// <summary>
        ///     Read the contents of the specified file line by line.  
        ///         The contents are placed in a collection object and 
        ///         can be later extracted by the specified value object.  
        ///         This is used to keep the file access in one location.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="file">The name of the file to read.</param>
        /// <returns>A collection of strings, one for each line
        ///     in the specified file.</returns>
        private ICollection ReadFromFile (String path, String file) {
            ArrayList fileContents = new ArrayList ();
            String cvsPath = this.CombineCvsDir (path);
            
            String filePath = Path.Combine (cvsPath, file);
			if (File.Exists(filePath)) {
				StreamReader sr = File.OpenText(filePath);
				
				while (true) {
					string line = sr.ReadLine();
					if (line == null) {
						break;
					}
					if (line.Length > 1) {		
					    if (LOGGER.IsDebugEnabled) {
					        String msg = "Found cvs file, adding contents.  " +
					            "path=[" + path + "]" +
					            "file=[" + file + "]" +
					            "line=[" + line + "]";
					        LOGGER.Debug (msg);
					    }
					    
						fileContents.Add(line);
					}
				}
				sr.Close();
			}
			else {
			    String msg = "File not found.  " + 
			        "path=[" + path + "]" +
			        "file=[" + file + "]";
			    throw new FileNotFoundException (msg);
			}
			
			return fileContents;
        }

    }
    
}			
