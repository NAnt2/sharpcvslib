#region "Copyright"
// CreatedResponse.cs
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
    /// Command:
    ///     Created pathname \n
    /// 
    ///     This is just like Updated and takes the same additional 
    ///     data, but is used only if no Entry, Modified, or Unchanged 
    ///     request has been sent for the file in question. The 
    ///     distinction between Created and Update-existing is so that 
    ///     the client can give an error message in several cases: 
    ///         (1) there is a file in the working directory, 
    ///             but not one for which Entry, Modified, or Unchanged 
    ///             was sent (for example, a file which was ignored, 
    ///             or a file for which Questionable was sent), 
    ///         (2) there is a file in the working directory whose name 
    ///             differs from the one mentioned in Created in ways that 
    ///             the client is unable to use to distinguish files. 
    ///             For example, the client is case-insensitive and the names 
    ///             differ only in case.
    /// </summary>
	public class CreatedResponse : IResponse
	{
	    private readonly ILog LOGGER = 
	        LogManager.GetLogger (typeof (CreatedResponse));
	    
        /// <summary>
        /// Process a created file response.
        /// </summary>
        /// <param name="cvsStream"></param>
        /// <param name="services"></param>
	    public void Process(CvsStream cvsStream, IResponseServices services)
	    {
	        CvsFileManager manager = new CvsFileManager ();
	        
			cvsStream.ReadLine();
			string orgpath   = cvsStream.ReadLine();
			string localpath = services.Repository.ToLocalPath(orgpath);
	        string directory = Path.GetDirectoryName (localpath);
	        
			string entry     = cvsStream.ReadLine();
			string flags     = cvsStream.ReadLine();
			string sizestr   = cvsStream.ReadLine();
			bool compress = sizestr[0] == 'z';
			
			if (LOGGER.IsDebugEnabled) {
			    String msg = "orgPath=[" + orgpath + "]" +
			        "localpath=[" + localpath + "]" +
			        "directory=[" + directory + "]" +
			        "entry=[" + entry + "]" +
			        "flags=[" + flags + "]" +
			        "sizestr=[" + sizestr + "]";
			    LOGGER.Debug (msg);
			}
			
			if (compress) {
				sizestr = sizestr.Substring(1);
			}
			
			int size  = Int32.Parse(sizestr);
			
			if (!Directory.Exists(directory)) {
				Directory.CreateDirectory(directory);
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
	        
	        manager.AddEntry (directory, e);
	        String cvsMsg = "cvs server: U " + orgpath;	        
	        manager.SendCvsMessage (cvsMsg);	        
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
