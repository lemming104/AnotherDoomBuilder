

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


using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.BuilderModes.General;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.ClassicModes
{
    public abstract class BaseClassicMode : ClassicMode
    {

        protected bool paintselectpressed; //mxd
        protected bool marqueSelectionIncludesThings; //mxd

        // Constructor
        protected BaseClassicMode()
        {
            // Initialize

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

        // This occurs when the user presses Copy. All selected geometry must be marked for copying!
        public override bool OnCopyBegin()
        {
            DoomBuilder.General.Map.Map.MarkAllSelectedGeometry(true, false, true, true, false);

            // Return true when anything is selected so that the copy continues
            // We only have to check vertices for the geometry, because without selected
            // vertices, no complete structure can exist.
            return (DoomBuilder.General.Map.Map.GetMarkedVertices(true).Count > 0) ||
                   (DoomBuilder.General.Map.Map.GetMarkedThings(true).Count > 0);
        }

        // This is called when pasting begins
        public override bool OnPasteBegin(PasteOptions options)
        {
            // These modes support pasting
            return true;
        }

        // This is called when something was pasted.
        public override void OnPasteEnd(PasteOptions options)
        {
            DoomBuilder.General.Map.Map.ClearAllSelected();
            DoomBuilder.General.Map.Map.SelectMarkedGeometry(true, true);
            DoomBuilder.General.Map.Renderer2D.UpdateExtraFloorFlag(); //mxd

            // Switch to EditSelectionMode
            EditSelectionMode editmode = new EditSelectionMode();
            editmode.Pasting = true;
            editmode.UpdateSlopes = false;
            editmode.PasteOptions = options;
            DoomBuilder.General.Editing.ChangeMode(editmode);
        }

        // Double-clicking
        public override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            int k = 0;
            if (e.Button == MouseButtons.Left) k = (int)Keys.LButton;
            if (e.Button == MouseButtons.Middle) k = (int)Keys.MButton;
            if (e.Button == MouseButtons.Right) k = (int)Keys.RButton;
            if (e.Button == MouseButtons.XButton1) k = (int)Keys.XButton1;
            if (e.Button == MouseButtons.XButton2) k = (int)Keys.XButton2;

            // Double select-click? Make that the same as single edit-click
            if (DoomBuilder.General.Actions.GetActionByName("builder_classicselect").KeyMatches(k))
            {
                Actions.Action a = DoomBuilder.General.Actions.GetActionByName("builder_classicedit");
                if (a != null) a.Invoke();
            }
        }

        //mxd
        protected override void OnUpdateMultiSelection()
        {
            base.OnUpdateMultiSelection();
            marqueSelectionIncludesThings = DoomBuilder.General.Interface.AltState;
        }

        //mxd
        protected override MarqueSelectionMode GetMultiSelectionMode()
        {
            if (DoomBuilder.General.Interface.CtrlState && DoomBuilder.General.Interface.ShiftState)
                return MarqueSelectionMode.INTERSECT;

            if (DoomBuilder.General.Interface.CtrlState)
                return MarqueSelectionMode.SUBTRACT;

            if (DoomBuilder.General.Interface.ShiftState ^ BuilderPlug.Me.AdditiveSelect)
                return MarqueSelectionMode.ADD;

            return MarqueSelectionMode.SELECT;
        }

        //mxd
        public override void OnMapTestEnd(bool testFromCurrentPosition)
        {
            base.OnMapTestEnd(testFromCurrentPosition);
            DoomBuilder.General.Interface.RedrawDisplay(); // Redraw display to hide changes :)
        }

        //mxd
        protected void PlaceThingsAtPositions(List<Vector2D> positions)
        {
            if (positions.Count < 1)
            {
                DoomBuilder.General.Interface.DisplayStatus(StatusType.Warning, "This action requires selection of some description!");
                return;
            }

            DoomBuilder.General.Map.UndoRedo.CreateUndo("Place " + (positions.Count > 1 ? "things" : "thing"));
            List<Thing> things = new List<Thing>();

            // Create things
            foreach (Vector2D pos in positions)
            {
                Thing t = DoomBuilder.General.Map.Map.CreateThing();
                if (t != null)
                {
                    DoomBuilder.General.Settings.ApplyDefaultThingSettings(t);
                    t.Move(pos);
                    t.UpdateConfiguration();
                    t.Selected = true;
                    t.SnapToAccuracy(); // Snap to map format accuracy
                    things.Add(t);
                }
            }

            //Operation failed?..
            if (things.Count < 1)
            {
                DoomBuilder.General.Interface.DisplayStatus(StatusType.Warning, "This action requires selection of some description!");
                DoomBuilder.General.Map.UndoRedo.WithdrawUndo();
                return;
            }

            //Show realtime thing edit dialog
            DoomBuilder.General.Interface.OnEditFormValuesChanged += thingEditForm_OnValuesChanged;
            if (DoomBuilder.General.Interface.ShowEditThings(things) == DialogResult.Cancel)
            {
                DoomBuilder.General.Map.UndoRedo.WithdrawUndo();
            }
            else
            {
                DoomBuilder.General.Interface.DisplayStatus(StatusType.Info, "Placed " + things.Count + " things.");
            }
            DoomBuilder.General.Interface.OnEditFormValuesChanged -= thingEditForm_OnValuesChanged;
        }

        //mxd
        protected static void DeleteThings(ICollection<Thing> things)
        {
            if (things.Count == 0) return;

            DoomBuilder.General.Map.Map.BeginAddRemove(); //mxd

            // Dispose selected things
            foreach (Thing t in things)
            {
                //mxd. Do some path reconnecting shenanigans...
                ThingTypeInfo info = DoomBuilder.General.Map.Data.GetThingInfo(t.Type);
                string targetclass = string.Empty;
                int targetarg = -1;

                // Thing type can be changed in MAPINFO DoomEdNums block...
                switch (info.ClassName.ToLowerInvariant())
                {
                    case "interpolationpoint":
                        if (t.Tag != 0 && t.Args[3] != 0)
                        {
                            targetclass = "interpolationpoint";
                            targetarg = 3;
                        }
                        break;

                    case "patrolpoint":
                        if (t.Tag != 0 && t.Args[0] != 0)
                        {
                            targetclass = "patrolpoint";
                            targetarg = 0;
                        }
                        break;
                }

                // Try to reconnect path...
                if (!string.IsNullOrEmpty(targetclass) && targetarg > -1)
                {
                    DoomBuilder.General.Map.Map.EndAddRemove(); // We'll need to unlock the things array...

                    foreach (Thing other in DoomBuilder.General.Map.Map.Things)
                    {
                        if (other.Index == t.Index)
                            continue;
                        info = DoomBuilder.General.Map.Data.GetThingInfo(other.Type);
                        if (info.ClassName.ToLowerInvariant() == targetclass && other.Args[targetarg] == t.Tag)
                        {
                            other.Move(other.Position); //hacky way to call BeforePropsChange()...
                            other.Args[targetarg] = t.Args[targetarg];
                            break;
                        }
                    }

                    DoomBuilder.General.Map.Map.BeginAddRemove(); // We'll need to lock it again...
                }

                // Get rid of the thing
                t.Dispose();
            }

            DoomBuilder.General.Map.Map.EndAddRemove(); //mxd
        }

        public virtual void OnViewSelectionNumbersChanged(bool enabled) { } //mxd
        public virtual void OnViewSelectionEffectsChanged(bool enabled) { } //mxd

        //mxd
        private void thingEditForm_OnValuesChanged(object sender, EventArgs e)
        {
            // Update things filter
            DoomBuilder.General.Map.ThingsFilter.Update();

            // Update entire display
            DoomBuilder.General.Interface.RedrawDisplay();
        }

        [BeginAction("placevisualstart")]
        public void PlaceVisualStartThing()
        {
            Thing thingfound = null;

            // Not during volatile mode
            if (this.Attributes.Volatile) return;

            // Mouse must be inside window
            if (!mouseinside) return;

            DoomBuilder.General.Interface.DisplayStatus(StatusType.Action, "Placed Visual Mode camera start thing.");

            // Go for all things
            List<Thing> things = new List<Thing>(DoomBuilder.General.Map.Map.Things);
            foreach (Thing t in things)
            {
                if (t.Type == DoomBuilder.General.Map.Config.Start3DModeThingType)
                {
                    if (thingfound == null)
                    {
                        // Move this thing
                        t.Move(mousemappos);
                        thingfound = t;
                    }
                    else
                    {
                        // One was already found and moved, delete this one
                        t.Dispose();
                    }
                }
            }

            // No thing found?
            if (thingfound == null)
            {
                // Make a new one
                Thing t = DoomBuilder.General.Map.Map.CreateThing();
                if (t != null)
                {
                    t.Type = DoomBuilder.General.Map.Config.Start3DModeThingType;
                    t.Move(mousemappos);
                    t.UpdateConfiguration();
                    DoomBuilder.General.Map.ThingsFilter.Update();
                    thingfound = t;
                }
            }

            if (thingfound != null)
            {
                // Make sure that the found thing is between ceiling and floor
                thingfound.DetermineSector();
                if (thingfound.Position.z < 0.0f) thingfound.Move(thingfound.Position.x, thingfound.Position.y, 0.0f);
                if (thingfound.Sector != null)
                {
                    if ((thingfound.Position.z + 50.0f) > (thingfound.Sector.CeilHeight - thingfound.Sector.FloorHeight))
                        thingfound.Move(thingfound.Position.x, thingfound.Position.y,
                            thingfound.Sector.CeilHeight - thingfound.Sector.FloorHeight - 50.0f);
                }
            }

            // Update Visual Mode camera
            DoomBuilder.General.Map.VisualCamera.PositionAtThing();

            // Redraw display to show changes
            DoomBuilder.General.Interface.RedrawDisplay();
        }

        //mxd
        [BeginAction("classicpaintselect")]
        protected virtual void OnPaintSelectBegin()
        {
            paintselectpressed = true;
        }

        //mxd
        [EndAction("classicpaintselect")]
        protected virtual void OnPaintSelectEnd()
        {
            paintselectpressed = false;
        }

        //mxd
        [BeginAction("syncedthingedit")]
        public void ToggleSyncronizedThingsEdit()
        {
            BuilderPlug.Me.SyncronizeThingEdit = !BuilderPlug.Me.SyncronizeThingEdit;
            DoomBuilder.General.Interface.DisplayStatus(StatusType.Info, BuilderPlug.Me.SyncronizeThingEdit ?
                "Things editing is SYNCRONIZED" :
                "Things editing is not syncronized");

            // Update interface
            BuilderPlug.Me.MenusForm.SyncronizeThingEditButton.Checked = BuilderPlug.Me.SyncronizeThingEdit;
            BuilderPlug.Me.MenusForm.SyncronizeThingEditLinedefsItem.Checked = BuilderPlug.Me.SyncronizeThingEdit;
            BuilderPlug.Me.MenusForm.SyncronizeThingEditSectorsItem.Checked = BuilderPlug.Me.SyncronizeThingEdit;
        }
    }
}
