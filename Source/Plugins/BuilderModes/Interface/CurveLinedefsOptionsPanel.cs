using CodeImp.DoomBuilder.BuilderModes.ClassicModes;
using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.Interface
{
    public partial class CurveLinedefsOptionsPanel : UserControl
    {

        public event EventHandler OnValueChanged;

        private bool blockevents;
        private bool fixedcurveoutwards;

        public int Vertices { get { return (int)verts.Value; } set { verts.Value = DoomBuilder.General.Clamp(value, (int)verts.Minimum, (int)verts.Maximum); } }
        public int Distance { get { return (int)distance.Value; } set { distance.Value = DoomBuilder.General.Clamp(value, (int)distance.Minimum, (int)distance.Maximum); } }
        public int DistanceIncrement { get { return (int)distance.Increment; } }
        public int Angle { get { return (int)angle.Value; } set { angle.Value = (decimal)DoomBuilder.General.Clamp(value, (float)angle.Minimum, (float)angle.Maximum); } }
        public int AngleIncrement { get { return (int)angle.Increment; } }
        public int MaximumAngle { get { return (int)angle.Maximum; } }
        public bool FixedCurve { get { return fixedcurve.Checked; } }
        public bool FixedCurveOutwards { get { return fixedcurveoutwards; } }

        public CurveLinedefsOptionsPanel()
        {
            InitializeComponent();
        }

        public void SetValues(int verts, int distance, int angle, bool fixedcurve, bool fixeddirection)
        {
            blockevents = true;

            this.verts.Value = DoomBuilder.General.Clamp(verts, (int)this.verts.Minimum, (int)this.verts.Maximum);
            this.distance.Value = DoomBuilder.General.Clamp(distance, (int)this.distance.Minimum, (int)this.distance.Maximum);
            this.angle.Value = DoomBuilder.General.Clamp(angle, (int)this.angle.Minimum, (int)this.angle.Maximum);
            this.fixedcurve.Checked = fixedcurve;
            this.fixedcurveoutwards = fixeddirection;

            blockevents = false;
        }

        public void Register()
        {
            DoomBuilder.General.Interface.BeginToolbarUpdate();

#if MONO_WINFORMS
			// Mono fix
			toolstrip.Items.Clear();
#endif

            DoomBuilder.General.Interface.AddButton(vertslabel);
            DoomBuilder.General.Interface.AddButton(verts);
            DoomBuilder.General.Interface.AddButton(distancelabel);
            DoomBuilder.General.Interface.AddButton(distance);
            DoomBuilder.General.Interface.AddButton(anglelabel);
            DoomBuilder.General.Interface.AddButton(angle);
            DoomBuilder.General.Interface.AddButton(flip);
            DoomBuilder.General.Interface.AddButton(reset);
            DoomBuilder.General.Interface.AddButton(separator1);
            DoomBuilder.General.Interface.AddButton(fixedcurve);
            DoomBuilder.General.Interface.AddButton(separator2);
            DoomBuilder.General.Interface.AddButton(apply);
            DoomBuilder.General.Interface.AddButton(cancel);
            DoomBuilder.General.Interface.EndToolbarUpdate();
        }

        public void Unregister()
        {
            DoomBuilder.General.Interface.BeginToolbarUpdate();
            DoomBuilder.General.Interface.RemoveButton(cancel);
            DoomBuilder.General.Interface.RemoveButton(apply);
            DoomBuilder.General.Interface.RemoveButton(anglelabel);
            DoomBuilder.General.Interface.RemoveButton(separator2);
            DoomBuilder.General.Interface.RemoveButton(fixedcurve);
            DoomBuilder.General.Interface.RemoveButton(separator1);
            DoomBuilder.General.Interface.RemoveButton(reset);
            DoomBuilder.General.Interface.RemoveButton(flip);
            DoomBuilder.General.Interface.RemoveButton(angle);
            DoomBuilder.General.Interface.RemoveButton(anglelabel);
            DoomBuilder.General.Interface.RemoveButton(distance);
            DoomBuilder.General.Interface.RemoveButton(distancelabel);
            DoomBuilder.General.Interface.RemoveButton(verts);
            DoomBuilder.General.Interface.RemoveButton(vertslabel);
            DoomBuilder.General.Interface.EndToolbarUpdate();
        }

        private void apply_Click(object sender, EventArgs e)
        {
            // Apply now
            DoomBuilder.General.Editing.AcceptMode();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            // Cancel now
            DoomBuilder.General.Editing.CancelMode();
        }

        private void OnUIValuesChanged(object sender, EventArgs e)
        {
            if (!blockevents && OnValueChanged != null) OnValueChanged(this, EventArgs.Empty);
        }

        private void fixedcurve_CheckedChanged(object sender, EventArgs e)
        {
            // Enable/disable controls
            distance.Enabled = !fixedcurve.Checked;
            distancelabel.Enabled = !fixedcurve.Checked;

            if (!blockevents && OnValueChanged != null) OnValueChanged(this, EventArgs.Empty);
        }

        private void flip_Click(object sender, EventArgs e)
        {
            if (fixedcurve.Checked)
            {
                fixedcurveoutwards = !fixedcurveoutwards;
                OnValueChanged(this, EventArgs.Empty);
            }
            else
                distance.Value = -distance.Value;

        }

        private void reset_Click(object sender, EventArgs e)
        {
            SetValues(CurveLinedefsMode.DEFAULT_VERTICES_COUNT, CurveLinedefsMode.DEFAULT_DISTANCE, CurveLinedefsMode.DEFAULT_ANGLE, false, true);
            if (OnValueChanged != null) OnValueChanged(this, EventArgs.Empty);
        }

    }
}
