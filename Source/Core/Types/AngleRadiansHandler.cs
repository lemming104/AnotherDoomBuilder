

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


using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Windows;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Types
{
    [TypeHandler(UniversalType.AngleRadians, "Radians", false)]
    internal class AngleRadiansHandler : AngleDegreesHandler
    {

        private new double value;

        public override bool IsBrowseable { get { return true; } }

        public override Image BrowseImage { get { return angleicons[General.ClampAngle(Angle2D.RealToDoom(value) + 22) / 45]; } }

        public override void Browse(IWin32Window parent)
        {
            value = Angle2D.DoomToReal(AngleForm.ShowDialog(parent, Angle2D.RealToDoom(value)));
        }

        public override void SetValue(object value)
        {
            // Null?
            if (value == null)
            {
                this.value = 0.0f;
            }
            // Compatible type?
            else if ((value is int) || (value is float) || (value is bool))
            {
                // Set directly
                this.value = Convert.ToSingle(value);
            }
            else
            {
                // Try parsing as string
                float result;
                if (float.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.CurrentCulture, out result))
                {
                    this.value = result;
                }
                else
                {
                    this.value = 0.0f;
                }
            }
        }

        public override object GetValue()
        {
            return this.value;
        }

        public override int GetIntValue()
        {
            return (int)this.value;
        }

        public override string GetStringValue()
        {
            return this.value.ToString();
        }

        public override object GetDefaultValue()
        {
            return 0f;
        }
    }
}
