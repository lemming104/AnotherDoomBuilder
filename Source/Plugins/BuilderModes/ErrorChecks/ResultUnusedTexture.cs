using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
    public class ResultUnusedTexture : ErrorResult
    {

        private readonly Sidedef side;
        private readonly SidedefPart part;

        public override int Buttons { get { return 1; } }
        public override string Button1Text { get { return "Remove Texture"; } }

        // Constructor
        public ResultUnusedTexture(Sidedef sd, SidedefPart part)
        {
            // Initialize
            this.side = sd;
            this.part = part;
            this.viewobjects.Add(sd);
            this.hidden = sd.IgnoredErrorChecks.Contains(this.GetType()); //mxd
            this.description = "This sidedef uses an upper or lower texture, which is not required (it will never be visible ingame). Click the Remove Texture button to remove the texture (this will also reset texture offsets and scale in UDMF map format).";
        }

        // This sets if this result is displayed in ErrorCheckForm (mxd)
        internal override void Hide(bool hide)
        {
            hidden = hide;
            Type t = this.GetType();
            if (hide) side.IgnoredErrorChecks.Add(t);
            else if (side.IgnoredErrorChecks.Contains(t)) side.IgnoredErrorChecks.Remove(t);
        }

        // This must return the string that is displayed in the listbox
        public override string ToString()
        {
            switch (part)
            {
                case SidedefPart.Upper:
                    return "Sidedef " + side.Index + " has unused upper texture \"" + side.HighTexture + "\"";

                case SidedefPart.Middle:
                    return "Sidedef " + side.Index + " has unused middle texture \"" + side.MiddleTexture + "\"";

                case SidedefPart.Lower:
                    return "Sidedef " + side.Index + " has unused lower texture \"" + side.LowTexture + "\"";

                default:
                    return "ERROR";
            }
        }

        // Rendering
        public override void PlotSelection(IRenderer2D renderer)
        {
            renderer.PlotLinedef(side.Line, DoomBuilder.General.Colors.Selection);
            renderer.PlotVertex(side.Line.Start, ColorCollection.VERTICES);
            renderer.PlotVertex(side.Line.End, ColorCollection.VERTICES);
        }

        // Fix by removing texture
        public override bool Button1Click(bool batchMode)
        {
            if (!batchMode) DoomBuilder.General.Map.UndoRedo.CreateUndo("Remove unused texture");
            if (DoomBuilder.General.Map.UDMF) side.Fields.BeforeFieldsChange();

            switch (part)
            {
                case SidedefPart.Upper:
                    side.SetTextureHigh("-");
                    if (DoomBuilder.General.Map.UDMF) UniFields.RemoveFields(side.Fields, new[] { "scalex_top", "scaley_top", "offsetx_top", "offsety_top" });
                    break;

                case SidedefPart.Lower:
                    side.SetTextureLow("-");
                    if (DoomBuilder.General.Map.UDMF) UniFields.RemoveFields(side.Fields, new[] { "scalex_bottom", "scaley_bottom", "offsetx_bottom", "offsety_bottom" });
                    break;
            }

            DoomBuilder.General.Map.Map.Update();
            return true;
        }
    }
}
