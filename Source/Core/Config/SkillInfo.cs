
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

namespace CodeImp.DoomBuilder.Config
{
    public class SkillInfo : INumberedTitle, IComparable<SkillInfo>
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // Properties
        #endregion

        #region ================== Properties

        public int Index { get; }
        public string Title { get; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal SkillInfo(int index, string title)
        {
            // Initialize
            this.Index = index;
            this.Title = title;
        }

        #endregion

        #region ================== Methods

        // This presents the item as string
        public override string ToString()
        {
            return Index + " - " + Title;
        }

        // This compares against another skill
        public int CompareTo(SkillInfo other)
        {
            if (this.Index < other.Index) return -1;
            else if (this.Index > other.Index) return 1;
            else return 0;
        }

        #endregion
    }
}
