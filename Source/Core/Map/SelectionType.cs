

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

namespace CodeImp.DoomBuilder.Map
{
    [Flags]
    public enum SelectionType
    {
        None = 0,
        Vertices = 1,
        Linedefs = 2,
        Sectors = 4,
        Things = 8,
        All = 0x7FFFFFFF,
    }
}
