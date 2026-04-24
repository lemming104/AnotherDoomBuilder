using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.BuilderModes.FindReplace
{
    //mxd. Encapsulates boring stuff
    internal class BaseFindSector : FindReplaceType
    {

        // This is called when a specific object is selected from the list
        public override void ObjectSelected(FindReplaceObject[] selection)
        {
            if (selection.Length == 1)
            {
                ZoomToSelection(selection);
                DoomBuilder.General.Interface.ShowSectorInfo(selection[0].Sector);
            }
            else
            {
                DoomBuilder.General.Interface.HideInfo();
            }

            DoomBuilder.General.Map.Map.ClearAllSelected();
            foreach (FindReplaceObject obj in selection) obj.Sector.Selected = true;
        }

        // Render selection
        public override void PlotSelection(IRenderer2D renderer, FindReplaceObject[] selection)
        {
            foreach (FindReplaceObject o in selection)
            {
                foreach (Sidedef sd in o.Sector.Sidedefs)
                    renderer.PlotLinedef(sd.Line, DoomBuilder.General.Colors.Selection);
            }
        }

        //mxd. Render selection highlight
        public override void RenderOverlaySelection(IRenderer2D renderer, FindReplaceObject[] selection)
        {
            if (!DoomBuilder.General.Settings.UseHighlight) return;

            int color = DoomBuilder.General.Colors.Selection.WithAlpha(64).ToInt();
            foreach (FindReplaceObject o in selection)
                renderer.RenderHighlight(o.Sector.FlatVertices, color);
        }

        // Edit objects
        public override void EditObjects(FindReplaceObject[] selection)
        {
            HashSet<Sector> sectors = new HashSet<Sector>();
            foreach (FindReplaceObject o in selection)
                if (!sectors.Contains(o.Sector)) sectors.Add(o.Sector);
            DoomBuilder.General.Interface.ShowEditSectors(sectors);
        }
    }
}
