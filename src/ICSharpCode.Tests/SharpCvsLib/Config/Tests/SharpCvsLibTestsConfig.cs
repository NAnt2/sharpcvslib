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

using ICSharpCode.SharpCvsLib.Util;

namespace ICSharpCode.SharpCvsLib.Config.Tests {
    
    /// <summary>
    ///     Holds the core configuration settings for sharpcvslib.
    /// </summary>
    [XmlRoot ("sharpcvslib-tests-config")]
    public class SharpCvsLibTestsConfig {
        
        /// <summary>
        ///     The sub section of this configuration entity in the application 
        ///         configuration file.
        /// </summary>
        public const String SUB_SECTION = "sharpcvslib-tests-config";
        
        String localPath = TestConstants.LOCAL_PATH;
        String cvsroot = TestConstants.CVSROOT;
        String module = TestConstants.MODULE;
        String validPassword = TestConstants.PASSWORD_VALID;
        String invalidPassword = TestConstants.PASSWORD_INVALID;
        String targetFile = TestConstants.TARGET_FILE;
        String targetDirectory = TestConstants.TARGET_DIRECTORY;
        String overrideDirectory = TestConstants.OVERRIDE_DIRECTORY;
        String tag1 = TestConstants.Revision.TAG_1;
        String tag2 = TestConstants.Revision.TAG_2;
        
        /// <summary>
        /// The cvsroot of the repository to target.
        /// </summary>
        [XmlElement ("cvsroot", typeof (String))]
        public String Cvsroot {
            get {return this.cvsroot;}
            set {this.cvsroot = value;}
        }

        /// <summary>
        /// The module directory that will be used to test the application.
        /// </summary>
        [XmlElement ("module", typeof (String))]        
        public String Module {
            get {return this.module;}
            set {this.module = value;}
        }
        
        /// <summary>A valid password to use against the repository.</summary>
        [XmlElement ("valid-password", typeof (String))]        
        public String ValidPassword {
            get {return this.validPassword;}
            set {this.validPassword = value;}
        }
        
        /// <summary>An invalid password to use against the repository.</summary> 
        [XmlElement ("invalid-password", typeof (String))]        
        public String InvalidPassword {
            get {return this.invalidPassword;}
            set {this.invalidPassword = value;}
        }
        
        /// <summary>A known file that exists in the repository.</summary>
        [XmlElement ("target-file", typeof (String))]        
        public String TargetFile {
            get {return this.targetFile;}
            set {this.targetFile = value;}
        }
        
        /// <summary>A known directory that exists in the repository.</summary>
        [XmlElement ("target-directory", typeof (String))]        
        public String TargetDirectory {
            get {return this.targetDirectory;}
            set {this.targetDirectory = value;}
        }
        
        /// <summary>A known tag that exists within the repository.</summary>
        [XmlElement ("tag1", typeof (String))]        
        public String Tag1 {
            get {return this.tag1;}
            set {this.tag1 = value;}
        }        
        
        /// <summary>A known tag that exists within the repository.</summary>
        [XmlElement ("tag2", typeof (String))]        
        public String Tag2 {
            get {return this.tag2;}
            set {this.tag2 = value;}
        }
        
        /// <summary>
        /// Directory to replace the module name when the local path is constructed.
        /// </summary>
        [XmlElement ("override-directory", typeof (String))]
        public String OverrideDirectory {
            get {return this.overrideDirectory;}
            set {this.overrideDirectory = value;}
        }
        
        /// <summary>
        ///     The path to the local test location.  Currently no override
        ///         for this in the config file.
        /// </summary>
        public String LocalPath {
            get {return this.localPath;}
        }
        
        /// <summary>Contents expected in file 1.</summary>
        public String Content1 {
            get {return TestConstants.Revision.CONTENT_1;}
        }
        
        /// <summary>Contents expected in file 2.</summary>
        public String Content2 {
            get {return TestConstants.Revision.CONTENT_2;}
        }
        
        /// <summary>Output the property settings.</summary>
        /// <returns>A String that represents the configured properties.</returns>
        public override String ToString () {
            ToStringFormatter formatter = new ToStringFormatter ("SharpCvsLibTestsConfig");
            
            formatter.AddProperty ("Content1", this.Content1);
            formatter.AddProperty ("Content2", this.Content2);
            formatter.AddProperty ("Cvsroot", this.Cvsroot);
            formatter.AddProperty ("InvalidPassword", this.InvalidPassword);
            formatter.AddProperty ("LocalPath", this.LocalPath);
            formatter.AddProperty ("Module", this.Module);
            formatter.AddProperty ("OverrideDirectory", this.OverrideDirectory);
            formatter.AddProperty ("Tag1", this.Tag1);
            formatter.AddProperty ("Tag2", this.Tag2);
            formatter.AddProperty ("TargetDirectory", this.TargetDirectory);
            formatter.AddProperty ("TargetFile", this.TargetFile);
            formatter.AddProperty ("ValidPassword", this.ValidPassword);
            
            return formatter.ToString ();
        }
        
        public SharpCvsLibTestsConfig () {
            localPath = TestConstants.LOCAL_PATH;
            cvsroot = TestConstants.CVSROOT;
            module = TestConstants.MODULE;
            validPassword = TestConstants.PASSWORD_VALID;
            invalidPassword = TestConstants.PASSWORD_INVALID;
            targetFile = TestConstants.TARGET_FILE;
            targetDirectory = TestConstants.TARGET_DIRECTORY;
            overrideDirectory = TestConstants.OVERRIDE_DIRECTORY;
            tag1 = TestConstants.Revision.TAG_1;
            tag2 = TestConstants.Revision.TAG_2;
        }
        
    }
}
