#region Copyright
// Entry.cs 
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
//    Author:     Clayton Harbour 
#endregion

using System;
using System.Collections;
using System.IO;
using System.Globalization;

using log4net;

using ICSharpCode.SharpCvsLib.Misc;

namespace ICSharpCode.SharpCvsLib.FileSystem { 
    /// <summary>
    ///     Used to parse out the important parts of the orgainization path 
    ///         response from the cvs server.
    /// </summary>
    public class PathTranslator {
        private readonly ILog LOGGER = 
            LogManager.GetLogger (typeof (PathTranslator));
        
        private String cvsRoot;
        private String relativePath;
        private String filename;
        private String localPath;
        
        public String CvsRoot {
            get {return this.cvsRoot;}
        }
        public String RelativePath {
            get {return this.relativePath;}
        }
        public String Filename {
            get {return this.filename;}
        }
        public String LocalPath {
            get {return this.localPath;}
        }
        public String LocalPathAndFilename {
            get {return Path.Combine (localPath, filename);}
        }
        
        public bool IsDirectory {
            get {return this.filename.Length == 0;}
        }
        /// <summary>
        ///     Constructor, parses out the orgainizational path response from
        ///         the cvs server.
        /// </summary>
        public PathTranslator (WorkingDirectory workingDirectory, String repositoryPath) {
            this.Parse (workingDirectory.CvsRoot.CvsRepository, repositoryPath);
            
            this.localPath = 
                Path.Combine (workingDirectory.LocalDirectory, 
                              this.relativePath);
        }
        
        private void Parse (String cvsRoot, String line) {
            if (LOGGER.IsDebugEnabled) {
                String msg = "Parsing relative server path.  " + 
                    "cvsRoot=[" + cvsRoot + "]" +
                    "line=[" + line + "]";
                LOGGER.Debug (msg);
            }
            this.cvsRoot = cvsRoot;
            String [] splitPath = line.Split ('/');
            this.filename = splitPath[splitPath.Length - 1];
            
            LOGGER.Debug ("filename=[" + filename + "]");
            String tempRelativePath =  
                line.Substring (cvsRoot.Length);
            if (tempRelativePath.Length >= this.filename.Length &&
                this.filename.Length > 0) {
                tempRelativePath = tempRelativePath.Replace (filename, "");
            }
            
            if (tempRelativePath[0].Equals ('/')) {
                this.relativePath = tempRelativePath.Substring (1);
            }
            else {
                this.relativePath = tempRelativePath;
            }
                
        }
        
        public override String ToString () {
            String msg = "OrgPath.ToString()=[" +
                "cvsRoot=[" + this.cvsRoot + "]" +
                "filename=[" + this.filename + "]" +
                "localPath=[" + this.localPath + "]" +
                "localPathAndFilename=[" + this.LocalPathAndFilename + "]" +
                "IsDirectory=[" + this.IsDirectory + "]";
            
            return msg;
        }
    }
}
