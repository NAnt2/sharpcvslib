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
using System.Diagnostics;
using System.IO;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Functions;

using Sporadicism.NAntExtras.Runner;
using Sporadicism.NAntExtras.Util.Shortcut;

namespace Sporadicism.NAntExtras.Functions {
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
    [FunctionSet("Shortcut", "ShortcutFunctions")]
    public class ShortcutFunctions : FunctionSetBase {
        public ShortcutFunctions(Project project, PropertyDictionary properties) :
            base (project, properties){
        }

        /// <summary>
        /// Show the full path that the shortcut points to.
        /// </summary>
        /// <param name="shortcut">The shortcut file to resolve.</param>
        /// <returns></returns>
        [Function("GetTarget")]
        public static string GetTarget(string shortcut) {
            ShortcutInfo sc = new ShortcutInfo(shortcut);
            try {
                string target = sc.TargetPath.FullName;
                return target;
            } catch (Exception) {
                throw new BuildException(
                    string.Format("Unable to resolve Target for shortcut {0}",
                    shortcut));
            }
        }

        [Function("Create")]
        public static string Create(string target, string shortcut) {
            ShortcutInfo sc = new ShortcutInfo(shortcut);
            sc.WorkingDirectory = new DirectoryInfo(Path.GetDirectoryName(target));
            sc.TargetPath = new FileInfo(target);
            sc.Save();
            return sc.FullName;
        }
	}
}
