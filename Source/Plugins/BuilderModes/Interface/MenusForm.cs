
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
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Windows;
using System;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
    public partial class MenusForm : Form
    {
        #region ================== Variables

        // Menus list
        private readonly ToolStripItem[] menus;

        // mxd. More menus
        private readonly ToolStripItem[] exportmenuitems;

        // mxd. Even more menus!
        private readonly ToolStripItem[] propsmenuitems;

        // Buttons list
        private readonly ToolStripItem[] buttons;

        //mxd
        public struct BrightnessGradientModes
        {
            public const string Sectors = "Sector Brightness";
            public const string Light = "Light Color";
            public const string Fade = "Fade Color";
            public const string LightAndFade = "Light and Fade Colors";
            public const string Floors = "Floor Brightness";
            public const string Ceilings = "Ceiling Brightness";
        }

        //mxd
        internal struct GradientInterpolationModes
        {
            public const string Linear = "Linear";
            public const string EaseInOutSine = "EaseInOutSine";
            public const string EaseInSine = "EaseInSine";
            public const string EaseOutSine = "EaseOutSine";
        }

        #endregion

        #region ================== Properties

        public ToolStripButton ViewSelectionNumbers { get; private set; }
        public ToolStripButton ViewSelectionEffects { get; private set; }
        public ToolStripSeparator SeparatorSectors1 { get; private set; }
        public ToolStripSeparator SeparatorSectors2 { get; private set; } //mxd
        public ToolStripSeparator SeparatorSectors3 { get; private set; } //mxd
        public ToolStripActionButton MakeGradientBrightness { get; private set; }
        public ToolStripActionButton MakeGradientFloors { get; private set; }
        public ToolStripActionButton MakeGradientCeilings { get; private set; }
        public ToolStripActionButton FlipSelectionV { get; private set; }
        public ToolStripActionButton FlipSelectionH { get; private set; }
        public ToolStripActionButton CurveLinedefs { get; private set; }
        public ToolStripActionButton CopyProperties { get; private set; }
        public ToolStripActionButton PasteProperties { get; private set; }
        public ToolStripActionButton PastePropertiesOptions { get; private set; } //mxd
        public ToolStripSeparator SeparatorCopyPaste { get; private set; }
        public ToolStripComboBox GradientModeMenu { get; private set; } //mxd
        public ToolStripComboBox GradientInterpolationMenu { get; private set; } //mxd
        public ToolStripButton MarqueSelectTouching { get; private set; } //mxd
        public ToolStripActionButton AlignThingsToWall { get; private set; } //mxd
        public ToolStripButton TextureOffsetLock { get; private set; } //mxd
        public ToolStripButton TextureOffset3DFloorLock { get; private set; }
        public ToolStripActionButton SyncronizeThingEditButton { get; private set; } //mxd
        public ToolStripMenuItem SyncronizeThingEditSectorsItem { get; private set; } //mxd
        public ToolStripMenuItem SyncronizeThingEditLinedefsItem { get; private set; } //mxd
        public ToolStripActionButton MakeDoor { get; private set; } //mxd

        //mxd. Thing mode radii buttons
        public ToolStripMenuItem ItemLightRadii { get; private set; }
        public ToolStripMenuItem ItemSoundRadii { get; private set; }
        public ToolStripButton ButtonLightRadii { get; private set; }
        public ToolStripButton ButtonSoundRadii { get; private set; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        public MenusForm()
        {
            // Initialize
            InitializeComponent();

            // Apply settings
            ViewSelectionNumbers.Checked = BuilderPlug.Me.ViewSelectionNumbers;
            ViewSelectionEffects.Checked = BuilderPlug.Me.ViewSelectionEffects; //mxd

            //mxd
            GradientModeMenu.Items.AddRange(new[] { BrightnessGradientModes.Sectors, BrightnessGradientModes.Light, BrightnessGradientModes.Fade, BrightnessGradientModes.LightAndFade, BrightnessGradientModes.Ceilings, BrightnessGradientModes.Floors });
            GradientModeMenu.SelectedIndex = 0;
            GradientInterpolationMenu.Items.AddRange(new[] { GradientInterpolationModes.Linear, GradientInterpolationModes.EaseInOutSine, GradientInterpolationModes.EaseInSine, GradientInterpolationModes.EaseOutSine });
            GradientInterpolationMenu.SelectedIndex = 0;

            // List all menus
            menus = new ToolStripItem[menustrip.Items.Count];
            for (int i = 0; i < menustrip.Items.Count; i++) menus[i] = menustrip.Items[i];

            //mxd. Export menu
            exportmenuitems = new ToolStripItem[exportStripMenuItem.DropDownItems.Count];
            for (int i = 0; i < exportStripMenuItem.DropDownItems.Count; i++)
                exportmenuitems[i] = exportStripMenuItem.DropDownItems[i];

            //mxd. Copy-paste propserties items
            propsmenuitems = new ToolStripItem[] { separatorcopyprops, itemcopyprops, itempasteprops, itempastepropsoptions };

            // List all buttons
            buttons = new ToolStripItem[globalstrip.Items.Count];
            for (int i = 0; i < globalstrip.Items.Count; i++) buttons[i] = globalstrip.Items[i];


#if MONO_WINFORMS
			// Mono fix
			menustrip.Items.Clear();
			exportStripMenuItem.DropDownItems.Clear();
			globalstrip.Items.Clear();
			manualstrip.Items.Clear();
			editmenuitem.DropDownItems.Clear();
#endif
        }

        #endregion

        #region ================== Methods

        // This registers with the core
        public void Register()
        {
            // Add the menus to the core
            foreach (ToolStripItem i in menus)
                General.Interface.AddMenu(i);

            // Add the buttons to the core
            foreach (ToolStripItem b in buttons)
                General.Interface.AddButton(b);

            //mxd. Export menu
            foreach (ToolStripItem i in exportmenuitems)
                General.Interface.AddMenu(i, MenuSection.FileExport);

            //mxd. Copy-paste propserties items
            foreach (ToolStripItem i in propsmenuitems)
                General.Interface.AddMenu(i, MenuSection.EditCopyPaste);
        }

        // This unregisters from the core
        public void Unregister()
        {
            // Remove the menus from the core
            foreach (ToolStripItem i in menus)
                General.Interface.RemoveMenu(i);

            // Remove the buttons from the core
            General.Interface.BeginToolbarUpdate(); //mxd
            foreach (ToolStripItem b in buttons)
                General.Interface.RemoveButton(b);
            General.Interface.EndToolbarUpdate(); //mxd

            //mxd. Export menu
            foreach (ToolStripItem i in exportmenuitems)
                General.Interface.RemoveMenu(i);

            //mxd. Copy-paste propserties items
            foreach (ToolStripItem i in propsmenuitems)
                General.Interface.RemoveMenu(i);
        }

        // This hides all menus
        public void HideAllMenus()
        {
            foreach (ToolStripItem m in menus) m.Visible = false;
        }

        // This hides all except one menu
        public void HideAllMenusExcept(ToolStripMenuItem showthis)
        {
            HideAllMenus();
            showthis.Visible = true;
        }

        // This shows the menu for the current editing mode
        public void ShowEditingModeMenu(EditMode mode)
        {
            Type sourcemode = typeof(object);
            if (mode != null)
            {
                sourcemode = mode.GetType();

                // When in a volatile mode, check against the last stable mode
                if (mode.Attributes.Volatile) sourcemode = General.Editing.PreviousStableMode;
            }

            // Final decision
            bool showcopyprops = true; //mxd
            if (sourcemode == typeof(LinedefsMode)) HideAllMenusExcept(linedefsmenu);
            else if (sourcemode == typeof(SectorsMode)) HideAllMenusExcept(sectorsmenu);
            else if (sourcemode == typeof(ThingsMode)) HideAllMenusExcept(thingsmenu); //mxd
            else if (sourcemode == typeof(VerticesMode)) HideAllMenusExcept(vertsmenu); //mxd
            else
            {
                HideAllMenus();
                showcopyprops = false; //mxd
            }

            //mxd. Copy-paste properties items
            foreach (ToolStripItem i in propsmenuitems)
                i.Visible = showcopyprops;
        }

        // This invokes an action from control event
        private void InvokeTaggedAction(object sender, EventArgs e)
        {
            General.Interface.InvokeTaggedAction(sender, e);
        }

        // View selection numbers clicked
        private void buttonselectionnumbers_Click(object sender, EventArgs e)
        {
            BuilderPlug.Me.ViewSelectionNumbers = ViewSelectionNumbers.Checked;

            //mxd. Notify current mode
            BaseClassicMode mode = General.Editing.Mode as BaseClassicMode;
            if (mode != null) mode.OnViewSelectionNumbersChanged(BuilderPlug.Me.ViewSelectionNumbers);

            General.Interface.RedrawDisplay();
            General.Interface.DisplayStatus(StatusType.Info, ViewSelectionNumbers.Checked ?
                "Show selection numbers" :
                "Don't show selection numbers");
        }

        //mxd
        private void buttonselectioneffects_Click(object sender, EventArgs e)
        {
            BuilderPlug.Me.ViewSelectionEffects = ViewSelectionEffects.Checked;

            // Notify current mode
            BaseClassicMode mode = General.Editing.Mode as BaseClassicMode;
            if (mode != null) mode.OnViewSelectionEffectsChanged(BuilderPlug.Me.ViewSelectionEffects);

            General.Interface.RedrawDisplay();
            General.Interface.DisplayStatus(StatusType.Info, ViewSelectionEffects.Checked ?
                "Show sector tags and effects" :
                "Don't show sector tags and effects");
        }

        //mxd
        private void buttonMarqueSelectTouching_Click(object sender, EventArgs e)
        {
            BuilderPlug.Me.MarqueSelectTouching = MarqueSelectTouching.Checked;
            General.Interface.DisplayStatus(StatusType.Info, MarqueSelectTouching.Checked ?
                "Select map elements touching selection rectangle" :
                "Select map elements inside of selection rectangle");
        }

        //mxd
        private void buttonTextureOffsetLock_Click(object sender, EventArgs e)
        {
            BuilderPlug.Me.LockSectorTextureOffsetsWhileDragging = TextureOffsetLock.Checked;
            General.Interface.DisplayStatus(StatusType.Info, TextureOffsetLock.Checked ?
                "Lock texture offsets when dragging sectors" :
                "Don't lock texture offsets when dragging sectors");
        }

        private void buttonTextureOffset3DFloorLock_Click(object sender, EventArgs e)
        {
            BuilderPlug.Me.Lock3DFloorSectorTextureOffsetsWhileDragging = TextureOffset3DFloorLock.Checked;
            General.Interface.DisplayStatus(StatusType.Info, TextureOffset3DFloorLock.Checked ?
                "Lock texture offsets of 3D floors when dragging tagged sectors" :
                "Don't lock texture offsets of 3D floors when dragging tagged sectors");
        }

        //mxd
        private void linedefsmenu_DropDownOpening(object sender, EventArgs e)
        {
            aligntexturesitem.Enabled = General.Map.UDMF;
            updatelightfogitem.Enabled = General.Map.UDMF;
        }

        //mxd
        private void gradientMode_DropDownClosed(object sender, EventArgs e)
        {
            General.Interface.FocusDisplay();
        }

        //mxd
        private void buttonlightradii_Click(object sender, EventArgs e)
        {
            BuilderPlug.Me.ShowLightRadii = !BuilderPlug.Me.ShowLightRadii;
            ButtonLightRadii.Checked = BuilderPlug.Me.ShowLightRadii;
            ItemLightRadii.Checked = BuilderPlug.Me.ShowLightRadii;

            General.Interface.DisplayStatus(StatusType.Info, "Light radii are " + (BuilderPlug.Me.ShowLightRadii ? "SHOWN" : "HIDDEN"));
            General.Interface.RedrawDisplay();
        }

        //mxd
        private void buttonsoundradii_Click(object sender, EventArgs e)
        {
            BuilderPlug.Me.ShowSoundRadii = !BuilderPlug.Me.ShowSoundRadii;
            ButtonSoundRadii.Checked = BuilderPlug.Me.ShowSoundRadii;
            ItemSoundRadii.Checked = BuilderPlug.Me.ShowSoundRadii;

            General.Interface.DisplayStatus(StatusType.Info, "Sound radii are " + (BuilderPlug.Me.ShowSoundRadii ? "SHOWN" : "HIDDEN"));
            General.Interface.RedrawDisplay();
        }

        #endregion
    }
}