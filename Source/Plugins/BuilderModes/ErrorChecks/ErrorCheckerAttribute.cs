

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

namespace CodeImp.DoomBuilder.BuilderModes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ErrorCheckerAttribute : Attribute
    {

        private string displayname;
        private bool defaultchecked;
        private int cost;

        public string DisplayName { get { return displayname; } set { displayname = value; } }
        public bool DefaultChecked { get { return defaultchecked; } set { defaultchecked = value; } }
        public int Cost { get { return cost; } set { cost = value; } }

        // Constructor
        public ErrorCheckerAttribute(string displayname, bool defaultchecked, int cost)
        {
            // Initialize
            this.displayname = displayname;
            this.defaultchecked = defaultchecked;
            this.cost = cost;
        }

        // String representation
        public override string ToString()
        {
            return displayname;
        }
    }
}
