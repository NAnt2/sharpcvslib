#region "Copyright"
// UpdatedResponse.cs
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
using System.IO;
using ICSharpCode.SharpCvsLib.Misc;

using log4net;

namespace ICSharpCode.SharpCvsLib.Responses { 

    /// <summary>
    /// Response from cvs server after update command.
    /// </summary>
	public class UpdatedResponse : IResponse
	{
	    private readonly ILog LOGGER = 
	        LogManager.GetLogger (typeof (UpdatedResponse));
	    
        /// <summary>
        /// Process the response from the cvs server.
        /// </summary>
        /// <param name="cvsStream"></param>
        /// <param name="services"></param>
	    public void Process(CvsStream cvsStream, IResponseServices services)
	    {
			cvsStream.ReadLine();
			string orgpath   = cvsStream.ReadLine();
			string localpath = services.Repository.ToLocalPath(orgpath);
			string entry     = cvsStream.ReadLine();
			string flags     = cvsStream.ReadLine();
			string sizestr   = cvsStream.ReadLine();
			bool compress = sizestr[0] == 'z';
			
			if (LOGGER.IsDebugEnabled) {
			    String msg = "orgpath=[" + orgpath + "]" +
    			    "entry=[" + entry + "]" +
    			    "flags=[" + flags + "]" +
    			    "sizestr=[" + sizestr + "]";
			    LOGGER.Debug (msg);
			}
	    	
			if (compress) {
				sizestr = sizestr.Substring(1);
			}
			
			int size  = Int32.Parse(sizestr);
			
			string newdir = Path.GetDirectoryName(localpath);
			if (!Directory.Exists(newdir)) {
				Directory.CreateDirectory(Path.GetDirectoryName(localpath));
			}
			
			if (services.NextFile != null && services.NextFile.Length > 0) {
				localpath = services.NextFile;
				services.NextFile = null;
			}
			
			Entry e = new Entry(entry);
			
			if (e.IsBinaryFile) {
				services.UncompressedFileHandler.ReceiveBinaryFile(cvsStream, localpath, size);
			} else {
				services.UncompressedFileHandler.ReceiveTextFile(cvsStream, localpath, size);
			}
			
			e.Date = services.NextFileDate;
			services.Repository.AddEntry(orgpath.Substring(0, orgpath.LastIndexOf('/')), e);
			services.NextFileDate = null;
			
			// set the timestamp from the server to the newly created file
			e.TimeStamp = e.TimeStamp.ToLocalTime();
			File.SetCreationTime(localpath, e.TimeStamp);
			File.SetLastAccessTime(localpath, e.TimeStamp);
			File.SetLastWriteTime(localpath, e.TimeStamp);
	    }
	    
        /// <summary>
        /// Indicator stating whether the response is terminating or not.
        /// </summary>
		public bool IsTerminating {
			get {
				return false;
			}
		}
	}
}
