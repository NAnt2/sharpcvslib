#region "Copyright"
/*
The MIT License

Copyright (c) 2004-2005 Clayton Harbour

Permission is hereby granted, free of charge, to any person obtaining a copy of this 
software and associated documentation files (the "Software"), to deal in the Software 
without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or 
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.

 */
#endregion "Copyright"

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Sporadicism.Builder.Runner;

namespace Sporadicism.Builder.NAnt {
	/// <summary>
	/// Summary description for NAntTargets.
	/// </summary>
    public class NAntTargets {
        public static NAntTargets Instance = new NAntTargets();

        public static DirectoryInfo BuildDir {
            get { return NAntRunner.NAntPath.Directory; }
        }

		public NAntTargets() {
		}
    
        public Target[] GetTargets () {
            Hashtable targets = new Hashtable();
            GetTargets(BuildDir, targets);
            return (Target[])new ArrayList(targets.Values).ToArray(typeof(Target));
        }

        private void GetTargets(DirectoryInfo dir, Hashtable targets) {
            foreach (FileInfo buildFile in BuildDir.GetFiles("*.xml")) {
                ParseTargets (buildFile, targets);
            }
            foreach (FileInfo buildFile in BuildDir.GetFiles("*.build")) {
                ParseTargets (buildFile, targets);
            }

            foreach (DirectoryInfo subDir in dir.GetDirectories()) {
                this.GetTargets(subDir, targets);
            }
        }

        private void ParseTargets(FileInfo buildFile, Hashtable targets) {
            XmlDocument doc = new XmlDocument();
            doc.Load(buildFile.FullName);
            foreach (XmlNode node in doc.SelectNodes("//target")) {
                if (node.Attributes["name"].Value != "*" &&
                    !targets.Contains(node.Attributes["name"].Value)) {
                    Target target = new Target();
                    target.File = buildFile;
                    target.Name = node.Attributes["name"].Value;
                    targets.Add(target.Name, target);
                }
            }
        }
	}
}
