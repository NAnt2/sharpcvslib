#region "Copyright"
// IResponseServices.cs
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

using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.FileHandler;
using ICSharpCode.SharpCvsLib.FileSystem;
using ICSharpCode.SharpCvsLib.Messages;


namespace ICSharpCode.SharpCvsLib.Client {
    /// <summary>
    /// Response services interface.
    /// </summary>
    public interface IResponseServices {
        /// <summary>
        /// Occurs when a message is sent to the cvs server.
        /// </summary>
        event MessageEventHandler RequestMessageEvent;
        /// <summary>
        /// Occurs when a message is received from the cvs server.
        /// </summary>
        event MessageEventHandler ResponseMessageEvent;

        /// <summary>
        /// Occurs when a <see cref="ICSharpCode.SharpCvsLib.Responses.UpdatedResponse"/> 
        /// is sent from the cvs server.
        /// </summary>
        event MessageEventHandler UpdatedResponseMessageEvent;
        /// <summary>
        /// Occurs when a <see cref="ICSharpCode.SharpCvsLib.Responses.SetStaticDirectoryResponse"/> 
        /// is sent from the cvs server.
        /// </summary>
        event MessageEventHandler SetStaticDirectoryResponseMessageEvent;

        /// <summary>
        /// Occurs when a <see cref="ICSharpCode.SharpCvsLib.Responses.ErrorResponse"/> is sent
        /// from the cvs server.
        /// </summary>
        event MessageEventHandler ErrorResponseMessageEvent;

        /// <summary>
        /// Occurs when a message event is sent from an object
        /// implementing <see cref="ICSharpCode.SharpCvsLib.Responses.IResponse"/> that does not
        /// have it's own specific event handler.  If the response is used often enough it 
        /// will be removed from this event handler and moved to it's own specific event handler.
        /// </summary>
        event MessageEventHandler UnspecifiedResponseMessageEvent;

        /// <summary>
        /// Send message
        /// </summary>
        /// <param name="msg"></param>
        void   SendMessage(string msg);

        /// <summary>
        /// Send an error message.
        /// </summary>
        /// <param name="msg"></param>
        void SendErrorMessage(string msg);

        /// <summary>
        /// Send a message to the appropriate response message handler, or the default handler if
        /// the specific response type is not implemented.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="responseType"></param>
        void SendResponseMessage(string msg, Type responseType);

        /// <summary>
        /// The repository object, contains information about
        ///     cvsroot, working directory, etc.
        /// </summary>
        WorkingDirectory Repository {get;}

        /// <summary>
        /// The next file date.
        /// </summary>
        string NextFileDate {get;set;}

        /// <summary>
        /// The next file.
        /// </summary>
        string NextFile {get;set;}

        /// <summary>
        /// Handler for uncompressed files.
        /// </summary>
        IFileHandler UncompressedFileHandler {get;}
    }
}
