
using CodeImp.DoomBuilder.Windows;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Types
{
    [TypeHandler(UniversalType.AngleByte, "Byte Angle", false)]
    internal class AngleByteHandler : AngleDegreesHandler
    {

        public override Image BrowseImage { get { return angleicons[General.ClampAngle((int)Math.Round((float)value / 256 * 360) + 22) / 45]; } }

        public override void Browse(IWin32Window parent)
        {
            value = (int)Math.Round((float)AngleForm.ShowDialog(parent, (int)Math.Round((float)value / 256 * 360)) / 360 * 256);
        }
    }
}
