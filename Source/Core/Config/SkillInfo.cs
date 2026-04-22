

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

namespace CodeImp.DoomBuilder.Config
{
    public class SkillInfo : INumberedTitle, IComparable<SkillInfo>
    {

        // Properties
        private int index;
        private string title;

        public int Index { get { return index; } }
        public string Title { get { return title; } }

        // Constructor
        internal SkillInfo(int index, string title)
        {
            // Initialize
            this.index = index;
            this.title = title;
        }

        // This presents the item as string
        public override string ToString()
        {
            return index + " - " + title;
        }

        // This compares against another skill
        public int CompareTo(SkillInfo other)
        {
            if (this.index < other.index) return -1;
            else if (this.index > other.index) return 1;
            else return 0;
        }
    }
}
