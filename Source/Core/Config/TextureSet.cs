

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
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.Config
{
    public abstract class TextureSet : IComparable<TextureSet>
    {

        protected string name;
        protected List<string> filters;

        public string Name { get { return name; } set { name = value; } }
        internal List<string> Filters { get { return filters; } }

        protected TextureSet()
        {
            this.name = "Unnamed Set";
            this.filters = new List<string>();
        }

        // This returns the name
        public override string ToString()
        {
            return name;
        }

        // Comparer for sorting alphabetically
        public int CompareTo(TextureSet other)
        {
            return string.Compare(this.name, other.name);
        }
    }
}
