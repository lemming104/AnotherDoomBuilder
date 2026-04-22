

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

namespace CodeImp.DoomBuilder.BuilderModes
{
    public class ResultThingOutside : ErrorResult
    {

        private readonly Thing thing;

        public override int Buttons { get { return 1; } }
        public override string Button1Text { get { return "Delete Thing"; } }

        // Constructor
        public ResultThingOutside(Thing t)
        {
            // Initialize
            thing = t;
            viewobjects.Add(t);
            hidden = t.IgnoredErrorChecks.Contains(this.GetType()); //mxd
            description = "This thing is completely outside the map.";
        }

        // This sets if this result is displayed in ErrorCheckForm (mxd)
        internal override void Hide(bool hide)
        {
            hidden = hide;
            Type t = this.GetType();
            if (hide) thing.IgnoredErrorChecks.Add(t);
            else if (thing.IgnoredErrorChecks.Contains(t)) thing.IgnoredErrorChecks.Remove(t);
        }

        // This must return the string that is displayed in the listbox
        public override string ToString()
        {
            return "Thing " + thing.Index + " (" + General.Map.Data.GetThingInfo(thing.Type).Title + ") is outside the map at " + thing.Position.x + ", " + thing.Position.y;
        }

        // Rendering
        public override void RenderOverlaySelection(IRenderer2D renderer)
        {
            renderer.RenderThing(thing, General.Colors.Selection, General.Settings.ActiveThingsAlpha);
        }

        // This removes the thing
        public override bool Button1Click(bool batchMode)
        {
            if (!batchMode) General.Map.UndoRedo.CreateUndo("Delete thing");
            thing.Dispose();
            General.Map.IsChanged = true;
            General.Map.ThingsFilter.Update();
            return true;
        }
    }
}
