

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


using System.Drawing;

namespace CodeImp.DoomBuilder.Data
{
    public sealed class UnknownImage : ImageData
    {

        private readonly Bitmap loadbitmap;

        // Constructor
        public UnknownImage()
        {
            // Initialize
            this.width = 0;
            this.height = 0;
            this.loadbitmap = new Bitmap(Properties.Resources.UnknownImage);
            SetName("");

            LoadImageNow();
        }

        // This 'loads' the image
        protected override LocalLoadResult LocalLoadImage()
        {
            return new LocalLoadResult(new Bitmap(loadbitmap));
        }

        // This returns a preview image
        public override Image GetPreview()
        {
            // To do: do we actually need a copy here?
            return new Bitmap(loadbitmap);
        }
    }
}
