#region ================== Copyright (c) 2021 Boris Iwanski

/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

#endregion

#region ================== Namespaces

using CodeImp.DoomBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
    struct ImageInfo
    {
        #region ================== Variables

        private string _fullname;
        private Vector2DWrapper _scale;

        #endregion

        #region ================== Properties

        /// <summary>
        /// Name of the image.
        /// </summary>
        public string name { get; }

        /// <summary>
        /// Width of the image.
        /// </summary>
        public int width { get; }

        /// <summary>
        /// Height of the image.
        /// </summary>
        public int height { get; }

        /// <summary>
        /// Scale of the image as `Vector2D`.
        /// </summary>
        public Vector2DWrapper scale
        {
            get { return _scale; }
        }

        /// <summary>
        /// If the image is a flat (`true`) or not (`false`).
        /// </summary>
        public bool isFlat { get; }

        #endregion

        #region ================== Constructors

        internal ImageInfo(ImageData image)
        {
            name = image.ShortName;
            _fullname = image.Name;
            width = image.Width;
            height = image.Height;
            _scale = new Vector2DWrapper(image.Scale);
            isFlat = image.TextureNamespace == TextureNamespace.FLAT;
        }

        #endregion
    }
}