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
    ///     Value object for the <code>Root</code> cvs file.  The root file
    ///         holds the cvsroot string.  The cvsroot is a string value
    ///         which has the following information:
    ///             <ol>
    ///                 <li>protocol</li>
    ///                 <li>user@servername.domainname</li>
    ///                 <li>server repository directory</li>
    ///             </ol>
    ///         seperated by a colan(<code>:</code>).
    ///
    ///     eg)     :pserver:anonymous@linux.sporadicism.com:/home/cvs/src/
    /// </summary>
    public class Root : AbstractCvsFile, ICvsFile {

        /// <summary>
        ///     The name of the root file.
        /// </summary>
        public const String FILE_NAME = "Root";

        /// <summary>
        ///     The name of the cvs file that the object represents.
        /// </summary>
        public String Filename {
            get {return Root.FILE_NAME;}
        }

        /// <summary>
        /// Create a new instance of the cvs object.
        /// </summary>
        /// <param name="fullPath">The full path to the object being managed.</param>
        /// <param name="fileContents">The contents of the cvs management file.</param>
        public Root (String fullPath, String fileContents) : 
            base (fullPath, fileContents) {
        }

        /// <summary>
        /// Parse the contents of the cvs file.
        /// </summary>
        /// <param name="line">Contents of the cvs file.</param>
        public override void Parse(String line) {
            this.FileContents = line;
        }

        /// <summary>
        ///     Determine if the two objects are equal.
        /// </summary>
        public override bool Equals (object obj) {
            if (obj is Root) {
                Root that = (Root)obj;
                if (that.GetHashCode ().Equals (this.GetHashCode ())) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Override the hashcode.  This is a combination of the entry
        ///         name and the path to the entry file.
        /// </summary>
        public override int GetHashCode () {
            return this.FileContents.GetHashCode ();
        }

        /// <summary>The type of file that this is.</summary>
        public Factory.FileType Type {get {return Factory.FileType.Root;}}

        /// <summary>Indicates whether the file can contain multiple
        /// lines.</summary>
        /// <returns><code>true</code> if the file can contain multiple
        /// lines; <code>false</code> otherwise.</returns>
        public bool IsMultiLined {
            get {return false;}
        }

    }
}
