using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
    public class ResultObsoleteThing : ErrorResult
    {

        private readonly Thing thing;

        public override int Buttons { get { return 2; } }
        public override string Button1Text { get { return "Edit Thing..."; } }
        public override string Button2Text { get { return "Delete Thing"; } }

        public ResultObsoleteThing(Thing t, string message)
        {
            // Initialize
            this.thing = t;
            this.viewobjects.Add(t);
            this.hidden = t.IgnoredErrorChecks.Contains(this.GetType());

            if (string.IsNullOrEmpty(message))
                this.description = "This thing is marked as obsolete in DECORATE. You should probably replace or delete it.";
            else
                this.description = "This thing is marked as obsolete in DECORATE: " + message;
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
            return "Thing " + thing.Index + " (" + DoomBuilder.General.Map.Data.GetThingInfo(thing.Type).Title + ") at " + thing.Position.x + ", " + thing.Position.y + " is obsolete.";
        }

        // Rendering
        public override void RenderOverlaySelection(IRenderer2D renderer)
        {
            renderer.RenderThing(thing, DoomBuilder.General.Colors.Selection, DoomBuilder.General.Settings.ActiveThingsAlpha);
        }

        // This edits the thing
        public override bool Button1Click(bool batchMode)
        {
            if (!batchMode) DoomBuilder.General.Map.UndoRedo.CreateUndo("Edit obsolete thing");

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
            if (!batchMode) DoomBuilder.General.Map.UndoRedo.CreateUndo("Delete obsolete thing");
            thing.Dispose();
            DoomBuilder.General.Map.IsChanged = true;
            DoomBuilder.General.Map.ThingsFilter.Update();
            return true;
        }
    }
}
