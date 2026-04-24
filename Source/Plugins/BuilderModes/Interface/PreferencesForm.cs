

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


using CodeImp.DoomBuilder.BuilderModes.General;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Windows;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.Interface
{
    public partial class PreferencesForm : DelayedForm
    {

        // Contrustor
        public PreferencesForm()
        {
            InitializeComponent();

            // Apply current settings to interface
            heightbysidedef.SelectedIndex = DoomBuilder.General.Settings.ReadPluginSetting("changeheightbysidedef", 0);
            editnewthing.Checked = DoomBuilder.General.Settings.ReadPluginSetting("editnewthing", true);
            editnewsector.Checked = DoomBuilder.General.Settings.ReadPluginSetting("editnewsector", false);
            additiveselect.Checked = DoomBuilder.General.Settings.ReadPluginSetting("additiveselect", false);
            additivepaintselect.Checked = DoomBuilder.General.Settings.ReadPluginSetting("additivepaintselect", additiveselect.Checked); // Use the same settign as additive select by default
            stitchrange.Text = DoomBuilder.General.Settings.ReadPluginSetting("stitchrange", 20).ToString();
            highlightrange.Text = DoomBuilder.General.Settings.ReadPluginSetting("highlightrange", 20).ToString();
            highlightthingsrange.Text = DoomBuilder.General.Settings.ReadPluginSetting("highlightthingsrange", 10).ToString();
            splitlinedefsrange.Text = DoomBuilder.General.Settings.ReadPluginSetting("splitlinedefsrange", 10).ToString();
            mouseselectionthreshold.Text = DoomBuilder.General.Settings.ReadPluginSetting("mouseselectionthreshold", 2).ToString();
            splitbehavior.SelectedIndex = (int)DoomBuilder.General.Settings.SplitLineBehavior; //mxd
            autoclearselection.Checked = BuilderPlug.Me.AutoClearSelection;
            visualmodeclearselection.Checked = BuilderPlug.Me.VisualModeClearSelection;
            autodragonpaste.Checked = BuilderPlug.Me.AutoDragOnPaste;
            autoaligntexturesoncreate.Checked = BuilderPlug.Me.AutoAlignTextureOffsetsOnCreate; //mxd
            dontMoveGeometryOutsideBounds.Checked = BuilderPlug.Me.DontMoveGeometryOutsideMapBoundary; //mxd
            syncSelection.Checked = BuilderPlug.Me.SyncSelection; //mxd
            switchviewmodes.Checked = DoomBuilder.General.Settings.SwitchViewModes; //mxd
            autodrawonedit.Checked = BuilderPlug.Me.AutoDrawOnEdit;
            defaultbrightness.Text = DoomBuilder.General.Settings.DefaultBrightness.ToString(); //mxd
            defaultceilheight.Text = DoomBuilder.General.Settings.DefaultCeilingHeight.ToString();//mxd
            defaultfloorheight.Text = DoomBuilder.General.Settings.DefaultFloorHeight.ToString(); //mxd
            scaletexturesonslopes.SelectedIndex = DoomBuilder.General.Settings.ReadPluginSetting("scaletexturesonslopes", 0);
            eventlinelabelvisibility.SelectedIndex = DoomBuilder.General.Settings.ReadPluginSetting("eventlinelabelvisibility", 3);
            eventlinelabelstyle.SelectedIndex = DoomBuilder.General.Settings.ReadPluginSetting("eventlinelabelstyle", 2);
            useoppositesmartpivothandle.Checked = DoomBuilder.General.Settings.ReadPluginSetting("useoppositesmartpivothandle", true);
            selectafterundoredo.Checked = DoomBuilder.General.Settings.ReadPluginSetting("selectchangedafterundoredo", false);
            usebuggyfloodselect.Checked = DoomBuilder.General.Settings.ReadPluginSetting("usebuggyfloodselect", false);
        }

        // When OK is pressed on the preferences dialog
        public void OnAccept(PreferencesController controller)
        {
            // Write preferred settings
            DoomBuilder.General.Settings.WritePluginSetting("changeheightbysidedef", heightbysidedef.SelectedIndex);
            DoomBuilder.General.Settings.WritePluginSetting("editnewthing", editnewthing.Checked);
            DoomBuilder.General.Settings.WritePluginSetting("editnewsector", editnewsector.Checked);
            DoomBuilder.General.Settings.WritePluginSetting("additiveselect", additiveselect.Checked);
            DoomBuilder.General.Settings.WritePluginSetting("additivepaintselect", additivepaintselect.Checked);
            DoomBuilder.General.Settings.WritePluginSetting("stitchrange", stitchrange.GetResult(0));
            DoomBuilder.General.Settings.WritePluginSetting("highlightrange", highlightrange.GetResult(0));
            DoomBuilder.General.Settings.WritePluginSetting("highlightthingsrange", highlightthingsrange.GetResult(0));
            DoomBuilder.General.Settings.WritePluginSetting("splitlinedefsrange", splitlinedefsrange.GetResult(0));
            DoomBuilder.General.Settings.WritePluginSetting("mouseselectionthreshold", mouseselectionthreshold.GetResult(0));
            DoomBuilder.General.Settings.WritePluginSetting("autoclearselection", autoclearselection.Checked);
            DoomBuilder.General.Settings.WritePluginSetting("visualmodeclearselection", visualmodeclearselection.Checked);
            DoomBuilder.General.Settings.WritePluginSetting("autodragonpaste", autodragonpaste.Checked);
            DoomBuilder.General.Settings.WritePluginSetting("autodrawonedit", autodrawonedit.Checked); //mxd
            DoomBuilder.General.Settings.WritePluginSetting("autoaligntextureoffsetsoncreate", autoaligntexturesoncreate.Checked);//mxd
            DoomBuilder.General.Settings.WritePluginSetting("dontmovegeometryoutsidemapboundary", dontMoveGeometryOutsideBounds.Checked);//mxd
            DoomBuilder.General.Settings.WritePluginSetting("syncselection", syncSelection.Checked);//mxd
            DoomBuilder.General.Settings.WritePluginSetting("scaletexturesonslopes", scaletexturesonslopes.SelectedIndex);
            DoomBuilder.General.Settings.WritePluginSetting("eventlinelabelvisibility", eventlinelabelvisibility.SelectedIndex);
            DoomBuilder.General.Settings.WritePluginSetting("eventlinelabelstyle", eventlinelabelstyle.SelectedIndex);
            DoomBuilder.General.Settings.WritePluginSetting("useoppositesmartpivothandle", useoppositesmartpivothandle.Checked);
            DoomBuilder.General.Settings.WritePluginSetting("selectchangedafterundoredo", selectafterundoredo.Checked);
            DoomBuilder.General.Settings.WritePluginSetting("usebuggyfloodselect", usebuggyfloodselect.Checked);
            DoomBuilder.General.Settings.SwitchViewModes = switchviewmodes.Checked; //mxd
            DoomBuilder.General.Settings.SplitLineBehavior = (SplitLineBehavior)splitbehavior.SelectedIndex;//mxd


            //default sector values
            DoomBuilder.General.Settings.DefaultBrightness = DoomBuilder.General.Clamp(defaultbrightness.GetResult(192), 0, 255);

            int ceilHeight = defaultceilheight.GetResult(128);
            int floorHeight = defaultfloorheight.GetResult(0);
            if (ceilHeight < floorHeight) DoomBuilder.General.Swap(ref ceilHeight, ref floorHeight);

            DoomBuilder.General.Settings.DefaultCeilingHeight = ceilHeight;
            DoomBuilder.General.Settings.DefaultFloorHeight = floorHeight;
        }

        // When Cancel is pressed on the preferences dialog
        public void OnCancel(PreferencesController controller)
        {
        }

        // This sets up the form with the preferences controller
        public void Setup(PreferencesController controller)
        {
            // Add tab pages
            foreach (TabPage p in tabs.TabPages)
            {
                controller.AddTab(p);
            }

            // Bind events
            controller.OnAccept += OnAccept;
            controller.OnCancel += OnCancel;
        }

    }
}