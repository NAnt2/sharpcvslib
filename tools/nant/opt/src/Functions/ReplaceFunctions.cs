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

namespace Sporadicism.NAntExtras.Functions {
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
    [FunctionSet("Replace", "ReplaceFunctions")]
    public class ReplaceFunctions : FunctionSetBase {
        public ReplaceFunctions(Project project, PropertyDictionary properties) :
            base (project, properties){
        }

        /// <summary>
        /// Replace all instance of the given string in the input file.
        /// </summary>
        /// <param name="inputFile">Input file.</param>
        /// <param name="searchString">String to search for in the input file.</param>
        /// <param name="replaceString">String to replace the search string with.</param>
        /// <returns></returns>
        [Function("String")]
        public static string String(string inputFile, string searchString, string replaceString) {
            return String(inputFile, inputFile, searchString, replaceString);
        }

        /// <summary>
        /// Replace all instances of the given string in the file specified.
        /// </summary>
        /// <param name="inputFile">Name of the input file.</param>
        /// <param name="outputFile">The name of the output file.</param>
        /// <param name="replaceString">The replacement string.</param>
        /// <param name="searchString">The string that will be repalced.</param>
        /// <returns>The process id of the given process.</returns>
        [Function("String")]
        public static string String(string inputFile, string outputFile, string searchString, string replaceString) {
            if (!Path.IsPathRooted(inputFile)) {
                inputFile = Path.Combine(Directory.GetCurrentDirectory(), inputFile);
            }
            if (!Path.IsPathRooted(outputFile)) {
                outputFile = Path.Combine(Directory.GetCurrentDirectory(), outputFile);
            }

            string inputFileContents;
            using (StreamReader reader = new StreamReader(inputFile)) {
                inputFileContents = reader.ReadToEnd();
                reader.Close();
            }

            string outputFileContents = 
                inputFileContents.Replace(searchString, replaceString);

            using (StreamWriter writer = new StreamWriter(outputFile)) {
                writer.Write(outputFileContents);
                writer.Close();
            }

            return outputFile;
        }
    }
}
