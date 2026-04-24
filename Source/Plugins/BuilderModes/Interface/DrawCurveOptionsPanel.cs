using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.Interface
{
    internal partial class DrawCurveOptionsPanel : UserControl
    {
        public event EventHandler OnValueChanged;
        public event EventHandler OnContinuousDrawingChanged;
        public event EventHandler OnAutoCloseDrawingChanged;
        public event EventHandler OnPlaceThingsAtVerticesChanged;
        private bool blockevents;

        public int SegmentLength { get { return (int)seglen.Value; } set { blockevents = true; seglen.Value = value; blockevents = false; } }
        public bool ContinuousDrawing { get { return continuousdrawing.Checked; } set { continuousdrawing.Checked = value; } }
        public bool AutoCloseDrawing { get { return autoclosedrawing.Checked; } set { autoclosedrawing.Checked = value; } }
        public bool PlaceThingsAtVertices { get { return placethingsatvertices.Checked; } set { placethingsatvertices.Checked = value; } }

        public DrawCurveOptionsPanel(int minLength, int maxLength)
        {
            InitializeComponent();

            seglen.Minimum = minLength;
            seglen.Maximum = maxLength;
        }

        private DrawCurveOptionsPanel() { InitializeComponent(); }

        public void Register()
        {
            DoomBuilder.General.Interface.BeginToolbarUpdate();
            DoomBuilder.General.Interface.AddButton(continuousdrawing);
            DoomBuilder.General.Interface.AddButton(autoclosedrawing);
            DoomBuilder.General.Interface.AddButton(placethingsatvertices);
            DoomBuilder.General.Interface.AddButton(toolStripSeparator1);
            DoomBuilder.General.Interface.AddButton(seglabel);
            DoomBuilder.General.Interface.AddButton(seglen);
            DoomBuilder.General.Interface.AddButton(reset);
            DoomBuilder.General.Interface.EndToolbarUpdate();
        }

        public void Unregister()
        {
            DoomBuilder.General.Interface.BeginToolbarUpdate();
            DoomBuilder.General.Interface.RemoveButton(reset);
            DoomBuilder.General.Interface.RemoveButton(seglen);
            DoomBuilder.General.Interface.RemoveButton(seglabel);
            DoomBuilder.General.Interface.RemoveButton(toolStripSeparator1);
            DoomBuilder.General.Interface.RemoveButton(placethingsatvertices);
            DoomBuilder.General.Interface.RemoveButton(autoclosedrawing);
            DoomBuilder.General.Interface.RemoveButton(continuousdrawing);
            DoomBuilder.General.Interface.EndToolbarUpdate();
        }

        private void seglen_ValueChanged(object sender, EventArgs e)
        {
            if (!blockevents && OnValueChanged != null) OnValueChanged(this, EventArgs.Empty);
        }

        private void reset_Click(object sender, EventArgs e)
        {
            seglen.Value = seglen.Minimum;
        }

        private void continuousdrawing_CheckedChanged(object sender, EventArgs e)
        {
            if (OnContinuousDrawingChanged != null) OnContinuousDrawingChanged(continuousdrawing.Checked, EventArgs.Empty);
        }

        private void autoclosedrawing_CheckedChanged(object sender, EventArgs e)
        {
            if (OnAutoCloseDrawingChanged != null) OnAutoCloseDrawingChanged(autoclosedrawing.Checked, EventArgs.Empty);
        }

        private void placethingsatvertices_CheckedChanged(object sender, EventArgs e)
        {
            if (OnPlaceThingsAtVerticesChanged != null) OnPlaceThingsAtVerticesChanged(placethingsatvertices.Checked, EventArgs.Empty);
        }
    }
}
