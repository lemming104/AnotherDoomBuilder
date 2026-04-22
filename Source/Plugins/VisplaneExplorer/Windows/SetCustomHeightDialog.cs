
/*
 * Copyright (c) 2021 Derek MacDonald
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Plugins.VisplaneExplorer.Windows
{
    public partial class SetCustomHeightDialog : Form
    {

        private int customheight;

        public SetCustomHeightDialog()
        {
            InitializeComponent();
        }

        public int CustomHeight { get { return customheight; } set { customheight = value; } }

        // Redraw the display using a user-entered view height. Blank input resets to the default.
        private void apply_Clicked(object sender, EventArgs e)
        {
            customheight = input.GetResult(0);

            if (customheight > 32767) customheight = 0;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Auto-focus on the view height input when the dialog pops up.
        private void Dialog_Shown(object sender, EventArgs e)
        {
            input.Focus();
            input.SelectAll();
        }
    }
}
