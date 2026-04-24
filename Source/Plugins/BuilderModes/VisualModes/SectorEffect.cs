namespace CodeImp.DoomBuilder.BuilderModes.VisualModes
{
    internal abstract class SectorEffect
    {
        protected readonly SectorData data;

        // Constructor
        protected SectorEffect(SectorData data)
        {
            this.data = data;
        }

        // This makes sure we are updated with the source linedef information
        public abstract void Update();
    }
}
