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
	[FunctionSet("Guid", "GuidFunctions")]
	public class GuidFunctions : FunctionSetBase {
		public GuidFunctions(Project project, PropertyDictionary properties) :
            base (project, properties){
		}

        /// <summary>
        /// Start the given process.
        /// </summary>
        /// <param name="target">The name of the process to start.</param>
        /// <returns>The process id of the given process.</returns>
        [Function("NewGuid")]
        public static string NewGuid() {
            return Guid.NewGuid().ToString().ToUpper().Trim();
        }
	}
}
