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
using System.Xml;
using System.Xml.Serialization;

namespace ICSharpCode.SharpCvsLib.Config.Logging {
    /// <summary>
    /// Configuration settings for the sharpcvslib debug log.  These are used
    ///     to configure the message delegates on the CvsStream class.
    /// </summary>
    public class Debug {

        private bool enabled = true;
        private String requestFile = "out.log";
        private String responseFile = "in.log";

        /// <summary>
        /// Constructor.
        /// </summary>
        public Debug () {
        }

        /// <summary>
        /// <code>true</code> if the debug log is enabled,
        ///     <code>false</code> otherwise.  If this is false nothing
        ///     will be logged to the request and response files.
        /// </summary>
        [XmlElement ("enabled", typeof (bool))]
        public bool Enabled {
            get {return this.enabled;}
            set {this.enabled = value;}
        }

        /// <summary>
        /// Configure the name of the file that requests to the cvs
        ///     server are logged to.
        /// </summary>
        [XmlElement ("request-file", typeof (String))]
        public String RequestFile {
            get {return this.requestFile;}
            set {this.requestFile = value;}
        }

        /// <summary>
        /// Configure the name of the file that responses from the cvs server
        ///     are logged to.
        /// </summary>
        [XmlElement ("response-file", typeof (String))]
        public String ResponseFile {
            get {return this.responseFile;}
            set {this.responseFile = value;}
        }

        /// <summary>
        /// Return a human readable representation of the object.
        /// </summary>
        /// <returns>A human readable representation of the object.</returns>
        public override String ToString () {
            ICSharpCode.SharpCvsLib.Util.ToStringFormatter formatter =
                new ICSharpCode.SharpCvsLib.Util.ToStringFormatter("Debug");
            formatter.AddProperty("Enabled", this.Enabled);
            formatter.AddProperty("RequestFile", this.RequestFile);
            formatter.AddProperty("ResponseFile", this.ResponseFile);
            return formatter.ToString();
        }
    }
}
