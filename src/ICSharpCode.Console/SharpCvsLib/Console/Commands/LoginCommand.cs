#region "Copyright"
//
// Copyright (C) 2003 Steve Kenzell
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
//    <author>Steve Kenzell</author>
#endregion

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using ICSharpCode.SharpCvsLib.Client;
using ICSharpCode.SharpCvsLib.Commands;
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Console;
using ICSharpCode.SharpCvsLib.Console.Parser;
using ICSharpCode.SharpCvsLib.FileSystem;
using ICSharpCode.SharpCvsLib.Protocols;

using log4net;

namespace ICSharpCode.SharpCvsLib.Console.Commands {

    /// <summary>
    /// Login to a cvs repository.
    /// </summary>
    public class LoginCommand : ICSharpCode.SharpCvsLib.Commands.LoginCommand {
        /// <summary>
        /// Login to a cvs repository.
        /// </summary>
        /// <param name="cvsRoot">User information</param>
        public LoginCommand(string cvsRoot) : base(new CvsRoot(cvsRoot)) {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cvsRoot"></param>
        public LoginCommand (CvsRoot cvsRoot) : base(cvsRoot) {
        }

        /// <summary>
        /// Login to a cvs repository with workDirectory object
        /// </summary>
        /// <param name="cvsRoot">The repository root.</param>
        /// <param name="workingDirectory">User information</param>
        public LoginCommand(CvsRoot cvsRoot, WorkingDirectory workingDirectory) :
            base (cvsRoot, workingDirectory){
        }
    }
}
