#region "Copyright"
// ClearStaticDirectoryResponse.cs
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

using log4net;

namespace ICSharpCode.SharpCvsLib.Responses { 
	
    /// <summary>
    /// Handle a clear static directory response.
    /// 
    /// from: http://www.loria.fr/~molli/cvs/doc/cvsclient_5.html
    ///    Clear-static-directory pathname \n
    /// 
    /// This instructs the client to un-set the Entries.Static flag, 
    /// which it should then send back to the server in a Static-directory 
    /// request whenever the directory is operated on. pathname ends in a 
    /// slash; its purpose is to specify a directory, not a file within a 
    /// directory.
    /// 
    /// </summary>
	public class ClearStaticDirectoryResponse : IResponse
	{
	    private readonly ILog LOGGER = 
	        LogManager.GetLogger (typeof (ClearStaticDirectoryResponse));
        /// <summary>
        /// Process a clear static directory response.
        /// </summary>
        /// <param name="cvsStream"></param>
        /// <param name="services"></param>
	    public void Process(CvsStream cvsStream, IResponseServices services)
	    {
            string localPath      = cvsStream.ReadLine();
            string repositoryPath = cvsStream.ReadLine();
	        if (LOGGER.IsDebugEnabled) {
	            String msg = "localPath=[" + localPath + "]" +
	                "repositoryPath=[" + repositoryPath + "]";
	        }
	    	// TODO : make something useful with this request
	    }
	    
        /// <summary>
        /// Return true if this response cancels the transaction
        /// </summary>
		public bool IsTerminating {
			get {
				return false;
			}
		}
	}
}
