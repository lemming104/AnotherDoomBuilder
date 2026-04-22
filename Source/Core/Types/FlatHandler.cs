

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


using CodeImp.DoomBuilder.Windows;
using System.Drawing;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Types
{
    [TypeHandler(UniversalType.Flat, "Flat", false)]
    internal class FlatHandler : TypeHandler
    {

        private string value = "";

        public override bool IsBrowseable { get { return true; } }

        public override Image BrowseImage { get { return Properties.Resources.List_Images; } }

        public override void Browse(IWin32Window parent)
        {
            this.value = TextureBrowserForm.Browse(parent, this.value, true); //mxd. was FlatBrowserForm
        }

        public override void SetValue(object value)
        {
            if (value != null)
                this.value = value.ToString();
            else
                this.value = "";
        }

        public override object GetValue()
        {
            return this.value;
        }

        public override string GetStringValue()
        {
            return this.value;
        }

        public override object GetDefaultValue()
        {
            return string.Empty;
        }
    }
}
