

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


using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Windows;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Types
{
    [TypeHandler(UniversalType.EnumBits, "Options", false)]
    internal class EnumBitsHandler : TypeHandler
    {

        protected EnumList list;
        protected int value;
        protected int defaultvalue; //mxd

        public override bool IsBrowseable { get { return true; } }

        public override Image BrowseImage { get { return Properties.Resources.List; } }

        // When set up for an argument
        public override void SetupArgument(TypeHandlerAttribute attr, ArgumentInfo arginfo)
        {
            defaultvalue = (int)arginfo.DefaultValue;//mxd
            base.SetupArgument(attr, arginfo);

            // Keep enum list reference
            list = arginfo.Enum;
        }

        // When set up for an UDMF field
        public override void SetupField(TypeHandlerAttribute attr, UniversalFieldInfo fieldinfo)
        {
            defaultvalue = (int)fieldinfo.Default;
            base.SetupField(attr, fieldinfo);

            // Keep enum list reference
            list = fieldinfo.Enum;
        }

        public override void Browse(IWin32Window parent)
        {
            value = BitFlagsForm.ShowDialog(parent, list, value);
        }

        public override void SetValue(object value)
        {
            // Null?
            if (value == null)
            {
                this.value = 0;
            }
            // Compatible type?
            else if ((value is int) || (value is float) || (value is bool))
            {
                // Set directly
                this.value = Convert.ToInt32(value);
            }
            else
            {
                // Try parsing as string
                int result;
                if (int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.CurrentCulture, out result))
                {
                    this.value = result;
                }
                else
                {
                    this.value = 0;
                }
            }
        }

        //mxd
        public override void ApplyDefaultValue()
        {
            value = defaultvalue;
        }

        public override object GetValue()
        {
            return this.value;
        }

        public override int GetIntValue()
        {
            return this.value;
        }

        public override string GetStringValue()
        {
            return this.value.ToString();
        }

        public override object GetDefaultValue()
        {
            return defaultvalue;
        }
    }
}
