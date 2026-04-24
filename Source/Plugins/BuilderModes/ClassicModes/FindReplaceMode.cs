

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


using CodeImp.DoomBuilder.BuilderModes.FindReplace;
using CodeImp.DoomBuilder.BuilderModes.General;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.ClassicModes
{
    [EditMode(DisplayName = "Find and Replace Mode",
              SwitchAction = "findmode",
              ButtonImage = "FindMode.png",
              ButtonOrder = 100,
              ButtonGroup = "002_tools",
              AllowCopyPaste = false,
              Volatile = true,
              UseByDefault = true)]

    public sealed class FindReplaceMode : BaseClassicMode
    {

        internal bool Volatile { get { return attributes.Volatile; } set { attributes.Volatile = value; } } //mxd

        public override void OnHelp()
        {
            DoomBuilder.General.ShowHelp("e_findreplace.html");
        }

        // Cancelled
        public override void OnCancel()
        {
            // Cancel base class
            base.OnCancel();

            // Return to base mode
            DoomBuilder.General.Editing.ChangeMode(DoomBuilder.General.Editing.PreviousStableMode.Name);
        }

        // Mode engages
        public override void OnEngage()
        {
            base.OnEngage();
            renderer.SetPresentation(Presentation.Standard);
            DoomBuilder.General.Map.Map.SelectionType = SelectionType.All;

            // Select linedefs by sectors
            foreach (Linedef ld in DoomBuilder.General.Map.Map.Linedefs)
            {
                if (ld.Selected == false)
                {
                    bool front, back;
                    if (ld.Front != null) front = ld.Front.Sector.Selected; else front = false;
                    if (ld.Back != null) back = ld.Back.Sector.Selected; else back = false;
                    ld.Selected = front | back;
                }
            }

            // Show toolbox window
            BuilderPlug.Me.FindReplaceForm.Show((Form)DoomBuilder.General.Interface, this);
        }

        // Disenagaging
        public override void OnDisengage()
        {
            base.OnDisengage();

            // Hide object info
            DoomBuilder.General.Interface.HideInfo();

            // Hide toolbox window
            BuilderPlug.Me.FindReplaceForm.Hide();
        }

        // This applies the curves and returns to the base mode
        public override void OnAccept()
        {
            // Snap to map format accuracy
            DoomBuilder.General.Map.Map.SnapAllToAccuracy();

            // Update caches
            DoomBuilder.General.Map.Map.Update();
            DoomBuilder.General.Map.IsChanged = true;

            // Return to base mode
            DoomBuilder.General.Editing.ChangeMode(DoomBuilder.General.Editing.PreviousStableMode.Name);
        }

        // Redrawing display
        public override void OnRedrawDisplay()
        {
            // Get the selection
            FindReplaceObject[] selection = BuilderPlug.Me.FindReplaceForm.GetSelection();

            renderer.RedrawSurface();

            // Render lines
            if (renderer.StartPlotter(true))
            {
                renderer.PlotLinedefSet(DoomBuilder.General.Map.Map.Linedefs);
                if (BuilderPlug.Me.FindReplaceForm.Finder != null)
                    BuilderPlug.Me.FindReplaceForm.Finder.PlotSelection(renderer, selection);
                renderer.PlotVerticesSet(DoomBuilder.General.Map.Map.Vertices);
                renderer.Finish();
            }

            // Render things
            if (renderer.StartThings(true))
            {
                renderer.RenderThingSet(DoomBuilder.General.Map.Map.Things, DoomBuilder.General.Settings.ActiveThingsAlpha);
                if (BuilderPlug.Me.FindReplaceForm.Finder != null)
                    BuilderPlug.Me.FindReplaceForm.Finder.RenderThingsSelection(renderer, selection);
                renderer.Finish();
            }

            // Render overlay
            if (renderer.StartOverlay(true))
            {
                if (BuilderPlug.Me.FindReplaceForm.Finder != null)
                    BuilderPlug.Me.FindReplaceForm.Finder.RenderOverlaySelection(renderer, selection);
                renderer.Finish();
            }

            renderer.Present();
        }
    }
}
