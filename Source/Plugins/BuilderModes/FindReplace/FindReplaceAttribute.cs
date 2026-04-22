

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
    internal class FindReplaceAttribute : Attribute
    {

        private string displayname;
        private bool browsebutton;

        public string DisplayName { get { return displayname; } set { displayname = value; } }
        public bool BrowseButton { get { return browsebutton; } set { browsebutton = value; } }

        // Constructor
        public FindReplaceAttribute(string displayname)
        {
            // Initialize
            this.displayname = displayname;
        }

        // String representation
        public override string ToString()
        {
            return displayname;
        }
    }
}
