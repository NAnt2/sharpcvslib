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
//
//    Author:     Mike Krueger,
//                Clayton Harbour  {claytonharbour@sporadicism.com}
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
    public class CreatedResponse : IResponse {
        private readonly ILog LOGGER =
            LogManager.GetLogger (typeof (CreatedResponse));

        /// <summary>
        /// Process a created file response.
        /// </summary>
        /// <param name="cvsStream"></param>
        /// <param name="services"></param>
        public void Process(CvsStream cvsStream, IResponseServices services) {
            Manager manager = new Manager (services.Repository.WorkingPath);

            String localPath = cvsStream.ReadLine();
            String reposPath = cvsStream.ReadLine ();

            String entry     = cvsStream.ReadLine();
            String flags     = cvsStream.ReadLine();
            String sizeStr   = cvsStream.ReadLine();

            PathTranslator orgPath =
                new PathTranslator (services.Repository, reposPath);
            String localPathAndFilename = orgPath.LocalPathAndFilename;
            String directory = orgPath.LocalPath;

            bool compress = sizeStr[0] == 'z';

            if (LOGGER.IsDebugEnabled) {
                String msg = "In created response process.  " +
                            "orgPath=[" + orgPath.ToString () + "]" +
                            "localPathAndFilename=[" + localPathAndFilename + "]" +
                            "directory=[" + directory + "]" +
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
            LOGGER.Debug("In created response, just added entry.  File date=[" + e.Date + "]");
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
