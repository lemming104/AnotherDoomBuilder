

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


using CodeImp.DoomBuilder.Config;
using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Windows
{
    internal partial class PasteOptionsForm : DelayedForm
    {

        private PasteOptions options;

        public PasteOptions Options { get { return options; } }

        // Constructor
        public PasteOptionsForm()
        {
            InitializeComponent();

            // Get defaults
            options = General.Settings.PasteOptions.Copy();
            pasteoptions.Setup(options);
        }

        // Paste clicked
        private void paste_Click(object sender, EventArgs e)
        {
            options = pasteoptions.GetOptions();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Cancel clicked
        private void cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
