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
//
//    Author:     Mike Krueger, 
//                Clayton Harbour  {claytonharbour@sporadicism.com}
#endregion

using System;
using System.IO;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.FileSystem;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Messages;

using log4net;

namespace ICSharpCode.SharpCvsLib.Responses { 

    /// <summary>
    /// Response from cvs server after update command.
    /// 
    /// Updated pathname \n
    ///     Additional data: New Entries line, \n, mode, \n, file transmission. 
    ///     A new copy of the file is enclosed. This is used for a new revision 
    ///     of an existing file, or for a new file, or for any other case in which 
    ///     the local (client-side) copy of the file needs to be updated, and after 
    ///     being updated it will be up to date. If any directory in pathname does 
    ///     not exist, create it. This response is not used if Created and 
    ///     Update-existing are supported.
    /// 
    /// Created pathname \n
    ///     This is just like Updated and takes the same additional data, but is 
    ///     used only if no Entry, Modified, or Unchanged request has been sent 
    ///     for the file in question. The distinction between Created and 
    ///     Update-existing is so that the client can give an error message in 
    ///     several cases: 
    ///         (1) There is a file in the working directory, but not one for which 
    ///             Entry, Modified, or Unchanged was sent (for example, a file 
    ///             which was ignored, or a file for which Questionable was sent)
    ///         (2) There is a file in the working directory whose name differs 
    ///             from the one mentioned in Created in ways that the client is 
    ///             unable to use to distinguish files. For example, the client 
    ///             is case-insensitive and the names differ only in case.
    /// 
    /// Update-existing pathname \n
    ///     This is just like Updated and takes the same additional data, but 
    ///     is used only if a Entry, Modified, or Unchanged request has been 
    ///     sent for the file in question. This response, or Merged, indicates 
    ///     that the server has determined that it is OK to overwrite the 
    ///     previous contents of the file specified by pathname. Provided that 
    ///     the client has correctly sent Modified or Is-modified requests 
    ///     for a modified file, and the file was not modified while CVS was 
    ///     running, the server can ensure that a user's modifications are 
    ///     not lost.
    /// 
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
	        Manager manager = new Manager ();
			cvsStream.ReadLine();
			PathTranslator orgPath   = 
			    new PathTranslator (services.Repository,
			                                 cvsStream.ReadLine());
			string localPathAndFilename = orgPath.LocalPathAndFilename;
	        string directory = orgPath.LocalPath;
	        
			string entry     = cvsStream.ReadLine();
			string flags     = cvsStream.ReadLine();
			string sizeStr   = cvsStream.ReadLine();
			bool compress = sizeStr[0] == 'z';
			
			if (LOGGER.IsDebugEnabled) {
			    String msg = "orgpath=[" + orgPath + "]" +
    			    "entry=[" + entry + "]" +
    			    "flags=[" + flags + "]" +
    			    "sizestr=[" + sizeStr + "]";
			    LOGGER.Debug (msg);
			}
	    	
			if (compress) {
				sizeStr = sizeStr.Substring(1);
			}
			
			int size  = Int32.Parse(sizeStr);
			
			if (!Directory.Exists(orgPath.LocalPath)) {
				Directory.CreateDirectory(orgPath.LocalPath);
			    
			}
			
			if (services.NextFile != null && services.NextFile.Length > 0) {
				localPathAndFilename = services.NextFile;
				services.NextFile = null;
			}
			
			Entry e = new Entry(directory, entry);
			
			if (e.IsBinaryFile) {
				services.UncompressedFileHandler.ReceiveBinaryFile(cvsStream, 
				                                                   localPathAndFilename, 
				                                                   size);
			} else {
				services.UncompressedFileHandler.ReceiveTextFile(cvsStream, 
				                                                 localPathAndFilename, 
				                                                 size);
			}
			
			e.Date = services.NextFileDate;
			//services.Repository.AddEntry(orgPath.Substring(0, orgPath.LastIndexOf('/')), e);
			services.NextFileDate = null;
			
	        manager.SetFileTimeStamp (localPathAndFilename, e.TimeStamp);
	        
	        UpdateMessage message = new UpdateMessage ();
	        message.Module = services.Repository.WorkingDirectoryName;
	        message.Repository =  orgPath.RelativePath;
	        message.Filename = e.Name;
	        services.SendMessage (message.Message);

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
