

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
/*

using System;
using System.Drawing;
using System.Drawing.Imaging;
using CodeImp.DoomBuilder.Rendering;

namespace CodeImp.DoomBuilder.Data
{
	internal sealed unsafe class ColorImage : ImageData
	{

		private PixelColor color;

		// Constructor
		public ColorImage(PixelColor color, int width, int height)
		{
			// Initialize
			this.width = width;
			this.height = height;
			this.color = color;
			SetName(color.ToColorValue().ToString());

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// This loads the image
		protected override void LocalLoadImage()
		{
			// Leave when already loaded
			if((width == 0) || (height == 0)) return;

			// Create bitmap
            Bitmap bitmap = null;
            string error = null;
			try
			{
				bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				BitmapData bitmapdata = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				PixelColor* pixels = (PixelColor*)bitmapdata.Scan0.ToPointer();
				for(int i = 0; i < (width * height); i++)
				{
					*pixels = color;
					pixels++;
				}
				bitmap.UnlockBits(bitmapdata);
			}
			catch(Exception e)
			{
				// Unable to make bitmap
				error = "Unable to create color image '" + this.Name + "'. " + e.GetType().Name + ": " + e.Message;
				bitmap?.Dispose();
			}

			EndLoadImage(bitmap, error);
		}
	}
}
*/