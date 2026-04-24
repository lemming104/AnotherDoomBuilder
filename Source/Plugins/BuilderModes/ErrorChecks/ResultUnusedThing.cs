using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
    public class ResultUnusedThing : ErrorResult
    {

        private readonly Thing thing;
        private readonly string details;

        public override int Buttons { get { return 2; } }
        public override string Button1Text { get { return "Delete Thing"; } }
        public override string Button2Text { get { return "Apply default flags"; } }

        public ResultUnusedThing(Thing t, string details)
        {
            // Initialize
            this.thing = t;
            this.details = details;
            this.viewobjects.Add(t);
            this.hidden = t.IgnoredErrorChecks.Contains(this.GetType());
            this.description = "This thing won't be shown in any game mode.";
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
            return "Thing " + thing.Index + " (" + DoomBuilder.General.Map.Data.GetThingInfo(thing.Type).Title + ") is unused. " + details;
        }

        // Rendering
        public override void RenderOverlaySelection(IRenderer2D renderer)
        {
            renderer.RenderThing(thing, DoomBuilder.General.Colors.Selection, DoomBuilder.General.Settings.ActiveThingsAlpha);
        }

        // This removes the thing
        public override bool Button1Click(bool batchMode)
        {
            if (!batchMode) DoomBuilder.General.Map.UndoRedo.CreateUndo("Delete thing");
            thing.Dispose();
            DoomBuilder.General.Map.IsChanged = true;
            DoomBuilder.General.Map.ThingsFilter.Update();
            return true;
        }

        // This sets default flags
        public override bool Button2Click(bool batchMode)
        {
            if (!batchMode) DoomBuilder.General.Map.UndoRedo.CreateUndo("Set default thing flags");
            foreach (string f in DoomBuilder.General.Map.Config.DefaultThingFlags) thing.SetFlag(f, true);
            DoomBuilder.General.Map.IsChanged = true;
            DoomBuilder.General.Map.ThingsFilter.Update();
            return true;
        }
    }
}
