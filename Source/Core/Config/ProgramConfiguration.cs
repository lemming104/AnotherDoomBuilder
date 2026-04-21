
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

using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Plugins;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Config
{
    public class ProgramConfiguration
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // Original configuration

        // Cached variables
        private float doublesidedalpha;
        private bool usehighlight; //mxd

        //mxd. Script editor settings

        //mxd. Text labels settings

        //mxd
        private int maxRecentFiles;

        //volte

        // These are not stored in the configuration, only used at runtime
        private List<string> defaultthingflags;

        // Autosave

        #endregion

        #region ================== Properties

        internal Configuration Config { get; private set; }
        public bool BlackBrowsers { get; internal set; }
        public int VisualFOV { get; internal set; }
        public int ImageBrightness { get; internal set; }
        public float DoubleSidedAlpha { get { return doublesidedalpha; } internal set { doublesidedalpha = value; DoubleSidedAlphaByte = (byte)(doublesidedalpha * 255f); } }
        public byte DoubleSidedAlphaByte { get; private set; }
        public float ActiveThingsAlpha { get; internal set; } //mxd
        public float InactiveThingsAlpha { get; internal set; } //mxd
        public float HiddenThingsAlpha { get; internal set; } //mxd
        public float BackgroundAlpha { get; internal set; }
        public float VisualMouseSensX { get; internal set; }
        public float VisualMouseSensY { get; internal set; }
        public bool QualityDisplay { get; internal set; }
        public bool TestMonsters { get; internal set; }
        public int DefaultViewMode { get; internal set; }
        public bool ClassicBilinear { get; internal set; }
        public bool VisualBilinear { get; internal set; }
        public int MouseSpeed { get; internal set; }
        public int MoveSpeed { get; internal set; }
        public float ViewDistance { get; internal set; }
        public bool InvertYAxis { get; internal set; }
        public int AutoScrollSpeed { get; internal set; }
        public int ZoomFactor { get; internal set; }
        public bool ShowErrorsWindow { get; internal set; }
        public bool AnimateVisualSelection { get; internal set; }
        internal string ScreenshotsPath { get; set; } //mxd
        internal int PreviousVersion { get; private set; }
        internal PasteOptions PasteOptions { get; set; }
        public int DockersPosition { get; internal set; }
        public bool CollapseDockers { get; internal set; }
        public int DockersWidth { get; internal set; }
        public bool ToolbarScript { get; internal set; }
        public bool ToolbarUndo { get; internal set; }
        public bool ToolbarCopy { get; internal set; }
        public bool ToolbarPrefabs { get; internal set; }
        public bool ToolbarFilter { get; internal set; }
        public bool ToolbarViewModes { get; internal set; }
        public bool ToolbarGeometry { get; internal set; }
        public bool ToolbarTesting { get; internal set; }
        public bool ToolbarFile { get; internal set; }
        public float FilterAnisotropy { get; internal set; }
        public int AntiAliasingSamples { get; internal set; } //mxd
        public bool ShowTextureSizes { get; internal set; }
        public bool TextureSizesBelow { get; internal set; }
        public bool LocateTextureGroup { get; internal set; } //mxd
        public SplitLineBehavior SplitLineBehavior { get; set; } //mxd
        public MergeGeometryMode MergeGeometryMode { get; internal set; } //mxd
        public bool SplitJoinedSectors { get; internal set; } //mxd
        public bool ShowFPS { get; internal set; }
        public int[] ColorDialogCustomColors { get; internal set; }
        public bool AutoLaunchOnTest { get; internal set; }
        public bool ParallelizedLinedefPlotting { get; internal set; }
        public bool ParallelizedVertexPlotting { get; internal set; }

        //mxd. Highlight mode
        public bool UseHighlight
        {
            get { return usehighlight; }
            set
            {
                usehighlight = value;
                General.Map.Renderer3D.ShowSelection = General.Settings.UseHighlight;
                General.Map.Renderer3D.ShowHighlight = General.Settings.UseHighlight;
            }
        }

        public bool SwitchViewModes { get; set; } //mxd

        //mxd. Script editor settings
        public string ScriptFontName { get; internal set; }
        public int ScriptFontSize { get; internal set; }
        public bool ScriptFontBold { get; internal set; }
        public bool ScriptOnTop { get; internal set; }
        public bool ScriptAutoIndent { get; internal set; }
        public bool ScriptAllmanStyle { get; internal set; } //mxd
        public bool ScriptUseTabs { get; internal set; } //mxd
        public int ScriptTabWidth { get; internal set; }
        public bool ScriptAutoCloseBrackets { get; internal set; } //mxd
        public bool ScriptShowLineNumbers { get; internal set; } //mxd
        public bool ScriptShowFolding { get; internal set; } //mxd
        public bool ScriptAutoShowAutocompletion { get; internal set; } //mxd

        //mxd. Text labels settings
        public string TextLabelFontName { get; internal set; }
        public int TextLabelFontSize { get; internal set; }
        public bool TextLabelFontBold { get; internal set; }

        //mxd 
        public ModelRenderMode GZDrawModelsMode { get; internal set; }
        public LightRenderMode GZDrawLightsMode { get; internal set; }
        public bool GZDrawFog { get; internal set; }
        public bool GZDrawSky { get; internal set; }
        public bool GZToolbarGZDoom { get; internal set; }
        public bool GZSynchCameras { get; internal set; }
        public bool GZShowEventLines { get; internal set; }
        public bool GZOldHighlightMode { get; internal set; }
        public int GZMaxDynamicLights { get; internal set; }
        public bool GZStretchView { get; internal set; }
        public float GZVertexScale2D { get; internal set; }
        public bool GZShowVisualVertices { get; internal set; }
        public float GZVertexScale3D { get; internal set; }
        public string LastUsedConfigName { get; internal set; }
        public string LastUsedMapFolder { get; internal set; }
        public bool GZMarkExtraFloors { get; internal set; }
        public bool EnhancedRenderingEffects { get; set; } = true; //mxd
        public int MaxRecentFiles { get { return maxRecentFiles; } internal set { maxRecentFiles = General.Clamp(value, 8, 25); } }
        public bool AutoClearSidedefTextures { get; internal set; }
        public bool StoreSelectedEditTab { get; internal set; }
        internal bool CheckForUpdates { get; set; } //mxd
        public bool RenderComments { get; internal set; } //mxd
        public bool FixedThingsScale { get; internal set; } //mxd
        public bool RenderGrid { get; internal set; } //mxd
        public bool DynamicGridSize { get; internal set; } //mxd
        internal int IgnoredRemoteRevision { get; set; } //mxd

        //volte
        public bool ClassicRendering { get; internal set; }

        public bool FlatShadeVertices { get; internal set; }

        public bool AlwaysShowVertices { get; internal set; }

        //mxd. Left here for compatibility reasons...
        public string DefaultTexture { get { return General.Map != null ? General.Map.Options.DefaultWallTexture : "-"; } set { if (General.Map != null) General.Map.Options.DefaultWallTexture = value; } }
        public string DefaultFloorTexture { get { return General.Map != null ? General.Map.Options.DefaultFloorTexture : "-"; } set { if (General.Map != null) General.Map.Options.DefaultFloorTexture = value; } }
        public string DefaultCeilingTexture { get { return General.Map != null ? General.Map.Options.DefaultCeilingTexture : "-"; } set { if (General.Map != null) General.Map.Options.DefaultCeilingTexture = value; } }
        public int DefaultBrightness { get; set; }
        public int DefaultFloorHeight { get; set; }
        public int DefaultCeilingHeight { get; set; }

        public int DefaultThingType { get; set; } = 1;
        public double DefaultThingAngle { get; set; }

        // Autosave
        public bool Autosave { get; internal set; }
        public int AutosaveCount { get; internal set; }
        public int AutosaveInterval { get; internal set; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal ProgramConfiguration()
        {
            // We have no destructor
            GC.SuppressFinalize(this);
            defaultthingflags = new List<string>();
            PasteOptions = new PasteOptions();
        }

        #endregion

        #region ================== Loading / Saving

        // This loads the program configuration
        internal bool Load(string cfgfilepathname, string defaultfilepathname, string legacyfilepathname)
        {
            // First parse it
            if (Read(cfgfilepathname, defaultfilepathname, legacyfilepathname))
            {
                // Read the cache variables
                BlackBrowsers = Config.ReadSetting("blackbrowsers", true);
                VisualFOV = Config.ReadSetting("visualfov", 80);
                VisualMouseSensX = Config.ReadSetting("visualmousesensx", 40f);
                VisualMouseSensY = Config.ReadSetting("visualmousesensy", 40f);
                ImageBrightness = Config.ReadSetting("imagebrightness", 3);
                doublesidedalpha = Config.ReadSetting("doublesidedalpha", 0.4f);
                DoubleSidedAlphaByte = (byte)(doublesidedalpha * 255f);
                ActiveThingsAlpha = Config.ReadSetting("activethingsalpha", Presentation.THINGS_ALPHA); //mxd
                InactiveThingsAlpha = Config.ReadSetting("inactivethingsalpha", Presentation.THINGS_BACK_ALPHA); //mxd
                HiddenThingsAlpha = Config.ReadSetting("hiddenthingsalpha", Presentation.THINGS_HIDDEN_ALPHA); //mxd
                BackgroundAlpha = Config.ReadSetting("backgroundalpha", 1.0f);
                QualityDisplay = Config.ReadSetting("qualitydisplay", true);
                TestMonsters = Config.ReadSetting("testmonsters", true);
                DefaultViewMode = Config.ReadSetting("defaultviewmode", (int)ViewMode.Normal);
                ClassicBilinear = Config.ReadSetting("classicbilinear", false);
                VisualBilinear = Config.ReadSetting("visualbilinear", false);
                MouseSpeed = Config.ReadSetting("mousespeed", 100);
                MoveSpeed = Config.ReadSetting("movespeed", 100);
                ViewDistance = Config.ReadSetting("viewdistance", 3000.0f);
                InvertYAxis = Config.ReadSetting("invertyaxis", false);
                ScreenshotsPath = Config.ReadSetting("screenshotspath", General.DefaultScreenshotsPath); //mxd
                AutoScrollSpeed = Config.ReadSetting("autoscrollspeed", 0);
                ZoomFactor = Config.ReadSetting("zoomfactor", 3);
                ShowErrorsWindow = Config.ReadSetting("showerrorswindow", true);
                AnimateVisualSelection = Config.ReadSetting("animatevisualselection", true);
                PreviousVersion = Config.ReadSetting("currentversion", 0);
                DockersPosition = Config.ReadSetting("dockersposition", 1);
                CollapseDockers = Config.ReadSetting("collapsedockers", false);
                DockersWidth = Config.ReadSetting("dockerswidth", 300);
                PasteOptions.ReadConfiguration(Config, "pasteoptions");
                ToolbarScript = Config.ReadSetting("toolbarscript", true);
                ToolbarUndo = Config.ReadSetting("toolbarundo", true);
                ToolbarCopy = Config.ReadSetting("toolbarcopy", true);
                ToolbarPrefabs = Config.ReadSetting("toolbarprefabs", true);
                ToolbarFilter = Config.ReadSetting("toolbarfilter", true);
                ToolbarViewModes = Config.ReadSetting("toolbarviewmodes", true);
                ToolbarGeometry = Config.ReadSetting("toolbargeometry", true);
                ToolbarTesting = Config.ReadSetting("toolbartesting", true);
                ToolbarFile = Config.ReadSetting("toolbarfile", true);
                FilterAnisotropy = General.Clamp(Config.ReadSetting("filteranisotropy", 16.0f), 1.0f, 16.0f);
                AntiAliasingSamples = General.Clamp(Config.ReadSetting("antialiasingsamples", 4), 0, 8) / 2 * 2; //mxd
                ShowTextureSizes = Config.ReadSetting("showtexturesizes", true);
                TextureSizesBelow = Config.ReadSetting("texturesizesbelow", false); // [ZZ]
                LocateTextureGroup = Config.ReadSetting("locatetexturegroup", true); //mxd
                SplitLineBehavior = (SplitLineBehavior)General.Clamp(Config.ReadSetting("splitlinebehavior", 0), 0, Enum.GetValues(typeof(SplitLineBehavior)).Length - 1); //mxd
                MergeGeometryMode = (MergeGeometryMode)General.Clamp(Config.ReadSetting("mergegeometrymode", (int)MergeGeometryMode.REPLACE), 0, Enum.GetValues(typeof(MergeGeometryMode)).Length - 1); //mxd
                SplitJoinedSectors = Config.ReadSetting("splitjoinedsectors", true); //mxd
                usehighlight = Config.ReadSetting("usehighlight", true); //mxd
                SwitchViewModes = Config.ReadSetting("switchviewmodes", false); //mxd
                ShowFPS = Config.ReadSetting("showfps", false);
                AutoLaunchOnTest = Config.ReadSetting("autolaunchontest", false);
                ParallelizedLinedefPlotting = Config.ReadSetting("parallelizedlinedefplotting", true);
                ParallelizedVertexPlotting = Config.ReadSetting("parallelizedvertexplotting", false);

                //mxd. Script editor
                ScriptFontName = Config.ReadSetting("scriptfontname", "Courier New");
                ScriptFontSize = Config.ReadSetting("scriptfontsize", 10);
                ScriptFontBold = Config.ReadSetting("scriptfontbold", false);
                ScriptOnTop = Config.ReadSetting("scriptontop", true);
                ScriptAutoIndent = Config.ReadSetting("scriptautoindent", true);
                ScriptAllmanStyle = Config.ReadSetting("scriptallmanstyle", false);
                ScriptUseTabs = Config.ReadSetting("scriptusetabs", true);
                ScriptTabWidth = Config.ReadSetting("scripttabwidth", 4);
                ScriptAutoCloseBrackets = Config.ReadSetting("scriptautoclosebrackets", true);
                ScriptShowLineNumbers = Config.ReadSetting("scriptshowlinenumbers", true);
                ScriptShowFolding = Config.ReadSetting("scriptshowfolding", true);
                ScriptAutoShowAutocompletion = Config.ReadSetting("scriptautoshowautocompletion", true);

                //mxd. Text labels
                TextLabelFontName = Config.ReadSetting("textlabelfontname", "Microsoft Sans Serif");
                TextLabelFontSize = Config.ReadSetting("textlabelfontsize", 10);
                TextLabelFontBold = Config.ReadSetting("textlabelfontbold", false);

                //mxd 
                GZDrawModelsMode = (ModelRenderMode)Config.ReadSetting("gzdrawmodels", (int)ModelRenderMode.ALL);
                GZDrawLightsMode = (LightRenderMode)Config.ReadSetting("gzdrawlights", (int)LightRenderMode.ALL);
                GZDrawFog = Config.ReadSetting("gzdrawfog", false);
                GZDrawSky = Config.ReadSetting("gzdrawsky", true);
                GZToolbarGZDoom = Config.ReadSetting("gztoolbargzdoom", true);
                GZSynchCameras = Config.ReadSetting("gzsynchcameras", true);
                GZShowEventLines = Config.ReadSetting("gzshoweventlines", true);
                GZOldHighlightMode = Config.ReadSetting("gzoldhighlightmode", false);
                GZMaxDynamicLights = Config.ReadSetting("gzmaxdynamiclights", 16);
                GZStretchView = Config.ReadSetting("gzstretchview", true);
                GZVertexScale2D = Config.ReadSetting("gzvertexscale2d", 1.0f);
                GZShowVisualVertices = Config.ReadSetting("gzshowvisualvertices", true);
                GZVertexScale3D = Config.ReadSetting("gzvertexscale3d", 1.0f);
                LastUsedConfigName = Config.ReadSetting("lastusedconfigname", "");
                LastUsedMapFolder = Config.ReadSetting("lastusedmapfolder", "");
                GZMarkExtraFloors = Config.ReadSetting("gzmarkextrafloors", true);
                maxRecentFiles = Config.ReadSetting("maxrecentfiles", 8);
                AutoClearSidedefTextures = Config.ReadSetting("autoclearsidetextures", true);
                StoreSelectedEditTab = Config.ReadSetting("storeselectededittab", true);
                CheckForUpdates = Config.ReadSetting("checkforupdates", true); //mxd
                RenderComments = Config.ReadSetting("rendercomments", true); //mxd
                FixedThingsScale = Config.ReadSetting("fixedthingsscale", false); //mxd
                RenderGrid = Config.ReadSetting("rendergrid", true); //mxd
                DynamicGridSize = Config.ReadSetting("dynamicgridsize", true); //mxd
                IgnoredRemoteRevision = Config.ReadSetting("ignoredremoterevision", 0); //mxd

                // volte
                ClassicRendering = Config.ReadSetting("classicrendering", false);
                AlwaysShowVertices = Config.ReadSetting("alwaysshowvertices", true);
                FlatShadeVertices = Config.ReadSetting("flatshadevertices", false);

                //mxd. Sector defaults
                DefaultCeilingHeight = Config.ReadSetting("defaultceilheight", 128);
                DefaultFloorHeight = Config.ReadSetting("defaultfloorheight", 0);
                DefaultBrightness = Config.ReadSetting("defaultbrightness", 192);

                // Color dialog custom colors
                ColorDialogCustomColors = new int[16] { 16777215, 16777215, 16777215, 16777215, 16777215, 16777215, 16777215, 16777215, 16777215, 16777215, 16777215, 16777215, 16777215, 16777215, 16777215, 16777215 }; // White
                IDictionary colordict = Config.ReadSetting("colordialogcustomcolors", new Hashtable());
                foreach (DictionaryEntry de in colordict)
                {
                    string colornum = Regex.Match(de.Key.ToString(), @"^color(\d+)$").Groups[1].Value;
                    if (string.IsNullOrEmpty(colornum))
                        continue;

                    try
                    {
                        int colorid = Convert.ToInt32(colornum, CultureInfo.InvariantCulture);
                        int colorval = Convert.ToInt32(de.Value.ToString(), CultureInfo.InvariantCulture);
                        if (colorid >= 0 && colorid < 16)
                            ColorDialogCustomColors[colorid] = colorval;

                    }
                    catch (FormatException)
                    {
                        // Do nothing
                    }
                }

                // Autosave
                Autosave = Config.ReadSetting("autosave", true);
                AutosaveCount = Config.ReadSetting("autosavecount", 5);
                AutosaveInterval = Config.ReadSetting("autosaveinterval", 5);

                // Success
                return true;
            }
            // Failed
            return false;
        }

        // This saves the program configuration
        internal void Save(string filepathname)
        {
            Version v = General.ThisAssembly.GetName().Version;

            // Write the cache variables
            Config.WriteSetting("blackbrowsers", BlackBrowsers);
            Config.WriteSetting("visualfov", VisualFOV);
            Config.WriteSetting("visualmousesensx", VisualMouseSensX);
            Config.WriteSetting("visualmousesensy", VisualMouseSensY);
            Config.WriteSetting("imagebrightness", ImageBrightness);
            Config.WriteSetting("qualitydisplay", QualityDisplay);
            Config.WriteSetting("testmonsters", TestMonsters);
            Config.WriteSetting("doublesidedalpha", doublesidedalpha);
            Config.WriteSetting("activethingsalpha", ActiveThingsAlpha); //mxd
            Config.WriteSetting("inactivethingsalpha", InactiveThingsAlpha); //mxd
            Config.WriteSetting("hiddenthingsalpha", HiddenThingsAlpha); //mxd
            Config.WriteSetting("backgroundalpha", BackgroundAlpha);
            Config.WriteSetting("defaultviewmode", DefaultViewMode);
            Config.WriteSetting("classicbilinear", ClassicBilinear);
            Config.WriteSetting("visualbilinear", VisualBilinear);
            Config.WriteSetting("mousespeed", MouseSpeed);
            Config.WriteSetting("movespeed", MoveSpeed);
            Config.WriteSetting("viewdistance", ViewDistance);
            Config.WriteSetting("invertyaxis", InvertYAxis);
            Config.WriteSetting("screenshotspath", ScreenshotsPath); //mxd
            Config.WriteSetting("autoscrollspeed", AutoScrollSpeed);
            Config.WriteSetting("zoomfactor", ZoomFactor);
            Config.WriteSetting("showerrorswindow", ShowErrorsWindow);
            Config.WriteSetting("animatevisualselection", AnimateVisualSelection);
            Config.WriteSetting("currentversion", (v.Major * 1000000) + v.Revision);
            Config.WriteSetting("dockersposition", DockersPosition);
            Config.WriteSetting("collapsedockers", CollapseDockers);
            Config.WriteSetting("dockerswidth", DockersWidth);
            PasteOptions.WriteConfiguration(Config, "pasteoptions");
            Config.WriteSetting("toolbarscript", ToolbarScript);
            Config.WriteSetting("toolbarundo", ToolbarUndo);
            Config.WriteSetting("toolbarcopy", ToolbarCopy);
            Config.WriteSetting("toolbarprefabs", ToolbarPrefabs);
            Config.WriteSetting("toolbarfilter", ToolbarFilter);
            Config.WriteSetting("toolbarviewmodes", ToolbarViewModes);
            Config.WriteSetting("toolbargeometry", ToolbarGeometry);
            Config.WriteSetting("toolbartesting", ToolbarTesting);
            Config.WriteSetting("toolbarfile", ToolbarFile);
            Config.WriteSetting("filteranisotropy", FilterAnisotropy);
            Config.WriteSetting("antialiasingsamples", AntiAliasingSamples); //mxd
            Config.WriteSetting("showtexturesizes", ShowTextureSizes);
            Config.WriteSetting("texturesizesbelow", TextureSizesBelow); // [ZZ]
            Config.WriteSetting("locatetexturegroup", LocateTextureGroup); //mxd
            Config.WriteSetting("splitlinebehavior", (int)SplitLineBehavior); //mxd
            Config.WriteSetting("mergegeometrymode", (int)MergeGeometryMode); //mxd
            Config.WriteSetting("splitjoinedsectors", SplitJoinedSectors); //mxd
            Config.WriteSetting("usehighlight", usehighlight); //mxd
            Config.WriteSetting("switchviewmodes", SwitchViewModes); //mxd
            Config.WriteSetting("showfps", ShowFPS);
            Config.WriteSetting("autolaunchontest", AutoLaunchOnTest);

            //mxd. Script editor
            Config.WriteSetting("scriptfontname", ScriptFontName);
            Config.WriteSetting("scriptfontsize", ScriptFontSize);
            Config.WriteSetting("scriptfontbold", ScriptFontBold);
            Config.WriteSetting("scriptontop", ScriptOnTop);
            Config.WriteSetting("scriptusetabs", ScriptUseTabs);
            Config.WriteSetting("scripttabwidth", ScriptTabWidth);
            Config.WriteSetting("scriptautoindent", ScriptAutoIndent);
            Config.WriteSetting("scriptallmanstyle", ScriptAllmanStyle);
            Config.WriteSetting("scriptautoclosebrackets", ScriptAutoCloseBrackets);
            Config.WriteSetting("scriptshowlinenumbers", ScriptShowLineNumbers);
            Config.WriteSetting("scriptshowfolding", ScriptShowFolding);
            Config.WriteSetting("scriptautoshowautocompletion", ScriptAutoShowAutocompletion);

            //mxd. Text labels
            Config.WriteSetting("textlabelfontname", TextLabelFontName);
            Config.WriteSetting("textlabelfontsize", TextLabelFontSize);
            Config.WriteSetting("textlabelfontbold", TextLabelFontBold);

            //mxd
            Config.WriteSetting("gzdrawmodels", (int)GZDrawModelsMode);
            Config.WriteSetting("gzdrawlights", (int)GZDrawLightsMode);
            Config.WriteSetting("gzdrawfog", GZDrawFog);
            Config.WriteSetting("gzdrawsky", GZDrawSky);
            Config.WriteSetting("gzsynchcameras", GZSynchCameras);
            Config.WriteSetting("gzshoweventlines", GZShowEventLines);
            Config.WriteSetting("gzoldhighlightmode", GZOldHighlightMode);
            Config.WriteSetting("gztoolbargzdoom", GZToolbarGZDoom);
            Config.WriteSetting("gzmaxdynamiclights", GZMaxDynamicLights);
            Config.WriteSetting("gzstretchview", GZStretchView);
            Config.WriteSetting("gzvertexscale2d", GZVertexScale2D);
            Config.WriteSetting("gzshowvisualvertices", GZShowVisualVertices);
            Config.WriteSetting("gzvertexscale3d", GZVertexScale3D);
            Config.WriteSetting("gzmarkextrafloors", GZMarkExtraFloors);
            if (!string.IsNullOrEmpty(LastUsedConfigName))
                Config.WriteSetting("lastusedconfigname", LastUsedConfigName);
            if (!string.IsNullOrEmpty(LastUsedMapFolder))
                Config.WriteSetting("lastusedmapfolder", LastUsedMapFolder);
            Config.WriteSetting("maxrecentfiles", maxRecentFiles);
            Config.WriteSetting("autoclearsidetextures", AutoClearSidedefTextures);
            Config.WriteSetting("storeselectededittab", StoreSelectedEditTab);
            Config.WriteSetting("checkforupdates", CheckForUpdates); //mxd
            Config.WriteSetting("rendercomments", RenderComments); //mxd
            Config.WriteSetting("fixedthingsscale", FixedThingsScale); //mxd
            Config.WriteSetting("rendergrid", RenderGrid); //mxd
            Config.WriteSetting("dynamicgridsize", DynamicGridSize); //mxd
            Config.WriteSetting("ignoredremoterevision", IgnoredRemoteRevision); //mxd

            //volte
            Config.WriteSetting("classicrendering", ClassicRendering);
            Config.WriteSetting("alwaysshowvertices", AlwaysShowVertices);
            Config.WriteSetting("flatshadevertices", FlatShadeVertices);

            // Toasts
            General.ToastManager.WriteSettings(Config);

            //mxd. Sector defaults
            Config.WriteSetting("defaultceilheight", DefaultCeilingHeight);
            Config.WriteSetting("defaultfloorheight", DefaultFloorHeight);
            Config.WriteSetting("defaultbrightness", DefaultBrightness);

            // Color dialog custom colors
            for (int i = 0; i < 16; i++)
                Config.WriteSetting("colordialogcustomcolors.color" + i, ColorDialogCustomColors[i]);

            // Autosave
            Config.WriteSetting("autosave", Autosave);
            Config.WriteSetting("autosavecount", AutosaveCount);
            Config.WriteSetting("autosaveinterval", AutosaveInterval);

            // Save settings configuration
            General.WriteLogLine("Saving program configuration to \"" + filepathname + "\"...");
            Config.SaveConfiguration(filepathname);
        }

        // This reads the configuration
        private bool Read(string cfgfilepathname, string defaultfilepathname, string legacyfilepathname)
        {
            // Check if no config for this user exists yet
            if (!File.Exists(cfgfilepathname))
            {
                // Does an legacy configuration exist?
                if (File.Exists(legacyfilepathname))
                {
                    General.WriteLogLine("Local user program configuration is missing!");
                    File.Copy(legacyfilepathname, cfgfilepathname);
                    General.WriteLogLine("Copied legacy configuration \"" + legacyfilepathname + "\" for local user");
                }
                else
                {
                    // Copy new configuration
                    General.WriteLogLine("Local user program configuration is missing!");
                    File.Copy(defaultfilepathname, cfgfilepathname);
                    General.WriteLogLine("New program configuration copied for local user");
                }
            }

            // Load it
            Config = new Configuration(cfgfilepathname, true);
            if (Config.ErrorResult)
            {
                // Error in configuration
                // Ask user for a new copy
                DialogResult result = General.ShowErrorMessage("Error in program configuration near line " + Config.ErrorLine + ": " + Config.ErrorDescription + "\n\nWould you like to overwrite your settings with a new configuration to restore the default settings?", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    // Remove old configuration and make a new copy
                    General.WriteLogLine("User requested a new copy of the program configuration");
                    File.Delete(cfgfilepathname);
                    File.Copy(defaultfilepathname, cfgfilepathname);
                    General.WriteLogLine("New program configuration copied for local user");

                    // Load it
                    Config = new Configuration(cfgfilepathname, true);
                    if (Config.ErrorResult)
                    {
                        // Error in configuration
                        General.WriteLogLine("Error in program configuration near line " + Config.ErrorLine + ": " + Config.ErrorDescription);
                        General.ShowErrorMessage("Default program configuration is corrupted. Please re-install Doom Builder.", MessageBoxButtons.OK);
                        return false;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    // User requested to cancel startup
                    General.WriteLogLine("User cancelled startup");
                    return false;
                }
            }

            // Check if a version number is missing
            PreviousVersion = Config.ReadSetting("currentversion", -1);
            if (!General.NoSettings && (PreviousVersion == -1))
            {
                // Remove old configuration and make a new copy
                General.WriteLogLine("Program configuration is outdated, new configuration will be copied for local user");
                File.Delete(cfgfilepathname);
                File.Copy(defaultfilepathname, cfgfilepathname);

                // Load it
                Config = new Configuration(cfgfilepathname, true);
                if (Config.ErrorResult)
                {
                    // Error in configuration
                    General.WriteLogLine("Error in program configuration near line " + Config.ErrorLine + ": " + Config.ErrorDescription);
                    General.ShowErrorMessage("Default program configuration is corrupted. Please re-install Doom Builder.", MessageBoxButtons.OK);
                    return false;
                }
            }

            // Success
            return true;
        }

        #endregion

        #region ================== Methods

        // This makes the path prefix for the given assembly
        private static string GetPluginPathPrefix(Assembly asm)
        {
            Plugin p = General.Plugins.FindPluginByAssembly(asm);
            return GetPluginPathPrefix(p.Name);
        }

        // This makes the path prefix for the given assembly
        private static string GetPluginPathPrefix(string assemblyname)
        {
            return "plugins." + assemblyname.ToLowerInvariant() + ".";
        }

        // ReadPluginSetting
        public string ReadPluginSetting(string setting, string defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public int ReadPluginSetting(string setting, int defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public float ReadPluginSetting(string setting, float defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public short ReadPluginSetting(string setting, short defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public long ReadPluginSetting(string setting, long defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public bool ReadPluginSetting(string setting, bool defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public byte ReadPluginSetting(string setting, byte defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public IDictionary ReadPluginSetting(string setting, IDictionary defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }

        // ReadPluginSetting with specific plugin
        public string ReadPluginSetting(string pluginname, string setting, string defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(pluginname) + setting, defaultsetting); }
        public int ReadPluginSetting(string pluginname, string setting, int defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(pluginname) + setting, defaultsetting); }
        public float ReadPluginSetting(string pluginname, string setting, float defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(pluginname) + setting, defaultsetting); }
        public short ReadPluginSetting(string pluginname, string setting, short defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(pluginname) + setting, defaultsetting); }
        public long ReadPluginSetting(string pluginname, string setting, long defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(pluginname) + setting, defaultsetting); }
        public bool ReadPluginSetting(string pluginname, string setting, bool defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(pluginname) + setting, defaultsetting); }
        public byte ReadPluginSetting(string pluginname, string setting, byte defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(pluginname) + setting, defaultsetting); }
        public IDictionary ReadPluginSetting(string pluginname, string setting, IDictionary defaultsetting) { return Config.ReadSetting(GetPluginPathPrefix(pluginname) + setting, defaultsetting); }

        // WritePluginSetting
        public bool WritePluginSetting(string setting, object settingvalue) { return Config.WriteSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, settingvalue); }

        // DeletePluginSetting
        public bool DeletePluginSetting(string setting) { return Config.DeleteSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting); }

        // ReadSetting
        internal string ReadSetting(string setting, string defaultsetting) { return Config.ReadSetting(setting, defaultsetting); }
        internal int ReadSetting(string setting, int defaultsetting) { return Config.ReadSetting(setting, defaultsetting); }
        internal float ReadSetting(string setting, float defaultsetting) { return Config.ReadSetting(setting, defaultsetting); }
        internal short ReadSetting(string setting, short defaultsetting) { return Config.ReadSetting(setting, defaultsetting); }
        internal long ReadSetting(string setting, long defaultsetting) { return Config.ReadSetting(setting, defaultsetting); }
        internal bool ReadSetting(string setting, bool defaultsetting) { return Config.ReadSetting(setting, defaultsetting); }
        internal byte ReadSetting(string setting, byte defaultsetting) { return Config.ReadSetting(setting, defaultsetting); }
        internal IDictionary ReadSetting(string setting, IDictionary defaultsetting) { return Config.ReadSetting(setting, defaultsetting); }

        // WriteSetting
        internal bool WriteSetting(string setting, object settingvalue) { return Config.WriteSetting(setting, settingvalue); }
        internal bool WriteSetting(string setting, object settingvalue, string pathseperator) { return Config.WriteSetting(setting, settingvalue, pathseperator); }

        // DeleteSetting
        internal bool DeleteSetting(string setting) { return Config.DeleteSetting(setting); }
        internal bool DeleteSetting(string setting, string pathseperator) { return Config.DeleteSetting(setting, pathseperator); }

        #endregion

        #region ================== Default Settings

        // This sets the default thing flags
        public void SetDefaultThingFlags(ICollection<string> setflags)
        {
            defaultthingflags = new List<string>(setflags);
        }

        /// <summary>
        /// This applies the settings of the last edited thing to the given thing.
        /// </summary>
        /// <param name="t">Thing to apply the settings to</param>
        public void ApplyDefaultThingSettings(Thing t)
        {
            t.Type = DefaultThingType;
            t.Rotate(DefaultThingAngle);
            foreach (string f in defaultthingflags) t.SetFlag(f, true);

            //mxd. Set default arguments
            ThingTypeInfo tti = General.Map.Data.GetThingInfoEx(t.Type);
            if (tti != null)
            {
                t.Args[0] = (int)tti.Args[0].DefaultValue;
                t.Args[1] = (int)tti.Args[1].DefaultValue;
                t.Args[2] = (int)tti.Args[2].DefaultValue;
                t.Args[3] = (int)tti.Args[3].DefaultValue;
                t.Args[4] = (int)tti.Args[4].DefaultValue;

                // Add user vars
                if (tti.Actor != null)
                {
                    Dictionary<string, UniversalType> uservars = tti.Actor.GetAllUserVars();
                    Dictionary<string, object> uservardefaults = tti.Actor.GetAllUserVarDefaults();

                    t.BeforeFieldsChange();

                    foreach (string fname in uservars.Keys)
                    {
                        if (uservardefaults.ContainsKey(fname))
                            t.Fields[fname] = new UniValue(uservars[fname], uservardefaults[fname]);
                    }
                }
            }
        }

        /// <summary>
        /// Applies clean thing settings to the given thing, with the flags and aruments set in the game's config.
        /// </summary>
        /// <param name="t">Thing to apply the settings to</param>
        /// <param name="type">Optional thing type. If not set the current thing type will be used, otherwise the type will be changed and used</param>
        public void ApplyCleanThingSettings(Thing t, int type = 0)
        {
            if (type > 0)
                t.Type = type;

            // Remove all current flags
            foreach (string flag in t.GetFlags().Keys)
                t.SetFlag(flag, false);

            // Add default flags
            foreach (string flag in General.Map.Config.DefaultThingFlags)
                t.SetFlag(flag, true);

            // Set default arguments
            ThingTypeInfo tti = General.Map.Data.GetThingInfoEx(t.Type);
            if (tti != null)
            {
                t.Args[0] = (int)tti.Args[0].DefaultValue;
                t.Args[1] = (int)tti.Args[1].DefaultValue;
                t.Args[2] = (int)tti.Args[2].DefaultValue;
                t.Args[3] = (int)tti.Args[3].DefaultValue;
                t.Args[4] = (int)tti.Args[4].DefaultValue;

                // Add user vars
                if (tti.Actor != null)
                {
                    Dictionary<string, UniversalType> uservars = tti.Actor.GetAllUserVars();
                    Dictionary<string, object> uservardefaults = tti.Actor.GetAllUserVarDefaults();

                    t.BeforeFieldsChange();

                    foreach (string fname in uservars.Keys)
                    {
                        if (uservardefaults.ContainsKey(fname))
                            t.Fields[fname] = new UniValue(uservars[fname], uservardefaults[fname]);
                    }
                }
            }
        }

        // This attempts to find the default drawing settings
        public void FindDefaultDrawSettings()
        {
            bool foundone;

            // Only possible when a map is loaded
            if (General.Map == null || General.Map.Options == null) return;

            // Default texture missing?
            if (!General.Map.Options.OverrideMiddleTexture || string.IsNullOrEmpty(General.Map.Options.DefaultWallTexture)) //mxd
            {
                // Find default texture from map
                foundone = false;
                foreach (Sidedef sd in General.Map.Map.Sidedefs)
                {
                    if (sd.MiddleTexture != "-" && General.Map.Data.GetTextureExists(sd.MiddleTexture))
                    {
                        foundone = true;
                        General.Map.Options.DefaultWallTexture = sd.MiddleTexture;
                        break;
                    }
                }

                // Not found yet?
                if (!foundone)
                {
                    //mxd. Use the wall texture from the game configuration?
                    if (!string.IsNullOrEmpty(General.Map.Config.DefaultWallTexture) && General.Map.Data.GetTextureExists(General.Map.Config.DefaultWallTexture))
                    {
                        General.Map.Options.DefaultWallTexture = General.Map.Config.DefaultWallTexture;
                        foundone = true;
                    }

                    // Pick the first STARTAN from the list.
                    // I love the STARTAN texture as default for some reason.
                    if (!foundone)
                    {
                        foreach (string s in General.Map.Data.TextureNames)
                        {
                            if (s.StartsWith("STARTAN"))
                            {
                                foundone = true;
                                General.Map.Options.DefaultWallTexture = s;
                                break;
                            }
                        }
                    }

                    // Otherwise just pick the first
                    if (!foundone)
                    {
                        if (General.Map.Data.TextureNames.Count > 1)
                            General.Map.Options.DefaultWallTexture = General.Map.Data.TextureNames[1];
                    }
                }
            }

            // Default floor missing?
            if (!General.Map.Options.OverrideFloorTexture || string.IsNullOrEmpty(General.Map.Options.DefaultFloorTexture))
            {
                // Find default texture from map
                foundone = false;
                if (General.Map.Map.Sectors.Count > 0)
                {
                    // Find one that is known
                    foreach (Sector s in General.Map.Map.Sectors)
                    {
                        if (General.Map.Data.GetFlatExists(s.FloorTexture))
                        {
                            foundone = true;
                            General.Map.Options.DefaultFloorTexture = s.FloorTexture;
                            break;
                        }
                    }
                }

                //mxd. Use the floor flat from the game configuration?
                if (!foundone && !string.IsNullOrEmpty(General.Map.Config.DefaultFloorTexture) && General.Map.Data.GetFlatExists(General.Map.Config.DefaultFloorTexture))
                {
                    General.Map.Options.DefaultFloorTexture = General.Map.Config.DefaultFloorTexture;
                    foundone = true;
                }

                // Pick the first FLOOR from the list.
                if (!foundone)
                {
                    foreach (string s in General.Map.Data.FlatNames)
                    {
                        if (s.StartsWith("FLOOR"))
                        {
                            foundone = true;
                            General.Map.Options.DefaultFloorTexture = s;
                            break;
                        }
                    }
                }

                // Otherwise just pick the first
                if (!foundone)
                {
                    if (General.Map.Data.FlatNames.Count > 1)
                        General.Map.Options.DefaultFloorTexture = General.Map.Data.FlatNames[1];
                }
            }

            // Default ceiling missing?
            if (!General.Map.Options.OverrideCeilingTexture || string.IsNullOrEmpty(General.Map.Options.DefaultCeilingTexture))
            {
                // Find default texture from map
                foundone = false;
                if (General.Map.Map.Sectors.Count > 0)
                {
                    // Find one that is known
                    foreach (Sector s in General.Map.Map.Sectors)
                    {
                        if (General.Map.Data.GetFlatExists(s.CeilTexture))
                        {
                            foundone = true;
                            General.Map.Options.DefaultCeilingTexture = s.CeilTexture;
                            break;
                        }
                    }
                }

                //mxd. Use the floor flat from the game configuration?
                if (!foundone && !string.IsNullOrEmpty(General.Map.Config.DefaultCeilingTexture) && General.Map.Data.GetFlatExists(General.Map.Config.DefaultCeilingTexture))
                {
                    General.Map.Options.DefaultCeilingTexture = General.Map.Config.DefaultCeilingTexture;
                    foundone = true;
                }

                // Pick the first CEIL from the list.
                if (!foundone)
                {
                    foreach (string s in General.Map.Data.FlatNames)
                    {
                        if (s.StartsWith("CEIL"))
                        {
                            foundone = true;
                            General.Map.Options.DefaultCeilingTexture = s;
                            break;
                        }
                    }
                }

                // Otherwise just pick the first
                if (!foundone)
                {
                    if (General.Map.Data.FlatNames.Count > 1)
                        General.Map.Options.DefaultCeilingTexture = General.Map.Data.FlatNames[1];
                }
            }

            // Texture names may not be null
            if (string.IsNullOrEmpty(General.Map.Options.DefaultWallTexture)) General.Map.Options.DefaultWallTexture = "-";
            if (string.IsNullOrEmpty(General.Map.Options.DefaultTopTexture) || !General.Map.Options.OverrideTopTexture) General.Map.Options.DefaultTopTexture = General.Map.Options.DefaultWallTexture; //mxd
            if (string.IsNullOrEmpty(General.Map.Options.DefaultBottomTexture) || !General.Map.Options.OverrideBottomTexture) General.Map.Options.DefaultBottomTexture = General.Map.Options.DefaultWallTexture; //mxd
            if (string.IsNullOrEmpty(General.Map.Options.DefaultFloorTexture)) General.Map.Options.DefaultFloorTexture = "-";
            if (string.IsNullOrEmpty(General.Map.Options.DefaultCeilingTexture)) General.Map.Options.DefaultCeilingTexture = "-";
        }

        #endregion
    }
}
