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

using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Messages;
using ICSharpCode.SharpCvsLib.FileSystem;
using ICSharpCode.SharpCvsLib.Streams;

using log4net;

namespace ICSharpCode.SharpCvsLib.Responses {

/// <summary>
/// Set-sticky pathname \n
///    Additional data: tagspec \n. Tell the client to set a sticky tag
///    or date, which should be supplied with the Sticky request for future
///    operations. pathname ends in a slash; its purpose is to specify a
///    directory, not a file within a directory. The client should store
///    tagspec and pass it back to the server as-is, to allow for future
///    expansion. The first character of tagspec is `T' for a tag, `D' for
///    a date, or something else for future expansion. The remainder of
///    tagspec contains the actual tag or date.
/// </summary>
public class SetStickyResponse : IResponse {

    private ILog LOGGER =
        LogManager.GetLogger (typeof (SetStickyResponse));

    /// <summary>
    /// Process the response stream.
    /// </summary>
    /// <param name="cvsStream"></param>
    /// <param name="services"></param>
    public void Process(CvsStream cvsStream, IResponseServices services)
    {
        string localPath      = cvsStream.ReadLine();
        string repositoryPath = cvsStream.ReadLine();
        string stickyTag      = cvsStream.ReadLine();

        PathTranslator orgPath   =
            new PathTranslator (services.Repository, repositoryPath);

        string localPathAndFilename = orgPath.LocalPathAndFilename;
        string directory = orgPath.LocalPath;

        if (LOGGER.IsDebugEnabled) {

        }
        Tag tag = new Tag (directory, stickyTag);
        Manager manager = new Manager ();
        manager.Add (tag);

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
