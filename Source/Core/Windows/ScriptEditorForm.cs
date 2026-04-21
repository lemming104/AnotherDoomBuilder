
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

using CodeImp.DoomBuilder.Controls;
using System;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
    public partial class ScriptEditorForm : DelayedForm
    {
        #region ================== Variables

        // Closing?
        private bool appclose;

        #endregion

        #region ================== Properties

        public ScriptEditorPanel Editor { get; private set; }

        #endregion

        #region ================== Constructor

        // Constructor
        public ScriptEditorForm()
        {
            InitializeComponent();
            Editor.Initialize(this);
            KeyPreview = true;
            PreviewKeyDown += new PreviewKeyDownEventHandler(ScriptEditorForm_PreviewKeyDown);
            KeyDown += new KeyEventHandler(ScriptEditorForm_KeyDown);
            KeyUp += new KeyEventHandler(ScriptEditorForm_KeyDown);
        }

        #endregion

        #region ================== Methods

        // This asks to save files and returns the result
        // Also does implicit saves
        // Returns false when cancelled by the user
        public bool AskSaveAll()
        {
            // Implicit-save the script lumps
            Editor.ImplicitSave();

            // Save other scripts
            return Editor.AskSaveAll();
        }

        // Close the window
        new public void Close()
        {
            appclose = true;
            base.Close();
        }

        //mxd
        internal void DisplayError(TextResourceErrorItem error)
        {
            Editor.ShowError(error);
        }

        //mxd
        /*internal void DisplayError(TextFileErrorItem error)
		{
			editor.ShowError(error);
		}*/

        #endregion

        #region ================== Events

        private void ScriptEditorForm_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.F10)
                e.IsInputKey = true;
        }

        private void ScriptEditorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F10)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        // Window is loaded
        private void ScriptEditorForm_Load(object sender, EventArgs e)
        {
            // Apply panel settings
            Editor.ApplySettings();
        }

        // Window is shown
        private void ScriptEditorForm_Shown(object sender, EventArgs e)
        {
            // Focus to script editor
            Editor.ForceFocus();
        }

        // Window is closing
        private void ScriptEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Editor.SaveSettings();

            // Only when closed by the user
            if (!appclose && (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.FormOwnerClosing))
            {
                // Remember if scipts are changed
                General.Map.ApplyScriptChanged();

                // Ask to save scripts
                if (AskSaveAll())
                {
                    // Let the general call close the editor
                    General.Map.CloseScriptEditor(true);
                }
                else
                {
                    // Cancel
                    e.Cancel = true;
                }
            }

            // Not cancelling?
            if (!e.Cancel) Editor.OnClose();
        }

        // Help
        private void ScriptEditorForm_HelpRequested(object sender, HelpEventArgs hlpevent)
        {
            if (!Editor.LaunchKeywordHelp()) General.ShowHelp("w_scripteditor.html"); //mxd
            hlpevent.Handled = true;
        }

        #endregion
    }
}