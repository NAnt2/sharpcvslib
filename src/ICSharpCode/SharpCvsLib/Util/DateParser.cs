#region "Copyright"
// CVSServerConnection.cs 
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
//                
#endregion

using System;
using System.Globalization;

using log4net;

namespace ICSharpCode.SharpCvsLib.Util {
    /// <summary>
    ///     Utility class to parse a string value into a date.
    /// </summary>
    public class DateParser {
        
        /// <summary>
        ///     Date format for the <code>RFC1123</code> specification.
        /// </summary>	    
	    public const String RFC1123 = 
	        "dd MMM yyyy HH':'mm':'ss '-0000'";
	    /// <summary>
	    ///     A standard cvs date format.
	    /// </summary>
	    public const String FORMAT_1 =
	        "ddd MMM dd HH':'mm':'ss yyyy";
//	        "ddd MMM dd HH:mm:ss yyyy";	    

        /// <summary>
        ///     Private constructor because all accessor methods are going to
        ///         be static public.
        /// </summary>
        private DateParser () {
        }
        
        /// <summary>
        ///     Parse the date string using a number of different potential
        ///         cvs date formats.
        /// </summary>
        public static DateTime ParseCvsDate (String date) {
            DateTime dateTime = DateTime.MinValue;
            
			if (date != null && date.Length > 0)  {
				try {
				    dateTime = DateParser.ParseRFC1123 (date);
				} catch (FormatException) {
					try {
					    dateTime = DateParser.ParseRFC1123WithZero (date);
					} catch (FormatException) {
					    try {
                            dateTime = DateParser.ParseFormat1 (date);
					    } catch (FormatException) {
					        try {
                                dateTime = DateTime.Parse (date);
					        } catch (FormatException e) {
					            dateTime = DateTime.MinValue;
					            // TODO: Determine if this should be removed.
					            throw e;
					        }
					    }
					}
				}				
			}

            return dateTime;            
        }
        
        private static DateTime ParseRFC1123 (String date) {
			return DateTime.ParseExact(date, 
			                                RFC1123,
			                                DateTimeFormatInfo.InvariantInfo);
        }
        
        private static DateTime ParseRFC1123WithZero (String date) {
			return DateTime.ParseExact("0" + date, 
			                                RFC1123, 
			                                DateTimeFormatInfo.InvariantInfo);
        }
        
        private static DateTime ParseFormat1 (String date) {
			return DateTime.ParseExact(date, 
			                                FORMAT_1, 
			                                DateTimeFormatInfo.InvariantInfo);
        }
    }
}
