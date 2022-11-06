#region "Copyright"
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

using System.IO;
using System.Text;

using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.FileSystem;
using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Messages;
using ICSharpCode.SharpCvsLib.Streams;

using log4net;

namespace ICSharpCode.SharpCvsLib.Responses {

    /// <summary>
    /// Merged pathname \n
    ///     This is just like Updated and takes the same additional data, with the 
    ///     one difference that after the new copy of the file is enclosed, it will 
    ///     still not be up to date. Used for the results of a merge, with or without 
    ///     conflicts. It is useful to preserve an copy of what the file looked like 
    ///     before the merge. This is basically handled by the server; before sending 
    ///     Merged it will send a Copy-file response. For example, if the file is 
    ///     `aa' and it derives from revision 1.3, the Copy-file response will tell 
    ///     the client to copy `aa' to `.#aa.1.3'. It is up to the client to decide 
    ///     how long to keep this file around; traditionally clients have left it 
    ///     around forever, thus letting the user clean it up as desired. But another 
    ///     answer, such as until the next commit, might be preferable.
    /// </summary>
    public class MergedResponse : IResponse {
        private readonly ILog LOGGER = LogManager.GetLogger(typeof (MergedResponse));
        /// <summary>
        /// Process a merged response.
        /// 
        /// TODO: Implementation copied from the UpdatedResponse, need to verify 
        ///     this is correctly implemented.
        /// </summary>
        /// <param name="cvsStream"></param>
        /// <param name="services"></param>
        public void Process(CvsStream cvsStream, IResponseServices services) {
            Manager manager = new Manager (services.Repository.WorkingPath);
            string localPath = cvsStream.ReadLine();
            string reposPath = cvsStream.ReadLine ();
            string entry     = cvsStream.ReadLine();
            string flags     = cvsStream.ReadLine();
            string sizeStr   = cvsStream.ReadLine();

            PathTranslator orgPath   =
                new PathTranslator (services.Repository,
                reposPath);
            string localPathAndFilename = orgPath.LocalPathAndFilename;
            string directory = orgPath.LocalPath;

            bool compress = sizeStr[0] == 'z';

            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder ();
                msg.Append ("reposPath=[").Append (reposPath).Append ("]");
                msg.Append ("entry=[").Append (entry).Append ("]");
                msg.Append ("flags=[").Append (flags).Append ("]");
                msg.Append ("sizestr=[").Append (sizeStr).Append ("]");
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

            Entry e = new Entry(orgPath.LocalPath, entry);

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
            services.NextFileDate = null;

            manager.Add(e);
            manager.SetFileTimeStamp (localPathAndFilename, e.TimeStamp, e.IsUtcTimeStamp);

            UpdateMessage message = new UpdateMessage ();
            message.Module = services.Repository.WorkingDirectoryName;
            message.Repository =  orgPath.RelativePath;
            message.Filename = e.Name;
            services.SendMessage (message.Message);
        }

        /// <summary>
        /// Return true if this response cancels the transaction
        /// </summary>
        public bool IsTerminating {
            get {return false;}
        }
    }
}
