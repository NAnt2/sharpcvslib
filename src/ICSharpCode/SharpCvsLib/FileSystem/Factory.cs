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

namespace ICSharpCode.SharpCvsLib.FileSystem {
    
    /// <summary>
    ///     Creates the cvs object necessary based on the filename.
    /// </summary>
    public class Factory {
        
        /// <summary>
        ///     Type of cvs file.
        /// </summary>
        public enum FileType {
            /// <summary>
            ///     Root file type.
            /// </summary>
            Root,
            /// <summary>
            ///     Repository file type.
            /// </summary>
            Repository,
            /// <summary>
            ///     The entries file type.
            /// </summary>
            Entries
        }
        /// <summary>
        ///     Constructor.
        /// </summary>
        public Factory () {
            
        }
        
        /// <summary>
        ///     Create the cvs file based on the filename.  Returns the 
        ///         cvs file interface.
        /// </summary>
        public ICvsFile CreateCvsObject (String path, 
                                       FileType fileType, 
                                       String line) {
            ICvsFile entry;
            switch (fileType) {
                case (FileType.Entries):
                    entry = new Entry (path, line);
                    break;
                case (FileType.Repository):
                    entry = new Repository (path, line);
                    break;
                case (FileType.Root):
                    entry = new Root (path, line);
                    break;
                default:
                    String msg = "Unable to create object.";
                    throw new Exception (msg);
                    
            }
            return entry;
            
        }
        
        /// <summary>
        ///     Get the name of the file given the file type.
        /// </summary>
        /// <param name="fileType">The type of the file.</param>
        /// <returns>The name of the cvs file.</returns>
        public String GetFilename (FileType fileType) {
            switch (fileType) {
                case (FileType.Entries):
                    return Entry.FILE_NAME;
                case (FileType.Repository):
                    return Repository.FILE_NAME;
                case (FileType.Root):
                    return Root.FILE_NAME;
                default:
                    String msg = "Unable to create object.";
                    throw new Exception (msg);
                    
            }            
        }
    }
}
