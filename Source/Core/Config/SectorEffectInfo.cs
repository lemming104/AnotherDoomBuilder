
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
    public class SectorEffectInfo : INumberedTitle, IComparable<SectorEffectInfo>
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // Properties
        #endregion

        #region ================== Properties

        public int Index { get; }
        public string Title { get; }
        public bool IsGeneralized { get; }
        public bool IsKnown { get; }
        public bool IsNull { get { return Index == 0; } }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal SectorEffectInfo(int index, string title, bool isknown, bool isgeneralized)
        {
            // Initialize
            this.Index = index;
            this.Title = title;
            this.IsKnown = isknown;
            this.IsGeneralized = isgeneralized;

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        #endregion

        #region ================== Methods

        // This presents the item as string
        public override string ToString()
        {
            return Index + " - " + Title;
        }

        // This compares against another action info
        public int CompareTo(SectorEffectInfo other)
        {
            if (this.Index < other.Index) return -1;
            else if (this.Index > other.Index) return 1;
            else return 0;
        }

        #endregion
    }
}
