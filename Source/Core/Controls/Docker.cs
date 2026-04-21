
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

using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
    public class Docker
    {
        #region ================== Variables

        #endregion

        #region ================== Variables

        public string Name { get; }
        internal string FullName { get; private set; }
        public string Title { get; }
        public Control Control { get; }

        #endregion

        #region ================== Constructor

        // Constructor
        public Docker(string name, string title, Control control)
        {
            this.Name = name;
            this.Title = title;
            this.Control = control;
        }

        #endregion

        #region ================== Methods

        // This makes the full name
        internal void MakeFullName(string prefix)
        {
            FullName = prefix + "_" + Name;
        }

        #endregion
    }
}
