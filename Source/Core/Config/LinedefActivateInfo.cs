

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
    public class LinedefActivateInfo : IComparable<LinedefActivateInfo>
    {

        // Properties
        private int intkey;
        private string key;
        private string title;
        private bool istrigger;

        public int Index { get { return intkey; } }
        public string Key { get { return key; } }
        public string Title { get { return title; } }
        public bool IsTrigger { get { return istrigger; } }

        // Constructor
        internal LinedefActivateInfo(string key, string title, bool istrigger)
        {
            // Initialize
            this.key = key;
            this.title = title;
            this.istrigger = istrigger;

            // Try parsing key as int for comparison
            if (!int.TryParse(key, out intkey)) intkey = 0;

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // This presents the item as string
        public override string ToString()
        {
            return title;
        }

        // This compares against another activate info
        public int CompareTo(LinedefActivateInfo other)
        {
            if (this.intkey < other.intkey) return -1;
            else if (this.intkey > other.intkey) return 1;
            else return 0;
        }
    }
}
