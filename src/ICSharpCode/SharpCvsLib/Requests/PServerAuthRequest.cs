#region "Copyright"
// PServerAuthRequest.cs
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
// As a special exception, if you link this library with other files to
// produce an executable, this library does not by itself cause the
// resulting executable to be covered by the GNU General Public License.
// This exception does not however invalidate any other reasons why the
// executable file might be covered by the GNU General Public License.
#endregion

using System;
using ICSharpCode.SharpCvsLib.Misc;

using log4net;

namespace ICSharpCode.SharpCvsLib.Requests {

/// <summary>
/// this isn't an official request, this is the authorization for the
/// pserver protocol.
/// </summary>
public class PServerAuthRequest : AbstractRequest
{
    private string cvsroot;
    private string username;
    private string password;

    private readonly ILog LOGGER =
        LogManager.GetLogger (typeof (PServerAuthRequest));

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="cvsroot"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public PServerAuthRequest(string cvsroot, string username, string password)
    {
        this.cvsroot  = cvsroot;
        this.username = username;
        this.password = password;
    }

    /// <summary>
    /// Authorization for the pserver protocol.
    /// </summary>
    public override string RequestString {
        get {
            if (LOGGER.IsDebugEnabled) {
            String msg = "Password Scrambled=[" +
                         PasswordScrambler.Scramble (password) +
                             "]";

                LOGGER.Debug (msg);
            }
            return "BEGIN AUTH REQUEST\n" +
                   cvsroot + "\n" +
                   username + "\n" +
                   PasswordScrambler.Scramble(password) + "\n" +
                   "END AUTH REQUEST\n";
        }
    }

    /// <summary>
    /// <code>false</code>, a response is not expected.
    /// </summary>
    public override bool IsResponseExpected {
        get {
            return false;
        }
    }
}
}
