namespace CodeImp.DoomBuilder.BuilderModes
{
    using System.Drawing;

    internal struct FitTextureOptions
    {
        public double HorizontalRepeat;
        public double VerticalRepeat;
        public int PatternWidth;
        public int PatternHeight;
        public bool FitWidth;
        public bool FitHeight;
        public bool FitAcrossSurfaces;
        public bool AutoWidth;
        public bool AutoHeight;
        public Rectangle GlobalBounds;
        public Rectangle Bounds;

        //Initial texture coordinats
        public double InitialOffsetX;
        public double InitialOffsetY;
        public double ControlSideOffsetX;
        public double ControlSideOffsetY;
        public double InitialScaleX;
        public double InitialScaleY;
    }
}
