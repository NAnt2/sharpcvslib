#region "Copyright"
// ModifiedRequest.cs
// Copyright (C) 2001 Mike Krueger
// comments are taken from CVS Client/Server reference manual which
// comes with the cvs client (www.cvshome.org)
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
// As a special exception, if you link this library with other files to
// produce an executable, this library does not by itself cause the
// resulting executable to be covered by the GNU General Public License.
// This exception does not however invalidate any other reasons why the
// executable file might be covered by the GNU General Public License.
#endregion

namespace ICSharpCode.SharpCvsLib.Requests {
    /// <summary>
    /// Response expected: no.
    /// Additional data: mode, \n, file transmission.
    /// Send the server a copy of one locally modified file.
    /// filename is relative to the most recent repository sent with Directory.
    /// If the user is operating on only some files in a directory, only those
    /// files need to be included. This can also be sent without Entry, if there
    /// is no entry for the file.
    /// </summary>
    public class ModifiedRequest : AbstractRequest {
        private string file;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">The name of the file that has been modified.</param>
        public ModifiedRequest(string file) {
            this.file = file;
        }

        /// <summary>
        /// Send a modified file to the repository.
        /// </summary>
        public override string RequestString {
            get {
                return "Modified " + file + "\n" + "u=rw,g=rw,o=rw\n";
            }
        }

        /// <summary>
        /// <code>false</code>, a response is not expected.
        /// </summary>
        public override bool IsResponseExpected {
            get {return false;}
        }
    }
}
