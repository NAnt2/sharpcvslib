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
//    <author>Mike Krueger</author>
//    <author>Clayton Harbour</author>
//
#endregion

using System;
using System.Collections;

using ICSharpCode.SharpCvsLib.FileSystem;

namespace ICSharpCode.SharpCvsLib.FileSystem {
    /// <summary>
    /// Represents a list of entries in the repository or
    ///     in simple terms a folder or directory on the
    ///     cvs server.
    /// </summary>
    public class Folder {
        private Entries entries;
        private Repository repository;
        private Root root;
        private Tag tag;

        /// <summary>
        /// Create a new instance of the folders object.  Initialize the entries 
        ///     collection.
        /// </summary>
        public Folder () {
            this.entries = new Entries();
        }
        /// <summary>
        ///     The repository object.
        /// </summary>
        [Obsolete ("Use Repository")]
        public Repository Repos {
            get {return this.repository;}
            set {this.repository = value;}
        }

        /// <summary>
        /// Root file, holds cvsroot information.
        /// </summary>
        public Root Root {
            get {return this.root;}
            set {this.root = value;}
        }

        /// <summary>
        /// Repository file, holds information about the relative path to the
        ///     folder on the server.
        /// </summary>
        public Repository Repository {
            get {return this.repository;}
            set {this.repository = value;}
        }

        /// <summary>
        /// Tag file: Optional file that records the current revision that
        ///     is checked out.  Only present if the revision is not the HEAD
        ///     revision.
        /// </summary>
        public Tag Tag {
            get {return this.tag;}
            set {this.tag = value;}
        }

        /// <summary>
        /// List of entries.
        /// </summary>
        public Entries Entries {
            get {return this.entries;}
            set {this.entries = value;}
        }

        /// <summary>
        /// Render the object as a human readable string.
        /// </summary>
        /// <returns></returns>
        public override String ToString() {
            ICSharpCode.SharpCvsLib.Util.ToStringFormatter formatter = new
                ICSharpCode.SharpCvsLib.Util.ToStringFormatter("Folder");
            formatter.AddProperty("Repository", this.Repository);
            formatter.AddProperty("Root", this.Root);
            formatter.AddProperty("Tag", this.Tag);
            formatter.AddProperty("Entries", this.Entries);
            return formatter.ToString();
        }
    }

}
