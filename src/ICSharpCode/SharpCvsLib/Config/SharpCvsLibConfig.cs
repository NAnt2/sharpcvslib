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
//
#endregion

using System;
using System.Xml;
using System.Xml.Serialization;

using ICSharpCode.SharpCvsLib.Config.Logging;

namespace ICSharpCode.SharpCvsLib.Config {

/// <summary>
///     Holds the core configuration settings for sharpcvslib.
/// </summary>
[XmlRoot ("sharpcvslib-config")]
public class SharpCvsLibConfig {

    /// <summary>
    ///     The sub section of this configuration entity in the application
    ///         configuration file.
    /// </summary>
    public const String SUB_SECTION = "sharpcvslib-config";

    int timeout;
    int authSleep;

    bool verbose = false;

    private LogConfig log;

    /// <summary>
    ///     The timeout value for the cvs server connection.
    /// </summary>
    [XmlElement ("timeout", typeof (int))]
    public int Timeout {
        get {return this.timeout;}
        set {this.timeout = value;}
    }

    /// <summary>
    ///     The time between when an authorization request is sent and the
    ///         response is read.  This is used to handle problems from a
    ///         slow network connection or a slow server.
    /// </summary>
    [XmlElement ("auth-sleep", typeof (int))]
    public int AuthSleep {
        get {return this.authSleep;}
        set {this.authSleep = value;}
    }

    /// <summary>
    ///     Set to <code>true</code> if the request/ response commands should
    ///         be sent to a log file.
    /// </summary>
    public bool Verbose {
        get {return this.verbose;}
        set {this.verbose = value;}
    }

    /// <summary>
    /// Logging configuration settings.
    /// </summary>
    [XmlElement ("log", typeof (LogConfig))]
    public LogConfig Log {
        get {return this.log;}
        set {this.log = value;}
    }

    /// <summary>
    /// Create a new instance of the logging configuration.
    /// </summary>
    public SharpCvsLibConfig () {
        this.Log = new LogConfig ();
    }
}
}
