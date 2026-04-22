

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
    public class SectorEffectInfo : INumberedTitle, IComparable<SectorEffectInfo>
    {

        // Properties
        private int index;
        private string title;
        private bool isknown;
        private bool isgeneralized;

        public int Index { get { return index; } }
        public string Title { get { return title; } }
        public bool IsGeneralized { get { return isgeneralized; } }
        public bool IsKnown { get { return isknown; } }
        public bool IsNull { get { return index == 0; } }

        // Constructor
        internal SectorEffectInfo(int index, string title, bool isknown, bool isgeneralized)
        {
            // Initialize
            this.index = index;
            this.title = title;
            this.isknown = isknown;
            this.isgeneralized = isgeneralized;

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // This presents the item as string
        public override string ToString()
        {
            return index + " - " + title;
        }

        // This compares against another action info
        public int CompareTo(SectorEffectInfo other)
        {
            if (this.index < other.index) return -1;
            else if (this.index > other.index) return 1;
            else return 0;
        }
    }
}
