
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;

#endregion

namespace CodeImp.DoomBuilder.IO
{
    public sealed class UniversalEntry
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        #endregion

        #region ================== Properties

        public string Key { get; }
        public object Value { get; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        public UniversalEntry(string key, object value)
        {
            // Initialize
            this.Key = key;
            this.Value = value;
        }

        #endregion

        #region ================== Methods

        // This checks if the value is of the given type
        // Will throw and exception when it is not
        public void ValidateType(Type t)
        {
            if (Value.GetType() != t) throw new Exception("The value of entry \"" + Key + "\" is of incompatible type (expected " + t.Name + ")");
        }

        //mxd 
        public bool IsValidType(Type t)
        {
            return Value.GetType() == t;
        }

        #endregion
    }
}
