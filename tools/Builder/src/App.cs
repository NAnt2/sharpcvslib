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
using System.Reflection;

namespace Sporadicism.Builder {
	/// <summary>
	/// Provides common methods/ properties that describe the application such as name, 
	/// and version.
	/// </summary>
	public class App {
        /// <summary>
        /// Version of the application.
        /// </summary>
        public Version Version {
            get { return this.GetType().Assembly.GetName().Version; }
        }

        /// <summary>
        /// Name of the application.
        /// </summary>
        public string Name {
            get { return this.GetType().Assembly.GetName().Name; }
        }

        public string Title {
            get { return 
                      ((AssemblyTitleAttribute)this.GetType().Assembly.
                      GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0]).Title;
            }
        }

        public string Url {
            get { 
                string url = System.Configuration.ConfigurationSettings.AppSettings["ProjectUrl"]; 
                if (url == null) {
                    url = "http://www.sporadicism.com";
                }
                return url;
            }
        }

        /// <summary>
        /// Static accessor for the application variable.  
        /// </summary>
        public static App Instance = new App();
		private App() {
		}
	}
}
