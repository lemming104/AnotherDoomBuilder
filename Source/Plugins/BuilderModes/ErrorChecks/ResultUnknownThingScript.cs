using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
    public class ResultUnknownThingScript : ErrorResult
    {

        private readonly Thing thing;
        private readonly bool namedscript;

        public override int Buttons { get { return 2; } }
        public override string Button1Text { get { return "Edit Thing..."; } }
        public override string Button2Text { get { return "Delete Thing"; } }

        public ResultUnknownThingScript(Thing t, bool isnamedscript)
        {
            // Initialize
            thing = t;
            namedscript = isnamedscript;
            viewobjects.Add(t);
            hidden = t.IgnoredErrorChecks.Contains(this.GetType()); //mxd
            description = "This thing references unknown ACS script " + (namedscript ? "name" : "number") + ".";
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
            if (namedscript)
                return "Thing references unknown ACS script name \"" + thing.Fields.GetValue("arg0str", string.Empty) + "\".";

            return "Thing references unknown ACS script number \"" + thing.Args[0] + "\".";
        }

        // Rendering
        public override void RenderOverlaySelection(IRenderer2D renderer)
        {
            renderer.RenderThing(thing, DoomBuilder.General.Colors.Selection, DoomBuilder.General.Settings.ActiveThingsAlpha);
        }

        // This edits the thing
        public override bool Button1Click(bool batchMode)
        {
            if (!batchMode) DoomBuilder.General.Map.UndoRedo.CreateUndo("Edit thing");

            if (DoomBuilder.General.Interface.ShowEditThings(new List<Thing> { thing }) == DialogResult.OK)
            {
                DoomBuilder.General.Map.IsChanged = true;
                DoomBuilder.General.Map.ThingsFilter.Update();
                return true;
            }

            return false;
        }

        // This removes the thing
        public override bool Button2Click(bool batchMode)
        {
            if (!batchMode) DoomBuilder.General.Map.UndoRedo.CreateUndo("Delete thing");
            thing.Dispose();
            DoomBuilder.General.Map.IsChanged = true;
            DoomBuilder.General.Map.ThingsFilter.Update();
            return true;
        }
    }
}
