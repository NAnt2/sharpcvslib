#region "Copyright"
// CvsRoot.cs
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
//  <author>Clayton Harbour</author>
#endregion

using System;
using System.Text;
using System.Text.RegularExpressions;

using ICSharpCode.SharpCvsLib.Config;

using log4net;

namespace ICSharpCode.SharpCvsLib.Misc {

    /// <summary>
    /// Class to encapsulate the properties of the cvsroot for the
    ///     repository you are communicating with.
    /// </summary>
    public class CvsRoot
    {
        private readonly ILog LOGGER = LogManager.GetLogger(typeof(CvsRoot));
        /// <summary>
        /// Identify the protocols that are currently supported.
        /// </summary>
        private class HostProtocol {
            /// <summary>
            /// Password server protocol.
            /// </summary>
            public const String PSERVER = "pserver";
            /// <summary>
            /// Ssh or secure socket handling server.  
            /// </summary>
            public const String SSH     = "ssh";
            /// <summary>
            /// External protocol, used for ssh protocol.
            /// </summary>
            public const String EXT     = "ext";
        }
        private string protocol         = String.Empty;
        private string user             = String.Empty;
        private string host             = String.Empty;
        private int port                = SharpCvsLibConfig.DEFAULT_PORT;
        private string cvsrepository    = String.Empty;

        private const int PROTOCOL_INDEX = 1;

        /// <summary>
        /// The protocol to use when communicating with the server.  Currently supported
        ///     and accepted values are:
        ///     <ol>
        ///         <li>pserver</li>
        ///         <li>ssh</li>
        ///         <li>ext</li>
        ///     </ol>
        /// </summary>
        public string Protocol {
            get {return protocol;}
            set {
                AssertNotEmpty(value, "Protocol");
                if (value.IndexOf(":") < 0) {
                    throw new CvsRootParseException(
                        String.Format("Protocol must contain a :."));
                }
                protocol = StripColan(value);}
        }

        /// <summary>
        /// User name used to access the repository.
        /// </summary>
        public string User {
            get {return user;}
            set {user = StripColan(value);}
        }

        /// <summary>
        /// Host running the repository.
        /// </summary>
        public string Host {
            get {return host;}
            set {
                AssertNotEmpty(value, "Host");
                host = StripColan(value);}
        }

        private string UserHost {
            set {
                if (value.IndexOf("@") > -1) {
                    string[] userHost = value.Split('@');
                    this.User = userHost[0];
                    this.Host = userHost[1];
                } else {
                    this.Host = value;
                }

                if (HasUserVar(this.Protocol)) {
                    AssertNotEmpty(this.User, "User");
                }
            }
        }

        private string StripColan(string value) {
            if (value.IndexOf(":") > -1) {
                value = value.Substring(1, value.Length - 1);
            }
            return value;
        }

        /// <summary>
        /// Module to use in command.
        /// </summary>
        public string CvsRepository {
            get {return cvsrepository;}
            set {
                AssertNotEmpty(value, "Repository");
                cvsrepository = StripColan(value);}
        }

        /// <summary>
        /// The port to use to connect to the server.
        /// </summary>
        public int Port {
            get {return port;}
            set {this.port = value;}
        }

        private string PortString {
            set {
                if (value != null || value != String.Empty) {
                    try {
                        this.Port = Convert.ToInt32(StripColan(value));
                    } catch (FormatException) {
                        LOGGER.Error(String.Format("Invalid number {0}, using default port.",
                            value));
                    }
                }
            }
        }

        /// <summary>
        /// Constructor.  Parses a cvsroot variable passed in as
        ///     a string into the different properties that make it
        ///     up.  The cvsroot can consisit of the following components:
        ///     
        ///         1) protocol: such as the pserver, ssh and ext protocols
        ///             NOTE: Currently unsupported, but valid cvs protocols include: sspi and ntserver
        ///         2) username: the login user for the remote client.  This will be
        ///             used to authenticate the user on the remote machine.
        ///         3) server:  server that the repository sits on.
        ///         4) path:    path to the repository on the server
        ///             
        /// </summary>
        /// <param name="cvsRoot"></param>
        /// <example>
        ///     Cvsroot examples:
        ///             1) :pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib
        ///             
        ///                 would be parsed as follows:
        ///                     protocol    = pserver (password server protocol)
        ///                     user        = anonymous
        ///                     server      = cvs.sourceforge.net
        ///                     port        = 2401 (default port)
        ///                     path        = /cvsroot/sharpcvslib
        ///                     
        ///             2) :pserver:anonymous@cvs.sourceforge.net:80:/cvsroot/sharpcvslib
        ///             
        ///                 would be parsed as follows:
        ///                     protocol    = pserver (password server protocol)
        ///                     user        = anonymous
        ///                     server      = cvs.sourceforge.net
        ///                     port        = 80
        ///                     path        = /cvsroot/sharpcvslib
        ///             
        ///     
        /// </example>
        /// <exception cref="CvsRootParseException">If the cvsroot does not
        ///     translate to a valid cvsroot.</exception>
        public CvsRoot(string cvsRoot) {
            this.Parse (cvsRoot);
            
        }

        /// <summary>
        /// Parse the cvs root.  
        /// </summary>
        /// <param name="cvsRoot">The array of cvs root variables</param>
        /// <exception cref="CvsRootParseException">A parse exception is thrown
        ///     if the cvsroot is not in a format that is recognized.</exception>
        private void Parse (String cvsRoot) {
            Regex regex = new Regex(@"(:ext|:pserver|:ssh|:local|:sspi)
    ([:]{1,1}[\w*@]*[\w*]{1}[\.\-\w*]*)
    ([:]*[\d]*)
    ([:]{1,1}[A-Za-z:/|:/]+[\w*/])", RegexOptions.IgnorePatternWhitespace);

            Match matches = regex.Match(cvsRoot);


            LOGGER.Debug(String.Format("Matches count: {0}.", matches.Groups.Count));
            if (matches.Groups.Count <= 0) {
                throw new CvsRootParseException(String.Format(@"Bad cvsroot. 
    Expected ( :protocol:[usename@]server[:port]:[C:]/path/to/repos ) 
    Found ( {0} )",
                    cvsRoot));
            }
            this.Protocol = matches.Groups[1].Value;
            this.UserHost = matches.Groups[2].Value;
            this.PortString = matches.Groups[3].Value;
            this.CvsRepository = matches.Groups[4].Value;
        }

        private bool HasUserVar (String[] vars) {
            return this.HasUserVar(vars[PROTOCOL_INDEX]);
        }

        private bool HasUserVar (String protocol) {
            if (null == protocol) {
                return false;
            }
            if (protocol.ToLower().Equals(HostProtocol.PSERVER) ||
                protocol.ToLower().Equals(HostProtocol.SSH) ||
                protocol.ToLower().Equals(HostProtocol.EXT)){
                return true;
            }
            return false;
        }

        private void AssertNotEmpty(string value, string fieldName) {
            if (null == value || String.Empty == value) {
                throw new CvsRootParseException(
                    String.Format("{0} must contain a value.", fieldName));
            }
        }

        /// <summary>
        /// Convert CvsRoot object to a human readable format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ':' + protocol + ':' + user + '@' + host + ':' + cvsrepository;
        }
    }
}
