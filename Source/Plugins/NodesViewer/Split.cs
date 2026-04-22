
using CodeImp.DoomBuilder.Geometry;

namespace CodeImp.DoomBuilder.Plugins.NodesViewer
{
    public struct Split
    {
        public Vector2D pos;
        public Vector2D delta;

        // Constructor
        public Split(Vector2D pos, Vector2D delta)
        {
            this.pos = pos;
            this.delta = delta;
        }
    }
}
