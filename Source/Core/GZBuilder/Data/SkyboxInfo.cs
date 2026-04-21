using System.Collections.Generic;

namespace CodeImp.DoomBuilder.GZBuilder.Data
{
    public sealed class SkyboxInfo
    {
        public string Name { get; }
        public readonly List<string> Textures;
        public bool FlipTop;

        public SkyboxInfo(string name)
        {
            this.Name = name;
            Textures = new List<string>();
        }
    }
}
