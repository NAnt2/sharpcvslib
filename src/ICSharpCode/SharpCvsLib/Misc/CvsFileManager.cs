#region "Copyright"
// WorkingDirectory.cs 
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
#endregion

using System;
using System.Collections;
using System.Text;
using System.IO;

using log4net;

namespace ICSharpCode.SharpCvsLib.Misc {
    
    /// <summary>
    ///     Manages the addition and creation of cvs files such as
    ///         - Entries
    ///         - Repository
    ///         - Root
    /// </summary>
    public class CvsFileManager {

        private readonly ILog LOGGER = 
            LogManager.GetLogger (typeof (CvsFileManager));
		/// <summary>The cvs directory information.</summary>
		public string CVS {
		    get {return "CVS";}
		}		    
		
		/// <summary>The cvs repository file information.</summary>	    
		public string REPOSITORY {
		    get {return this.CVS + Path.DirectorySeparatorChar + "Repository";}
		}
		
		/// <summary>The cvs entries file information.</summary>	    
		public string ENTRIES {
		    get {return this.CVS + Path.DirectorySeparatorChar + "Entries";}
		}
		
		/// <summary>
		/// The cvs entries log information, platform inspecific.
		///     TODO:I don't think this is used by cvs, I think this is just
		///     some sort of optimization or easy way to view the entries folder.
		/// </summary>
		public string ENTRIES_LOG {
		    get {return this.CVS + Path.DirectorySeparatorChar + "Entries" + 
		                ".log";}

		}
		
	    /// <summary>The cvs root file information.</summary>
	    public string ROOT {
	        get {return this.CVS + Path.DirectorySeparatorChar + "Root";}
	    }            

        /// <summary>Constructory</summary>        
        public CvsFileManager () {
        }

        /// <summary>
        ///     Add the file information to the <code>Entries</code>
        ///         file in the cvs directory under the path specified.
        /// </summary>
        /// <param name="path">The current path where the file exists.</param>
        /// <param name="entry">The cvs entry for the file being added locally.</param>
        private void AddEntry (String path, String entry) {            
            this.WriteToFile (path, this.ENTRIES, entry, true);
        }
        
        /// <summary>
        ///     Add the cvs line entry to the <code>Entries</code> file
        ///         in the cvs directory under the path specified.
        /// </summary>
        /// <param name="path">The current path where the file exists.</param>
        /// <param name="entry">An object that represents the cvs entry.</param>
        public void AddEntry (String path, Entry entry) {
            this.AddEntry (path, entry.CvsEntry);
        }
        
        /// <summary>
        ///     Add the cvs line entry to the <code>Entries</code> file
        ///         in the cvs directory under the path specified.
        /// </summary>
        /// <param name="localBase">The local base/ working path.</param>
        /// <param name="localPath">The local path identified by the cvs repository response.</param>
        /// <param name="entry">The cvs entry to add to the entries file.</param>
        public void AddEntry (String localBase, String localPath, Entry entry) {
            string cvsPath = Path.Combine (localBase, localPath);
            this.AddEntry (cvsPath, entry.CvsEntry);
        }
        
        /// <summary>
        ///     Add a cvs entry to the log entry file.
        /// </summary>
        public void AddLogEntry (String path, Entry entry) {
            this.WriteToFile (path, this.ENTRIES_LOG, entry.CvsEntry, true);
        }
        
        /// <summary>
        ///     Remove the log entry from the log file.
        /// </summary>
        public void RemoveLogEntry (String path, Entry entry) {
            this.RemoveFromFile (path, this.ENTRIES_LOG, entry.CvsEntry);
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
            this.WriteToFile (path, file, newFileLines);
        }
        
        /// <summary>
        ///     Add all of the cvs entries to the <code>Entries</code> file
        ///         in the cvs directory under the path specified.
        /// </summary>
        public void AddEntries (String path, Entry[] entries) {
            foreach (Entry entry in entries) {
                this.AddEntry (path, entry);
            }
        }
                
        /// <summary>
        ///     Add the <code>Root</code> file to the cvs directory.
        /// </summary>
        /// <param name="path">The path to the directory where the cvs 
        ///     directory is located or will be added.</param>
        /// <param name="localBase">The local base/ working path.</param>
        /// <param name="localPath">The local path identified by the cvs repository response.</param>
        /// <param name="fileEntry">The cvs root entry to add to the root file.</param>
        public void AddRoot (String localBase, String localPath, String fileEntry) {
            string cvsPath = Path.Combine (localBase, localPath);
            this.WriteToFile (cvsPath, this.ROOT, fileEntry, false);
        }
        
