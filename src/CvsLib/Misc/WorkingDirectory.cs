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
        /// Folders.
        ///     TODO: Figure out what this is for.
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
        /// <param name="cvsroot"></param>
        /// <param name="localdirectory"></param>
        /// <param name="repositoryname"></param>
		public WorkingDirectory(CvsRoot cvsroot, string localdirectory,  string repositoryname)
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
        /// TODO: Figure out what this is doing.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
		public string ToRemotePath(string directory)
		{
			return directory.Substring(localdirectory.Length).Replace("\\", "/");
		}
		
        /// <summary>
        /// TODO: Figure out what this is doing.
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
		public string ToLocalPath(string directory)
		{
			return localdirectory + directory.Substring(cvsroot.CvsRepository.Length).Replace("/", "\\");
		}
		
		private void CreateFilesIn(string path, string repository, ArrayList entries)
		{
			if (!Directory.Exists(path + "\\CVS")) {
				Directory.CreateDirectory(path + "\\CVS");
			}
			
			StreamWriter sw = new StreamWriter(path + "\\CVS\\Repository", false, Encoding.ASCII);
			sw.Write(repository.Substring(cvsroot.CvsRepository.Length + 1));
			sw.Close();
			
			sw = new StreamWriter(path + "\\CVS\\Root", false, Encoding.ASCII);
			sw.Write(cvsroot.ToString());
			sw.Close();
			
			sw = new StreamWriter(path + "\\CVS\\Entries", false, Encoding.ASCII);
			
			bool created = false;
			if (entries != null && entries.Count > 0) {
				foreach (Entry entry in entries) {
					sw.WriteLine(entry.ToString());
				}
				sw.WriteLine("D");
				sw.Close();
				created = true;
				sw = new StreamWriter(path + "\\CVS\\Entries.Log", false, Encoding.ASCII);
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
				File.Delete(path + "\\CVS\\Entries.Log");
			}
		}
		
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
        /// Add the entries in the specified directory.
        /// </summary>
        /// <param name="directory"></param>
		public void AddEntriesIn(string directory)
		{
			Entry[] entries = Entry.RetrieveEntries(directory);
			if (entries != null && entries.Length > 0) {
				string cvsdir    = ToRemotePath(directory);
				if (File.Exists(directory + "\\CVS\\Repository")) {
					StreamReader sr = File.OpenText(directory + "\\CVS\\Repository");
					string line = sr.ReadLine();
					if (line != null && line.Length > 0) {
						cvsdir = line + "/" + cvsdir;
					}
					sr.Close();
				}
				foreach (Entry entry in entries) {
					if (entry.IsDirectory) {
						AddEntriesIn(directory + Path.DirectorySeparatorChar + entry.Name);
					}
					AddEntry(cvsdir, entry);
				}
			}
		}
		
        /// <summary>
        /// Add all files in the specified directory.
        /// </summary>
        /// <param name="directory"></param>
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
			AddEntriesIn(localdirectory);
		}
		
        /// <summary>
        /// Create the cvs folder and the files used by cvs to track the
        ///     sources locally.
        /// </summary>
		public void CreateCVSFiles()
		{
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
						if (created[localdir] == null) {
							CreateFilesIn(localdir, r, null);
							created[localdir] = true;
						}
					} else 
						break;
				}
			}
		}
	}
}
