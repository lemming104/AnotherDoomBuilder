using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.Interface
{
    internal partial class DrawEllipseOptionsPanel : UserControl
    {
        public event EventHandler OnValueChanged;
        public event EventHandler OnContinuousDrawingChanged;
        public event EventHandler OnShowGuidelinesChanged;
        public event EventHandler OnRadialDrawingChanged;
        public event EventHandler OnPlaceThingsAtVerticesChanged;

        private bool blockevents;

        public int Spikiness { get { return (int)spikiness.Value; } set { blockevents = true; spikiness.Value = value; blockevents = false; } }
        public int Subdivisions { get { return (int)subdivs.Value; } set { blockevents = true; subdivs.Value = value; blockevents = false; } }
        public int Angle { get { return (int)angle.Value; } set { blockevents = true; angle.Value = value; blockevents = false; } }
        public int MaxSubdivisions { get { return (int)subdivs.Maximum; } set { subdivs.Maximum = value; } }
        public int MinSubdivisions { get { return (int)subdivs.Minimum; } set { subdivs.Minimum = value; } }
        public int MaxSpikiness { get { return (int)spikiness.Maximum; } set { spikiness.Maximum = value; } }
        public int MinSpikiness { get { return (int)spikiness.Minimum; } set { spikiness.Minimum = value; } }
        public bool ContinuousDrawing { get { return continuousdrawing.Checked; } set { continuousdrawing.Checked = value; } }
        public bool ShowGuidelines { get { return showguidelines.Checked; } set { showguidelines.Checked = value; } }
        public bool RadialDrawing { get { return radialdrawing.Checked; } set { radialdrawing.Checked = value; } }
        public bool PlaceThingsAtVertices { get { return placethingsatvertices.Checked; } set { placethingsatvertices.Checked = value; } }

        public DrawEllipseOptionsPanel()
        {
            InitializeComponent();
        }

        public void Register()
        {
            spikiness.ValueChanged += ValueChanged;
            subdivs.ValueChanged += ValueChanged;
            angle.ValueChanged += ValueChanged;

            DoomBuilder.General.Interface.BeginToolbarUpdate();

#if MONO_WINFORMS
			// Mono fix
			toolStrip1.Items.Clear();
#endif

            DoomBuilder.General.Interface.AddButton(continuousdrawing);
            DoomBuilder.General.Interface.AddButton(showguidelines);
            DoomBuilder.General.Interface.AddButton(radialdrawing);
            DoomBuilder.General.Interface.AddButton(placethingsatvertices);
            DoomBuilder.General.Interface.AddButton(toolStripSeparator1);
            DoomBuilder.General.Interface.AddButton(subdivslabel);
            DoomBuilder.General.Interface.AddButton(subdivs);
            DoomBuilder.General.Interface.AddButton(spikinesslabel);
            DoomBuilder.General.Interface.AddButton(spikiness);
            DoomBuilder.General.Interface.AddButton(anglelabel);
            DoomBuilder.General.Interface.AddButton(angle);
            DoomBuilder.General.Interface.AddButton(reset);
            DoomBuilder.General.Interface.EndToolbarUpdate();
        }

        public void Unregister()
        {
            DoomBuilder.General.Interface.BeginToolbarUpdate();
            DoomBuilder.General.Interface.RemoveButton(reset);
            DoomBuilder.General.Interface.RemoveButton(angle);
            DoomBuilder.General.Interface.RemoveButton(anglelabel);
            DoomBuilder.General.Interface.RemoveButton(spikiness);
            DoomBuilder.General.Interface.RemoveButton(spikinesslabel);
            DoomBuilder.General.Interface.RemoveButton(subdivs);
            DoomBuilder.General.Interface.RemoveButton(subdivslabel);
            DoomBuilder.General.Interface.RemoveButton(toolStripSeparator1);
            DoomBuilder.General.Interface.RemoveButton(showguidelines);
            DoomBuilder.General.Interface.RemoveButton(continuousdrawing);
            DoomBuilder.General.Interface.RemoveButton(radialdrawing);
            DoomBuilder.General.Interface.RemoveButton(placethingsatvertices);
            DoomBuilder.General.Interface.EndToolbarUpdate();
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            if (!blockevents && OnValueChanged != null) OnValueChanged(this, EventArgs.Empty);
        }

        private void reset_Click(object sender, EventArgs e)
        {
            // Reset values
            blockevents = true;
            spikiness.Value = 0;
            angle.Value = 0;
            subdivs.Value = 6;
            blockevents = false;

            // Dispatch event
            OnValueChanged(this, EventArgs.Empty);
        }

        private void continuousdrawing_CheckedChanged(object sender, EventArgs e)
        {
            if (OnContinuousDrawingChanged != null) OnContinuousDrawingChanged(continuousdrawing.Checked, EventArgs.Empty);
        }

        private void showguidelines_CheckedChanged(object sender, EventArgs e)
        {
            if (OnShowGuidelinesChanged != null) OnShowGuidelinesChanged(showguidelines.Checked, EventArgs.Empty);
        }

        private void radialdrawing_CheckedChanged(object sender, EventArgs e)
        {
            if (OnRadialDrawingChanged != null) OnRadialDrawingChanged(radialdrawing.Checked, EventArgs.Empty);
        }

        private void placethingsatvertices_CheckedChanged(object sender, EventArgs e)
        {
            if (OnPlaceThingsAtVerticesChanged != null) OnPlaceThingsAtVerticesChanged(placethingsatvertices.Checked, EventArgs.Empty);
        }
    }
}
