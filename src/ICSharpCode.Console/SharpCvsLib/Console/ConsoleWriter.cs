#region "Copyright"
// ConsoleWriter.cs
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
using System.Text;
using System.Text.RegularExpressions;

using ICSharpCode.SharpCvsLib.Messages;

namespace ICSharpCode.SharpCvsLib.Console
{
	/// <summary>
	/// Provides a common interface for writing to the console.
	/// </summary>
	public class ConsoleWriter {
        //private bool debug = false;

        private const string REGEX_TEXT = @"text[\s]*([\w]*)";
        private const string REGEX_FNAME = @"fname[\s]*([\w]+[/\w]*[.\w]*)";

        private static Regex TextRegex = new Regex(REGEX_TEXT, RegexOptions.Multiline);
        private static Regex FNameRegex = new Regex(REGEX_FNAME, RegexOptions.Multiline);

        /// <summary>
        /// Create a new instance of the console writer.
        /// </summary>
		public ConsoleWriter() {
		}

        /// <summary>
        /// Write the given message to the console and include a carriage return at line end.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WriteLine(object sender, MessageEventArgs e) {
            string message = e.Message;
            string prefix = e.Prefix;

            if (e.Response != null && 
                e.Response.GetType() == typeof(ICSharpCode.SharpCvsLib.Responses.UpdatedResponse)) {
                System.Console.WriteLine(String.Format("[{0}]: [{1}]", prefix, message));
            }
        }

        private string GetText(string line) {
            Match match = null;
            StringBuilder text = new StringBuilder(String.Empty);
            
            match = TextRegex.Match(line);
            if (match.Groups.Count != 0) {
                text.Append(match.Groups[1].Value);
            }

            match = FNameRegex.Match(line);
            if (match.Groups.Count != 0) {
                text.Append(match.Groups[1].Value);
            }
            return text.ToString();
        }
	}
}
