#region "Copyright"
// Copyright (C) 2005 Clayton Harbour
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
#endregion

using System;

using ICSharpCode.SharpCvsLib.Exceptions;

namespace ICSharpCode.SharpCvsLib.Assertions {
	/// <summary>
	/// Provides a means of checking internal state of the library for validaty.
	/// </summary>
	public class Assert{
		private Assert(){
		}

        public static void True (bool condition) {
            if (condition) {
                return;
            }
            throw new AssertionException("{0}, expected true", condition);
        }

        public static void Equal(object obj1, object obj2) {
            if (obj1.GetType() == obj2.GetType()) {
                if (obj1.Equals(obj2)) {
                    return;
                }
            }
            throw new AssertionException("({0}) should be equal to ({1})", obj1, obj2);
        }

        public static void NotEqual(object obj1, object obj2) {
            if (obj1.GetType() != obj2.GetType()) {
                return; 
            }

            if (!obj1.Equals(obj2)) {
                return;
            }
            throw new AssertionException("({0}) should not be equal to ({1})", obj1, obj2);
        }

        public static void EndsWith(string val, string end) {
            if (val.EndsWith(end)) {
                return;
            }
            throw new AssertionException("({0}) should end with ({1})", val, end);
        }

        public static void NotEndsWith(string val, string end) {
            if (!val.EndsWith(end)) {
                return;
            }
            throw new AssertionException("({0}) should NOT end with ({1})", val, end);
        }
	}
}
