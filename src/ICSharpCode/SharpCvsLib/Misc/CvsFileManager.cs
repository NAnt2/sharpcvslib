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
		    get {return Path.DirectorySeparatorChar + "CVS";}
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
        public void AddEntry (String path, String entry) {
            this.AppendToFile (path, this.ENTRIES, entry);
            this.AppendToFile (path, this.ENTRIES_LOG, entry);
        }
        
        /// <summary>
        ///     Add the cvs line entry to the <code>Entries</code> file
        ///         in the cvs directory under the path specified.
        /// </summary>
        /// <param name="path">The current path where the file exists.</param>
        /// <param name="entry">An object that represents the cvs entry.</param>
        public void AddEntry (String path, Entry entry) {
            this.AppendToFile (path, this.ENTRIES, entry.FormattedEntry);
            this.AppendToFile (path, this.ENTRIES_LOG, entry.FormattedEntry);
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
            this.AppendToFile (cvsPath, this.ENTRIES, entry.FormattedEntry);
            this.AppendToFile (cvsPath, this.ENTRIES_LOG, entry.FormattedEntry);
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
        ///     Add all of the directory entries to the <code>Entries</code>
        ///         file in the cvs folder.
        /// </summary>
        /// <param name="localPath">The local path that starts the spider.</param>
        public void AddDirectoryEntries (String localPath) {
            foreach (String dir in Directory.GetDirectories (localPath)) {
                this.AddDirectoryEntry (dir, dir.Substring (localPath.Length + 1));
                if (dir.IndexOf ("CVS") < 0) {
                    this.AddDirectoryEntries (dir);
                }
            }
        }
        /// <summary>
        ///     Add a directory entry to the <code>Entries</code> file
        ///         in the cvs directory under the path specified.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="directoryName">The name of the directory to add.</param>
        public void AddDirectoryEntry (String path, String directoryName) {
            string _entry = "D/" + directoryName + "////";
            this.AppendToFile (path, this.ENTRIES, _entry);
            this.AppendToFile (path, this.ENTRIES_LOG, _entry);
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
            this.OverwriteFile (cvsPath, this.ROOT, fileEntry);
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
            this.OverwriteFile (cvsPath, this.REPOSITORY, fileEntry);
        }
                
        private void AppendToFile (String path, String file, String text) {
            this.WriteToFile (path, file, text, true);
        }
        
        private void OverwriteFile (String path, String file, String text) {
            this.WriteToFile (path, file, text, false);
        }
        
        private void WriteToFile (String path, String file, String text, bool append) {
            if (path.LastIndexOf (Path.DirectorySeparatorChar) != path.Length) {
                path = path + Path.DirectorySeparatorChar;
            }
            this.CreateDirectory (path);
            text = text.Replace ("\\", "/");
            
            if (LOGGER.IsDebugEnabled) {
                String msg = "Writing to a cvs file.  " +
                    "path=[" + path + "]" +
                    "file=[" + file + "]" +
                    "append=[" + append + "]" +
                    "text=[" + text + "]";
                LOGGER.Debug (msg);
            }

            String _fileText;            
            if (append) {
                if (File.Exists (path)) {
                    StreamReader sr = new StreamReader (path + file);
                    _fileText = sr.ReadToEnd () + text + "\n";
                }
                else {
                    _fileText = text;
                }
            } else {
                _fileText = text;
            }
			StreamWriter sw = 
			    new StreamWriter(path + file, append, Encoding.ASCII);                
            sw.WriteLine (_fileText);    
            
            
			sw.Close();
        }
        
        private void CreateDirectory (String path) {
			if (!Directory.Exists(path + this.CVS)) {
				Directory.CreateDirectory(path + this.CVS);
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
        public Entry [] ReadEntries (String path) {
            ArrayList entries = new ArrayList ();
            ArrayList entryStrings = new ArrayList ();
            
            entryStrings.Add (this.ReadFromFile (path, this.ENTRIES));
            
            foreach (String entryString in entryStrings) {
                entries.Add (new Entry (entryString));
            }
            
            return (Entry[])entries.ToArray (typeof (Entry));
        }
        
        private ICollection ReadFromFile (String path, String file) {
            ArrayList fileContents = new ArrayList ();
            string filePath = path + file;
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
			
			return fileContents;
        }
    }
}			
