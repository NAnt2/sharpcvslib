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
#endregion

using System;
using System.Collections;

using log4net;

namespace ICSharpCode.SharpCvsLib.Options
{
	/// <summary>
	/// The entries collection holds a collection of objects that represent cvs
	///     entries.  The key to the collection is the path to the file that the
	///     cvs entry represents on the file system.
	/// </summary>
	public class CheckoutOptions : AbstractOptions {
        private ILog LOGGER = LogManager.GetLogger (typeof (CheckoutOptions));

        /// <summary>Specifies the revision or sticky tag will follow.</summary>
        public const string REVISION = "r";
        /// <summary>Specifies that a directory replacing the module directory
        ///     will follow.</summary>
        public const string OVERRIDE_DIRECTORY = "d";

        /// <summary>
        /// Initialize the available options for the checkout options class.
        /// </summary>
        public CheckoutOptions () : base () {
            //this.Available.Add (new Option ("A"));
            //this.Available.Add (new Option ("N"));
            //this.Available.Add (new Option ("P"));
            //this.Available.Add (new Option ("R"));
            //this.Available.Add (new Option ("c"));
            //this.Available.Add (new Option ("f"));
            //this.Available.Add (new Option ("l"));
            //this.Available.Add (new Option ("n"));
            //this.Available.Add (new Option ("p"));
            //this.Available.Add (new Option ("s"));
            this.Available.Add (new Option ("r"));
            //this.Available.Add (new Option ("D"));
            this.Available.Add (new Option ("d"));
            //this.Available.Add (new Option ("k"));
            //this.Available.Add (new Option ("j"));
        }
    }
}
