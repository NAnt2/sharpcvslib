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
    public class CvsRoot {
        /// <summary>
        /// Regular expression for matching a cvsroot.
        /// </summary>
        public const string CVSROOT_REGEX = 
              @":(ext|pserver|ssh|local|sspi)
              :((?:[\w]*@)?[\w]+(?:\.[\w|-]+)*)
              :?((?:[\d]*)?)
              :((?:(?:[A-Za-z]:/)|/).[^\s]*)";

        private readonly ILog LOGGER = LogManager.GetLogger(typeof(CvsRoot));
        /// <summary>
        /// Identify the protocols that are currently supported.
        /// 
        /// NOTE: This probably should be replaced by enums.
        /// </summary>
        public class HostProtocol {
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
                LOGGER.Debug(String.Format("Protocol: {0}", value)); 
                AssertNotEmpty(value, "Protocol");
                protocol = value;
            }
        }

        /// <summary>
        /// User name used to access the repository.
        /// </summary>
        public string User {
            get {return user;}
            set {
                LOGGER.Debug(String.Format("User: {0}", value)); 
                user = value;}
        }

        /// <summary>
        /// Host running the repository.
        /// </summary>
        public string Host {
            get {return host;}
            set {
                LOGGER.Debug(String.Format("Host: {0}", value)); 
                AssertNotEmpty(value, "Host");
                host = value;}
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

        /// <summary>
        /// Module to use in command.
        /// </summary>
        public string CvsRepository {
            get {return cvsrepository;}
            set {
                LOGGER.Debug(String.Format("Repository: {0}", value)); 
                AssertNotEmpty(value, "Repository");
                cvsrepository = value;}
        }

        /// <summary>
        /// The port to use to connect to the server.
        /// </summary>
        public int Port {
            get {return port;}
            set {
                LOGGER.Debug(String.Format("Port: {0}", value)); 
                this.port = value;}
        }

        private string PortString {
            set {
                if (value != null || value != String.Empty) {
                    try {
                        this.Port = Convert.ToInt32(value);
                    } catch (FormatException) {
                        LOGGER.Debug(String.Format("Invalid number {0}, using {1} port.",
                            value, SharpCvsLibConfig.DEFAULT_PORT));
                        this.Port = SharpCvsLibConfig.DEFAULT_PORT;
                    }
                }
            }
        }

        /// <summary>
        /// Constructor.  Parses a cvsroot variable passed in as
        ///     a string into the different properties that make it
        ///     up.  The cvsroot can consisit of the following components:
        ///     
        ///     <list type="table">
        ///         <term>protocol:</term>
        ///         <description>pserver, ssh and ext protocols
        ///             <warn>NOTE: Currently unsupported, but valid cvs protocols include: sspi and ntserver</warn>
        ///         </description>
        ///         <term>username:</term>
        ///         <description>the login user for the remote client.  This will be
        ///             used to authenticate the user on the remote machine.
        ///         </description>
        ///         <term>server:</term>
        ///         <description>server that the repository sits on.</description>
        ///         <term>path:</term>
        ///         <description>path to the repository on the server</description>
        ///     </list>
        /// </summary>
        /// <param name="cvsRoot"></param>
        /// <example>
        ///     Cvsroot examples:
        ///     <table>
        ///     
        ///         <item>:pserver:anonymous@cvs.sourceforge.net:/cvsroot/sharpcvslib
        ///         
        ///                 would be parsed as follows:
        ///                 <list>
        ///                     <item>protocol    = pserver (password server protocol)</item>
        ///                     <item>user        = anonymous</item>
        ///                     <item>server      = cvs.sourceforge.net</item>
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
            Regex regex = new Regex(CVSROOT_REGEX, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

            Match matches = regex.Match(cvsRoot);

            LOGGER.Debug(String.Format("Matches count: {0}.", matches.Groups.Count));
            if (!matches.Success) {
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

        /// <summary>
        /// Determine if the given string is a valid cvs root or not.
        /// </summary>
        /// <param name="cvsRoot">A string value that represents a potential cvs root.</param>
        /// <returns><code>true</code> if the string is a valid cvs root, 
        ///     otherwise <code>false</code>.</returns>
        public static bool IsValid (string cvsRoot) {
            Regex regex = new Regex(CVSROOT_REGEX, RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline);

            Match matches = regex.Match(cvsRoot);

            if (!matches.Success) {
                return false;
            }
            return true;
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
        public override string ToString() {
            return ':' + protocol + ':' + user + '@' + host + ':' + cvsrepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() {
            return this.ToString().GetHashCode();
        }


        /// <summary>
        /// <code>true</code> if the two cvs roots are equal, otherwise <code>false</code>.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            if (null == obj) {
                return false;
            }

            if (!obj.GetType().Equals(this.GetType())) {
                return false;
            }

            CvsRoot root1 = (CvsRoot)obj;
            CvsRoot root2 = this;

            LOGGER.Debug(String.Format("root1: {0}; root2: {1}; are equal {2}",
                root1, root2, root1.ToString().Equals(root2.ToString())));
            return root1.ToString() == root2.ToString();
        }

    }
}
