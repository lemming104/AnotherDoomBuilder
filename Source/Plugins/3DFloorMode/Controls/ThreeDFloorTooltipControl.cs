using System.Drawing;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
    public partial class ThreeDFloorHelperTooltipElementControl : UserControl
    {
        public bool Highlighted { get; set; }

        public ThreeDFloorHelperTooltipElementControl()
        {
            Highlighted = false;

            InitializeComponent();
        }

        private void ThreeDFloorHelperTooltipElementControl_Paint(object sender, PaintEventArgs e)
        {
            Color c = Color.FromArgb(0, 192, 0); //  Color.FromArgb(255, Color.Green);

            ControlPaint.DrawBorder(
                e.Graphics,
                this.ClientRectangle,
                c, // leftColor
                5, // leftWidth
                Highlighted ? ButtonBorderStyle.Solid : ButtonBorderStyle.None, // leftStyle
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
    }
}
