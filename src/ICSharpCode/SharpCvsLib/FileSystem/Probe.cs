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
//    <author>Clayton Harbour</author>
//
#endregion

using System;
using System.Collections;
using System.IO;

namespace ICSharpCode.SharpCvsLib.FileSystem {
    /// <summary>
    /// Takes a single file or a collection of files and creates a new
    ///     list of files based on the following rules:
    ///         1) If a single file is specified then a check is performed to 
    ///             determine if the file exists.  
    ///                 a) If the file does not exist then the NonExistingFiles 
    ///                     collection is populated.
    ///                 b) If the file doe exist then the ExistingFile collection
    ///                     is populated.
    ///         2) If a collection of files is specified, WITHOUT a directory
    ///             then a non-recursive search is performed.  The ExistingFiles and
    ///             NonExistingFiles collections are populated.
    ///         3) If a collection of files is specified that contains a directory,
    ///             or a directory is specified then a recursive search is performed
    ///             to populate the ExistingFiles and NonExistingFiles collection.
    /// </summary>
    public class Probe {
        
        const String ALL = "*";
        ArrayList nonExistingFiles;
        ArrayList existingFiles;
        ICollection originalFiles;
        
        /// <summary>Files that do not exist on the filesystem.</summary>
        public ICollection NonExistingFiles {
            get {return this.nonExistingFiles;}
        }
        
        /// <summary>Filest that exist on the filesystem.</summary>
        public ICollection ExistingFiles {
            get {return this.existingFiles;}
        }
        
        /// <summary>Original list of files that will be sorted.</summary>
        public ICollection OriginalFiles {
            get {return this.originalFiles;}
            set {this.originalFiles = value;}
        }
        
        /// <summary>
        /// Initialize the existing and non-existing file collections.
        /// </summary>
        public Probe () {
            nonExistingFiles = new ArrayList ();
            existingFiles = new ArrayList ();
        }
        
        /// <summary>
        /// Begin searching the list of files and categorizing them into existing
        ///     or non-existing.
        /// </summary>
        /// <exception>IllegalArgumentException if the or</exception>
		public void Execute () {
		    if (this.originalFiles.Count < 1) {
		        // TODO: Create a custom exception.
		        throw new Exception ("No files to search.");
		    }
		    foreach (String file in this.originalFiles) {
		        if (Path.GetDirectoryName (file) == Path.GetFileName (file)) {
		            this.GetFiles (file);
		        } else {
		            SortFile (file);
		        }
		    }
		}

        /// <summary>
        /// Sort the file into <code>Existing</code> if the file exists on the 
        ///     client's filesystem; otherwise sort the file as 
        ///     <code>Non-Existing</code>.
        /// </summary>
        private void SortFile (String file) {
            if (File.Exists (file)) {
                this.existingFiles.Add (file);
            } else {
                this.nonExistingFiles.Add (file);
            }
        }
        
        /// <summary>
        /// Perform a recursive search through the current directory specified.
        ///     Sort all files into existing or non-existing categories.
        /// </summary>
        /// <param name="currentDirectory">A directory to begin this current
        ///     recursive level search.</param>
		private void GetFiles(String currentDirectory) {
			String[] files = Directory.GetFiles(currentDirectory, ALL);
		    foreach (String file in files) {
			    this.SortFile (file);
			}
			
			String[] directories = Directory.GetDirectories(currentDirectory);
		    foreach (String directory in directories) {
				GetFiles(directory);
			}
		}
    }
}
