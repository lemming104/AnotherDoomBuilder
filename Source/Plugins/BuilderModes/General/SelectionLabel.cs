namespace CodeImp.DoomBuilder.BuilderModes.General
{
    internal class SelectionLabel : LineLengthLabel
    {
        // Constructor
        public SelectionLabel() : base(false, true) { }

        // We don't want any changes here
        protected override void UpdateText() { }
    }
}
