

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


using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Actions
{
    internal struct KeyControl
    {

        public int key;
        public string name;

        // Constructor
        public KeyControl(Keys key, string name)
        {
            // Initialize
            this.key = (int)key;
            this.name = name;
        }

        // Constructor
        public KeyControl(SpecialKeys key, string name)
        {
            // Initialize
            this.key = (int)key;
            this.name = name;
        }

        // Constructor
        public KeyControl(int key, string name)
        {
            // Initialize
            this.key = key;
            this.name = name;
        }

        // Returns name
        public override string ToString()
        {
            return name;
        }
    }
}
