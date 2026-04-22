
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.SoundPropagationMode
{
    public class SoundEnvironment
    {

        public const string DEFAULT_NAME = "Unknown sound environment"; //mxd

        public HashSet<Sector> Sectors { get; private set; }
        public List<Thing> Things { get; set; }
        public List<Linedef> Linedefs { get; set; }
        public PixelColor Color { get; set; }
        public int ID { get; set; }
        public string Name { get; set; } //mxd
        public FlatVertex[] SectorsGeometry; //mxd

        public SoundEnvironment()
        {
            Sectors = new HashSet<Sector>();
            Things = new List<Thing>();
            Linedefs = new List<Linedef>();
            Color = General.Colors.Background;
            ID = -1;
            Name = DEFAULT_NAME; //mxd
        }
    }
}
