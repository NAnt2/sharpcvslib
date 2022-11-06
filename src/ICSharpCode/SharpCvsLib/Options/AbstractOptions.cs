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
	public class AbstractOptions : DictionaryBase {

        private AvailableOptions available = new AvailableOptions();
        /// <summary>
        /// Options that are available for the given command.
        /// </summary>
        protected AvailableOptions Available {
            get {return this.available;}
        }

        private ILog LOGGER = LogManager.GetLogger (typeof (AbstractOptions));

        /// <summary>
        /// Create a new instance of the options class.
        /// </summary>
		public AbstractOptions() : base() {

		}

        /// <summary>
        /// Set the option to the given location.
        /// </summary>
        public Option this[String name] {
            get { return ((Option)(Dictionary[name])); }
            set { Dictionary[name] = value; }
        }

        /// <summary>
        /// Add a new option to the option collection, using the Option.Name value
        ///     as the key..
        /// </summary>
        /// <param name="option">The option to add to the collection.</param>
        public void Add (Option option) {
            if (!this.Available.Contains(option.Name)) {
                throw new UnsupportedOptionException ("Option name: " + option.Name);
            }
            Dictionary.Add(option.Name, option);
        }

        /// <summary>
        /// Add a new option to the option collection.
        /// </summary>
        /// <param name="name">The name of the option to add to the collection.</param>
        /// <param name="option">The option to add to the collection.</param>
        public void Add(String name, Option option) {
            if (!this.Available.Contains(option.Name)) {
                throw new UnsupportedOptionException ("Option name: " + option.Name);
            }
            Dictionary.Add(name, option);
        }

        /// <summary>
        /// Remove the given option from the collection.
        /// </summary>
        /// <param name="name">The name of the option to remove from the collection.</param>
        public void Remove(String name) {
            Dictionary.Remove(name);
        }

        /// <summary>
        /// Determines if the given collection contains an option.
        /// </summary>
        /// <param name="name">The name of the option to search for.</param>
        /// <returns></returns>
        public bool Contains(String name) {
            return Dictionary.Contains(name);
        }

        /// <summary>
        /// Return the collection of values for the dictionary.
        /// </summary>
        public ICollection Values {
            get {return this.Dictionary.Values;}
        }

        /// <summary>
        /// Render the options collection as a human readable string.
        /// </summary>
        /// <returns></returns>
        public override String ToString() {
            ICSharpCode.SharpCvsLib.Util.ToStringFormatter formatter = new
                ICSharpCode.SharpCvsLib.Util.ToStringFormatter("Entries");
            foreach (DictionaryEntry option in Dictionary) {
                formatter.AddProperty("Option name", option.Key);
                formatter.AddProperty("Option value", option.Value);
            }
            return formatter.ToString();
        }
	}
}
