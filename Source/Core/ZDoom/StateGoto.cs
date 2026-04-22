

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

namespace CodeImp.DoomBuilder.ZDoom
{
    internal class StateGoto
    {

        internal string classname;
        internal string statename;
        internal int spriteoffset;

        public string ClassName { get { return classname; } }
        public string StateName { get { return statename; } }
        public int SpriteOffset { get { return spriteoffset; } }

        // Constructor
        internal StateGoto()
        {

        }
    }
}
