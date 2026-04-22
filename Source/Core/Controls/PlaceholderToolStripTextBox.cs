
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Controls
{
    // This is based on https://stackoverflow.com/questions/50918225/how-to-add-placeholder-text-to-toolstriptextbox
    [ToolboxBitmap(typeof(ToolStripTextBox))]
    public class PlaceholderToolStripTextBox : ToolStripTextBox
    {

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);

        private const int EM_SETCUEBANNER = 0x1501;

        private string placeholder;

        public string PlaceholderText
        {
            get { return placeholder; }
            set
            {
                placeholder = value;
                UpdatePlaceholderText();
            }
        }

        public PlaceholderToolStripTextBox()
        {
            Control.HandleCreated += Control_HandleCreated;
        }

        private void Control_HandleCreated(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(placeholder))
                UpdatePlaceholderText();
        }

        private void UpdatePlaceholderText()
        {
#if !MONO_WINFORMS
            SendMessage(Control.Handle, EM_SETCUEBANNER, 0, placeholder);
#endif
        }
    }
}