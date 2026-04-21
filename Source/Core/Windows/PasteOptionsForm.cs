
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

using CodeImp.DoomBuilder.Config;
using System;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
    internal partial class PasteOptionsForm : DelayedForm
    {
        #region ================== Variables

        #endregion

        #region ================== Properties

        public PasteOptions Options { get; private set; }

        #endregion

        #region ================== Constructor

        // Constructor
        public PasteOptionsForm()
        {
            InitializeComponent();

            // Get defaults
            Options = General.Settings.PasteOptions.Copy();
            pasteoptions.Setup(Options);
        }

        #endregion

        #region ================== Events

        // Paste clicked
        private void paste_Click(object sender, EventArgs e)
        {
            Options = pasteoptions.GetOptions();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Cancel clicked
        private void cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion
    }
}
