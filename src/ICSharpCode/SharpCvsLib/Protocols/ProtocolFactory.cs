using System;
using System.Collections;

using ICSharpCode.SharpCvsLib.Attributes;
using ICSharpCode.SharpCvsLib.Exceptions;

namespace ICSharpCode.SharpCvsLib.Protocols {
	/// <summary>
	/// Summary description for ProtocolFactory.
	/// </summary>
	public class ProtocolFactory {
        /// <summary>
        /// An instance of the protocol factory.
        /// </summary>
        public static ProtocolFactory Instance = new ProtocolFactory();

        private Hashtable Protocols = new Hashtable();

		public ProtocolFactory() {
            PopulateProtocols();
		}

        private void PopulateProtocols() {
            foreach (Type type in this.GetType().Assembly.GetTypes()) {
                if (!type.IsAbstract && type.IsSubclassOf(typeof(IProtocol))) {
                    ProtocolAttribute protocol = 
                        (ProtocolAttribute)type.GetCustomAttributes(typeof(ProtocolAttribute), false)[0];
                    this.Protocols.Add(protocol.Protocol, type);
                }
            }
        }

        /// <summary>
        /// Indicates if the protocol exists.
        /// </summary>
        /// <param name="protocol"><see langword="true"/> if the protocol exists,
        /// otherwise <see langword="false"/>.</param>
        public bool Exists(string protocol) {
            if (this.Protocols.Contains(protocol)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the specified protocol.  If the protocol does not exist an exception is
        /// thrown.
        /// </summary>
        /// <param name="protocol">String value of the protocol to get.</param>
        /// <returns>The requested implementation of <see cref="IProtocol"/>.</returns>
        /// <exception cref="UnsupportedProtocolException">If the specified protocol does
        /// not exist.</exception>
        public IProtocol GetProtocol(string protocol) {
            if (!this.Exists(protocol)) {
                throw new UnsupportedProtocolException (
                    string.Format("Unknown protocol=[{0}]", protocol));
            }
            return (IProtocol)this.Protocols[protocol];
        }
	}
}
