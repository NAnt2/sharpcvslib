#region "Copyright"
// WorkingDirectory.cs 
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
//                Clayton Harbour
#endregion

using System;
using System.Collections;
using System.Text;
using System.IO;

using log4net;

using ICSharpCode.SharpCvsLib.FileSystem;

namespace ICSharpCode.SharpCvsLib.Misc { 
		
    /// <summary>
    /// Encapsulates the information for the local working directory and
    ///     the cvs server module name.  Also contains the <code>CvsRoot</code>
    ///     object required to connect to the server.  The only thing you 
    ///     have to add for this is the password.
    /// </summary>
	public class WorkingDirectory
	{
	    private Manager manager = new Manager ();
	    private readonly ILog LOGGER = 
	        LogManager.GetLogger (typeof (WorkingDirectory));
		private CvsRoot cvsroot;
		private string  localdirectory;
		private string  repositoryname;
	    private String revision;
		
		private Hashtable folders = new Hashtable();
	    
	    private Folder[] foldersToUpdate;
		
        /// <summary>
        /// The name of the module.
        /// </summary>
		public string ModuleName {
			get {
				return repositoryname;
			}
			set {
				repositoryname = value;
			}
		}		    
		
        /// <summary>
        /// The name of the working directory.  
        ///     TODO: Figure out if this should be the repository name
        ///         or if it would be better to allow this to be overridden.
        /// </summary>
		public string WorkingDirectoryName {
			get {
				return repositoryname;
			}
			set {
				repositoryname = value;
			}
		}
		
        /// <summary>
        /// A list of the cvs folders on the local host.
        /// </summary>
		public Hashtable Folders {
			get {
				return folders;
			}
		}
		
		/// <summary>
		///     The cvs folders that are to be updated/ manipulated when requests
		///         are sent to the server.
		/// </summary>
		public Folder[] FoldersToUpdate {
		    get {return this.foldersToUpdate;}
		    set {this.foldersToUpdate = value;}
		}
		
        /// <summary>
        /// The local directory to use for sources.
        /// </summary>
		public string LocalDirectory {
			get {
				return localdirectory;
			}
			set {
				localdirectory = value;
			}
		}
		
        /// <summary>
        /// Object encapsulating information to connect to a cvs server.
        /// </summary>
		public CvsRoot CvsRoot {
			get {
				return cvsroot;
			}
			set {
				cvsroot = value;
			}
		}
		
		/// <summary>Used to specify the revision of the module
		/// requested.  This should correspond to a module tag.</summary>
		public String Revision {
		    get {
		        return this.revision;
		    }
		    set {
		        this.revision = value;
		    }
		}
		
		/// <summary>Determine if a revision has been specified.</summary>
		/// <returns><code>true</code> if a specific revision has been 
		/// specified and the <code>Revision</code> field is non-null;
		/// <code>false</code> otherwise.</returns>
		public bool HasRevision {
		    get {return String.Empty != this.Revision ||
		                null != this.Revision;}
		}

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="cvsroot">The cvs root string, contains information 
        ///     about the connection and path on the cvs server.</param>
        /// <param name="localdirectory">The local base directory to check the 
        ///     module out in.</param>
        /// <param name="repositoryname">The name of the repository.  This is
        ///     appended to the base localdirectory to check the sources out into.</param>
		public WorkingDirectory(    CvsRoot cvsroot, 
		                            string localdirectory,  
		                            string repositoryname)
		{
			this.repositoryname = repositoryname;
			this.cvsroot        = cvsroot;
			this.localdirectory = localdirectory;
		}
		
        /// <summary>
        /// Clear folders collection.
        /// </summary>
		public void Clear()
		{
			folders = new Hashtable();
		}
		
        /// <summary>
        /// Add a new entry to the folders collection.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="entry"></param>
		public void AddEntry(string folder, Entry entry)
		{
			if (folders[folder] == null) {
				folders[folder] = new Folder();
			}
			((Folder)folders[folder]).Entries.Add(entry);
		}
		
        /// <summary>
        /// Converting the local directory string to a remote/ *nix 
        ///     string.
        /// </summary>
        /// <param name="directory">The directory path.</param>
        /// <returns></returns>
		public string ToRemotePath(string directory) {
			return directory.Substring(
			                           localdirectory.Length).Replace(Path.DirectorySeparatorChar, '/');
		}
		
