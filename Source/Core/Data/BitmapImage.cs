

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


using System;
using System.Drawing;

namespace CodeImp.DoomBuilder.Data
{
    public class BitmapImage : ImageData
    {

        // Image source
        private Bitmap img;

        // Constructor
        public BitmapImage(Bitmap img, string name)
        {
            // Initialize
            this.img = new Bitmap(img);
            this.AllowUnload = false;
            SetName(name);

            // Get width and height from image
            width = img.Size.Width;
            height = img.Size.Height;
            scale.x = 1.0f;
            scale.y = 1.0f;

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // This loads the image
        protected override LocalLoadResult LocalLoadImage()
        {
            // No failure checking here. I anything fails here, it is not the user's fault,
            // because the resources this loads are in the assembly.

            return new LocalLoadResult(new Bitmap(img));
        }
    }
}
