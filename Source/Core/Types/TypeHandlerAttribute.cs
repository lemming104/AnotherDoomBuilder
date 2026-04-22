

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

namespace CodeImp.DoomBuilder.Types
{
    public sealed class TypeHandlerAttribute : Attribute
    {

        private readonly int index;
        private readonly string name;
        private Type type;
        private readonly bool customusable;

        public int Index { get { return index; } }
        public string Name { get { return name; } }
        public bool IsCustomUsable { get { return customusable; } }
        public Type Type { get { return type; } set { type = value; } }

        // Constructor
        public TypeHandlerAttribute(UniversalType index, string name, bool customusable)
        {
            // Initialize
            this.index = (int)index;
            this.name = name;
            this.customusable = customusable;
        }

        // String representation
        public override string ToString()
        {
            return name;
        }
    }
}
