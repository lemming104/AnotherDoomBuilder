
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

namespace CodeImp.DoomBuilder.BuilderModes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class ErrorCheckerAttribute : Attribute
    {
        #region ================== Variables

        #endregion

        #region ================== Properties

        public string DisplayName { get; set; }
        public bool DefaultChecked { get; set; }
        public int Cost { get; set; }

        #endregion

        #region ================== Constructor / Destructor

        // Constructor
        public ErrorCheckerAttribute(string displayname, bool defaultchecked, int cost)
        {
            // Initialize
            this.DisplayName = displayname;
            this.DefaultChecked = defaultchecked;
            this.Cost = cost;
        }

        #endregion

        #region ================== Methods

        // String representation
        public override string ToString()
        {
            return DisplayName;
        }

        #endregion
    }
}