        /// <summary>
        /// Convert the directory name to a win32 directory name
        ///     with the appropriate slashes.
        /// 
        /// TODO: Clean up this dirty bad boy and move the functionality
        ///     to the Manager
        /// </summary>
        /// <param name="orgPath">The directory path.</param>
        /// <returns></returns>
        [Obsolete ("Use the OrgPath to parse the org path string")]
		public string ToLocalPath(string orgPath) {
		    string _localBasePath = this.localdirectory;
		    
		    string _orgPathWithoutRoot =
		        orgPath.Substring (this.cvsroot.CvsRepository.Length + 1);
		    
		    String [] splitOrgPath = orgPath.Split ('/');
		    String filename = splitOrgPath[splitOrgPath.Length - 1];
		    string _orgPathWithoutFilename =
		        _orgPathWithoutRoot.Replace (filename, "");
		    string _localPath = 
		        Path.Combine (_localBasePath, _orgPathWithoutFilename);
		    
		    _localPath = 
		        _localPath.Replace ('/', Path.DirectorySeparatorChar);
		    if (LOGGER.IsDebugEnabled) {
                String msg = "Converting server path and filename to local path and filename.  " +
                    "_localBasePath=[" + _localBasePath + "]" +
                    "_orgPathWithoutRoot=[" + _orgPathWithoutRoot + "]" +
                    "_orgPathWithoutFilename=[" + _orgPathWithoutFilename + "]" +
                    "localPath=[" + _localPath + "]";
		        LOGGER.Debug (msg);
		    }
		        
			return _localPath;
		}
				
		/// <summary>
		/// Uses the assumption that ASCII 0 or ASCII 255 are only
		///     found in non-text files to determine if the file
		///     is a binary or a text file.
		/// Also the assumption is made that a non-text file will
		///     have an ASCII 0 or ASCII 255 character.
		/// </summary>
//        [Obsolete ("This is moving to the CvsFileManager class")]
		private bool IsBinary(string filename)
		{
			FileStream fs = File.OpenRead(filename);
			
			byte[] content = new byte[fs.Length];
			
			fs.Read(content, 0, (int)fs.Length);
			fs.Close();
			
			// assume that ascii 0 or 
			// ascii 255 are only found in non text files.
			// and that all non text files contain 0 and 255
			foreach (byte b in content) {
				if (b == 0 || b == 255)
					return true;
			}
			
			return false;
		}

        /// <summary>
        /// Recurses through all child directories starting with
        ///     base directory.  Add all the cvs entries found
        ///     to the folder collection.
        /// </summary>
        /// <param name="directory">The name of the directory.</param>
//        [Obsolete ("This is moving to the CvsFileManager class")]
		public void AddEntriesIn(string directory)
		{
		    if (LOGGER.IsDebugEnabled) {
		        String msg = "Adding cvs entries to request updates.  " +
		            "directory=[" + directory + "]";
		        LOGGER.Debug (msg);
		    }
		    ArrayList entryCollection = 
		        new ArrayList (this.manager.Fetch (directory, Factory.FileType.Entries));
			Entry[] entries = (Entry[])entryCollection.ToArray (typeof (Entry));
		    // TODO: Remove this line -- Entry.RetrieveEntries(directory);
			if (entries != null && entries.Length > 0) {
				string cvsdir    = ToRemotePath(directory);
				if (File.Exists(Path.Combine (directory, Repository.FILE_NAME))) {
					StreamReader sr = 
					    File.OpenText(Path.Combine (directory, Repository.FILE_NAME));
					string line = sr.ReadLine();
					if (line != null && line.Length > 0) {
					    // TODO: Figure out what to do with this path seperator
						cvsdir = "/" + line; // + "/" + cvsdir;
					}
					sr.Close();
				}
				foreach (Entry entry in entries) {
					if (entry.IsDirectory && null != entry.Name) {
						AddEntriesIn(Path.Combine (directory, entry.Name));
					}
					
					if (LOGGER.IsDebugEnabled) {
					    String msg = "Adding entry.  " +
					        ";  cvsdir=[" + cvsdir + "]" +
					        ";  entry=[" + entry + "]";
					    LOGGER.Debug (msg);
					}
					AddEntry(cvsdir, entry);
				}
			}
		}
						
        /// <summary>
        /// Read all the existing entries.
        /// </summary>
//        [Obsolete ("This is moving to the CvsFileManager class")]
		public void ReadAllExistingEntries()
		{
			Clear();
		    if (LOGGER.IsDebugEnabled) {
		        String msg = "Read all existing entries in the " +
		            "localdirectory=[" + this.localdirectory + "]";
		        LOGGER.Debug (msg);
		    }
		    string wd = 
		        Path.Combine (localdirectory, this.ModuleName);
			AddEntriesIn(wd);
//		    if (null == this.Folders || 0 == this.Folders.Count) {
//		        AddEntriesIn (Path.Combine (localdirectory, this.ModuleName));
//		    }
		}
		
		/// <summary>
		///     Set the collection of folders to the working directory.
		/// </summary>
		/// <param name="folders">The cvs folders in the working directory.</param>
		public void SetFolders (Folder[] folders) {
		    
		}
	}
}
