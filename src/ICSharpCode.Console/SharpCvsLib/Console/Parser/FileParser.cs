#region "Copyright"
//
// Copyright (C) 2005 Clayton Harbour
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
//    <author>Clayton Harbour</author>
#endregion

using System;
using System.Collections;
using System.IO;

using ICSharpCode.SharpCvsLib.FileSystem;

namespace ICSharpCode.SharpCvsLib.Console.Parser {
	/// <summary>
	/// Parses out file names from command line arguments and attempts to resovle the file path
	/// using the current working directory.  Constructs a collection of <see cref="FileInfo"/>
	/// objects.
	/// </summary>
	public class FileParser {
        private Hashtable _files = new Hashtable();
        private Folders _folders = new Folders();

        private string[] _args;

        public IList Files {
            get { return new ArrayList(this._files.Values); }
        }

        public Folders Folders {
            get { return this._folders; }
        }

		public FileParser(string[] args) {
            this._args = args;
            this.Parse(args);
            this.ConvertToFolders();
		}

        private void Parse(string[] args) {
            if (args != null && args.Length > 0) {
                foreach (string arg in args) {
                    if (!arg.StartsWith("-") && arg != string.Empty) {
                        string file = arg;
                        if (!Path.IsPathRooted(file)) {
                            file = Path.Combine(Directory.GetCurrentDirectory(), file);
                        }

                        if (Directory.Exists(file)) {
                            this.GetFilesInDir(new DirectoryInfo(file));
                        } else {
                            this._files.Add(file, new FileInfo(file));
                        }
                    }
                }
            }
        }

        private void GetFilesInDir(DirectoryInfo dir) {
            foreach (FileInfo file in dir.GetFiles()) {
                this._files.Add(file.FullName, file);
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories()) {
                this.GetFilesInDir(subDir);
            }
        }

        private void ConvertToFolders() {
            foreach (FileInfo file in this._files.Values) {
                Folder folder;
                if (this._folders.Contains(file.Directory.FullName)) {
                    folder = this._folders[file.Directory.FullName];
                } else {
                    folder = new Folder(file.Directory);
                    this._folders.Add(folder.Path.FullName, folder);
                }

                // try to load the entry from the CVS\Entries file
                Entry entry = Entry.Load(file);

                // if the file does not exist in the local cvs management folders, there
                // is a chance that it has not been brought down, let the server resolve it
                if (null == entry) {
                    entry = Entry.CreateEntry(file);
                }

                if (!folder.Entries.Contains(file.FullName)) {
                    folder.Entries.Add(file.FullName, entry);
                }
            }
        }
	}
}
