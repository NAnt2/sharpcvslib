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
#endregion

using System;
using System.Collections;
using System.Text;
using System.IO;

using log4net;

namespace ICSharpCode.SharpCvsLib.Misc { 
		
    /// <summary>
    /// Encapsulates the information for the local working directory and
    ///     the cvs server module name.  Also contains the <code>CvsRoot</code>
    ///     object required to connect to the server.  The only thing you 
    ///     have to add for this is the password.
    /// </summary>
	public class WorkingDirectory
	{
	    private readonly ILog LOGGER = 
	        LogManager.GetLogger (typeof (WorkingDirectory));
		private CvsRoot cvsroot;
		private string  localdirectory;
		private string  repositoryname;
		
		private Hashtable folders = new Hashtable();
		
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
		
		/// <summary>The cvs directory information.</summary>
		public readonly string CVS =
		    /*Path.DirectorySeparatorChar +*/ "CVS";
		/// <summary>The cvs repository file information.</summary>	    
		public readonly string REPOSITORY = 
		    Path.DirectorySeparatorChar + "CVS" + 
		    Path.DirectorySeparatorChar + "Repository";
		/// <summary>The cvs entries file information.</summary>	    
	    public readonly string ENTRIES =
	        Path.DirectorySeparatorChar + "CVS" + 
	        Path.DirectorySeparatorChar + "Entries";
	    
		/// <summary>
		/// The cvs entries log information, platform inspecific.
		///     TODO:I don't think this is used by cvs, I think this is just
		///     some sort of optimization or easy way to view the entries folder.
		/// </summary>
	    public readonly string ENTRIES_LOG =
	        Path.DirectorySeparatorChar + "CVS" +
	        Path.DirectorySeparatorChar + "Entries" + 
	        ".log";
	    /// <summary>The cvs root file information.</summary>
	    public readonly string ROOT =
            Path.DirectorySeparatorChar + "CVS" +
            Path.DirectorySeparatorChar + "Root";
		    
		
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
		public string ToRemotePath(string directory)
		{
			return directory.Substring(
			                           localdirectory.Length).Replace(Path.DirectorySeparatorChar, '/');
		}
		
        /// <summary>
        /// Convert the directory name to a win32 directory name
        ///     with the appropriate slashes.
        /// </summary>
        /// <param name="directory">The directory path.</param>
        /// <returns></returns>
		public string ToLocalPath(string directory)
		{
		    string _localBasePath = this.localdirectory;
		    string _localModulePath = this.ModuleName;
		    
		    string _serverWithModuleName = 
		        directory.Substring (this.cvsroot.CvsRepository.Length);
		    string _serverModulePath = this.ModuleName;
		    
		    string _serverBasePath =
		        _serverWithModuleName.Replace (_serverModulePath + "/", "");
	    
		    string localPathAndFileName = 
		        _localBasePath + Path.DirectorySeparatorChar +
		        _localModulePath + _serverBasePath;
		    
		    localPathAndFileName = 
		        localPathAndFileName.Replace ('/', Path.DirectorySeparatorChar);
		    if (LOGGER.IsDebugEnabled) {
                String msg = "Converting server path and filename to local path and filename.  " +
                    "_localBasePath=[" + _localBasePath + "]" +
                    "_localModulePath=[" + _localModulePath + "]" +
                    "_serverBasePath=[" + _serverBasePath + "]" +
                    "_serverModulePath=[" + _serverModulePath + "]" +
                    "localPathAndFileName=[" + localPathAndFileName + "]";
		        LOGGER.Debug (msg);
		    }
		        
			return localPathAndFileName;
		}
				
		private void CreateFilesIn(string path, string repository, ArrayList entries)
		{
			if (!Directory.Exists(path + CVS)) {
				Directory.CreateDirectory(path + CVS);
			}
			
			StreamWriter sw = 
			    new StreamWriter(path + REPOSITORY, false, Encoding.ASCII);
			sw.Write(repository.Substring(cvsroot.CvsRepository.Length + 1));
			sw.Close();
			
			sw = new StreamWriter(path + ROOT, false, Encoding.ASCII);
			sw.Write(cvsroot.ToString());
			sw.Close();
			
			sw = new StreamWriter(path + ENTRIES, false, Encoding.ASCII);
			
			bool created = false;
			if (entries != null && entries.Count > 0) {
				foreach (Entry entry in entries) {
					sw.WriteLine(entry.ToString());
				}
				sw.WriteLine("D");
				sw.Close();
				created = true;
				sw = new StreamWriter(path + ENTRIES_LOG, false, Encoding.ASCII);
			}
			
			bool empty = true;
			Hashtable taken = new Hashtable();
			foreach (DictionaryEntry entry2 in folders) {
				string s1 = entry2.Key.ToString();
				if (s1.StartsWith(repository) && s1.Length != repository.Length) {
					Entry e = new Entry();
					e.IsDirectory = true;
					e.Name  = s1.Substring(repository.Length + 1);
										
					if (e.Name.IndexOf('/') >= 0)
						e.Name = e.Name.Substring(0, e.Name.IndexOf('/'));
					
					if (taken[e.ToString()] == null) {
						if (created)
							sw.WriteLine("A " + e.ToString());
						else
							sw.WriteLine(e.ToString());
						taken[e.ToString()] = true;
					}
					empty = false;
				}
			}
			sw.Close();
			if (empty && created) {
				File.Delete(path + ENTRIES_LOG);
			}
		}
		
