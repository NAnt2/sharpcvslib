#region "Copyright"
// SetStaticDirectoryResponse.cs
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
using System.Text;

using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Messages;
using ICSharpCode.SharpCvsLib.FileSystem;
using ICSharpCode.SharpCvsLib.Streams;

using log4net;

namespace ICSharpCode.SharpCvsLib.Responses {
    /// <summary>
    /// "Set-static-directory pathname \n"
    ///
    ///     This instructs the client to set the Entries.Static flag,
    ///     which it should then send back to the server in a
    ///     Static-directory request whenever the directory is operated on.
    ///     Pathname ends in a slash; its purpose is to specify a directory,
    ///     not a file within a directory.
    ///
    /// </summary>
    public class SetStaticDirectoryResponse : AbstractResponse {
        private ILog LOGGER =
            LogManager.GetLogger (typeof (SetStaticDirectoryResponse));

        /// <summary>
        /// Process the response stream.
        /// </summary>
        public override void Process() {
            string localPath      = this.ReadLine();
            string repositoryPath = this.ReadLine();

            if (LOGGER.IsDebugEnabled) {
                StringBuilder msg = new StringBuilder ();
                msg.Append ("localPath=[").Append (localPath).Append ("]");
                msg.Append ("repositoryPath=[").Append (repositoryPath).Append ("]");
                LOGGER.Debug (msg);
            }
            PathTranslator pathTranslator =
                new PathTranslator (Services.Repository,
                                    repositoryPath);
            if (LOGGER.IsDebugEnabled) {
                LOGGER.Debug (pathTranslator.ToString ());
            }
            LOGGER.Debug ("directory exitsts=[" + Directory.Exists (pathTranslator.LocalPath) + "]");
            if (!Directory.Exists (pathTranslator.LocalPath)) {
                LOGGER.Debug ("Creating directory=[" + pathTranslator.LocalPath + "]");
                if (!(pathTranslator.LocalPath == null && pathTranslator.LocalPath == String.Empty)) {
                    Directory.CreateDirectory (pathTranslator.LocalPath);
                }
            }
            // TODO : make something useful with this request
        }

        /// <summary>
        /// Indicator stating whether the response is terminating or not.
        /// </summary>
        public override bool IsTerminating {
            get {return false;}
        }
    }
}