        /// <summary>
        ///     Add the <code>Repository</code> file to cvs directory.
        /// </summary>
        /// <param name="localBase">The path to the local working directory.</param>
        /// <param name="localPath">The local relative path, usually identified by the
        ///     cvs server.</param>
        /// <param name="fileEntry">The string to add to the repository file.</param>
        /// <param name="repository">The repository text.</param>
        public void AddRepository (String localBase, String localPath, String fileEntry) {
            string cvsPath = Path.Combine (localBase, localPath);
            this.WriteToFile (cvsPath, this.REPOSITORY, fileEntry, false);
        }
        
        private bool IsEntry (String file) {
            if (file.Equals (this.ENTRIES)) {
                return true;
            }
            return false;
        }
        
        private bool IsEntryLog (String file) {
            if (file.Equals (this.ENTRIES_LOG)) {
                return true;
            }
            return false;
        }        
        
        /// <summary>
        ///     Adds a collection of lines to the cvs file.  The first
        ///         entry overwrites any file currently in the directory
        ///         and all other following entries are appended to the
        ///         file.
        /// </summary>
        /// <param name="path">The current working directory.</param>
        /// <param name="file">The cvs file to write to.</param>
        /// <param name="lines">A collection of lines to add to the cvs file.</param>
        private void WriteToFile (    String path,
                                      String file, 
                                      ICollection lines) {
            bool overWriteFile = true;
            foreach (String line in lines) {
                this.WriteToFile (path, file, line, overWriteFile);
                if (overWriteFile) {
                    overWriteFile = false;
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
            string fileAndPath = Path.Combine (path, file);
            this.CreateDirectory (path);
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
        
        private void CreateDirectory (String path) {
			if (!Directory.Exists(path + this.CVS)) {
			    string cvsDir = 
			        Path.Combine (path, this.CVS);
				Directory.CreateDirectory(cvsDir);
			}            
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
        ///     Reads all the entries in the <code>CVS\Entries</code>
        ///         file and returns the 
        /// </summary>
        /// <param name="path">The path to look for cvs entries.</param>
        /// <returns>An array of entries if found, or an empty array if no 
        ///     entries were found.</returns>
        public ICollection ReadEntries (String path) {
            ArrayList entries = new ArrayList ();
            ICollection entryStrings;
            
            entryStrings = this.ReadFromFile (path, this.ENTRIES);

            foreach (String entryString in entryStrings) {
                Entry entry = new Entry (entryString);
                if (LOGGER.IsDebugEnabled) {
                    String msg = "Adding entry to entries collection.  " +
                        "entry=[" + entry + "]";
                }
                entries.Add (new Entry (entryString));
            }    
            if (entries.Count > 0) {
                this.SyncEntriesWithLog (path, entries);
            }
            
            return entries;
        }
        
        /// <summary>
        ///     Read the entries from the entries log and the instruction
        ///         appended to the log (i.e. A to add an entry, R to remove, etc.)
        ///         and apply that to the main entries file.
        /// </summary>
        /// <param name="entries">The collection of entries in the main entries
        ///     file.</param>
        private void SyncEntriesWithLog (String path, ArrayList entries) {
            ICollection entryLogStrings =
                this.ReadFromFile (path, this.ENTRIES_LOG);
            
            foreach (String entryLogString in entryLogStrings) {
                if (entryLogString.Length > 1) {
                    switch (entryLogString[0]) {
                        // Add file
                        case 'A': {   
    						entries.Add(new Entry(entryLogString.Substring(2)));
    						break;
                        }
                        // Remove file
                        case 'R': {
    						Entry removeMe = new Entry(entryLogString.Substring(2));
    						Entry removeObject = null;
    						
    						foreach (Entry entry in entries) {
    							if (removeMe.CvsEntry.Equals (entry.CvsEntry)) {
    								removeObject = entry;
    								break;
    							}
    						}
    						if (removeObject != null) {
    							entries.Remove(removeObject);
    						}
    						break;
    
                        }
                        // Should not be here.
                        default: {
                            String msg = "In the default method of the entry " +
                                "log parsing.  EntryString=[" + entryLogString + "]";
                            LOGGER.Warn (msg);
                            break;
                        }
                    }
                }
            }

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
            string filePath = 
                Path.Combine (path, file);
			if (File.Exists(filePath)) {
				StreamReader sr = File.OpenText(filePath);
				
				while (true) {
					string line = sr.ReadLine();
					if (line == null) {
						break;
					}
					if (line.Length > 1) {					    
						fileContents.Add(line);
					}
				}
				sr.Close();
			}
			else {
			    // TODO: Figure out if I should throw an exception here
			    //    or if this is normal...
			    if (LOGGER.IsDebugEnabled) {
			        String msg = "Unable to find cvs file." +
			            "filePath=[" + filePath + "]";
			        LOGGER.Debug (msg);
			    }
			}
			
			return fileContents;
        }
    }
}			
