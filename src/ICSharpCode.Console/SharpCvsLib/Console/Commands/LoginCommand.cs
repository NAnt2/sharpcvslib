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
using ICSharpCode.SharpCvsLib.Misc;
using ICSharpCode.SharpCvsLib.Console.Parser;

namespace ICSharpCode.SharpCvsLib.Console.Commands{

/// <summary>
/// Login to a cvs repository.
/// </summary>
public class LoginCommand{

    private string password;
    private string username;

    /// <summary>
    /// The text value of the password that will be used to login.  This should be
    ///     translated into one of the public API command objects.
    /// </summary>
    public String Password {
        get {return this.password;}
    }

    /// <summary>
    /// Login to a cvs repository.
    /// </summary>
    /// <param name="cvsroot">User information</param>
    public LoginCommand(string cvsroot){
        // get cvsroot
        CvsRoot root = new CvsRoot(cvsroot);
        // get username from cvsroot
        username = root.User;
    }

    /// <summary>
    /// Login to a cvs repository with workDirectory object
    /// </summary>
    /// <param name="workingDirectory">User information</param>
    public LoginCommand(WorkingDirectory workingDirectory){
        username = workingDirectory.CvsRoot.User;
        // Is there a password file?
        //     yes, get password for this username
        //     no, prompt user for password to use
    }

    /// <summary>
    /// Process the login command with cvs library API calls
    /// </summary>
    public void Execute (){
        // Is there a password file?
        //     yes, get password for this username
        //     no, prompt user for password to use
        System.Console.Write("CVS password for {0}: ", username);
        password = System.Console.ReadLine();
    }
}
}
