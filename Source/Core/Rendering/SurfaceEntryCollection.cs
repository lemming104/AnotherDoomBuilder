

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


using System.Collections.Generic;

namespace CodeImp.DoomBuilder.Rendering
{
    // This contains information to update surface entries with. This may exceed the maximum number
    // of sector vertices, the surface manager will take care of splitting it up in several SurfaceEntries.
    internal class SurfaceEntryCollection : List<SurfaceEntry>
    {
        public int totalvertices;
    }
}
