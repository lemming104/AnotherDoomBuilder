using CodeImp.DoomBuilder.BuilderModes.General;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
    public class ResultUnknownThingAction : ErrorResult
    {

        private readonly Thing thing;

        public override int Buttons { get { return 2; } }
        public override string Button1Text { get { return "Remove Action"; } }
        public override string Button2Text { get { return "Browse Action..."; } } //mxd

        // Constructor
        public ResultUnknownThingAction(Thing t)
        {
            // Initialize
            thing = t;
            viewobjects.Add(t);
            hidden = t.IgnoredErrorChecks.Contains(this.GetType()); //mxd
            description = "This thing uses unknown action. This can potentially cause gameplay issues.";
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
            return "Thing " + thing.Index + " uses unknown action " + thing.Action;
        }

        // Rendering
        public override void RenderOverlaySelection(IRenderer2D renderer)
        {
            renderer.RenderThing(thing, DoomBuilder.General.Colors.Selection, DoomBuilder.General.Settings.ActiveThingsAlpha);
        }

        // Fix by removing action
        public override bool Button1Click(bool batchmode)
        {
            if (!batchmode) DoomBuilder.General.Map.UndoRedo.CreateUndo("Unknown thing action removal");
            thing.Action = 0;
            DoomBuilder.General.Map.Map.Update();
            return true;
        }

        // Fix by picking action
        public override bool Button2Click(bool batchmode)
        {
            if (!batchmode) DoomBuilder.General.Map.UndoRedo.CreateUndo("Unknown thing action correction");
            thing.Action = DoomBuilder.General.Interface.BrowseLinedefActions(BuilderPlug.Me.ErrorCheckForm, thing.Action);
            DoomBuilder.General.Map.Map.Update();
            return true;
        }
    }
}
