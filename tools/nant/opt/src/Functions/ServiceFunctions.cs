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
using System.ServiceProcess;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Functions;

namespace Sporadicism.NAntExtras.Functions {
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	[FunctionSet("Service", "ServiceFunctions")]
	public class ServiceFunctions : FunctionSetBase {
		public ServiceFunctions(Project project, PropertyDictionary properties) :
            base (project, properties){
		}

        /// <summary>
        /// Indicate if the service is running or not.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns><see langword="false" /> if the service is stopped or
        ///     paused.  If the service is in any other <see cref="ServiceControllerStatus">
        ///     state</see> returns
        ///     <see langword="true"/>.</returns>
        [Function("IsRunning")]
        public static bool IsRunning(string serviceName) {
            ServiceController serviceController = new ServiceController(serviceName);
            if (serviceController.Status != ServiceControllerStatus.Stopped ||
                serviceController.Status != ServiceControllerStatus.Paused) {
                return true;
            } 
            return false;
        }
	}
}
