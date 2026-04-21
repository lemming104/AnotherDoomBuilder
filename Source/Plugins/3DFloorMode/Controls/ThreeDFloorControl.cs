using CodeImp.DoomBuilder.Map;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
    public partial class ThreeDFloorHelperControl : UserControl
    {
        public Linedef linedef;

        public ThreeDFloor ThreeDFloor { get; private set; }
        public bool IsNew { get; private set; }
        public Sector Sector { get; private set; }
        public List<int> CheckedSectors { get; private set; }
        public bool Used { get; set; }

        // Create the control from an existing linedef
        public ThreeDFloorHelperControl(ThreeDFloor threeDFloor)
        {
            InitializeComponent();

            sectorTopFlat.Initialize();
            sectorBorderTexture.Initialize();
            sectorBottomFlat.Initialize();

            Update(threeDFloor);
        }

        // Create a duplicate of the given control
        public ThreeDFloorHelperControl(ThreeDFloorHelperControl ctrl) : this()
        {
            Update(ctrl);
        }

        // Create a blank control for a new 3D floor
        public ThreeDFloorHelperControl()
        {
            InitializeComponent();

            sectorTopFlat.Initialize();
            sectorBorderTexture.Initialize();
            sectorBottomFlat.Initialize();

            SetDefaults();
        }

        public void SetDefaults()
        {
            IsNew = true;

            ThreeDFloor = new ThreeDFloor();

            sectorBorderTexture.TextureName = General.Settings.DefaultTexture;
            sectorTopFlat.TextureName = General.Settings.DefaultCeilingTexture;
            sectorBottomFlat.TextureName = General.Settings.DefaultFloorTexture;
            sectorCeilingHeight.Text = General.Settings.DefaultCeilingHeight.ToString();
            sectorFloorHeight.Text = General.Settings.DefaultFloorHeight.ToString();

            typeArgument.Setup(General.Map.Config.LinedefActions[160].Args[1]);
            flagsArgument.Setup(General.Map.Config.LinedefActions[160].Args[2]);
            alphaArgument.Setup(General.Map.Config.LinedefActions[160].Args[3]);

            typeArgument.SetDefaultValue();
            flagsArgument.SetDefaultValue();
            alphaArgument.SetDefaultValue();

            tagsLabel.Text = "0";

            AddSectorCheckboxes();

            for (int i = 0; i < checkedListBoxSectors.Items.Count; i++)
                checkedListBoxSectors.SetItemChecked(i, true);

            //When creating a NEW 3d sector, find information about what is selected to populate the defaults
            int FloorHeight = int.MinValue;
            int SectorDarkest = int.MaxValue;
            foreach (Sector s in BuilderPlug.TDFEW.SelectedSectors)
            {
                if (s.FloorHeight > FloorHeight)
                    FloorHeight = s.FloorHeight;
                if (s.Brightness < SectorDarkest)
                    SectorDarkest = s.Brightness;
            }

            //set the floor height to match the lowest sector selected, then offset the height by the configured default
            if (FloorHeight != int.MinValue)
            {
                int DefaultHeight = General.Settings.DefaultCeilingHeight - General.Settings.DefaultFloorHeight;
                sectorFloorHeight.Text = FloorHeight.ToString();
                sectorCeilingHeight.Text = (FloorHeight + DefaultHeight).ToString();
            }

            //set the brightness to match the darkest of all the selected sectors by default
            if (SectorDarkest != int.MaxValue)
            {
                sectorBrightness.Text = SectorDarkest.ToString();
            }
            else
            {
                sectorBrightness.Text = General.Settings.DefaultBrightness.ToString();
            }

            Sector = General.Map.Map.CreateSector();
        }

        public void Update(ThreeDFloorHelperControl ctrl)
        {
            sectorBorderTexture.TextureName = ThreeDFloor.BorderTexture = ctrl.ThreeDFloor.BorderTexture;
            sectorTopFlat.TextureName = ThreeDFloor.TopFlat = ctrl.ThreeDFloor.TopFlat;
            sectorBottomFlat.TextureName = ThreeDFloor.BottomFlat = ctrl.ThreeDFloor.BottomFlat;
            sectorCeilingHeight.Text = ctrl.ThreeDFloor.TopHeight.ToString();
            sectorFloorHeight.Text = ctrl.ThreeDFloor.BottomHeight.ToString();
            borderHeightLabel.Text = (ctrl.ThreeDFloor.TopHeight - ctrl.ThreeDFloor.BottomHeight).ToString();

            ThreeDFloor.TopHeight = ctrl.ThreeDFloor.TopHeight;
            ThreeDFloor.BottomHeight = ctrl.ThreeDFloor.BottomHeight;

            typeArgument.SetValue(ctrl.ThreeDFloor.Type);
            flagsArgument.SetValue(ctrl.ThreeDFloor.Flags);
            alphaArgument.SetValue(ctrl.ThreeDFloor.Alpha);
            sectorBrightness.Text = ctrl.ThreeDFloor.Brightness.ToString();

            ThreeDFloor.FloorSlope = ctrl.ThreeDFloor.FloorSlope;
            ThreeDFloor.FloorSlopeOffset = ctrl.ThreeDFloor.FloorSlopeOffset;
            ThreeDFloor.CeilingSlope = ctrl.ThreeDFloor.CeilingSlope;
            ThreeDFloor.CeilingSlopeOffset = ctrl.ThreeDFloor.CeilingSlopeOffset;

            for (int i = 0; i < checkedListBoxSectors.Items.Count; i++)
                checkedListBoxSectors.SetItemChecked(i, ctrl.checkedListBoxSectors.GetItemChecked(i));
        }

        public void Update(ThreeDFloor threeDFloor)
        {
            IsNew = false;

            this.ThreeDFloor = threeDFloor;

            sectorBorderTexture.TextureName = threeDFloor.BorderTexture;
            sectorTopFlat.TextureName = threeDFloor.TopFlat;
            sectorBottomFlat.TextureName = threeDFloor.BottomFlat;
            sectorCeilingHeight.Text = threeDFloor.TopHeight.ToString();
            sectorFloorHeight.Text = threeDFloor.BottomHeight.ToString();
            borderHeightLabel.Text = (threeDFloor.TopHeight - threeDFloor.BottomHeight).ToString();

            typeArgument.Setup(General.Map.Config.LinedefActions[160].Args[1]);
            flagsArgument.Setup(General.Map.Config.LinedefActions[160].Args[2]);
            alphaArgument.Setup(General.Map.Config.LinedefActions[160].Args[3]);

            typeArgument.SetValue(threeDFloor.Type);
            flagsArgument.SetValue(threeDFloor.Flags);
            alphaArgument.SetValue(threeDFloor.Alpha);
            sectorBrightness.Text = threeDFloor.Brightness.ToString();

            AddSectorCheckboxes();

            if (Sector == null || Sector.IsDisposed)
                Sector = General.Map.Map.CreateSector();

            if (threeDFloor.Sector != null)
            {
                threeDFloor.Sector.CopyPropertiesTo(Sector);
                tagsLabel.Text = String.Join(", ", Sector.Tags.Select(o => o.ToString()).ToArray());
            }

            if (Sector != null && !Sector.IsDisposed)
                Sector.Selected = false;
        }

        public void ApplyToThreeDFloor()
        {
            Regex r = new Regex(@"\d+");

            ThreeDFloor.TopHeight = sectorCeilingHeight.GetResult(ThreeDFloor.TopHeight);
            ThreeDFloor.BottomHeight = sectorFloorHeight.GetResult(ThreeDFloor.BottomHeight);
            ThreeDFloor.TopFlat = sectorTopFlat.TextureName;
            ThreeDFloor.BottomFlat = sectorBottomFlat.TextureName;
            ThreeDFloor.BorderTexture = sectorBorderTexture.TextureName;

            ThreeDFloor.Type = typeArgument.GetResult(ThreeDFloor.Type);
            ThreeDFloor.Flags = flagsArgument.GetResult(ThreeDFloor.Flags);
            ThreeDFloor.Alpha = alphaArgument.GetResult(ThreeDFloor.Alpha);
            ThreeDFloor.Brightness = sectorBrightness.GetResult(ThreeDFloor.Brightness);

            ThreeDFloor.Tags = Sector.Tags;

            ThreeDFloor.IsNew = IsNew;

            if (ThreeDFloor.Sector != null)
            {
                Sector.CopyPropertiesTo(ThreeDFloor.Sector);
                tagsLabel.Text = String.Join(", ", Sector.Tags.Select(o => o.ToString()).ToArray());
            }

            ThreeDFloor.TaggedSectors = new List<Sector>();

            for (int i = 0; i < checkedListBoxSectors.Items.Count; i++)
            {
                string text = checkedListBoxSectors.Items[i].ToString();
                bool ischecked = !(checkedListBoxSectors.GetItemCheckState(i) == CheckState.Unchecked);

                if (ischecked)
                {
                    var matches = r.Matches(text);
                    Sector s = General.Map.Map.GetSectorByIndex(int.Parse(matches[0].ToString()));
                    ThreeDFloor.TaggedSectors.Add(s);
                }
            }
        }

        private void AddSectorCheckboxes()
        {
            List<Sector> sectors = new List<Sector>(BuilderPlug.TDFEW.SelectedSectors.OrderBy(o => o.Index));

            CheckedSectors = new List<int>();

            checkedListBoxSectors.Items.Clear();

            foreach (Sector s in ThreeDFloor.TaggedSectors)
            {
                if (!sectors.Contains(s))
                    sectors.Add(s);
            }

            if (sectors == null)
                return;

            foreach (Sector s in sectors)
            {
                int i = checkedListBoxSectors.Items.Add("Sector " + s.Index.ToString(), ThreeDFloor.TaggedSectors.Contains(s));

                if (ThreeDFloor.TaggedSectors.Contains(s))
                    CheckedSectors.Add(s.Index);

                if (!BuilderPlug.TDFEW.SelectedSectors.Contains(s))
                {
                    checkedListBoxSectors.SetItemCheckState(i, CheckState.Indeterminate);
                }
            }
        }

        private void buttonDuplicate_Click(object sender, EventArgs e)
        {
            ((ThreeDFloorEditorWindow)this.ParentForm).DuplicateThreeDFloor(this);
        }

        private void buttonSplit_Click(object sender, EventArgs e)
        {
            ((ThreeDFloorEditorWindow)this.ParentForm).SplitThreeDFloor(this);
        }

        private void buttonCheckAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBoxSectors.Items.Count; i++)
                checkedListBoxSectors.SetItemChecked(i, true);
        }

        private void buttonUncheckAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBoxSectors.Items.Count; i++)
                checkedListBoxSectors.SetItemChecked(i, false);
        }

        private void buttonEditSector_Click(object sender, EventArgs e)
        {
            Sector.SetCeilTexture(sectorTopFlat.TextureName);
            Sector.SetFloorTexture(sectorBottomFlat.TextureName);
            Sector.CeilHeight = sectorCeilingHeight.GetResult(Sector.CeilHeight);
            Sector.FloorHeight = sectorFloorHeight.GetResult(Sector.FloorHeight);
            Sector.Brightness = sectorBrightness.GetResult(Sector.Brightness);

            DialogResult result = General.Interface.ShowEditSectors(new List<Sector> { Sector });

            if (result == DialogResult.OK)
            {
                sectorTopFlat.TextureName = Sector.CeilTexture;
                sectorBottomFlat.TextureName = Sector.FloorTexture;
                sectorCeilingHeight.Text = Sector.CeilHeight.ToString();
                sectorFloorHeight.Text = Sector.FloorHeight.ToString();
                sectorBrightness.Text = Sector.Brightness.ToString();
                tagsLabel.Text = String.Join(", ", Sector.Tags.Select(o => o.ToString()).ToArray());
            }
        }

        private void checkedListBoxSectors_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.CurrentValue == CheckState.Indeterminate)
            {
                e.NewValue = CheckState.Indeterminate;
            }
            else
            {
                Regex r = new Regex(@"\d+");

                if (((ListBox)sender).SelectedItem == null)
                    return;

                var matches = r.Matches(((ListBox)sender).SelectedItem.ToString());

                int sectornum = int.Parse(matches[0].ToString());

                if (e.NewValue == CheckState.Checked)
                    CheckedSectors.Add(sectornum);
                else
                    CheckedSectors.Remove(sectornum);
            }
        }

        private void ThreeDFloorHelperControl_Paint(object sender, PaintEventArgs e)
        {
            Color c = Color.FromArgb(0, 192, 0); //  Color.FromArgb(255, Color.Green);

            if (IsNew)
                ControlPaint.DrawBorder(
                    e.Graphics,
                    this.ClientRectangle,
                    c, // leftColor
                    5, // leftWidth
                    ButtonBorderStyle.Solid, // leftStyle
                    c, // topColor
                    0, // topWidth
                    ButtonBorderStyle.None, // topStyle
                    c, // rightColor
                    0, // rightWidth
                    ButtonBorderStyle.None, // rightStyle
                    c, // bottomColor
                    0, // bottomWidth
                    ButtonBorderStyle.None // bottomStyle
                );
        }

        private void RecomputeBorderHeight(object sender, EventArgs e)
        {
            borderHeightLabel.Text = (sectorCeilingHeight.GetResult(ThreeDFloor.TopHeight) - sectorFloorHeight.GetResult(ThreeDFloor.BottomHeight)).ToString();
        }

        private void buttonDetach_Click(object sender, EventArgs e)
        {
            ((ThreeDFloorEditorWindow)this.ParentForm).DetachThreeDFloor(this);
        }
    }
}
