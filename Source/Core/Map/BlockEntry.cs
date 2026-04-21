
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

using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.Map
{
    public class BlockEntry
    {
        #region ================== Variables

        // Members
        #endregion

        #region ================== Properties

        public List<Linedef> Lines { get; }
        public List<Thing> Things { get; }
        public List<Sector> Sectors { get; }
        public List<Vertex> Vertices { get; } //mxd

        #endregion

        #region ================== Constructor

        // Constructor for empty block
        public BlockEntry()
        {
            Lines = new List<Linedef>(2);
            Things = new List<Thing>(2);
            Sectors = new List<Sector>(2);
            Vertices = new List<Vertex>(2); //mxd
        }

        #endregion
    }
}
