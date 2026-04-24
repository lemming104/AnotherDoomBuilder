
/*
 * Copyright (c) 2021 Boris Iwanski
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */


using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.BuilderModes.FindReplace
{
    internal class BaseFindMapElement : FindReplaceType
    {

        // This is called when a specific object is selected from the list
        public override void ObjectSelected(FindReplaceObject[] selection)
        {
            if (selection.Length == 1)
            {
                ZoomToSelection(selection);

                if (selection[0].Object is Linedef)
                    DoomBuilder.General.Interface.ShowLinedefInfo(selection[0].Linedef);
                else if (selection[0].Object is Sidedef)
                    DoomBuilder.General.Interface.ShowLinedefInfo(selection[0].Sidedef.Line);
                else if (selection[0].Object is Sector)
                    DoomBuilder.General.Interface.ShowSectorInfo(selection[0].Sector);
                else if (selection[0].Object is Thing)
                    DoomBuilder.General.Interface.ShowThingInfo(selection[0].Thing);
                else if (selection[0].Object is Vertex)
                    DoomBuilder.General.Interface.ShowVertexInfo(selection[0].Vertex);
            }
            else
            {
                DoomBuilder.General.Interface.HideInfo();
            }

            DoomBuilder.General.Map.Map.ClearAllSelected();
            foreach (FindReplaceObject obj in selection)
            {
                // Sidedefs can not be selected, so we have to select its Linedef
                if (obj.Object is Sidedef)
                    obj.Sidedef.Line.Selected = true;
                else
                    ((SelectableElement)obj.Object).Selected = true;
            }
        }

        // Render selection
        public override void PlotSelection(IRenderer2D renderer, FindReplaceObject[] selection)
        {
            foreach (FindReplaceObject o in selection)
            {
                if (o.Object is Linedef)
                    renderer.PlotLinedef(o.Linedef, DoomBuilder.General.Colors.Selection);
                else if (o.Object is Sidedef)
                    renderer.PlotLinedef(o.Sidedef.Line, DoomBuilder.General.Colors.Selection);
                else if (o.Object is Sector)
                {
                    foreach (Sidedef sd in o.Sector.Sidedefs)
                        renderer.PlotLinedef(sd.Line, DoomBuilder.General.Colors.Selection);
                }
                else if (o.Object is Thing)
                    renderer.RenderThing(o.Thing, DoomBuilder.General.Colors.Selection, DoomBuilder.General.Settings.ActiveThingsAlpha);
                else if (o.Object is Vertex)
                    renderer.PlotVertex(o.Vertex, ColorCollection.SELECTION);
            }
        }

        // Edit objects
        public override void EditObjects(FindReplaceObject[] selection)
        {
            HashSet<Linedef> linedefs = new HashSet<Linedef>();
            HashSet<Sector> sectors = new HashSet<Sector>();
            HashSet<Thing> things = new HashSet<Thing>();
            HashSet<Vertex> vertices = new HashSet<Vertex>();

            foreach (FindReplaceObject o in selection)
            {
                if (o.Object is Linedef)
                {
                    if (!linedefs.Contains(o.Linedef)) linedefs.Add(o.Linedef);
                }
                else if (o.Object is Sidedef)
                {
                    if (!linedefs.Contains(o.Sidedef.Line)) linedefs.Add(o.Sidedef.Line);
                }
                else if (o.Object is Sector)
                {
                    if (!sectors.Contains(o.Sector)) sectors.Add(o.Sector);
                }
                else if (o.Object is Thing)
                {
                    if (!things.Contains(o.Thing)) things.Add(o.Thing);
                }
                else if (o.Object is Vertex)
                    if (!vertices.Contains(o.Vertex)) vertices.Add(o.Vertex);
            }

            if (linedefs.Count > 0)
                DoomBuilder.General.Interface.ShowEditLinedefs(linedefs);

            if (sectors.Count > 0)
                DoomBuilder.General.Interface.ShowEditSectors(sectors);

            if (things.Count > 0)
                DoomBuilder.General.Interface.ShowEditThings(things);

            if (vertices.Count > 0)
                DoomBuilder.General.Interface.ShowEditVertices(vertices);
        }
    }
}
