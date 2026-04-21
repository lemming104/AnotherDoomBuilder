
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

using System;

#endregion

namespace CodeImp.DoomBuilder.Editing
{
    /// <summary>
    /// This registers an EditMode derived class as a known editing mode within Doom Builder.
    /// Allows automatic binding with an action and a button on the toolbar/menu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class EditModeAttribute : Attribute
    {
        #region ================== Variables

        // Properties
        #endregion

        #region ================== Properties

        /// <summary>
        /// Sets the action name (as defined in the Actions.cfg resource) to
        /// switch to this mode by using a shortcut key, toolbar button or menu item.
        /// </summary>
        public string SwitchAction { get; set; }

        /// <summary>
        /// Image resource name of the embedded resource that will be used for the
        /// toolbar button and menu item. Leave this property out or set to null to
        /// display no button for this mode.
        /// </summary>
        public string ButtonImage { get; set; }

        /// <summary>
        /// Sorting number for the order of buttons on the toolbar. Buttons with
        /// lower values will be more to the left than buttons with higher values.
        /// </summary>
        public int ButtonOrder { get; set; }

        /// <summary>
        /// Grouping name for buttons on the toolbar. Groups are sorted alphabetically.
        /// </summary>
        public string ButtonGroup { get; set; } = "~none";

        /// <summary>
        /// When set to false, this mode will always be available for use and the user cannot
        /// change this in the game configuration.
        /// </summary>
        public bool Optional { get; set; } = true;

        /// <summary>
        /// Set this to true to select this editing mode for use in all game configurations
        /// by default. This only applies the first time and can still be changed by the user.
        /// THIS OPTION MAY BE INTRUSIVE TO THE USER, USE WITH GREAT CARE!
        /// </summary>
        public bool UseByDefault { get; set; }

        /// <summary>
        /// When set to true, this mode is cancelled when core actions like
        /// undo and save are performed. The editing mode should then return to
        /// a non-volatile mode.
        /// </summary>
        public bool Volatile { get; set; }

        /// <summary>
        /// Name to display in the game configuration editing modes list and on the
        /// information bar when the mode is currently active.
        /// </summary>
        public string DisplayName { get; set; } = "<unnamed mode>";

        /// <summary>
        /// When set to false, the actions Cut, Copy and Paste cannot be used
        /// in this mode. Default for this property is true.
        /// </summary>
        public bool AllowCopyPaste { get; set; } = true;

        /// <summary>
        /// Set this to true when it is safe to have the editor start in this mode when
        /// opening a map. The user can then select this as starting mode in the configuration.
        /// </summary>
        public bool SafeStartMode { get; set; }

        /// <summary>
        /// List of map formats this mode can work with. Null means all map formats are supported (mxd)
        /// </summary>
        public string[] SupportedMapFormats { get; set; }

        /// <summary>
        /// List of required map features to make the mode usable. Uses strings of GameConfiguration class properties
        /// </summary>
        public string[] RequiredMapFeatures { get; set; }

        /// <summary>
        /// When set to true the DeprecationMessage will be shown as a warning in the errors and warnings dialog
        /// </summary>
        public bool IsDeprecated { get; set; } = false;

        /// <summary>
        /// Message to be shown as a warning in the errors and warnings dialog when IsDeprecated is true
        /// </summary>
        public string DeprecationMessage { get; set; } = string.Empty;

        #endregion

        #region ================== Constructor / Disposer

        /// <summary>
        /// This registers an EditMode derived class as a known editing mode within Doom Builder.
        /// Allows automatic binding with an action and a button on the toolbar/menu.
        /// </summary>
        public EditModeAttribute()
        {
            // Initialize
        }

        #endregion

        #region ================== Methods

        #endregion
    }
}
