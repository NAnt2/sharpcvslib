#region "Copyright"
// ValidResponsesRequest.cs 
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
	/// Tell the server what responses the client will accept. request-list is a space 
	/// separated list of tokens. 
	/// </summary>
	public class ValidResponsesRequest : AbstractRequest
	{
        /// <summary>
        /// List of valid request responses.
        /// </summary>
		public override string RequestString {
			get {
		        return "Valid-responses ok error Valid-requests Checked-in New-entry Checksum Copy-file Updated Created Update-existing Merged Patched Rcs-diff Mode Mod-time Removed Remove-entry Set-static-directory Clear-static-directory Set-sticky Clear-sticky Template Set-checkin-prog Set-update-prog Notified Module-expansion Wrapper-rcsOption M Mbinary E F MT\n";
			}
		}
		
        /// <summary>
        /// Determine if a response is expected from this request
        /// </summary>
		public override bool IsResponseExpected {
			get {
				return false;
			}
		}
	}
}
