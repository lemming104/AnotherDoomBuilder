

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

namespace CodeImp.DoomBuilder.Controls
{
    public class Docker
    {

        private string shortname;
        private string fullname;
        private string title;
        private Control control;

        public string Name { get { return shortname; } }
        internal string FullName { get { return fullname; } }
        public string Title { get { return title; } }
        public Control Control { get { return control; } }

        // Constructor
        public Docker(string name, string title, Control control)
        {
            this.shortname = name;
            this.title = title;
            this.control = control;
        }

        // This makes the full name
        internal void MakeFullName(string prefix)
        {
            fullname = prefix + "_" + shortname;
        }
    }
}
