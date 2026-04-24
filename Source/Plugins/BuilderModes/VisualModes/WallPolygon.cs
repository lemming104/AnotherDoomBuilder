using CodeImp.DoomBuilder.Geometry;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.BuilderModes.VisualModes
{
    internal class WallPolygon : List<Vector3D>
    {
        // The color that the wall should have
        public int color;

        // Constructors
        public WallPolygon() { }
        public WallPolygon(int capacity) : base(capacity) { }

        // This copies all the wall properties
        public void CopyProperties(WallPolygon target)
        {
            target.color = this.color;
        }
    }
}
