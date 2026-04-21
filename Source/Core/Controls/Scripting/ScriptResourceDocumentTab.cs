#region ================== Namespaces

using CodeImp.DoomBuilder.Compilers;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Data.Scripting;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
    //mxd. Document tab bound to a resource entry. Script type can't be changed. Can be readonly.
    //Must be replaced with ScriptFileDocumentTab when unable to locate target resource entry to save to.
    internal sealed class ScriptResourceDocumentTab : ScriptDocumentTab
    {
        #region ================== Variables

        private string hash;
        private string filepathname;

        #endregion

        #region ================== Properties

        public override bool IsReconfigurable { get { return false; } }
        public override bool IsSaveAsRequired { get { return false; } }
        public override string Filename { get { return filepathname; } }
        internal ScriptResource Resource { get; }

        #endregion

        #region ================== Constructor

        internal ScriptResourceDocumentTab(ScriptEditorPanel panel, ScriptResource resource, ScriptConfiguration config) : base(panel, config)
        {
            // Store resource
            Resource = resource;

            // Load the data
            MemoryStream stream = Resource.Resource.LoadFile(Resource.Filename, Resource.LumpIndex);
            if (stream != null)
            {
                hash = MD5Hash.Get(stream);
                editor.SetText(stream.ToArray());
                editor.Scintilla.ReadOnly = Resource.IsReadOnly;
                editor.ClearUndoRedo();
                editor.SetSavePoint();
            }
            else
            {
                General.ErrorLogger.Add(ErrorType.Warning, "Failed to load " + Resource.ScriptType + " resource \"" + Resource.Filename + "\" from \"" + Resource.Resource.Location.GetDisplayName() + "\".");
            }

            // Set title and tooltip
            tabtype = ScriptDocumentTabType.RESOURCE;
            filepathname = Resource.FilePathName;
            SetTitle(Resource.ToString());
            this.ToolTipText = filepathname;

            // Update navigator
            panel.ShowErrors(UpdateNavigator(), true);
        }

        #endregion

        #region ================== Methods

        public override void Compile()
        {
            List<CompilerError> errors = new List<CompilerError>();
            DataReader reader = Resource.Resource;
            if (reader != null && reader.CompileLump(Resource.Filename, Resource.LumpIndex, config, errors))
            {
                // Update script navigator
                errors.AddRange(UpdateNavigator());
            }

            // Feed errors to panel
            panel.ShowErrors(errors, false);
        }

        // This checks if a script error applies to this script
        public override bool VerifyErrorForScript(CompilerError e)
        {
            return string.Compare(e.filename, Resource.Filename, true) == 0;
        }

        // This saves the document (used for both explicit and implicit)
        // Return true when successfully saved
        public override bool Save()
        {
            if (Resource.IsReadOnly || !editor.IsChanged) return false;

            // [ZZ] remove trailing whitespace
            RemoveTrailingWhitespace();

            // Find lump, check it's hash
            bool dosave = true;
            DataReader reader = Resource.Resource;
            // reload the reader
            if (reader.FileExists(Resource.Filename, Resource.LumpIndex))
            {
                using (MemoryStream ms = reader.LoadFile(Resource.Filename, Resource.LumpIndex))
                {
                    if (MD5Hash.Get(ms) != hash
                        && MessageBox.Show("Target lump was modified by another application. Do you still want to replace it?", "Warning", MessageBoxButtons.OKCancel)
                        == DialogResult.Cancel)
                    {
                        dosave = false;
                    }
                }
            }

            if (dosave)
            {
                // Store the lump data
                using (MemoryStream stream = new MemoryStream(editor.GetText()))
                {
                    if (reader.SaveFile(stream, Resource.Filename, Resource.LumpIndex))
                    {
                        // Update what must be updated
                        hash = MD5Hash.Get(stream);
                        editor.SetSavePoint();
                        UpdateTitle();
                    }
                }
            }

            return dosave;
        }

        internal override ScriptDocumentSettings GetViewSettings()
        {
            // Store resource location
            var settings = base.GetViewSettings();
            DataReader reader = Resource.Resource;
            if (reader != null)
            {
                settings.ResourceLocation = reader.Location.location;
                settings.Filename = Path.Combine(reader.Location.location, filepathname); // Make unique location
            }
            return settings;
        }

        //mxd. Check if resource still exists
        internal void OnReloadResources()
        {
            DataReader reader = Resource.Resource;
            if (reader == null)
            {
                // Ask script editor to replace us with ScriptFileDocumentTab
                panel.OnScriptResourceLost(this);
            }
            else
            {
                // Some paths may need updating...
                filepathname = Resource.FilePathName;
                this.ToolTipText = filepathname;
            }
        }

        #endregion
    }
}
