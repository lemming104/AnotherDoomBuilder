

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


using CodeImp.DoomBuilder.BuilderModes.ErrorChecks;
using CodeImp.DoomBuilder.BuilderModes.General;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.ClassicModes
{
    [EditMode(DisplayName = "Map Analysis Mode",
              SwitchAction = "errorcheckmode",
              ButtonImage = "MapAnalysisMode.png",
              ButtonOrder = 200,
              ButtonGroup = "002_tools",
              AllowCopyPaste = false,
              Volatile = true,
              UseByDefault = true)]

    public sealed class ErrorCheckMode : BaseClassicMode
    {

        public override void OnHelp()
        {
            DoomBuilder.General.ShowHelp("e_mapanalysis.html");
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

            // Save selection as marks
            DoomBuilder.General.Map.Map.ClearAllMarks(false);
            DoomBuilder.General.Map.Map.MarkAllSelectedGeometry(true, false, false, false, false);
            DoomBuilder.General.Map.Map.ClearAllSelected();
            DoomBuilder.General.Map.Map.SelectionType = SelectionType.All;

            // Show toolbox window
            BuilderPlug.Me.ErrorCheckForm.Show((Form)DoomBuilder.General.Interface);
        }

        // Disenagaging
        public override void OnDisengage()
        {
            base.OnDisengage();

            // Hide object info
            DoomBuilder.General.Interface.HideInfo();

            // Restore selection
            DoomBuilder.General.Map.Map.SelectMarkedGeometry(true, true);
            DoomBuilder.General.Map.Map.ClearAllMarks(false);

            // Hide toolbox window
            BuilderPlug.Me.ErrorCheckForm.CloseWindow();
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
            List<ErrorResult> selection = BuilderPlug.Me.ErrorCheckForm.SelectedResults; //mxd

            renderer.RedrawSurface();

            // Render lines
            if (renderer.StartPlotter(true))
            {
                renderer.PlotLinedefSet(DoomBuilder.General.Map.Map.Linedefs);
                renderer.PlotVerticesSet(DoomBuilder.General.Map.Map.Vertices);
                foreach (ErrorResult result in selection) result.PlotSelection(renderer); //mxd
                renderer.Finish();
            }

            // Render things
            if (renderer.StartThings(true))
            {
                renderer.RenderThingSet(DoomBuilder.General.Map.Map.Things, DoomBuilder.General.Settings.ActiveThingsAlpha);
                //foreach(ErrorResult result in selection) result.RenderThingsSelection(renderer); //mxd
                renderer.Finish();
            }

            // Render overlay
            if (renderer.StartOverlay(true))
            {
                foreach (ErrorResult result in selection) result.RenderOverlaySelection(renderer); //mxd
                renderer.Finish();
            }

            renderer.Present();
        }
    }
}
