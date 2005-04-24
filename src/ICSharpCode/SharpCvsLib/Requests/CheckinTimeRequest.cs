#region "Copyright"
// ModifiedRequest.cs
// Copyright (C) 2005 Clayton Harbour
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

using System;
using System.IO;

using ICSharpCode.SharpCvsLib.Attributes;
using ICSharpCode.SharpCvsLib.Util;

namespace ICSharpCode.SharpCvsLib.Requests {

    /// <summary>
    /// <para><b>Checkin-time time \n</br></para>
    /// <para>For the file specified by the next Modified request, use time as the time of the checkin. 
    /// The time is in the format specified by RFC822 as modified by RFC1123. The client may specify 
    /// any timezone it chooses; servers will want to convert that to their own timezone as appropriate. 
    /// 
    /// <para>An example of this format is:
    /// 
    /// 26 May 1997 13:01:40 -0400
    /// </para>
    /// 
    /// <para>There is no requirement that the client and server clocks be synchronized. The client 
    /// just sends its recommendation for a timestamp (based on file timestamps or whatever), and the 
    /// server should just believe it (this means that the time might be in the future, for example). 
    /// Note that this is not a general-purpose way to tell the server about the timestamp of a file; 
    /// that would be a separate request (if there are servers which can maintain timestamp and time 
    /// of checkin separately). This request should affect the import request, and may optionally 
    /// affect the ci request or other relevant requests if any.
    /// </para>    
    /// </summary>
    [Author("Clayton Harbour", "claytonharbour@sporadicism.com", "2005")]
    public class CheckinTimeRequest : AbstractRequest {
        private string _path;
        private DateTime _checkinTime;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">The name of the file that has been modified.</param>
        public CheckinTimeRequest(string path) {
            this._path = path;
            if (File.Exists(this._path)) {
                this._checkinTime = File.GetLastWriteTime(this._path).ToUniversalTime();
            } else {
                this._checkinTime = DateTime.Now.ToUniversalTime();
            }
        }

        /// <summary>
        /// Send the checkin time request.
        /// </summary>
        public override string RequestString {
            get {
                return string.Format("Checkin-time {0}\n", 
                    DateParser.GetCvsDateString(this._checkinTime));
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
