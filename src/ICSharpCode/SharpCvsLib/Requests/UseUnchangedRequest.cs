#region "Copyright"
// UseUnchangedRequest.cs
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
/// To specify the version of the protocol described in this document,
/// servers must support this request (although it need not do anything)
/// and clients must issue it.
/// </summary>
public class UseUnchangedRequest : AbstractRequest
{
    /// <summary>
    /// Specifies the version of the protocol described in this document.
    /// </summary>
    public override string RequestString {
        get {
            return "UseUnchanged\n";
        }
    }

    /// <summary>
    /// Indicator specifying if the response is respected.
    /// </summary>
    public override bool IsResponseExpected {
        get {
            return false;
        }
    }
}
}
