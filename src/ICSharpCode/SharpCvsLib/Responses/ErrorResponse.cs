#region "Copyright"
// ErrorResponse.cs
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

using System;

using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Streams;

using log4net;

namespace ICSharpCode.SharpCvsLib.Responses { 
	
    /// <summary>
    /// Handle an error response from the cvs server.
    /// </summary>
	public class ErrorResponse : IResponse
	{
	    private readonly ILog LOGGER = 
	        LogManager.GetLogger (typeof (ErrorResponse));
        /// <summary>
        /// Process an error response.
        /// </summary>
        /// <param name="cvsStream"></param>
        /// <param name="services"></param>
	    public void Process(CvsStream cvsStream, IResponseServices services)
	    {
	    	string message = cvsStream.ReadLine();
	        // TODO: Figure out if I should spit this out to the client.
	    	//services.SendMessage("cvs server: Error " + error);
            String msg = "cvs server: M " + message;
	        LOGGER.Debug (msg);
	        
	    }
	    
        /// <summary>
        /// Return true if this response cancels the transaction
        /// </summary>
		public bool IsTerminating {
			get {
				return true;
			}
		}
	}
}
