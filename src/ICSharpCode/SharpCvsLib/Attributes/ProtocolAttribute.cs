using System;

namespace ICSharpCode.SharpCvsLib.Attributes {
	/// <summary>
	/// Summary description for ProtocolAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false,Inherited=false)] 
	public class ProtocolAttribute : System.Attribute {
        private string _protocol;

        public string Protocol {
            get { return this._protocol; }
            set { this._protocol = value; }
        }

		public ProtocolAttribute(string protocol) {
            this._protocol = protocol;
		}
	}
}
