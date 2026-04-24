using CodeImp.DoomBuilder.Geometry;
using System.Drawing;

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
    public class ResultMapTooBig : ErrorResult
    {

        private readonly bool toowide;
        private readonly bool toohigh;
        private readonly Vector2D min;
        private readonly Vector2D max;

        public override int Buttons { get { return 0; } }

        public ResultMapTooBig(Vector2D min, Vector2D max)
        {
            // Initialize
            this.min = min;
            this.max = max;
            this.toowide = max.x - min.x > DoomBuilder.General.Map.Config.SafeBoundary;
            this.toohigh = max.y - min.y > DoomBuilder.General.Map.Config.SafeBoundary;
            description = "Map is too big.";
        }

        public override RectangleF GetZoomArea()
        {
            const float scaler = 0.5f;
            return new RectangleF((float)min.x * scaler, (float)min.y * scaler, (float)(max.x - min.x) * scaler, (float)(max.y - min.y) * scaler);
        }

        // This sets if this result is displayed in ErrorCheckForm (mxd)
        internal override void Hide(bool hide)
        {
            hidden = hide;
        }

        // This must return the string that is displayed in the listbox
        public override string ToString()
        {
            if (toowide && toohigh) return "Map's width and height is bigger than " + DoomBuilder.General.Map.Config.SafeBoundary + " m.u. This can cause rendering and physics issues.";
            if (toowide) return "Map is wider than " + DoomBuilder.General.Map.Config.SafeBoundary + " m.u. This can cause rendering and physics issues.";
            return "Map is taller than " + DoomBuilder.General.Map.Config.SafeBoundary + " m.u. This can cause rendering and physics issues.";
        }
    }
}
