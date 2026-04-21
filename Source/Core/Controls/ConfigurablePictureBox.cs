#region ================== Namespaces

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
    public class ConfigurablePictureBox : PictureBox
    {
        #region ================== Constants

        private const int BORDER_SIZE = 4;

        #endregion

        #region ================== Variables

        private readonly Color highlight = Color.FromArgb(196, SystemColors.Highlight);

        #endregion

        #region ================== Properties

        public InterpolationMode InterpolationMode { get; set; } = InterpolationMode.NearestNeighbor;
        public SmoothingMode SmoothingMode { get; set; } = SmoothingMode.Default;
        public CompositingQuality CompositingQuality { get; set; } = CompositingQuality.Default;
        public PixelOffsetMode PixelOffsetMode { get; set; } = PixelOffsetMode.None;
        public GraphicsUnit PageUnit { get; set; } = GraphicsUnit.Pixel;
        public bool Highlighted { get; set; }

        #endregion

        #region ================== Events

        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.InterpolationMode = InterpolationMode;
            pe.Graphics.SmoothingMode = SmoothingMode;
            pe.Graphics.CompositingQuality = CompositingQuality;
            pe.Graphics.PageUnit = PageUnit;
            pe.Graphics.PixelOffsetMode = PixelOffsetMode;
            base.OnPaint(pe);

            if (Highlighted)
            {
                pe.Graphics.PixelOffsetMode = PixelOffsetMode.None;
                ControlPaint.DrawBorder(pe.Graphics, DisplayRectangle,
                                  highlight, BORDER_SIZE, ButtonBorderStyle.Solid,
                                  highlight, BORDER_SIZE, ButtonBorderStyle.Solid,
                                  highlight, BORDER_SIZE, ButtonBorderStyle.Solid,
                                  highlight, BORDER_SIZE, ButtonBorderStyle.Solid);
            }
        }

        #endregion
    }
}
