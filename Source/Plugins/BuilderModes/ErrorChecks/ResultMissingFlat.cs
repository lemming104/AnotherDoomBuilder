using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
    public class ResultMissingFlat : ErrorResult
    {

        private readonly Sector sector;
        private readonly bool ceiling;
        private static string imagename = "-"; //mxd

        public override int Buttons { get { return 2; } }
        public override string Button1Text { get { return "Add Default Flat"; } }
        public override string Button2Text { get { return "Browse Flat..."; } } //mxd

        // Constructor
        public ResultMissingFlat(Sector s, bool ceiling)
        {
            // Initialize
            this.sector = s;
            this.ceiling = ceiling;
            this.viewobjects.Add(s);
            this.hidden = s.IgnoredErrorChecks.Contains(this.GetType()); //mxd
            imagename = "-"; //mxd

            string objname = ceiling ? "ceiling" : "floor";
            this.description = "This sector's " + objname + " is missing a flat where it is required and could cause a 'Hall Of Mirrors' visual problem in the map.";
        }

        // This sets if this result is displayed in ErrorCheckForm (mxd)
        internal override void Hide(bool hide)
        {
            hidden = hide;
            Type t = this.GetType();
            if (hide) sector.IgnoredErrorChecks.Add(t);
            else if (sector.IgnoredErrorChecks.Contains(t)) sector.IgnoredErrorChecks.Remove(t);
        }

        // This must return the string that is displayed in the listbox
        public override string ToString()
        {
            return "Sector " + sector.Index + " has no " + (ceiling ? "ceiling" : "floor") + " flat.";
        }

        // Rendering
        public override void PlotSelection(IRenderer2D renderer)
        {
            renderer.PlotSector(sector, DoomBuilder.General.Colors.Selection);
        }

        //mxd. More rendering
        public override void RenderOverlaySelection(IRenderer2D renderer)
        {
            if (!DoomBuilder.General.Settings.UseHighlight) return;
            renderer.RenderHighlight(sector.FlatVertices, DoomBuilder.General.Colors.Selection.WithAlpha(64).ToInt());
        }

        // Fix by setting default flat
        public override bool Button1Click(bool batchMode)
        {
            if (!batchMode) DoomBuilder.General.Map.UndoRedo.CreateUndo("Missing flat correction");
            DoomBuilder.General.Settings.FindDefaultDrawSettings();

            if (ceiling)
                sector.SetCeilTexture(DoomBuilder.General.Map.Options.DefaultCeilingTexture);
            else
                sector.SetFloorTexture(DoomBuilder.General.Map.Options.DefaultFloorTexture);

            DoomBuilder.General.Map.Map.Update();
            DoomBuilder.General.Map.Data.UpdateUsedTextures();
            return true;
        }

        //mxd. Fix by picking a flat
        public override bool Button2Click(bool batchMode)
        {
            if (!batchMode) DoomBuilder.General.Map.UndoRedo.CreateUndo("Missing flat correction");
            if (imagename == "-") imagename = DoomBuilder.General.Interface.BrowseFlat(DoomBuilder.General.Interface, imagename);
            if (imagename == "-") return false;

            if (ceiling) sector.SetCeilTexture(imagename);
            else sector.SetFloorTexture(imagename);

            DoomBuilder.General.Map.Map.Update();
            DoomBuilder.General.Map.Data.UpdateUsedTextures();
            return true;
        }
    }
}
