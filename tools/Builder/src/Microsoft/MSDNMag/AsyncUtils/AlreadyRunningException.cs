using System;

namespace Microsoft.MSDNMag.AsyncUtils {
    /// <summary>
    /// Exception thrown by AsyncOperation if attempt to start
    /// an already running process is attempted.
    /// </summary>
    /// <remarks>From the MSDN Magazine Article "Give Your .NET-based 
    /// Application a Fast and Responsive UI with Multiple Threads" by 
    /// Ian Griffiths. 
    /// See the May 2003 issue at http://msdn.microsoft.com/
    /// MSDN Magazine does not make any representation or warranty, 
    /// express or implied, with respect to any code or other information
    /// herein. 
    /// MSDN Magazine disclaims any liability whatsoever for any use of such
    /// code 
    /// or other information. 
    ///</remarks>
    public class AlreadyRunningException : System.ApplicationException {
        /// <summary>
        /// Constructor.
        /// </summary>
        public AlreadyRunningException() : base("Asynchronous operation already running") { 
        }
    }
}
