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
            Entries,
            /// <summary>Used to specify specific repository revisions or
            /// sticky tags.</summary>
            Tag
        }
        /// <summary>
        ///     Constructor.
        /// </summary>
        public Factory () {

        }

        /// <summary>
        /// Create a cvs management object for the given path.  The path specified
        ///     should be the folder above the cvs directory.  The name of the file
        ///     and full path is then derived from the cvs line in the case of an
        ///     Entries line, or in the case of a single line cvs management file
        ///     (i.e. Root, Repository, etc.) the object being managed is the
        ///     entire directory.
        /// </summary>
        /// <param name="path">The path to the folder above the cvs directory.</param>
        /// <param name="fileName">The name of the cvs file that is being modified/
        ///     created.</param>
        /// <param name="line">The line to add to the file.</param>
        /// <returns>A new cvs file that contains properties for the different
        ///     elements in the line.</returns>
        /// <exception cref="UnsupportedFileTypeException">If the cvs filetype specified
        ///     is unknown.</exception>
        /// <example>
        ///     The following will produce an entries file 
        ///         (directory seperator character may be different):
        ///         
        ///         path            = c:/dev/sharpcvslib
        ///         fileName        = Entries
        ///         line            = /SharpCvsLib.build/1.1///
        ///         
        ///     With the following information:
        ///         FileContents    = /SharpCvsLib.build/1.1///
        ///         FileName        = Entries
        ///         FullPath        = c:/dev/sharpcvslib/SharpCvsLib.build
        ///         IsMultiLined    = true
        ///         Path            = c:/dev/sharpcvslib/
        ///         
        ///     NOTE:
        ///     <ul>
        ///         <li>The path seperator may face the other way</li>
        ///         <li>There will be an ending path seperator after every directory,
        ///             as in the path.</li>
        ///     </ul>
        /// </example>
        public ICvsFile CreateCvsObject (String path, String fileName, String line) {
            FileType fileType = this.GetFileType(fileName);
            return this.CreateCvsObject(path, fileType, line);
        }

        /// <summary>
        /// Create the cvs file based on the filename.  Returns the
        ///     cvs file interface.
        /// </summary>
        public ICvsFile CreateCvsObject (String path,
                                        FileType fileType,
                                        String line) {
            ICvsFile entry;
            switch (fileType) {
                case (FileType.Entries): {
                    entry = new Entry(path, line);
                    break;
                }
                case (FileType.Repository):{
                    entry = new Repository (path, line);
                    break;
                }
                case (FileType.Root):{
                    entry = new Root (path, line);
                    break;
                }
                case (FileType.Tag):{
                    entry = new Tag (path, line);
                    break;
                }
                default:{
                    StringBuilder msg = new StringBuilder();
                    msg.Append("Unknown file type specified.");
                    msg.Append("fileType=[").Append(fileType.ToString()).Append("]");
                    throw new UnsupportedFileTypeException (msg.ToString());
                }

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
                case (FileType.Entries):{
                    return Entry.FILE_NAME;
                }
                case (FileType.Repository):{
                    return Repository.FILE_NAME;
                }
                case (FileType.Root):{
                    return Root.FILE_NAME;
                }
                case (FileType.Tag):{
                    return Tag.FILE_NAME;
                }
                default:{
                    StringBuilder msg = new StringBuilder();
                    msg.Append("Unknown file type specified.");
                    msg.Append("fileType=[").Append(fileType.ToString()).Append("]");
                    throw new UnsupportedFileTypeException (msg.ToString());
                }
            }
        }

        /// <summary>
        /// Derive the file type from the name of the cvs file.
        /// </summary>
        /// <param name="name">The name of the cvs file.</param>
        /// <returns>The type of the file.</returns>
        public FileType GetFileType (String name) {
            switch (name) {
                case (Entry.FILE_NAME): {
                    return FileType.Entries;
                }
                case (Repository.FILE_NAME): {
                    return FileType.Repository;
                }
                case (Root.FILE_NAME): {
                    return FileType.Root;
                }
                case (Tag.FILE_NAME): {
                    return FileType.Tag;
                }
                default: {
                    StringBuilder msg = new StringBuilder();
                    msg.Append("Unknown file type specified.");
                    msg.Append("name=[").Append(name).Append("]");
                    throw new UnsupportedFileTypeException (msg.ToString());
                }
            }
        }
    }
}
