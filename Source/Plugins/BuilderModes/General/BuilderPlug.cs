
#region ================== Copyright (c) 2007 Pascal vd Heiden

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

#endregion

#region ================== Namespaces

using CodeImp.DoomBuilder.Actions;
using CodeImp.DoomBuilder.BuilderModes.Interface;
using CodeImp.DoomBuilder.BuilderModes.IO;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Plugins;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
    internal class ToastMessages
    {
        public static readonly string VISUALSLOPING = "visualsloping";
        public static readonly string CHANGEMAPELEMENTINDEX = "changemapelementindex";
    }

    public class BuilderPlug : Plug
    {
        #region ================== API Declarations

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        #endregion

        #region ================== Constants

        internal const int WS_HSCROLL = 0x100000;
        internal const int WS_VSCROLL = 0x200000;
        internal const int GWL_STYLE = -16;

        #endregion

        #region ================== Structs (mxd)

        public struct MakeDoorSettings
        {
            public readonly string DoorTexture;
            public readonly string TrackTexture;
            public readonly string CeilingTexture;
            public readonly bool ResetOffsets;
            public readonly bool ApplyActionSpecials;
            public readonly bool ApplyTag;

            public MakeDoorSettings(string doortexture, string tracktexture, string ceilingtexture, bool resetoffsets, bool applyactionspecials, bool applytag)
            {
                DoorTexture = doortexture;
                TrackTexture = tracktexture;
                CeilingTexture = ceilingtexture;
                ResetOffsets = resetoffsets;
                ApplyActionSpecials = applyactionspecials;
                ApplyTag = applytag;
            }
        }

        #endregion

        #region ================== Variables

        // Static instance

        // Main objects
        private FindReplaceForm findreplaceform;
        private ErrorCheckForm errorcheckform;

        // Dockers
        private UndoRedoPanel undoredopanel;
        private Docker undoredodocker;
        private SectorDrawingOptionsPanel drawingOverridesPanel; //mxd
        private Docker drawingOverridesDocker; //mxd

        // Settings
        private Point copiedoffsets;
        private bool dontMoveGeometryOutsideMapBoundary;//mxd

        #endregion

        #region ================== Properties

        public override string Name { get { return "Ultimate Doom Builder"; } } //mxd
        public static BuilderPlug Me { get; private set; }

        //mxd. BuilderModes.dll revision should always match the main module revision
        public override bool StrictRevisionMatching { get { return true; } }
        public override int MinimumRevision { get { return Assembly.GetExecutingAssembly().GetName().Version.Revision; } }

        public MenusForm MenusForm { get; private set; }
        public FindReplaceForm FindReplaceForm { get { return findreplaceform ?? (findreplaceform = new FindReplaceForm()); } }
        public ErrorCheckForm ErrorCheckForm { get { return errorcheckform ?? (errorcheckform = new ErrorCheckForm()); } }
        public PreferencesForm PreferencesForm { get; private set; }

        // Settings
        public int ShowVisualThings { get; set; }
        public bool UseGravity { get; set; }
        public int ChangeHeightBySidedef { get; private set; }
        public bool EditNewThing { get; private set; }
        public bool EditNewSector { get; private set; }
        public bool AdditiveSelect { get; private set; }
        public bool AdditivePaintSelect { get; private set; }
        public bool AutoClearSelection { get; private set; }
        public bool VisualModeClearSelection { get; private set; }
        public string CopiedTexture { get; set; }
        public string CopiedFlat { get; set; }
        public Point CopiedOffsets { get { return copiedoffsets; } set { copiedoffsets = value; } }
        public VertexProperties CopiedVertexProps { get; set; }
        public SectorProperties CopiedSectorProps { get; set; }
        public SidedefProperties CopiedSidedefProps { get; set; }
        public LinedefProperties CopiedLinedefProps { get; set; }
        public ThingProperties CopiedThingProps { get; set; }
        public bool ViewSelectionNumbers { get; set; }
        public bool ViewSelectionEffects { get; set; } //mxd
        public float StitchRange { get; internal set; }
        public float HighlightRange { get; private set; }
        public float HighlightThingsRange { get; private set; }
        public float SplitLinedefsRange { get; private set; }
        public float MouseSelectionThreshold { get; private set; }
        public bool AutoDragOnPaste { get; set; }
        public bool AutoDrawOnEdit { get; set; } //mxd
        public bool AutoAlignTextureOffsetsOnCreate { get; set; } //mxd
        public bool DontMoveGeometryOutsideMapBoundary { get { return dontMoveGeometryOutsideMapBoundary; } set { DontMoveGeometryOutsideMapBoundary = value; } } //mxd
        public bool MarqueSelectTouching { get; set; } //mxd
        public bool SyncSelection { get; set; } //mxd
        public bool LockSectorTextureOffsetsWhileDragging { get; internal set; } //mxd
        public bool Lock3DFloorSectorTextureOffsetsWhileDragging { get; internal set; } //mxd
        public bool SyncronizeThingEdit { get; internal set; } //mxd
        public bool AlphaBasedTextureHighlighting { get; internal set; } //mxd
        public bool ShowLightRadii { get; internal set; } //mxd
        public bool ShowSoundRadii { get; internal set; } //mxd
        public int ScaleTexturesOnSlopes { get; internal set; }
        public int EventLineLabelVisibility { get; internal set; }
        public int EventLineLabelStyle { get; internal set; }
        public bool EventLineDistinctColors { get; internal set; }
        public bool UseOppositeSmartPivotHandle { get; internal set; }
        public bool SelectChangedafterUndoRedo { get; internal set; }
        public bool SelectAdjacentVisualVertexSlopeHandles { get; internal set; }
        public bool UseBuggyFloodSelect { get; internal set; }

        //mxd. "Make Door" action persistent settings
        internal MakeDoorSettings MakeDoor;

        #endregion

        #region ================== Initialize / Dispose

        // When plugin is initialized
        public override void OnInitialize()
        {
            // Setup
            Me = this;

            // Settings
            ShowVisualThings = 2;
            UseGravity = false;
            LoadSettings();
            LoadUISettings(); //mxd

            // Load menus form and register it
            MenusForm = new MenusForm();
            MenusForm.Register();
            MenusForm.TextureOffsetLock.Checked = LockSectorTextureOffsetsWhileDragging; //mxd
            MenusForm.TextureOffset3DFloorLock.Checked = Lock3DFloorSectorTextureOffsetsWhileDragging;
            MenusForm.SyncronizeThingEditButton.Checked = SyncronizeThingEdit; //mxd
            MenusForm.SyncronizeThingEditSectorsItem.Checked = SyncronizeThingEdit; //mxd
            MenusForm.SyncronizeThingEditLinedefsItem.Checked = SyncronizeThingEdit; //mxd
            MenusForm.ItemLightRadii.Checked = ShowLightRadii;
            MenusForm.ButtonLightRadii.Checked = ShowLightRadii;
            MenusForm.ItemSoundRadii.Checked = ShowSoundRadii;
            MenusForm.ButtonSoundRadii.Checked = ShowSoundRadii;

            // Load Undo\Redo docker
            undoredopanel = new UndoRedoPanel();
            undoredodocker = new Docker("undoredo", "Undo / Redo", undoredopanel);
            General.Interface.AddDocker(undoredodocker);

            //mxd. Create Overrides docker
            drawingOverridesPanel = new SectorDrawingOptionsPanel();
            drawingOverridesDocker = new Docker("drawingoverrides", "Draw Settings", drawingOverridesPanel);

            //mxd
            General.Actions.BindMethods(this);

            // Register toasts
            General.ToastManager.RegisterToast(ToastMessages.VISUALSLOPING, "Visual sloping", "Toasts related to visual sloping");
            General.ToastManager.RegisterToast(ToastMessages.CHANGEMAPELEMENTINDEX, "Change map element index", "Toasts related to changing the index of map elements");
        }

        // Disposer
        public override void Dispose()
        {
            // Not already disposed?
            if (!IsDisposed)
            {
                // Clean up
                General.Interface.RemoveDocker(undoredodocker);

                undoredopanel.Dispose();
                drawingOverridesPanel.Dispose(); //mxd
                MenusForm.Unregister();
                MenusForm.Dispose();
                MenusForm = null;

                //mxd. These are created on demand, so they may be nulls.
                if (findreplaceform != null)
                {
                    findreplaceform.Dispose();
                    findreplaceform = null;
                }
                if (errorcheckform != null)
                {
                    errorcheckform.Dispose();
                    errorcheckform = null;
                }

                // Done
                Me = null;
                base.Dispose();
            }
        }

        #endregion

        #region ================== Methods

        // This loads the plugin settings
        private void LoadSettings()
        {
            ChangeHeightBySidedef = General.Settings.ReadPluginSetting("changeheightbysidedef", 0);
            EditNewThing = General.Settings.ReadPluginSetting("editnewthing", true);
            EditNewSector = General.Settings.ReadPluginSetting("editnewsector", false);
            AdditiveSelect = General.Settings.ReadPluginSetting("additiveselect", false);
            AdditivePaintSelect = General.Settings.ReadPluginSetting("additivepaintselect", AdditiveSelect); // use the same value as additiveselect by default
            AutoClearSelection = General.Settings.ReadPluginSetting("autoclearselection", false);
            VisualModeClearSelection = General.Settings.ReadPluginSetting("visualmodeclearselection", false);
            StitchRange = General.Settings.ReadPluginSetting("stitchrange", 20);
            HighlightRange = General.Settings.ReadPluginSetting("highlightrange", 20);
            HighlightThingsRange = General.Settings.ReadPluginSetting("highlightthingsrange", 10);
            SplitLinedefsRange = General.Settings.ReadPluginSetting("splitlinedefsrange", 10);
            MouseSelectionThreshold = General.Settings.ReadPluginSetting("mouseselectionthreshold", 2);
            AutoDragOnPaste = General.Settings.ReadPluginSetting("autodragonpaste", false);
            AutoDrawOnEdit = General.Settings.ReadPluginSetting("autodrawonedit", true); //mxd
            AutoAlignTextureOffsetsOnCreate = General.Settings.ReadPluginSetting("autoaligntextureoffsetsoncreate", false); //mxd
            dontMoveGeometryOutsideMapBoundary = General.Settings.ReadPluginSetting("dontmovegeometryoutsidemapboundary", false); //mxd
            SyncSelection = General.Settings.ReadPluginSetting("syncselection", false); //mxd
            ScaleTexturesOnSlopes = General.Settings.ReadPluginSetting("scaletexturesonslopes", 0);
            EventLineLabelVisibility = General.Settings.ReadPluginSetting("eventlinelabelvisibility", 3);
            EventLineLabelStyle = General.Settings.ReadPluginSetting("eventlinelabelstyle", 2);
            EventLineDistinctColors = General.Settings.ReadPluginSetting("eventlinedistinctcolors", true);
            UseOppositeSmartPivotHandle = General.Settings.ReadPluginSetting("useoppositesmartpivothandle", true);
            SelectChangedafterUndoRedo = General.Settings.ReadPluginSetting("selectchangedafterundoredo", false);
            UseBuggyFloodSelect = General.Settings.ReadPluginSetting("usebuggyfloodselect", false);
        }

        //mxd. Load settings, which can be changed via UI
        private void LoadUISettings()
        {
            LockSectorTextureOffsetsWhileDragging = General.Settings.ReadPluginSetting("locktextureoffsets", false);
            Lock3DFloorSectorTextureOffsetsWhileDragging = General.Settings.ReadPluginSetting("lock3dfloortextureoffsets", false);
            ViewSelectionNumbers = General.Settings.ReadPluginSetting("viewselectionnumbers", true);
            ViewSelectionEffects = General.Settings.ReadPluginSetting("viewselectioneffects", true);
            SyncronizeThingEdit = General.Settings.ReadPluginSetting("syncthingedit", true);
            AlphaBasedTextureHighlighting = General.Settings.ReadPluginSetting("alphabasedtexturehighlighting", true);
            ShowLightRadii = General.Settings.ReadPluginSetting("showlightradii", true);
            ShowSoundRadii = General.Settings.ReadPluginSetting("showsoundradii", true);
        }

        //mxd. Save settings, which can be changed via UI
        private void SaveUISettings()
        {
            General.Settings.WritePluginSetting("locktextureoffsets", LockSectorTextureOffsetsWhileDragging);
            General.Settings.WritePluginSetting("lock3dfloortextureoffsets", Lock3DFloorSectorTextureOffsetsWhileDragging);
            General.Settings.WritePluginSetting("viewselectionnumbers", ViewSelectionNumbers);
            General.Settings.WritePluginSetting("viewselectioneffects", ViewSelectionEffects);
            General.Settings.WritePluginSetting("syncthingedit", SyncronizeThingEdit);
            General.Settings.WritePluginSetting("alphabasedtexturehighlighting", AlphaBasedTextureHighlighting);
            General.Settings.WritePluginSetting("showlightradii", ShowLightRadii);
            General.Settings.WritePluginSetting("showsoundradii", ShowSoundRadii);
        }

        //mxd. These should be reset when changing maps
        private void ResetCopyProperties()
        {
            CopiedVertexProps = null;
            CopiedThingProps = null;
            CopiedLinedefProps = null;
            CopiedSidedefProps = null;
            CopiedSectorProps = null;
        }

        #endregion

        #region ================== Events

        // When floor surface geometry is created for classic modes
        public override void OnSectorFloorSurfaceUpdate(Sector s, ref FlatVertex[] vertices)
        {
            ImageData img = General.Map.Data.GetFlatImage(s.LongFloorTexture);
            if ((img != null) && img.IsImageLoaded)
            {
                //mxd. Merged from GZDoomEditing plugin
                if (General.Map.UDMF)
                {
                    // Fetch ZDoom fields
                    Vector2D offset = new Vector2D(s.Fields.GetValue("xpanningfloor", 0.0),
                                                   s.Fields.GetValue("ypanningfloor", 0.0));
                    Vector2D scale = new Vector2D(s.Fields.GetValue("xscalefloor", 1.0),
                                                  s.Fields.GetValue("yscalefloor", 1.0));
                    double rotate = s.Fields.GetValue("rotationfloor", 0.0);
                    int color, light;
                    bool absolute;

                    //mxd. Apply GLDEFS override?
                    if (General.Map.Data.GlowingFlats.ContainsKey(s.LongFloorTexture)
                        && General.Map.Data.GlowingFlats[s.LongFloorTexture].Fullbright)
                    {
                        color = -1;
                        light = 255;
                        absolute = true;
                    }
                    else
                    {
                        color = PixelColor.Modulate(PixelColor.FromInt(s.Fields.GetValue("lightcolor", -1)), PixelColor.FromInt(s.Fields.GetValue("color_floor", -1))).ToInt();
                        light = s.Fields.GetValue("lightfloor", 0);
                        absolute = s.Fields.GetValue("lightfloorabsolute", false);
                    }

                    // Setup the vertices with the given settings
                    SetupSurfaceVertices(vertices, s, img, offset, scale, rotate, color, light, absolute);
                }
                else
                {
                    // Make scalars
                    float sw = 1.0f / img.ScaledWidth;
                    float sh = 1.0f / img.ScaledHeight;

                    // Make proper texture coordinates
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i].u = vertices[i].u * sw;
                        vertices[i].v = -vertices[i].v * sh;
                    }
                }
            }
            else // [ZZ] proper fallback please.
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].u = vertices[i].u / 64;
                    vertices[i].v = -vertices[i].v / 64;
                }
            }
        }

        // When ceiling surface geometry is created for classic modes
        public override void OnSectorCeilingSurfaceUpdate(Sector s, ref FlatVertex[] vertices)
        {
            ImageData img = General.Map.Data.GetFlatImage(s.LongCeilTexture);
            if ((img != null) && img.IsImageLoaded)
            {
                //mxd. Merged from GZDoomEditing plugin
                if (General.Map.UDMF)
                {
                    // Fetch ZDoom fields
                    Vector2D offset = new Vector2D(s.Fields.GetValue("xpanningceiling", 0.0),
                                                   s.Fields.GetValue("ypanningceiling", 0.0));
                    Vector2D scale = new Vector2D(s.Fields.GetValue("xscaleceiling", 1.0),
                                                  s.Fields.GetValue("yscaleceiling", 1.0));
                    double rotate = s.Fields.GetValue("rotationceiling", 0.0);
                    int color, light;
                    bool absolute;

                    //mxd. Apply GLDEFS override?
                    if (General.Map.Data.GlowingFlats.ContainsKey(s.LongCeilTexture)
                        && General.Map.Data.GlowingFlats[s.LongCeilTexture].Fullbright)
                    {
                        color = -1;
                        light = 255;
                        absolute = true;
                    }
                    else
                    {
                        color = PixelColor.Modulate(PixelColor.FromInt(s.Fields.GetValue("lightcolor", -1)), PixelColor.FromInt(s.Fields.GetValue("color_ceiling", -1))).ToInt();
                        light = s.Fields.GetValue("lightceiling", 0);
                        absolute = s.Fields.GetValue("lightceilingabsolute", false);
                    }

                    // Setup the vertices with the given settings
                    SetupSurfaceVertices(vertices, s, img, offset, scale, rotate, color, light, absolute);
                }
                else
                {
                    // Make scalars
                    float sw = 1.0f / img.ScaledWidth;
                    float sh = 1.0f / img.ScaledHeight;

                    // Make proper texture coordinates
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i].u = vertices[i].u * sw;
                        vertices[i].v = -vertices[i].v * sh;
                    }
                }
            }
            else // [ZZ] proper fallback please.
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].u = vertices[i].u / 64;
                    vertices[i].v = -vertices[i].v / 64;
                }
            }
        }

        // When the editing mode changes
        public override bool OnModeChange(EditMode oldmode, EditMode newmode)
        {
            // Show the correct menu for the new mode
            MenusForm.ShowEditingModeMenu(newmode);

            return base.OnModeChange(oldmode, newmode);
        }

        // When the Preferences dialog is shown
        public override void OnShowPreferences(PreferencesController controller)
        {
            base.OnShowPreferences(controller);

            // Load preferences
            PreferencesForm = new PreferencesForm();
            PreferencesForm.Setup(controller);
        }

        // When the Preferences dialog is closed
        public override void OnClosePreferences(PreferencesController controller)
        {
            base.OnClosePreferences(controller);

            // Apply settings that could have been changed
            LoadSettings();

            // Unload preferences
            PreferencesForm.Dispose();
            PreferencesForm = null;
        }

        // New map created
        public override void OnMapNewEnd()
        {
            base.OnMapNewEnd();
            undoredopanel.SetBeginDescription("New Map");
            undoredopanel.UpdateList();

            //mxd
            General.Interface.AddDocker(drawingOverridesDocker);
            drawingOverridesPanel.Setup();
            MakeDoor = new MakeDoorSettings(General.Map.Config.MakeDoorDoor, General.Map.Config.MakeDoorTrack, General.Map.Config.MakeDoorCeiling, MakeDoor.ResetOffsets, MakeDoor.ApplyActionSpecials, MakeDoor.ApplyTag);
            ResetCopyProperties();
        }

        // Map opened
        public override void OnMapOpenEnd()
        {
            base.OnMapOpenEnd();
            undoredopanel.SetBeginDescription("Opened Map");
            undoredopanel.UpdateList();

            //mxd
            General.Interface.AddDocker(drawingOverridesDocker);
            drawingOverridesPanel.Setup();
            General.Map.Renderer2D.UpdateExtraFloorFlag();
            MakeDoor = new MakeDoorSettings(General.Map.Config.MakeDoorDoor, General.Map.Config.MakeDoorTrack, General.Map.Config.MakeDoorCeiling, MakeDoor.ResetOffsets, MakeDoor.ApplyActionSpecials, MakeDoor.ApplyTag);
            ResetCopyProperties();
        }

        //mxd
        public override void OnMapCloseBegin()
        {
            drawingOverridesPanel.Terminate();
            General.Interface.RemoveDocker(drawingOverridesDocker);
        }

        // Map closed
        public override void OnMapCloseEnd()
        {
            base.OnMapCloseEnd();
            undoredopanel.UpdateList();
            errorcheckform = null; //mxd. Error checks may need to be reinitialized

            //mxd. Save settings
            SaveUISettings();
        }

        //mxd. Error checks may need to be reinitialized
        public override void OnMapReconfigure()
        {
            errorcheckform = null;
        }

        // Redo performed
        public override void OnRedoEnd()
        {
            base.OnRedoEnd();
            undoredopanel.UpdateList();
        }

        // Undo performed
        public override void OnUndoEnd()
        {
            base.OnUndoEnd();
            undoredopanel.UpdateList();
        }

        // Undo created
        public override void OnUndoCreated()
        {
            base.OnUndoCreated();
            undoredopanel.UpdateList();
        }

        // Undo withdrawn
        public override void OnUndoWithdrawn()
        {
            base.OnUndoWithdrawn();
            undoredopanel.UpdateList();
        }

        #endregion

        #region ================== Tools

        //mxd. merged from GZDoomEditing plugin
        // This applies the given values on the vertices
        private static void SetupSurfaceVertices(FlatVertex[] vertices, Sector s, ImageData img, Vector2D offset,
                                          Vector2D scale, double rotate, int color, int light, bool absolute)
        {
            // Prepare for math!
            rotate = Angle2D.DegToRad(rotate);
            Vector2D texscale = new Vector2D(1.0f / img.ScaledWidth, 1.0f / img.ScaledHeight);
            if (!absolute) light = s.Brightness + light;
            PixelColor lightcolor = PixelColor.FromInt(color);
            PixelColor brightness = PixelColor.FromInt(General.Map.Renderer2D.CalculateBrightness(light));
            PixelColor finalcolor = PixelColor.Modulate(lightcolor, brightness);
            color = finalcolor.WithAlpha(255).ToInt();

            // Do the math for all vertices
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector2D pos = new Vector2D(vertices[i].x, vertices[i].y);
                pos = pos.GetRotated(rotate);
                pos.y = -pos.y;
                pos = (pos + offset) * scale * texscale;
                vertices[i].u = (float)pos.x;
                vertices[i].v = (float)pos.y;
                vertices[i].c = color;
            }
        }

        // This finds all class types that inherits from the given type
        public Type[] FindClasses(Type t)
        {
            List<Type> found = new List<Type>();

            // Get all exported types
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type it in types)
            {
                // Compare types
                if (t.IsAssignableFrom(it)) found.Add(it);
            }

            // Return list
            return found.ToArray();
        }

        #endregion

        #region ================== Actions (mxd)

        [BeginAction("exporttoidstudio")]
        private void ExportToidStudio()
        {
            idStudioExporterForm form = new idStudioExporterForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                idStudioExporter exporter = new idStudioExporter();
                exporter.Export(form);
                MessageBox.Show("Map exported successfully", "idStudio Exporter", MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }

        [BeginAction("exporttoobj")]
        private void ExportToObj()
        {
            // Convert geometry selection to sectors
            General.Map.Map.ConvertSelection(SelectionType.Sectors);

            //get sectors
            ICollection<Sector> sectors = General.Map.Map.SelectedSectorsCount == 0 ? General.Map.Map.Sectors : General.Map.Map.GetSelectedSectors(true);
            if (sectors.Count == 0)
            {
                General.Interface.DisplayStatus(StatusType.Warning, "OBJ export failed. Map has no sectors!");
                return;
            }

            //show settings form
            WavefrontSettingsForm form = new WavefrontSettingsForm(General.Map.Map.SelectedSectorsCount == 0 ? -1 : sectors.Count);
            if (form.ShowDialog() == DialogResult.OK)
            {
                WavefrontExportSettings data = new WavefrontExportSettings(form);
                WavefrontExporter e = new WavefrontExporter();
                e.Export(sectors, data);
            }
        }

        [BeginAction("exporttoimage")]
        private void ExportToImage()
        {
            // Get sectors
            ICollection<Sector> sectors = General.Map.Map.SelectedSectorsCount == 0 ? General.Map.Map.Sectors : General.Map.Map.GetSelectedSectors(true);
            if (sectors.Count == 0)
            {
                General.Interface.DisplayStatus(StatusType.Warning, "Image export failed. Map has no sectors!");
                return;
            }

            ImageExportSettingsForm form = new ImageExportSettingsForm();
            form.ShowDialog();
        }

        #endregion
    }
}
