#region "Copyright"
// ResponseFactory.cs
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
#endregion

namespace ICSharpCode.SharpCvsLib.Responses { 
	
    /// <summary>
    /// Factory method for instanciating the correct response handler
    ///     for the cvs response.
    /// </summary>
	public sealed class ResponseFactory
	{
        /// <summary>
        /// Create the response object based on the response string.
        /// </summary>
        /// <param name="responseStr"></param>
        /// <returns></returns>
		public static IResponse CreateResponse(string responseStr)
		{
			switch (responseStr) {
				case "M":
					return new MessageResponse();
				case "E":
					return new ErrorMessageResponse();
				case "MT":
					return new MessageTaggedResponse();
				case "Checked-in":
					return new CheckedInResponse();
				case "Mod-time":
					return new ModTimeResponse();
				case "ok":
					return new OkResponse();
				case "error":
					return new ErrorResponse();
				case "Updated":
					return new UpdatedResponse();
				case "Created":
					return new CreatedResponse();
				case "Module-expansion":
					return new ModuleExpansionResponse();
				case "Clear-sticky":
					return new ClearStickyResponse();
				case "Set-static-directory":
					return new SetStaticDirectoryResponse();
				case "Clear-static-directory":
					return new ClearStaticDirectoryResponse();
				case "Valid-requests":
					return new ValidRequestsResponse();
			    case "Set-sticky":
			        return new SetStickyResponse ();
				default:
					return null;
			}
		}
	}
}
