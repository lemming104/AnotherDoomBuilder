

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


using System;

namespace CodeImp.DoomBuilder.IO
{
    public sealed class UniversalEntry
    {

        private string key;
        private object value;

        public string Key { get { return key; } }
        public object Value { get { return value; } }

        // Constructor
        public UniversalEntry(string key, object value)
        {
            // Initialize
            this.key = key;
            this.value = value;
        }

        // This checks if the value is of the given type
        // Will throw and exception when it is not
        public void ValidateType(Type t)
        {
            if (value.GetType() != t) throw new Exception("The value of entry \"" + key + "\" is of incompatible type (expected " + t.Name + ")");
        }

        //mxd 
        public bool IsValidType(Type t)
        {
            return value.GetType() == t;
        }
    }
}
