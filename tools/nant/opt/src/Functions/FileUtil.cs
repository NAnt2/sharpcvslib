using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Functions;

using Sporadicism.NAntExtras.Runner;

namespace Sporadicism.NAntExtras.Functions {
	/// <summary>
	/// Generate guid ids.
	/// </summary>
	[FunctionSet("FileUtil", "FileUtilFunctions")]
	public class FileUtilFunctions : FunctionSetBase {
		public FileUtilFunctions(Project project, PropertyDictionary properties) :
            base (project, properties){
		}

        /// <summary>
        /// Read the given file and return the contents as a string.
        /// </summary>
        /// <param name="target">The name of the file to read.</param>
        /// <returns>The contents of the file.</returns>
        [Function("Read")]
        public static string Read(string filePath) {
            if (!File.Exists(filePath)) {
                return "An opportunity to practice tolerance: file does not exist.";
            }

            string contents;
            using (StreamReader reader = new StreamReader(filePath)) {
                contents = reader.ReadToEnd();
            }
            return contents;
        }
	}
}
