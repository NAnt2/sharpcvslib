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
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;

using log4net;

namespace ICSharpCode.SharpCvsLib.Config.Tests {

    /// <summary>
    ///     Encapsulates the configuration file.  If the default settings cannot
    ///         be loaded then the default values from the TestConstants are used.
    /// </summary>
    public class TestSettings {
        
        private SharpCvsLibTestsConfig config;
        private readonly ILog LOGGER = LogManager.GetLogger (typeof (TestSettings));
        
        /// <summary>
        /// The configuration settings for the #cvslib tests.
        /// </summary>
        public SharpCvsLibTestsConfig Config {
            get {return this.config;}
        }

        /// <summary>
        ///  Attempts to fetch the application configuration file.  If it cannot
        ///     be retrieved then a configuration file is created with the
        ///     default values found in the TestConstants class.    
        /// </summary>
        public TestSettings () {
            try {
                this.config = 
                    (SharpCvsLibTestsConfig)ConfigurationSettings.GetConfig 
                        (SharpCvsLibTestsConfigHandler.APP_CONFIG_SECTION);
                
                if (null == this.config) {
                    this.config = new SharpCvsLibTestsConfig ();
                }
            } catch (Exception e) {
                LOGGER.Error ("Unable to load config file, loading default settings.", e);
                // The default values are initialized in the config file.
                this.config = new SharpCvsLibTestsConfig ();
            }
        }
    }
}
