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
//    <credit>Credit to Dick Grune, Vrije Universiteit, Amsterdam, for writing
//    the shell-script CVS system that this is based on.  In addition credit
//    to Brian Berliner and Jeff Polk for their work on the cvsnt port of
//    this work. </credit>
//    <author>Steve Kenzell</author>
//    <author>Clayton Harbour</author>
#endregion

using System;

namespace ICSharpCode.SharpCvsLib.Console.Parser {

    /// <summary>Holds a list of the available command line common
    ///     names and synonyms.  Also provides a container for the
    ///     command line variables that are needed for the library.</summary>
    public class CommandNames {

        private Command [] commands =
            {new Command("add",      "ad",       "new"),
             new Command("admin",    "adm",      "rcs"),
             new Command("annotate", "ann",      (string)null),
             //#if defined(SERVER_SUPPORT)
             new Command("authserver",  "pserver",   (string)null),
             //#endif
             new Command("chacl",    "setacl",   "setperm"),
             new Command("checkout", "co",       "get"),
             new Command("chown",     "setowner", (string)null),
             new Command("commit",   "ci",       "com"),
             new Command("diff",     "di",       "dif"),
             new Command("edit",     (string)null,       (string)null),
             new Command("editors",  (string)null,       (string)null),
             new Command("export",   "exp",      "ex"),
             new Command("history",  "hi",       "his"),
             new Command("import",   "im",       "imp"),
             new Command("init",     (string)null,       (string)null),
             new Command("info",      "inf",      (string)null),
             new Command("log",      "lo",       (string)null),
             //#ifdef CLIENT_SUPPORT
             new Command("login",    "logon",    "lgn"),
             new Command("logout",   (string)null,       (string)null),
             //#endif /* CLIENT_SUPPORT */
             new Command("ls",        "dir",       "list"),
             new Command("lsacl",     "lsattr",    "listperm"),
             new Command("passwd",    "password",  "setpass"),
             new Command("rannotate", "rann",      "ra"),
             new Command("rdiff",     "patch",     "pa"),
             new Command("release",   "re",        "rel"),
             new Command("cvs_rename", "ren",      "move"),
             new Command("remove",   "rm",       "delete"),
             new Command("rcsfile",  (string)null,        (string)null),
             new Command("rlog",     "rl",       (string)null),
             new Command("rtag",     "rt",       "rfreeze"),
             //#ifdef SERVER_SUPPORT
             new Command("server",   (string)null,       (string)null),
             //#endif
             new Command("status",   "st",       "cvs_stat"),
             new Command("tag",      "ta",       "freeze"),
             new Command("unedit",   (string)null,       (string)null),
             new Command("update",   "up",       "upd"),
             new Command("version",  "ve",       "ver"),
             new Command("watch",    (string)null,       (string)null),
             new Command("watchers", (string)null,       (string)null),
             new Command("xml",     (string)null,       (string)null)
            } ;

        /// <summary>Holds a list of command objects.</summary>
        public Command [] Commands {
            get {return this.commands;}
        }

        /// <summary>Creates and instance of the commands object.  This object
        ///     holds a list of the commands that can be executed against the repository.
        /// </summary>
        public CommandNames () {
        }
    }
}
