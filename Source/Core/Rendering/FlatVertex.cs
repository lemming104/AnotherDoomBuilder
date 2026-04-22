

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

using System.Runtime.InteropServices;

namespace CodeImp.DoomBuilder.Rendering
{
    // FlatVertex
    [StructLayout(LayoutKind.Sequential)]
    public struct FlatVertex
    {
        // Vertex format
        public const int Stride = 24; //6 * 4

        // Members
        public float x;
        public float y;
        public float z;
        public int c;
        public float u;
        public float v;
    }
}
