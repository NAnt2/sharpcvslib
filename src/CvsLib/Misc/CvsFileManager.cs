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
        /// <param name="cvsroot">The cvs root text to add to the local cache.</param>
        public void AddRoot (String path, String cvsroot) {
            this.OverwriteFile (path, this.ROOT, cvsroot);
        }
        
        /// <summary>
        ///     Add the <code>Repository</code> file to cvs directory.
        /// </summary>
        /// <param name="path">The path to the directory level with cvs.</param>
        /// <param name="repository">The repository text.</param>
        public void AddRepository (String path, String repository) {
            this.OverwriteFile (path, this.REPOSITORY, repository);
        }
        
        /// <summary>
        ///     Add the <code>Repository</code> file to the cvs directory.
        /// </summary>
        /// <param name="path">The path to the directory level with cvs.</param>
        /// <param name="cvsroot">The cvsroot of the checkout.</param>
        /// <param name="orgPath">The path and filename of the current entry returned from the cvs server.</param>
        public void AddRepository (String path, String cvsroot, String orgPath) {
            String _repository = orgPath.Substring (cvsroot.Length + 1);
            _repository.Remove (_repository.LastIndexOf ('/'), _repository.Length);
            this.AddRepository (path, _repository);
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
        
    }
    
}			
