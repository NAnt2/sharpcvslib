#region Copyright
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
using System.IO;
using System.Globalization;

using log4net;

namespace ICSharpCode.SharpCvsLib.FileSystem {
    /// <summary>
    /// Information about the repository file.  This file is used to identify
    ///     the relative path (from the cvsroot) of the file in the cvs
    ///     repository.  Combined with the entry from the cvs entries file
    ///     this provides the relative path to the file on the cvs server.
    /// </summary>
    public class Repository : AbstractCvsFile, ICvsFile {
        private ILog LOGGER = LogManager.GetLogger (typeof (Repository));

        /// <summary>
        ///     The name of the repository file.
        /// </summary>
        public const String FILE_NAME = "Repository";

        /// <summary>
        ///     Create a new repository object taking the path to the
        ///         folder above the CVS directory and the line to enter
        ///         into the repository file.
        ///
        ///     The repository file stores the relative path to the directory
        ///         from the server's perspective.
        /// </summary>
        /// <param name="path">The path to the directory above the CVS directory.</param>
        /// <param name="line">The line to enter into the repository file.</param>
        public Repository (String path, String line) : base (path, line) {
        }

        /// <summary>
        ///     The name of this file should correspond to the name required
        ///         for a cvs repository.
        /// </summary>
        public String Filename {
            get {return Repository.FILE_NAME;}
        }

        /// <summary>
        /// Format the string as a repository entry.  Remove any trailing
        ///     slashes from the line.
        /// </summary>
        public override void Parse (String line) {
            if (line.EndsWith ("/")) {
                 line = line.Substring (0, line.Length - 1);
            }
            this.FileContents = line;
        }

        /// <summary>
        ///     Determines if the objects are equal based on the file contents
        ///
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        public override bool Equals (object obj) {
            if (obj is Repository) {
                Repository that = (Repository)obj;
                if (that.GetHashCode ().Equals (this.GetHashCode ())) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Another object will be unique if id identifies the same
        ///         file contents as this file.
        /// </summary>
        public override int GetHashCode () {
            return this.FileContents.GetHashCode ();
        }

        /// <summary>The type of file that this is.</summary>
        public Factory.FileType Type {get {return Factory.FileType.Repository;}}

        /// <summary>Indicates whether the file can contain multiple
        /// lines.</summary>
        /// <returns><code>true</code> if the file can contain multiple
        /// lines; <code>false</code> otherwise.</returns>
        public bool IsMultiLined {
            get {return false;}
        }

    }
}
