using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.Interface
{
    internal partial class DrawLineOptionsPanel : UserControl
    {
        public event EventHandler OnContinuousDrawingChanged;
        public event EventHandler OnAutoCloseDrawingChanged;
        public event EventHandler OnShowGuidelinesChanged;

        public bool ContinuousDrawing { get { return continuousdrawing.Checked; } set { continuousdrawing.Checked = value; } }
        public bool AutoCloseDrawing { get { return autoclosedrawing.Checked; } set { autoclosedrawing.Checked = value; } }
        public bool ShowGuidelines { get { return showguidelines.Checked; } set { showguidelines.Checked = value; } }

        public DrawLineOptionsPanel()
        {
            InitializeComponent();
        }

        public void Register()
        {
            DoomBuilder.General.Interface.BeginToolbarUpdate();

#if MONO_WINFORMS
			// Mono fix
			toolStrip1.Items.Clear();
#endif

            DoomBuilder.General.Interface.AddButton(continuousdrawing);
            DoomBuilder.General.Interface.AddButton(autoclosedrawing);
            DoomBuilder.General.Interface.AddButton(showguidelines);
            DoomBuilder.General.Interface.EndToolbarUpdate();
        }

        public void Unregister()
        {
            DoomBuilder.General.Interface.BeginToolbarUpdate();
            DoomBuilder.General.Interface.RemoveButton(showguidelines);
            DoomBuilder.General.Interface.RemoveButton(autoclosedrawing);
            DoomBuilder.General.Interface.RemoveButton(continuousdrawing);
            DoomBuilder.General.Interface.EndToolbarUpdate();
        }

        private void continuousdrawing_CheckedChanged(object sender, EventArgs e)
        {
            if (OnContinuousDrawingChanged != null) OnContinuousDrawingChanged(continuousdrawing.Checked, EventArgs.Empty);
        }

        private void autoclosedrawing_CheckedChanged(object sender, EventArgs e)
        {
            if (OnAutoCloseDrawingChanged != null) OnAutoCloseDrawingChanged(autoclosedrawing.Checked, EventArgs.Empty);
        }

        private void showguidelines_CheckedChanged(object sender, EventArgs e)
        {
            if (OnShowGuidelinesChanged != null) OnShowGuidelinesChanged(showguidelines.Checked, EventArgs.Empty);
        }
    }
}
