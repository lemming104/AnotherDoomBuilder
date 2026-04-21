
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

using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.Plugins;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.VisualModes;
using System;
using System.Drawing;
using System.IO;

#endregion

namespace CodeImp.DoomBuilder.Editing
{
    internal class EditModeInfo : IComparable<EditModeInfo>
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // Mode type

        // Mode switching
        private ActionDelegate switchactiondel;

        // Mode button
        private readonly int buttonorder = int.MaxValue;

        //mxd. Disposing
        private bool isdisposed;

        #endregion

        #region ================== Properties

        public Plugin Plugin { get; private set; }
        public Type Type { get; }
        public bool IsOptional { get { return ((SwitchAction != null) || (ButtonImage != null)) && Attributes.Optional; } }
        public BeginActionAttribute SwitchAction { get; }
        public Image ButtonImage { get; }
        public string ButtonDesc { get; }
        public EditModeAttribute Attributes { get; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        public EditModeInfo(Plugin plugin, Type type, EditModeAttribute attr)
        {
            // Initialize
            this.Plugin = plugin;
            this.Type = type;
            this.Attributes = attr;

            // Make switch action info
            if (!string.IsNullOrEmpty(Attributes.SwitchAction))
                SwitchAction = new BeginActionAttribute(Attributes.SwitchAction);

            // Make button info
            if (!string.IsNullOrEmpty(attr.ButtonImage))
            {
                using (Stream stream = plugin.GetResourceStream(attr.ButtonImage))
                {
                    if (stream != null)
                    {
                        ButtonImage = Image.FromStream(stream);
                        ButtonDesc = attr.DisplayName + (Attributes.IsDeprecated ? " (deprecated)" : "");
                        buttonorder = attr.ButtonOrder;
                    }
                }
            }

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Disposer
        public void Dispose()
        {
            // Not already disposed?
            if (!isdisposed)
            {
                // Dispose
                UnbindSwitchAction();
                if (ButtonImage != null) ButtonImage.Dispose();

                // Clean up
                Plugin = null;

                // Done
                isdisposed = true;
            }
        }

        #endregion

        #region ================== Methods

        // This binds the action to switch to this editing mode
        public void BindSwitchAction()
        {
            if ((switchactiondel == null) && (SwitchAction != null))
            {
                switchactiondel = UserSwitchToMode;
                General.Actions.BindBeginDelegate(Plugin.Assembly, switchactiondel, SwitchAction);
            }
        }

        // This unbind the switch action
        public void UnbindSwitchAction()
        {
            if (switchactiondel != null)
            {
                General.Actions.UnbindBeginDelegate(Plugin.Assembly, switchactiondel, SwitchAction);
                switchactiondel = null;
            }
        }

        // This switches to the mode by user command (when user presses shortcut key)
        public void UserSwitchToMode()
        {
            // Only when a map is opened
            if (General.Map != null)
            {
                //mxd. Not the same mode?
                if (Type != General.Editing.Mode.GetType())
                {
                    // Switching from volatile mode to a different volatile mode?
                    if ((General.Editing.Mode != null) && General.Editing.Mode.Attributes.Volatile && this.Attributes.Volatile)
                    {
                        // First cancel previous volatile mode
                        General.Editing.CancelVolatileMode();
                    }

                    // Create instance
                    EditMode newmode = Plugin.CreateObject<EditMode>(Type);

                    //mxd. Switch mode?
                    if (newmode != null) General.Editing.ChangeMode(newmode);
                }
                // When in VisualMode and switching to the same VisualMode, switch back to the previous classic mode
                else if (General.Editing.Mode is VisualMode)
                {
                    // Switch back to last classic mode
                    General.Editing.ChangeMode(General.Editing.PreviousClassicMode.Name);
                }
                //mxd. Switch between view floor and view ceiling textures?
                else if (General.Editing.Mode is ClassicMode && General.Settings.SwitchViewModes)
                {
                    ClassicMode.SetViewMode(General.Map.Renderer2D.ViewMode == ViewMode.FloorTextures ? ViewMode.CeilingTextures : ViewMode.FloorTextures);
                }
            }
        }

        // This switches to the mode
        public void SwitchToMode()
        {
            // Only when a map is opened
            if (General.Map != null)
            {
                // Create instance
                EditMode newmode = Plugin.CreateObject<EditMode>(Type);

                //mxd. Switch mode?
                if (newmode != null) General.Editing.ChangeMode(newmode);
            }
        }

        // This switches to the mode with arguments
        public void SwitchToMode(object[] args)
        {
            // Only when a map is opened
            if (General.Map != null)
            {
                // Create instance
                EditMode newmode = Plugin.CreateObjectA<EditMode>(Type, args);

                // Switch mode
                if (!General.Editing.ChangeMode(newmode))
                {
                    // When cancelled, dispose mode
                    newmode.Dispose();
                }
            }
        }

        // String representation
        public override string ToString()
        {
            return Attributes.DisplayName;
        }

        // Compare by button order
        public int CompareTo(EditModeInfo other)
        {
            if (this.buttonorder > other.buttonorder) return 1;
            if (this.buttonorder < other.buttonorder) return -1;
            return 0;
        }

        #endregion
    }
}