		/// <summary>
		/// Uses the assumption that ASCII 0 or ASCII 255 are only
		///     found in non-text files to determine if the file
		///     is a binary or a text file.
		/// Also the assumption is made that a non-text file will
		///     have an ASCII 0 or ASCII 255 character.
		/// </summary>
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
		public void AddEntriesIn(string directory)
		{
		    if (LOGGER.IsDebugEnabled) {
		        String msg = "Adding cvs entries to request updates.  " +
		            "directory=[" + directory + "]";
		        LOGGER.Debug (msg);
		    }
			Entry[] entries = Entry.RetrieveEntries(directory);
			if (entries != null && entries.Length > 0) {
				string cvsdir    = ToRemotePath(directory);
				if (File.Exists(Path.Combine (directory, REPOSITORY))) {
					StreamReader sr = 
					    File.OpenText(Path.Combine (directory, REPOSITORY));
					string line = sr.ReadLine();
					if (line != null && line.Length > 0) {
					    // TODO: Figure out what to do with this path seperator
						cvsdir = "/" + line; // + "/" + cvsdir;
					}
					sr.Close();
				}
				foreach (Entry entry in entries) {
					if (entry.IsDirectory) {
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
        /// Add all files in the specified directory.
        /// </summary>
        /// <param name="directory">The directory to search in.</param>
		public void AddAllFiles(string directory)
		{
			string[] directories = Directory.GetDirectories(directory);
			string[] files       = Directory.GetFiles(directory);
			
			string cvsdir        = ToRemotePath(directory);
			
			if (files != null) {
				foreach (string file in files) {
					string dir = Path.GetDirectoryName(file);
					Entry entry = new Entry();
					entry.Name = Path.GetFileName(file);
					entry.IsBinaryFile = IsBinary(file);
					AddEntry(cvsdir, entry);
					
//					Entry entry = null;
//					
//					Entry[] entries = Entry.RetrieveEntries(dir);
//					foreach (Entry entry2 in entries) {
//						if (entry2.Name == Path.GetFileName(file)) {
//							AddEntry(cvsdir, entry);
//						}
//					}
//					
//					if (entry == null) {
//					}
				}
			}
			
			foreach (string dir in directories) {
				if (Path.GetFileName(dir) != "CVS") {
					AddAllFiles(dir);
				}
			}
		}
		
		/// <summary>
		/// Reads the whole repository with all sub directories and
		/// creates new entries for all files.
		/// (used for the import command).
		/// </summary>
		public void CreateNewEntries()
		{
			Clear();
		    if (LOGGER.IsDebugEnabled){
			    LOGGER.Debug(localdirectory);
		    }
			AddAllFiles(localdirectory);
		}
		
        /// <summary>
        /// Read all the existing entries.
        /// </summary>
		public void ReadAllExistingEntries()
		{
			Clear();
		    if (LOGGER.IsDebugEnabled) {
		        String msg = "Read all existing entries in the " +
		            "localdirectory=[" + this.localdirectory + "]";
		        LOGGER.Debug (msg);
		    }
			AddEntriesIn(localdirectory);
		    if (null == this.Folders || 0 == this.Folders.Count) {
		        AddEntriesIn (Path.Combine (localdirectory, this.ModuleName));
		    }
		}
		
        /// <summary>
        /// Create the cvs folder and the files used by cvs to track the
        ///     sources locally.
        /// </summary>
		public void CreateCVSFiles()
		{
		    if (LOGGER.IsDebugEnabled) {
		        String msg = "Creating cvs files.  " +
		            "workingDirectory=[" + this + "]";
		        LOGGER.Debug (msg);
		    }
			Hashtable created = new Hashtable();
			foreach (DictionaryEntry entry in folders) {
				Folder folder = (Folder)entry.Value;
				string r      = entry.Key.ToString();
				
				string localdir = ToLocalPath(r);
				
				CreateFilesIn(localdir, r, folder.Entries);
				created[localdir] = true;
			}
			
			foreach (DictionaryEntry entry in folders) {
				string r = entry.Key.ToString();
				while (r.Length > 0) {
					r = r.Substring(0, r.LastIndexOf('/'));
					if (r.Length > cvsroot.CvsRepository.Length) {
						string localdir = ToLocalPath(r);
					    if (LOGGER.IsDebugEnabled) {
					        String msg = "Creating cvs files in " +
					            "local path=[" + localdir + "]" + 
					            "r=[" + r + "]";
					        LOGGER.Debug (msg);
					    }
					    
						if (created[localdir] == null) {
							CreateFilesIn(localdir, r, null);
							created[localdir] = true;
						}
					} else {
						break;
					}
				}
			}
		}
	}
}
