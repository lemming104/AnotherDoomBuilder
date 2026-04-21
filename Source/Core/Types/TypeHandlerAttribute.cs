
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

namespace CodeImp.DoomBuilder.Types
{
    public sealed class TypeHandlerAttribute : Attribute
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        #endregion

        #region ================== Properties

        public int Index { get; }
        public string Name { get; }
        public bool IsCustomUsable { get; }
        public Type Type { get; set; }

        #endregion

        #region ================== Constructor / Destructor

        // Constructor
        public TypeHandlerAttribute(UniversalType index, string name, bool customusable)
        {
            // Initialize
            this.Index = (int)index;
            this.Name = name;
            this.IsCustomUsable = customusable;
        }

        #endregion

        #region ================== Methods

        // String representation
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
