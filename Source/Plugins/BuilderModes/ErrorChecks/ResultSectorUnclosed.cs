

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


using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
    public class ResultSectorUnclosed : ErrorResult
    {

        private readonly Sector sector;
        private readonly List<Vertex> vertices;
        private readonly int index;

        // Constructor
        public ResultSectorUnclosed(Sector s, List<Vertex> v)
        {
            // Initialize
            sector = s;
            vertices = new List<Vertex>(v);
            viewobjects.Add(s);
            hidden = s.IgnoredErrorChecks.Contains(this.GetType()); //mxd
            foreach (Vertex vv in v) this.viewobjects.Add(vv);
            description = "This sector is not a closed region and could cause problems with clipping and rendering in the game. The 'leaks' in the sector are indicated by the colored vertices.";
            index = s.Index;
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
            return "Sector " + index + " is not closed";
        }

        // Rendering
        public override void PlotSelection(IRenderer2D renderer)
        {
            renderer.PlotSector(sector, DoomBuilder.General.Colors.Selection);
            foreach (Vertex v in vertices)
                renderer.PlotVertex(v, ColorCollection.SELECTION);
        }
    }
}
