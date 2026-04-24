

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */


using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.BuilderModes.ClassicModes
{
    // No action or button for this mode, it is automatic.
    // The EditMode attribute does not have to be specified unless the
    // mode must be activated by class name rather than direct instance.
    // In that case, just specifying the attribute like this is enough:
    // [EditMode]

    [EditMode(DisplayName = "Linedefs",
              AllowCopyPaste = false,
              Volatile = true)]

    public sealed class DragLinedefsMode : DragGeometryMode
    {

        private ICollection<Linedef> draglines;
        private ICollection<Linedef> unmovinglines;

        // Constructor to start dragging immediately
        public DragLinedefsMode(Vector2D dragstartmappos, ICollection<Linedef> lines)
        {
            // Mark what we are dragging
            DoomBuilder.General.Map.Map.ClearAllMarks(false);

            draglines = new List<Linedef>(lines.Count);
            foreach (Linedef ld in lines)
            {
                ld.Marked = true;
                draglines.Add(ld);
            }

            ICollection<Vertex> verts = DoomBuilder.General.Map.Map.GetVerticesFromLinesMarks(true);
            foreach (Vertex v in verts) v.Marked = true;

            // Get line collections
            unmovinglines = DoomBuilder.General.Map.Map.GetSelectedLinedefs(false);

            // Initialize
            base.StartDrag(dragstartmappos);
            undodescription = draglines.Count == 1 ? "Drag linedef" : "Drag " + draglines.Count + " linedefs"; //mxd

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Disposer
        public override void Dispose()
        {
            // Not already disposed?
            if (!isdisposed)
            {
                // Clean up

                // Done
                base.Dispose();
            }
        }

        // This redraws the display
        public override void OnRedrawDisplay()
        {
            renderer.RedrawSurface();

            UpdateRedraw();

            if (CheckViewChanged())
            {
                // Start rendering things
                if (renderer.StartThings(true))
                {
                    renderer.RenderThingSet(DoomBuilder.General.Map.Map.Things, DoomBuilder.General.Settings.ActiveThingsAlpha);
                    renderer.Finish();
                }
            }

            renderer.Present();
        }

        // This redraws only the required things
        protected override void UpdateRedraw()
        {
            // Start rendering structure
            if (renderer.StartPlotter(true))
            {
                // Render lines and vertices
                renderer.PlotLinedefSet(unmovinglines);

                foreach (Linedef ld in draglines)
                {
                    if (ld.Selected)
                        renderer.PlotLinedef(ld, DoomBuilder.General.Colors.Selection);
                    else
                        renderer.PlotLinedef(ld, DoomBuilder.General.Colors.Highlight);
                }

                renderer.PlotVerticesSet(DoomBuilder.General.Map.Map.Vertices);

                // Draw the dragged item highlighted
                // This is important to know, because this item is used
                // for snapping to the grid and snapping to nearest items
                renderer.PlotVertex(dragitem, ColorCollection.HIGHLIGHT);

                // Done
                renderer.Finish();
            }

            //mxd. Render things
            if (renderer.StartThings(true))
            {
                renderer.RenderThingSet(DoomBuilder.General.Map.ThingsFilter.HiddenThings, DoomBuilder.General.Settings.HiddenThingsAlpha);
                renderer.RenderThingSet(unselectedthings, DoomBuilder.General.Settings.ActiveThingsAlpha);
                renderer.RenderThingSet(selectedthings, DoomBuilder.General.Settings.ActiveThingsAlpha);
                renderer.Finish();
            }

            // Redraw overlay
            if (renderer.StartOverlay(true))
            {
                renderer.RenderText(labels);
                renderer.Finish();
            }
        }
    }
}
