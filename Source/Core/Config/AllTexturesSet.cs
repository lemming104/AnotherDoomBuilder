

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


using CodeImp.DoomBuilder.Data;
using System;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.Config
{
    internal sealed class AllTextureSet : TextureSet, IFilledTextureSet
    {

        private const string NAME = "All";

        // Matching textures and flats
        private Dictionary<string, ImageData> textures;
        private Dictionary<string, ImageData> flats;

        public ICollection<ImageData> Textures { get { return textures.Values; } }
        public ICollection<ImageData> Flats { get { return flats.Values; } }

        // New texture set constructor
        public AllTextureSet()
        {
            this.name = NAME;
            this.textures = new Dictionary<string, ImageData>(StringComparer.Ordinal);
            this.flats = new Dictionary<string, ImageData>(StringComparer.Ordinal);
        }

        internal void AddTexture(ImageData image)
        {
            //mxd. Use short name when adding a texture with "classic" name to override same-named textures 
            // with textures loaded from directory/pk3 containters
            textures[/*image.DisplayName.Length > 8 ?*/ image.Name /*: image.ShortName*/] = image;
        }

        internal void AddFlat(ImageData image)
        {
            //mxd. Same with flats
            flats[image.DisplayName.Length > 8 ? image.Name : image.ShortName] = image;
        }
    }
}
