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
#endregion

using System;
using System.Collections;
using System.IO;

using log4net;

using ICSharpCode.SharpCvsLib.Attributes;

namespace ICSharpCode.SharpCvsLib.FileSystem
{
	/// <summary>
	/// The entries collection holds a collection of objects that represent cvs
	///     entries.  The key to the collection is the path to the file that the
	///     cvs entry represents on the file system.
	/// </summary>
    [Author("Clayton Harbour", "claytonharbour@sporadicism.com", "2003-2005")]
	public class Entries : DictionaryBase {
        private FileInfo _entriesPath;
        private ILog LOGGER = LogManager.GetLogger (typeof (Entries));

        public FileInfo EntriesPath {
            get { return this._entriesPath; }
            set { this._entriesPath = value; }
        }

        /// <summary>
        /// Create a new instance of the entries class.
        /// </summary>
		public Entries(DirectoryInfo cvsDir) : base() {
            if (!cvsDir.FullName.ToLower().EndsWith("cvs")) {
                cvsDir = new DirectoryInfo(Path.Combine(cvsDir.FullName, "CVS"));
            }
            this._entriesPath = new FileInfo(Path.Combine(cvsDir.FullName, "Entries"));
		}

        /// <summary>
        /// Set the entry to the given location.  The path to the entry on the filesystem
        ///     is the key for the entry.
        /// </summary>
        public Entry this[String fullPath] {
            get { return ((Entry)(Dictionary[fullPath])); }
            set { Dictionary[fullPath] = value; }
        }

        public void Add(Entry entry) {
            if (null == entry || null == entry.FullPath) {
                throw new ArgumentException("Entry must contain a path value.");
            }
            if (this.Contains(entry.FullPath)) {
                this[entry.FullPath] = entry;
            } else {
                Dictionary.Add(entry.FullPath, entry);
            }
        }

        /// <summary>
        /// Add a new entry to the entry collection.
        /// </summary>
        /// <param name="path">The path of the file the entry represents 
        ///     on the filesystem.</param>
        /// <param name="entry">The entry object to add to the collection.</param>
        public void Add(String path, Entry entry) {
            Dictionary.Add(path, entry);
        }

        /// <summary>
        /// Remove the given entry from the collection.
        /// </summary>
        /// <param name="path">The path of the file on the current filesystem
        ///     that the entry represents.</param>
        public void Remove(String path) {
            Dictionary.Remove(path);
        }

        /// <summary>
        /// Determines if the given collection contains an entry.
        /// </summary>
        /// <param name="path">The path to the file on the file system that the
        ///     entry represents.</param>
        /// <returns></returns>
        public bool Contains(String path) {
            return Dictionary.Contains(path);
        }

        /// <summary>
        /// Return the collection of values for the dictionary.
        /// </summary>
        public ICollection Values {
            get {return this.Dictionary.Values;}
        }

        public static Entries Load(DirectoryInfo cvsDir) {
            if (cvsDir.Name != "CVS") {
                cvsDir = new DirectoryInfo(
                    System.IO.Path.Combine(cvsDir.FullName, "CVS"));
            }
            return Load(new FileInfo(
                System.IO.Path.Combine(cvsDir.FullName, Entry.FILE_NAME)));
        }

        /// <summary>
        /// Load the given string.
        /// </summary>
        /// <param name="cvsFile">Path to the file being managed, this will
        /// load the corresponding Entry from the Entries file.</param>
        /// <returns></returns>
        public static Entries Load (FileInfo cvsFile) {
            Entries entries = new Entries(cvsFile.Directory);
            using (StreamReader reader = new StreamReader(cvsFile.FullName)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    entries.Add(new Entry(cvsFile, line));
                }
            }
            return entries;
        }

        public static void Save(Entry entry) {
            Entries entries = Entries.Load(entry.CvsFile.Directory);
            entries.Save();
        }

        public void Save() {
            if (null != this.Dictionary && this.Dictionary.Count > 0) {
                Entries currentEntries = Entries.Load(this._entriesPath);
                foreach(Entry currentEntry in currentEntries.Values) {
                    if (!this.Contains(currentEntry.FullPath)) {
                        this.Add(currentEntry);
                    }
                }
                if (!this.EntriesPath.Directory.Exists) {
                    this.EntriesPath.Directory.Create();
                }
                using (StreamWriter writer = new StreamWriter(this.EntriesPath.FullName)) {
                    foreach (Entry entryVal in this.Dictionary.Values) {
                        writer.WriteLine(entryVal.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Render the entries collection as a human readable string.
        /// </summary>
        /// <returns></returns>
        public override String ToString() {
            ICSharpCode.SharpCvsLib.Util.ToStringFormatter formatter = new
                ICSharpCode.SharpCvsLib.Util.ToStringFormatter("Entries");
            foreach (DictionaryEntry entry in Dictionary) {
                formatter.AddProperty("Entry key", entry.Key);
                formatter.AddProperty("Entry value", entry.Value);
            }
            return formatter.ToString();
        }
	}
}
