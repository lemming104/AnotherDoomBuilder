
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

using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Dehacked;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

#endregion

namespace CodeImp.DoomBuilder.Config
{
    public struct CompatibilityOptions
    {
        public bool FixNegativePatchOffsets;
        public bool FixMaskedPatchOffsets;

        public CompatibilityOptions(Configuration cfg)
        {
            FixNegativePatchOffsets = cfg.ReadSetting("compatibility.fixnegativepatchoffsets", false);
            FixMaskedPatchOffsets = cfg.ReadSetting("compatibility.fixmaskedpatchoffsets", false);
        }
    }

    public enum SkewStyle
    {
        None,
        GZDoom,
        EternityEngine
    }

    public class GameConfiguration
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // Original configuration
        private readonly Configuration cfg;

        // General settings
        private readonly bool testlinuxpaths;

        // Skills

        // Map lumps

        //mxd. Map format

        // Static limits for the base game and map format.

        // Visplane Explorer plugin settings.

        // Texture/flat/voxel sources

        // Things
        private readonly List<string> defaultthingflags;
        private readonly Dictionary<string, string> thingflags;
        private readonly List<ThingCategory> thingcategories;
        private readonly Dictionary<int, ThingTypeInfo> things;

        // Linedefs
        private readonly Dictionary<string, string> linedefflags;
        private readonly Dictionary<int, LinedefActionInfo> linedefactions;

        //mxd. Sidedefs
        private readonly Dictionary<string, string> sidedefflags; //mxd

        // Sectors
        private readonly Dictionary<string, string> sectorflags; //mxd
        private readonly Dictionary<string, string> ceilportalflags; //mxd
        private readonly Dictionary<string, string> floorportalflags; //mxd
        private readonly Dictionary<int, SectorEffectInfo> sectoreffects;

        // Universal fields

        // Enums
        private readonly Dictionary<string, EnumList> enums;

        //mxd. DamageTypes
        private HashSet<string> damagetypes;

        //mxd. Internal sounds. These logical sound names won't trigger a warning when they are not bound to actual sounds in SOUNDINFO.

        //mxd. Stuff to ignore

        // [ZZ] This implements error message if GZDoom.pk3 is required but not loaded

        // Defaults

        //mxd. Holds base game type (doom, heretic, hexen or strife)

        // [ZZ] compat

        // Compatibility options
        CompatibilityOptions compatibility;

        // Dehacked

        // Skew style

        // File title style (how long file names are converted to lump names)
        private FileTitleStyle filetitlestyle;

        #endregion

        #region ================== Properties

        // General settings
        public string Name { get; }
        public string EngineName { get; }
        public string DefaultSaveCompiler { get; }
        public string DefaultTestCompiler { get; }
        public float DefaultTextureScale { get; }
        public float DefaultFlatScale { get; }
        public string DefaultWallTexture { get; } //mxd
        public string DefaultFloorTexture { get; } //mxd
        public string DefaultCeilingTexture { get; } //mxd
        public bool ScaledTextureOffsets { get; }
        public string FormatInterface { get; }
        public string DefaultLinedefActivationFlag { get; } //mxd
        public string SingleSidedFlag { get; }
        public string DoubleSidedFlag { get; }
        public string ImpassableFlag { get; }
        public string UpperUnpeggedFlag { get; }
        public string LowerUnpeggedFlag { get; }
        public bool MixTexturesFlats { get; }
        public bool GeneralizedActions { get; }
        public bool GeneralizedEffects { get; }
        public int Start3DModeThingType { get; }
        public int LinedefActivationsFilter { get; }
        public string TestParameters { get; }
        public bool TestShortPaths { get; }
        public bool TestLinuxPaths { get; internal set; }
        public string MakeDoorTrack { get; }
        public string MakeDoorDoor { get; } //mxd
        public string MakeDoorCeiling { get; } //mxd
        public int MakeDoorAction { get; }
        public int MakeDoorActivate { get; }
        public Dictionary<string, bool> MakeDoorFlags { get; }
        public int[] MakeDoorArgs { get; }
        public bool LineTagIndicatesSectors { get; }
        public string DecorateGames { get; }
        public string SkyFlatName { get; internal set; } //mxd. Added setter
        public Dictionary<string, string> DefaultSkyTextures { get; } //mxd
        public int MaxTextureNameLength { get; }
        public bool UseLongTextureNames { get; } //mxd
        public int LeftBoundary { get; }
        public int RightBoundary { get; }
        public int TopBoundary { get; }
        public int BottomBoundary { get; }
        public int SafeBoundary { get; } //mxd
        public bool DoomLightLevels { get; }
        public bool DoomThingRotationAngles { get; } //mxd. When set to true, thing rotation angles will be clamped to the nearest 45 deg increment
        public string ActionSpecialHelp { get; } //mxd
        public string ThingClassHelp { get; } //mxd
        internal bool SidedefCompressionIgnoresAction { get; } //mxd

        // Skills
        public List<SkillInfo> Skills { get; }

        // Map lumps
        public Dictionary<string, MapLumpInfo> MapLumps { get; }

        //mxd. Map format
        public bool UDMF { get; }
        public bool HEXEN { get; }
        public bool DOOM { get; }

        // Static limits for the base game and map format.
        public StaticLimits StaticLimits { get; }

        public int VisplaneViewHeightDefault { get; }
        public Dictionary<string, string> VisplaneViewHeights { get; }

        public bool UseLocalSidedefTextureOffsets { get; } //MaxW
        public bool Effect3DFloorSupport { get; }
        public bool PlaneEquationSupport { get; }
        public bool VertexHeightSupport { get; }
        public bool DistinctFloorAndCeilingBrightness { get; }
        public bool DistinctWallBrightness { get; }
        public bool DistinctSidedefPartBrightness { get; }
        public bool SectorMultiTag { get; }
        public bool SidedefTextureSkewing { get; }

        // Texture/flat/voxel sources
        public IDictionary TextureRanges { get; }
        public IDictionary HiResRanges { get; } //mxd
        public IDictionary FlatRanges { get; }
        public IDictionary PatchRanges { get; }
        public IDictionary SpriteRanges { get; }
        public IDictionary ColormapRanges { get; }
        public IDictionary VoxelRanges { get; } //mxd

        // Things
        public ICollection<string> DefaultThingFlags { get { return defaultthingflags; } }
        public IDictionary<string, string> ThingFlags { get { return thingflags; } }
        public List<FlagTranslation> ThingFlagsTranslation { get; }
        public Dictionary<string, ThingFlagsCompareGroup> ThingFlagsCompare { get; } //mxd
        public Dictionary<string, string> ThingRenderStyles { get; } //mxd
        public IReadOnlyDictionary<int, ThingTypeInfo> ThingTypes { get { return things; } }

        // Linedefs
        public IDictionary<string, string> LinedefFlags { get { return linedefflags; } }
        public List<string> SortedLinedefFlags { get; }
        public IDictionary<int, LinedefActionInfo> LinedefActions { get { return linedefactions; } }
        public List<LinedefActionInfo> SortedLinedefActions { get; }
        public List<LinedefActionCategory> ActionCategories { get; }
        public List<LinedefActivateInfo> LinedefActivates { get; }
        public List<GeneralizedCategory> GenActionCategories { get; }
        public List<FlagTranslation> LinedefFlagsTranslation { get; }
        public Dictionary<string, string> LinedefRenderStyles { get; } //mxd

        //mxd. Sidedefs
        public IDictionary<string, string> SidedefFlags { get { return sidedefflags; } }

        // Sectors
        public IDictionary<string, string> SectorFlags { get { return sectorflags; } } //mxd
        public IDictionary<string, string> CeilingPortalFlags { get { return ceilportalflags; } } //mxd
        public IDictionary<string, string> FloorPortalFlags { get { return floorportalflags; } } //mxd
        public IDictionary<int, SectorEffectInfo> SectorEffects { get { return sectoreffects; } }
        public List<SectorEffectInfo> SortedSectorEffects { get; }
        public List<GeneralizedOption> GenEffectOptions { get; }
        public StepsList BrightnessLevels { get; }
        public Dictionary<string, string> SectorRenderStyles { get; } //mxd
        public Dictionary<string, string> SectorPortalRenderStyles { get; } //mxd

        // Universal fields
        public List<UniversalFieldInfo> LinedefFields { get; }
        public List<UniversalFieldInfo> SectorFields { get; }
        public List<UniversalFieldInfo> SidedefFields { get; }
        public List<UniversalFieldInfo> ThingFields { get; }
        public List<UniversalFieldInfo> VertexFields { get; }

        // Enums
        public IDictionary<string, EnumList> Enums { get { return enums; } }

        //mxd. DamageTypes
        internal IEnumerable<string> DamageTypes { get { return damagetypes; } }

        //mxd. Internal sounds
        internal HashSet<string> InternalSoundNames { get; }

        //mxd. Stuff to ignore
        internal HashSet<string> IgnoredFileExtensions { get; }
        internal HashSet<string> IgnoredDirectoryNames { get; }

        // [ZZ] This implements error message if GZDoom.pk3 is required but not loaded
        internal List<RequiredArchive> RequiredArchives { get; }

        // Defaults
        internal List<DefinedTextureSet> TextureSets { get; }
        public List<ThingsFilter> ThingsFilters { get; }

        //mxd
        public string BaseGame { get; }

        // [ZZ] compat
        public bool BuggyModelDefPitch { get; } // reverses +USEACTORPITCH (as in before GZDoom 2.4, but after +INHERITACTORPITCH)

        // Compatibility options
        public CompatibilityOptions Compatibility { get { return compatibility; } }

        // Dehacked
        public DehackedData DehackedData { get; }

        // Skew style
        public SkewStyle SkewStyle { get; }

        // File title style
        public FileTitleStyle FileTitleStyle { get { return filetitlestyle; } }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal GameConfiguration(Configuration cfg)
        {
            // Initialize
            this.cfg = cfg;
            this.thingflags = new Dictionary<string, string>(StringComparer.Ordinal);
            this.defaultthingflags = new List<string>();
            this.thingcategories = new List<ThingCategory>();
            this.things = new Dictionary<int, ThingTypeInfo>();
            this.linedefflags = new Dictionary<string, string>(StringComparer.Ordinal);
            this.SortedLinedefFlags = new List<string>();
            this.linedefactions = new Dictionary<int, LinedefActionInfo>();
            this.ActionCategories = new List<LinedefActionCategory>();
            this.SortedLinedefActions = new List<LinedefActionInfo>();
            this.LinedefActivates = new List<LinedefActivateInfo>();
            this.sidedefflags = new Dictionary<string, string>(StringComparer.Ordinal); //mxd
            this.GenActionCategories = new List<GeneralizedCategory>();
            this.sectorflags = new Dictionary<string, string>(StringComparer.Ordinal); //mxd
            this.ceilportalflags = new Dictionary<string, string>(StringComparer.Ordinal); //mxd
            this.floorportalflags = new Dictionary<string, string>(StringComparer.Ordinal); //mxd
            this.sectoreffects = new Dictionary<int, SectorEffectInfo>();
            this.SortedSectorEffects = new List<SectorEffectInfo>();
            this.GenEffectOptions = new List<GeneralizedOption>();
            this.enums = new Dictionary<string, EnumList>(StringComparer.Ordinal);
            this.Skills = new List<SkillInfo>();
            this.TextureSets = new List<DefinedTextureSet>();
            this.MakeDoorArgs = new int[Linedef.NUM_ARGS];
            this.MapLumps = new Dictionary<string, MapLumpInfo>(StringComparer.Ordinal);
            this.ThingFlagsTranslation = new List<FlagTranslation>();
            this.LinedefFlagsTranslation = new List<FlagTranslation>();
            this.ThingsFilters = new List<ThingsFilter>();
            this.ThingFlagsCompare = new Dictionary<string, ThingFlagsCompareGroup>(); //mxd
            this.BrightnessLevels = new StepsList();
            this.MakeDoorFlags = new Dictionary<string, bool>(StringComparer.Ordinal);
            this.LinedefRenderStyles = new Dictionary<string, string>(StringComparer.Ordinal); //mxd
            this.SectorRenderStyles = new Dictionary<string, string>(StringComparer.Ordinal); //mxd
            this.SectorPortalRenderStyles = new Dictionary<string, string>(StringComparer.Ordinal); //mxd
            this.ThingRenderStyles = new Dictionary<string, string>(StringComparer.Ordinal); //mxd
            this.DefaultSkyTextures = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); //mxd

            // Read general settings
            Name = cfg.ReadSetting("game", "<unnamed game>");

            //mxd
            BaseGame = cfg.ReadSetting("basegame", string.Empty).ToLowerInvariant();
            if (!GameType.GameTypes.Contains(BaseGame))
            {
                if (!string.IsNullOrEmpty(BaseGame))
                    General.ErrorLogger.Add(ErrorType.Error, "Unknown basegame value specified in current Game Configuration: \"" + BaseGame + "\"");
                BaseGame = GameType.UNKNOWN;
            }

            EngineName = cfg.ReadSetting("engine", "");
            DefaultSaveCompiler = cfg.ReadSetting("defaultsavecompiler", "");
            DefaultTestCompiler = cfg.ReadSetting("defaulttestcompiler", "");
            DefaultTextureScale = cfg.ReadSetting("defaulttexturescale", 1f);
            DefaultFlatScale = cfg.ReadSetting("defaultflatscale", 1f);
            DefaultWallTexture = cfg.ReadSetting("defaultwalltexture", "STARTAN"); //mxd
            DefaultFloorTexture = cfg.ReadSetting("defaultfloortexture", "FLOOR0_1"); //mxd
            DefaultCeilingTexture = cfg.ReadSetting("defaultceilingtexture", "CEIL1_1"); //mxd
            ScaledTextureOffsets = cfg.ReadSetting("scaledtextureoffsets", true);
            FormatInterface = cfg.ReadSetting("formatinterface", "");
            MixTexturesFlats = cfg.ReadSetting("mixtexturesflats", false);
            GeneralizedActions = cfg.ReadSetting("generalizedlinedefs", false);
            GeneralizedEffects = cfg.ReadSetting("generalizedsectors", false);
            Start3DModeThingType = cfg.ReadSetting("start3dmode", 0);
            LinedefActivationsFilter = cfg.ReadSetting("linedefactivationsfilter", 0);
            TestParameters = cfg.ReadSetting("testparameters", "");
            TestShortPaths = cfg.ReadSetting("testshortpaths", false);
            testlinuxpaths = cfg.ReadSetting("testlinuxpaths", false);
            MakeDoorTrack = cfg.ReadSetting("makedoortrack", "-");
            MakeDoorDoor = cfg.ReadSetting("makedoordoor", "-"); //mxd
            MakeDoorCeiling = cfg.ReadSetting("makedoorceil", "-"); //mxd
            MakeDoorAction = cfg.ReadSetting("makedooraction", 0);
            MakeDoorActivate = cfg.ReadSetting("makedooractivate", 0);
            LineTagIndicatesSectors = cfg.ReadSetting("linetagindicatesectors", false);
            DecorateGames = cfg.ReadSetting("decorategames", "");
            SkyFlatName = cfg.ReadSetting("skyflatname", "F_SKY1");
            LeftBoundary = cfg.ReadSetting("leftboundary", -32768);
            RightBoundary = cfg.ReadSetting("rightboundary", 32767);
            TopBoundary = cfg.ReadSetting("topboundary", 32767);
            BottomBoundary = cfg.ReadSetting("bottomboundary", -32768);
            SafeBoundary = cfg.ReadSetting("safeboundary", 32767); //mxd
            DoomLightLevels = cfg.ReadSetting("doomlightlevels", true);
            DoomThingRotationAngles = cfg.ReadSetting("doomthingrotationangles", false); //mxd
            ActionSpecialHelp = cfg.ReadSetting("actionspecialhelp", string.Empty); //mxd
            ThingClassHelp = cfg.ReadSetting("thingclasshelp", string.Empty); //mxd
            SidedefCompressionIgnoresAction = cfg.ReadSetting("sidedefcompressionignoresaction", false); //mxd
            DefaultLinedefActivationFlag = cfg.ReadSetting("defaultlinedefactivation", ""); //mxd
            UseLocalSidedefTextureOffsets = cfg.ReadSetting("localsidedeftextureoffsets", false); //MaxW
            Effect3DFloorSupport = cfg.ReadSetting("effect3dfloorsupport", false);
            PlaneEquationSupport = cfg.ReadSetting("planeequationsupport", false);
            VertexHeightSupport = cfg.ReadSetting("vertexheightsupport", false);
            SidedefTextureSkewing = cfg.ReadSetting("sidedeftextureskewing", false);
            DistinctFloorAndCeilingBrightness = cfg.ReadSetting("distinctfloorandceilingbrightness", false);
            DistinctWallBrightness = cfg.ReadSetting("distinctwallbrightness", false);
            DistinctSidedefPartBrightness = cfg.ReadSetting("distinctsidedefpartbrightness", false);
            SectorMultiTag = cfg.ReadSetting("sectormultitag", false);
            for (int i = 0; i < Linedef.NUM_ARGS; i++) MakeDoorArgs[i] = cfg.ReadSetting("makedoorarg" + i.ToString(CultureInfo.InvariantCulture), 0);

            //mxd. Update map format flags
            UDMF = FormatInterface == "UniversalMapSetIO";
            HEXEN = FormatInterface == "HexenMapSetIO";
            DOOM = FormatInterface == "DoomMapSetIO";

            // Read static limits for the base game and map format.
            StaticLimits = new StaticLimits(cfg);

            // Read the Visplane Explorer plugin's default selectable view heights.
            VisplaneViewHeightDefault = cfg.ReadSetting("visplaneexplorer.viewheightdefault", 41);
            VisplaneViewHeights = new Dictionary<string, string>(StringComparer.Ordinal);
            LoadStringDictionary(VisplaneViewHeights, "visplaneexplorer.viewheights");

            //mxd. Texture names length
            UseLongTextureNames = cfg.ReadSetting("longtexturenames", false);
            MaxTextureNameLength = UseLongTextureNames ? short.MaxValue : DataManager.CLASIC_IMAGE_NAME_LENGTH;

            // [ZZ] compat
            BuggyModelDefPitch = cfg.ReadSetting("buggymodeldefpitch", false);

            // Flags have special (invariant culture) conversion
            // because they are allowed to be written as integers in the configs
            object obj = cfg.ReadSettingObject("singlesidedflag", 0);
            if (obj is int) SingleSidedFlag = ((int)obj).ToString(CultureInfo.InvariantCulture); else SingleSidedFlag = obj.ToString();
            obj = cfg.ReadSettingObject("doublesidedflag", 0);
            if (obj is int) DoubleSidedFlag = ((int)obj).ToString(CultureInfo.InvariantCulture); else DoubleSidedFlag = obj.ToString();
            obj = cfg.ReadSettingObject("impassableflag", 0);
            if (obj is int) ImpassableFlag = ((int)obj).ToString(CultureInfo.InvariantCulture); else ImpassableFlag = obj.ToString();
            obj = cfg.ReadSettingObject("upperunpeggedflag", 0);
            if (obj is int) UpperUnpeggedFlag = ((int)obj).ToString(CultureInfo.InvariantCulture); else UpperUnpeggedFlag = obj.ToString();
            obj = cfg.ReadSettingObject("lowerunpeggedflag", 0);
            if (obj is int) LowerUnpeggedFlag = ((int)obj).ToString(CultureInfo.InvariantCulture); else LowerUnpeggedFlag = obj.ToString();

            // Get texture and flat sources
            TextureRanges = cfg.ReadSetting("textures", new Hashtable());
            HiResRanges = cfg.ReadSetting("hires", new Hashtable()); //mxd
            FlatRanges = cfg.ReadSetting("flats", new Hashtable());
            PatchRanges = cfg.ReadSetting("patches", new Hashtable());
            SpriteRanges = cfg.ReadSetting("sprites", new Hashtable());
            ColormapRanges = cfg.ReadSetting("colormaps", new Hashtable());
            VoxelRanges = cfg.ReadSetting("voxels", new Hashtable()); //mxd

            // Map lumps
            LoadMapLumps();

            // Skills
            LoadSkills();

            // Enums
            LoadEnums();

            //mxd. Load damage types and internal sound names
            char[] splitter = { ' ' };
            damagetypes = new HashSet<string>(cfg.ReadSetting("damagetypes", "None").Split(splitter, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
            InternalSoundNames = new HashSet<string>(cfg.ReadSetting("internalsoundnames", string.Empty).Split(splitter, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);

            //mxd. Load stuff to ignore
            IgnoredDirectoryNames = new HashSet<string>(cfg.ReadSetting("ignoreddirectories", string.Empty).Split(splitter, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
            IgnoredFileExtensions = new HashSet<string>(cfg.ReadSetting("ignoredextensions", string.Empty).Split(splitter, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);

            // [ZZ]
            IDictionary requiredArchives = cfg.ReadSetting("requiredarchives", new Hashtable());
            RequiredArchives = new List<RequiredArchive>();
            foreach (DictionaryEntry cde in requiredArchives)
            {
                string filename = cfg.ReadSetting("requiredarchives." + cde.Key + ".filename", "gzdoom.pk3");
                bool exclude = cfg.ReadSetting("requiredarchives." + cde.Key + ".need_exclude", true);
                IDictionary entries = cfg.ReadSetting("requiredarchives." + cde.Key, new Hashtable());
                List<RequiredArchiveEntry> reqEntries = new List<RequiredArchiveEntry>();
                foreach (DictionaryEntry cde2 in entries)
                {
                    if ((string)cde2.Key == "filename") continue;
                    string lumpname = cfg.ReadSetting("requiredarchives." + cde.Key + "." + cde2.Key + ".lump", (string)null);
                    string classname = cfg.ReadSetting("requiredarchives." + cde.Key + "." + cde2.Key + ".class", (string)null);
                    reqEntries.Add(new RequiredArchiveEntry(classname, lumpname));
                }
                RequiredArchives.Add(new RequiredArchive((string)cde.Key, filename, exclude, reqEntries));
            }

            // Things
            LoadThingFlags();
            LoadDefaultThingFlags();
            LoadThingCategories();
            LoadStringDictionary(ThingRenderStyles, "thingrenderstyles"); //mxd

            // Linedefs
            LoadLinedefFlags();
            LoadLinedefActions();
            LoadLinedefActivations();
            LoadLinedefGeneralizedActions();
            LoadStringDictionary(LinedefRenderStyles, "linedefrenderstyles"); //mxd

            //mxd. Sidedefs
            LoadStringDictionary(sidedefflags, "sidedefflags");

            // Sectors
            LoadStringDictionary(sectorflags, "sectorflags"); //mxd
            LoadStringDictionary(ceilportalflags, "ceilingportalflags"); //mxd
            LoadStringDictionary(floorportalflags, "floorportalflags"); //mxd
            LoadBrightnessLevels();
            LoadSectorEffects();
            LoadSectorGeneralizedEffects();
            LoadStringDictionary(SectorRenderStyles, "sectorrenderstyles"); //mxd
            LoadStringDictionary(SectorPortalRenderStyles, "sectorportalrenderstyles"); //mxd

            // Universal fields
            LinedefFields = LoadUniversalFields("linedef");
            SectorFields = LoadUniversalFields("sector");
            SidedefFields = LoadUniversalFields("sidedef");
            ThingFields = LoadUniversalFields("thing");
            VertexFields = LoadUniversalFields("vertex");

            // Defaults
            LoadTextureSets();
            LoadThingFilters();

            //mxd. Vanilla sky textures
            LoadDefaultSkies();

            // Make door flags
            LoadMakeDoorFlags();

            // Compatibility options
            compatibility = new CompatibilityOptions(cfg);

            // Dehacked
            DehackedData = new DehackedData(cfg, "dehacked");

            // Determine skew style
            SkewStyle = SkewStyle.None;
            if (SidedefTextureSkewing)
            {
                if (SidedefFields.Any(lf => lf.Name == "skew_top" || lf.Name == "skew_middle" || lf.Name == "skew_bottom"))
                    SkewStyle = SkewStyle.GZDoom;
                else if (SidedefFields.Any(lf => lf.Name == "skew_top_type" || lf.Name == "skew_middle_type" || lf.Name == "skew_bottom_type"))
                    SkewStyle = SkewStyle.EternityEngine;
            }

            // Determine file title style
            string filetitlestylestring = cfg.ReadSetting("filetitlestyle", "default");
            if (!Enum.TryParse(filetitlestylestring, true, out filetitlestyle))
            {
                General.ErrorLogger.Add(ErrorType.Error, "Unknown file title style \"" + filetitlestylestring + "\" specified in current Game Configuration. Falling back to \"default\"");
                filetitlestyle = FileTitleStyle.DEFAULT;
            }
        }

        // Destructor
        ~GameConfiguration()
        {
            foreach (ThingCategory tc in thingcategories) tc.Dispose();
            foreach (LinedefActionCategory ac in ActionCategories) ac.Dispose();
            foreach (ThingsFilter tf in ThingsFilters) tf.Dispose(); //mxd
            foreach (GeneralizedCategory gc in GenActionCategories) gc.Dispose(); //mxd
        }

        #endregion

        #region ================== Loading

        // This loads the map lumps
        private void LoadMapLumps()
        {
            // Get map lumps list
            IDictionary dic = cfg.ReadSetting("maplumpnames", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                // Make map lumps
                MapLumpInfo lumpinfo = new MapLumpInfo(de.Key.ToString(), cfg);
                MapLumps.Add(de.Key.ToString(), lumpinfo);
            }
        }

        // This loads the enumerations
        private void LoadEnums()
        {
            // Get enums list
            IDictionary dic = cfg.ReadSetting("enums", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                // Make new enum
                EnumList list = new EnumList(de.Key.ToString(), cfg);
                enums.Add(de.Key.ToString(), list);
            }
        }

        // This loads a universal fields list
        private List<UniversalFieldInfo> LoadUniversalFields(string elementname)
        {
            List<UniversalFieldInfo> list = new List<UniversalFieldInfo>();

            // Get fields
            IDictionary dic = cfg.ReadSetting("universalfields." + elementname, new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
#if !DEBUG
				try
				{
#endif
                // Read the field info and add to list
                UniversalFieldInfo uf = new UniversalFieldInfo(elementname, de.Key.ToString(), this.Name, cfg, enums);
                list.Add(uf);
#if !DEBUG
				}
				catch(Exception)
				{
					General.ErrorLogger.Add(ErrorType.Warning, "Unable to read universal field definition \"universalfields." + elementname + "." + de.Key + "\" from game configuration \"" + this.Name + "\"");
				}
#endif
            }

            // Return result
            return list;
        }

        // Things and thing categories
        private void LoadThingCategories()
        {
            // Get thing categories
            IDictionary dic = cfg.ReadSetting("thingtypes", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                if (de.Value is IDictionary)
                {
                    // Make a category
                    ThingCategory thingcat = new ThingCategory(cfg, null, de.Key.ToString(), enums);

                    //mxd. Otherwise nesting problems might occure
                    if (thingcat.IsValid)
                    {
                        // Add all things in category to the big list
                        AddThingsFromCategory(thingcat); //mxd

                        // Add category to list
                        thingcategories.Add(thingcat);
                    }
                }
            }
        }

        //mxd. This recursively adds all things from a ThingCategory and it's children
        private void AddThingsFromCategory(ThingCategory thingcat)
        {
            if (!thingcat.IsValid) return;

            // Add all things in category to the big list
            foreach (ThingTypeInfo t in thingcat.Things)
            {
                if (!things.ContainsKey(t.Index))
                    things.Add(t.Index, t);
                else
                    General.ErrorLogger.Add(ErrorType.Warning, "Thing number " + t.Index + " is defined more than once (as \"" + things[t.Index].Title + "\" and \"" + t.Title + "\") in the \"" + this.Name + "\" game configuration");
            }

            // Recursively add things from child categories
            foreach (ThingCategory c in thingcat.Children) AddThingsFromCategory(c);
        }

        // Linedef flags
        private void LoadLinedefFlags()
        {
            // Get linedef flags
            LoadStringDictionary(linedefflags, "linedefflags"); //mxd

            // Get translations
            IDictionary dic = cfg.ReadSetting("linedefflagstranslation", new Hashtable());
            foreach (DictionaryEntry de in dic)
                LinedefFlagsTranslation.Add(new FlagTranslation(de));

            // Sort flags?
            MapSetIO io = MapSetIO.Create(FormatInterface);
            if (io.HasNumericLinedefFlags)
            {
                // Make list for integers that we can sort
                List<int> sortlist = new List<int>(linedefflags.Count);
                foreach (KeyValuePair<string, string> f in linedefflags)
                {
                    int num;
                    if (int.TryParse(f.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out num)) sortlist.Add(num);
                }

                // Sort
                sortlist.Sort();

                // Make list of strings
                foreach (int i in sortlist)
                    SortedLinedefFlags.Add(i.ToString(CultureInfo.InvariantCulture));
            }

            // Sort the flags, because they must be compared highest first!
            LinedefFlagsTranslation.Sort();
        }

        // Linedef actions and action categories
        private void LoadLinedefActions()
        {
            Dictionary<string, LinedefActionCategory> cats = new Dictionary<string, LinedefActionCategory>(StringComparer.Ordinal);

            // Get linedef categories
            IDictionary dic = cfg.ReadSetting("linedeftypes", new Hashtable());
            foreach (DictionaryEntry cde in dic)
            {
                if (cde.Value is IDictionary)
                {
                    // Read category title
                    string cattitle = cfg.ReadSetting("linedeftypes." + cde.Key + ".title", "");

                    // Make or get category
                    LinedefActionCategory ac;
                    if (cats.ContainsKey(cde.Key.ToString()))
                        ac = cats[cde.Key.ToString()];
                    else
                    {
                        ac = new LinedefActionCategory(cde.Key.ToString(), cattitle);
                        cats.Add(cde.Key.ToString(), ac);
                    }

                    // Go for all line types in category
                    IDictionary catdic = cfg.ReadSetting("linedeftypes." + cde.Key, new Hashtable());
                    foreach (DictionaryEntry de in catdic)
                    {
                        // Check if the item key is numeric
                        int actionnumber;
                        if (int.TryParse(de.Key.ToString(),
                            NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                            CultureInfo.InvariantCulture, out actionnumber))
                        {
                            // Check if the item value is a structure
                            if (de.Value is IDictionary)
                            {
                                //mxd. Sanity check...
                                if (linedefactions.ContainsKey(actionnumber))
                                {
                                    General.ErrorLogger.Add(ErrorType.Error, "Structure \"linedeftypes\" contains duplicate action definition for action " + actionnumber
                                        + " in the \"" + this.Name + "\" game configuration. If you want to override the existing action definition, make sure to put it in the same category (\""
                                        + linedefactions[actionnumber].Category + "\").");
                                }
                                else
                                {
                                    // Make the line type
                                    LinedefActionInfo ai = new LinedefActionInfo(actionnumber, cfg, cde.Key.ToString(), enums);

                                    // Add action to category and sorted list
                                    SortedLinedefActions.Add(ai);
                                    linedefactions.Add(actionnumber, ai);
                                    ac.Add(ai);
                                }
                            }
                            else
                            {
                                // Failure
                                if (de.Value != null)
                                    General.ErrorLogger.Add(ErrorType.Warning, "Structure \"linedeftypes\" contains invalid types in the \"" + this.Name + "\" game configuration. All types must be expanded structures.");
                            }
                        }
                    }
                }
            }

            // Sort the actions list
            SortedLinedefActions.Sort();

            // Copy categories to final list
            ActionCategories.Clear();
            ActionCategories.AddRange(cats.Values);

            // Sort the categories list
            ActionCategories.Sort();
        }

        // Linedef activates
        private void LoadLinedefActivations()
        {
            // Get linedef activations
            IDictionary dic = cfg.ReadSetting("linedefactivations", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                // If the value is a dictionary read the values from that
                if (de.Value is ICollection)
                {
                    string name = cfg.ReadSetting("linedefactivations." + de.Key.ToString() + ".name", de.Key.ToString());
                    bool istrigger = cfg.ReadSetting("linedefactivations." + de.Key.ToString() + ".istrigger", true);
                    LinedefActivates.Add(new LinedefActivateInfo(de.Key.ToString(), name, istrigger));
                }
                else
                {
                    // Add to the list
                    LinedefActivates.Add(new LinedefActivateInfo(de.Key.ToString(), de.Value.ToString(), true));
                }
            }

            //mxd. Sort only when activations are numeric
            MapSetIO io = MapSetIO.Create(FormatInterface);
            if (io.HasNumericLinedefActivations)
            {
                LinedefActivates.Sort();
            }
        }

        // Linedef generalized actions
        private void LoadLinedefGeneralizedActions()
        {
            // Get linedef activations
            IDictionary dic = cfg.ReadSetting("gen_linedeftypes", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                // Check for valid structure
                if (de.Value is IDictionary)
                {
                    // Add category
                    GenActionCategories.Add(new GeneralizedCategory("gen_linedeftypes", de.Key.ToString(), cfg));
                }
                else
                {
                    General.ErrorLogger.Add(ErrorType.Warning, "Structure \"gen_linedeftypes\" contains invalid entries in the \"" + this.Name + "\" game configuration");
                }
            }
        }

        // Sector effects
        private void LoadSectorEffects()
        {
            // Get sector effects
            IDictionary dic = cfg.ReadSetting("sectortypes", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                // Try parsing the action number
                int actionnumber;
                if (int.TryParse(de.Key.ToString(),
                    NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                    CultureInfo.InvariantCulture, out actionnumber))
                {
                    // Make effects
                    SectorEffectInfo si = new SectorEffectInfo(actionnumber, de.Value.ToString(), true, false);

                    // Add action to category and sorted list
                    SortedSectorEffects.Add(si);
                    sectoreffects.Add(actionnumber, si);
                }
                else
                {
                    General.ErrorLogger.Add(ErrorType.Warning, "Structure \"sectortypes\" contains invalid keys in the \"" + this.Name + "\" game configuration");
                }
            }

            // Sort the actions list
            SortedSectorEffects.Sort();
        }

        // Brightness levels
        private void LoadBrightnessLevels()
        {
            // Get brightness levels structure
            IDictionary dic = cfg.ReadSetting("sectorbrightness", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                // Try paring the level
                int level;
                if (int.TryParse(de.Key.ToString(),
                    NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                    CultureInfo.InvariantCulture, out level))
                {
                    BrightnessLevels.Add(level);
                }
                else
                {
                    General.ErrorLogger.Add(ErrorType.Warning, "Structure \"sectorbrightness\" contains invalid keys in the \"" + this.Name + "\" game configuration");
                }
            }

            // Sort the list
            BrightnessLevels.Sort();
        }

        // Sector generalized effects
        private void LoadSectorGeneralizedEffects()
        {
            // Get sector effects
            IDictionary dic = cfg.ReadSetting("gen_sectortypes", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                // Check for valid structure
                IDictionary value = de.Value as IDictionary;
                if (value != null)
                {
                    // Add option
                    GenEffectOptions.Add(new GeneralizedOption("gen_sectortypes", "", de.Key.ToString(), value));
                }
                else
                {
                    General.ErrorLogger.Add(ErrorType.Warning, "Structure \"gen_sectortypes\" contains invalid entries in the \"" + this.Name + "\" game configuration");
                }
            }
        }

        // Thing flags
        private void LoadThingFlags()
        {
            // Get thing flags
            LoadStringDictionary(thingflags, "thingflags"); //mxd

            // Get translations
            IDictionary dic = cfg.ReadSetting("thingflagstranslation", new Hashtable());
            foreach (DictionaryEntry de in dic)
                ThingFlagsTranslation.Add(new FlagTranslation(de));

            // Get thing compare flag info (for the stuck thing error checker
            HashSet<string> flagscache = new HashSet<string>();
            dic = cfg.ReadSetting("thingflagscompare", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                string group = de.Key.ToString(); //mxd
                ThingFlagsCompare[group] = new ThingFlagsCompareGroup(cfg, group); //mxd
                foreach (string s in ThingFlagsCompare[group].Flags.Keys)
                {
                    if (flagscache.Contains(s))
                        General.ErrorLogger.Add(ErrorType.Warning, "ThingFlagsCompare flag \"" + s + "\" is double defined in the \"" + group + "\" group of the \"" + this.Name + "\" game configuration");
                    else
                        flagscache.Add(s);
                }
            }

            //mxd. Integrity check
            foreach (KeyValuePair<string, ThingFlagsCompareGroup> group in ThingFlagsCompare)
            {
                foreach (ThingFlagsCompare flag in group.Value.Flags.Values)
                {
                    // Required groups are missing?
                    foreach (string s in flag.RequiredGroups)
                    {
                        if (!ThingFlagsCompare.ContainsKey(s))
                        {
                            General.ErrorLogger.Add(ErrorType.Warning, "ThingFlagsCompare group \"" + s + "\" required by flag \"" + flag.Flag + "\" does not exist in the \"" + this.Name + "\" game configuration");
                            flag.RequiredGroups.Remove(s);
                        }
                    }

                    // Ignored groups are missing?
                    foreach (string s in flag.IgnoredGroups)
                    {
                        if (!ThingFlagsCompare.ContainsKey(s))
                        {
                            General.ErrorLogger.Add(ErrorType.Warning, "ThingFlagsCompare group \"" + s + "\", ignored by flag \"" + flag.Flag + "\" does not exist in the \"" + this.Name + "\" game configuration");
                            flag.IgnoredGroups.Remove(s);
                        }
                    }

                    // Required flag is missing?
                    if (!string.IsNullOrEmpty(flag.RequiredFlag) && !flagscache.Contains(flag.RequiredFlag))
                    {
                        General.ErrorLogger.Add(ErrorType.Warning, "ThingFlagsCompare flag \"" + flag.RequiredFlag + "\", required by flag \"" + flag.Flag + "\" does not exist in the \"" + this.Name + "\" game configuration");
                        flag.RequiredFlag = string.Empty;
                    }
                }
            }

            // Sort the translation flags, because they must be compared highest first!
            ThingFlagsTranslation.Sort();
        }

        // Default thing flags
        private void LoadDefaultThingFlags()
        {
            // Get linedef flags
            IDictionary dic = cfg.ReadSetting("defaultthingflags", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                // Check if flag exists
                if (thingflags.ContainsKey(de.Key.ToString()))
                {
                    defaultthingflags.Add(de.Key.ToString());
                }
                else
                {
                    General.ErrorLogger.Add(ErrorType.Warning, "Structure \"defaultthingflags\" contains unknown thing flags in the \"" + this.Name + "\" game configuration");
                }
            }
        }

        // Skills
        private void LoadSkills()
        {
            // Get skills
            IDictionary dic = cfg.ReadSetting("skills", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                int num;
                if (int.TryParse(de.Key.ToString(), out num))
                {
                    Skills.Add(new SkillInfo(num, de.Value.ToString()));
                }
                else
                {
                    General.ErrorLogger.Add(ErrorType.Warning, "Structure \"skills\" contains invalid skill numbers in the \"" + this.Name + "\" game configuration");
                }
            }
        }

        // Texture Sets
        private void LoadTextureSets()
        {
            // Get sets
            IDictionary dic = cfg.ReadSetting("texturesets", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                DefinedTextureSet s = new DefinedTextureSet(cfg, "texturesets." + de.Key);
                TextureSets.Add(s);
            }
        }

        // Thing Filters
        private void LoadThingFilters()
        {
            // Get sets
            IDictionary dic = cfg.ReadSetting("thingsfilters", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                ThingsFilter f = new ThingsFilter(cfg, "thingsfilters." + de.Key);
                ThingsFilters.Add(f);
            }
        }

        // Make door flags
        private void LoadMakeDoorFlags()
        {
            IDictionary dic = cfg.ReadSetting("makedoorflags", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                // Using minus will unset the flag
                if (de.Key.ToString()[0] == '-')
                {
                    MakeDoorFlags[de.Key.ToString().TrimStart('-')] = false;
                }
                else
                {
                    MakeDoorFlags[de.Key.ToString()] = true;
                }
            }
        }

        //mxd
        private void LoadDefaultSkies()
        {
            IDictionary dic = cfg.ReadSetting("defaultskytextures", new Hashtable());
            char[] separator = new[] { ',' };
            foreach (DictionaryEntry de in dic)
            {
                string skytex = de.Key.ToString();
                if (DefaultSkyTextures.ContainsKey(skytex))
                {
                    General.ErrorLogger.Add(ErrorType.Warning, "Sky texture \"" + skytex + "\" is double defined in the \"" + this.Name + "\" game configuration");
                    continue;
                }

                string[] maps = de.Value.ToString().Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (maps.Length == 0)
                {
                    General.ErrorLogger.Add(ErrorType.Warning, "Sky texture \"" + skytex + "\" has no map names defined in the \"" + this.Name + "\" game configuration");
                    continue;
                }

                foreach (string map in maps)
                {
                    if (DefaultSkyTextures.ContainsKey(map))
                    {
                        General.ErrorLogger.Add(ErrorType.Warning, "Map \"" + map + "\" is double defined in the \"DefaultSkyTextures\" block of \"" + this.Name + "\" game configuration");
                        continue;
                    }

                    DefaultSkyTextures[map] = skytex;
                }
            }
        }

        //mxd
        private void LoadStringDictionary(Dictionary<string, string> target, string settingname)
        {
            IDictionary dic = cfg.ReadSetting(settingname, new Hashtable());
            foreach (DictionaryEntry de in dic)
                target.Add(de.Key.ToString(), de.Value.ToString());
        }

        #endregion

        #region ================== Methods

        // ReadSetting
        public string ReadSetting(string setting, string defaultsetting) { return cfg.ReadSetting(setting, defaultsetting); }
        public int ReadSetting(string setting, int defaultsetting) { return cfg.ReadSetting(setting, defaultsetting); }
        public float ReadSetting(string setting, float defaultsetting) { return cfg.ReadSetting(setting, defaultsetting); }
        public double ReadSetting(string setting, double defaultsetting) { return cfg.ReadSetting(setting, defaultsetting); }
        public short ReadSetting(string setting, short defaultsetting) { return cfg.ReadSetting(setting, defaultsetting); }
        public long ReadSetting(string setting, long defaultsetting) { return cfg.ReadSetting(setting, defaultsetting); }
        public bool ReadSetting(string setting, bool defaultsetting) { return cfg.ReadSetting(setting, defaultsetting); }
        public byte ReadSetting(string setting, byte defaultsetting) { return cfg.ReadSetting(setting, defaultsetting); }
        public IDictionary ReadSetting(string setting, IDictionary defaultsetting) { return cfg.ReadSetting(setting, defaultsetting); }

        // This gets a list of things categories
        internal List<ThingCategory> GetThingCategories()
        {
            return new List<ThingCategory>(thingcategories);
        }

        // This gets a list of things
        internal Dictionary<int, ThingTypeInfo> GetThingTypes()
        {
            return new Dictionary<int, ThingTypeInfo>(things);
        }

        // This checks if an action is generalized or predefined
        public static bool IsGeneralized(int action) { return IsGeneralized(action, General.Map.Config.GenActionCategories); }
        public static bool IsGeneralized(int action, IEnumerable<GeneralizedCategory> categories)
        {
            // Only actions above 0
            if (action > 0)
            {
                // Go for all categories
                foreach (GeneralizedCategory ac in categories)
                {
                    // Check if the action is within range of this category
                    if ((action >= ac.Offset) && (action < (ac.Offset + ac.Length))) return true;
                }
            }

            // Not generalized
            return false;
        }

        // This gets the generalized action category from action number
        public GeneralizedCategory GetGeneralizedActionCategory(int action)
        {
            // Only actions above 0
            if (action > 0)
            {
                // Go for all categories
                foreach (GeneralizedCategory ac in GenActionCategories)
                {
                    // Check if the action is within range of this category
                    if ((action >= ac.Offset) && (action < (ac.Offset + ac.Length))) return ac;
                }
            }

            // Not generalized
            return null;
        }

        //mxd
        public static bool IsGeneralizedSectorEffect(int effect) { return IsGeneralizedSectorEffect(effect, General.Map.Config.GenEffectOptions); }
        public static bool IsGeneralizedSectorEffect(int effect, List<GeneralizedOption> options)
        {
            if (effect == 0) return false;

            int cureffect = effect;
            for (int i = options.Count - 1; i > -1; i--)
            {
                for (int j = options[i].Bits.Count - 1; j > -1; j--)
                {
                    GeneralizedBit bit = options[i].Bits[j];
                    if (bit.Index > cureffect) continue;
                    if (bit.Index > 0 && (cureffect & bit.Index) == bit.Index) return true;
                    cureffect -= bit.Index;
                }
            }

            return false;
        }

        //mxd
        public SectorEffectData GetSectorEffectData(int effect) { return GetSectorEffectData(effect, General.Map.Config.GenEffectOptions); }
        public SectorEffectData GetSectorEffectData(int effect, List<GeneralizedOption> options)
        {
            SectorEffectData result = new SectorEffectData();
            if (effect > 0)
            {
                int cureffect = effect;

                if (General.Map.Config.GeneralizedEffects)
                {
                    for (int i = options.Count - 1; i > -1; i--)
                    {
                        for (int j = options[i].Bits.Count - 1; j > -1; j--)
                        {
                            GeneralizedBit bit = options[i].Bits[j];
                            if (bit.Index > 0 && (cureffect & bit.Index) == bit.Index)
                            {
                                cureffect -= bit.Index;
                                result.GeneralizedBits.Add(bit.Index);
                            }
                        }
                    }
                }

                if (cureffect > 0) result.Effect = cureffect;
            }

            return result;
        }

        //mxd
        public string GetGeneralizedSectorEffectName(int effect)
        {
            if (effect == 0) return "None";
            string title = "Unknown generalized effect";
            int matches = 0;

            int nongeneralizedeffect = effect;

            // Check all options, in bigger to smaller order
            for (int i = GenEffectOptions.Count - 1; i > -1; i--)
            {
                for (int j = GenEffectOptions[i].Bits.Count - 1; j > -1; j--)
                {
                    GeneralizedBit bit = GenEffectOptions[i].Bits[j];
                    if (bit.Index > 0 && (effect & bit.Index) == bit.Index)
                    {
                        title = GenEffectOptions[i].Name + ": " + bit.Title;
                        nongeneralizedeffect -= bit.Index;
                        matches++;
                        break;
                    }
                }
            }

            // Make generalized effect title
            string gentitle = matches > 1 ? "Generalized (" + matches + " effects)" : title;

            // Generalized effect only
            if (nongeneralizedeffect <= 0) return gentitle;

            // Classic and generalized effects
            if (General.Map.Config.SectorEffects.ContainsKey(nongeneralizedeffect))
                return General.Map.Config.SectorEffects[nongeneralizedeffect].Title + " + " + gentitle;

            if (matches > 0) return "Unknown effect + " + gentitle;
            return "Unknown effect";
        }

        // This checks if a specific edit mode class is listed
        public bool IsEditModeSpecified(string classname)
        {
            return cfg.SettingExists("editingmodes." + classname.ToString(CultureInfo.InvariantCulture));
        }

        // This returns information on a linedef type
        public LinedefActionInfo GetLinedefActionInfo(int action)
        {
            // No action?
            if (action == 0) return new LinedefActionInfo(0, "None", true, false);

            // Known type?
            if (linedefactions.ContainsKey(action)) return linedefactions[action];

            // Generalized action?
            if (IsGeneralized(action, GenActionCategories))
                return new LinedefActionInfo(action, "Generalized (" + GetGeneralizedActionCategory(action) + ")", true, true);

            // Unknown action...
            return new LinedefActionInfo(action, "Unknown", false, false);
        }

        // This returns information on a sector effect
        public SectorEffectInfo GetSectorEffectInfo(int effect)
        {
            // No effect?
            if (effect == 0) return new SectorEffectInfo(0, "None", true, false);

            // Known type?
            if (sectoreffects.ContainsKey(effect)) return sectoreffects[effect];

            //mxd. Generalized sector effect?
            if (IsGeneralizedSectorEffect(effect, GenEffectOptions))
                return new SectorEffectInfo(effect, GetGeneralizedSectorEffectName(effect), true, true);

            // Unknown sector effect...
            return new SectorEffectInfo(effect, "Unknown", false, false);
        }

        /// <summary>
        /// Checks if there a script lumps defined in the configuration
        /// </summary>
        /// <returns>true if there are script lumps defined, false if not</returns>
        public bool HasScriptLumps()
        {
            return MapLumps.Values.Count(o => o.ScriptBuild || o.Script != null) > 0;
        }

        /// <summary>
        /// Checks if this game configuration supports the requested map feature(s)
        /// </summary>
        /// <param name="features">Array of strings of property names of the GameConfiguration class</param>
        /// <returns></returns>
        public bool SupportsMapFeatures(string[] features, [CallerMemberName] string callername = "")
        {
            bool supported = true;

            foreach (string rmf in features)
            {
                PropertyInfo pi = GetType().GetProperty(rmf);

                if (pi == null)
                {
                    General.ErrorLogger.Add(ErrorType.Error, "Check for supported map features (" + string.Join(", ", features) + ") was requested my " + callername + ", but property \"" + rmf + "\" does not exist.");
                    return false;
                }

                object value = pi.GetValue(this);

                if (value is bool && (bool)value == false)
                {
                    supported = false;
                    break;
                }
            }

            return supported;
        }

        /// <summary>
        /// Checks if a MapElement type has a UDMF field or flag defined.
        /// </summary>
        /// <typeparam name="T">Type inherited from MapElement</typeparam>
        /// <param name="name">Name of the UDMF field or flag</param>
        /// <returns>true if the field or flag exists, false if it doesn't</returns>
        public bool HasUniversalFieldOrFlag<T>(string name) where T : MapElement
        {
            Type type = typeof(T);
            List<UniversalFieldInfo> ufi;
            Dictionary<string, string> flags;

            if (type == typeof(Thing))
            {
                ufi = ThingFields;
                flags = thingflags;
            }
            else if (type == typeof(Linedef))
            {
                ufi = LinedefFields;
                flags = linedefflags;
            }
            else if (type == typeof(Sidedef))
            {
                ufi = SidedefFields;
                flags = sidedefflags;
            }
            else if (type == typeof(Sector))
            {
                ufi = SectorFields;
                flags = sectorflags;
            }
            else if (type == typeof(Vertex))
            {
                ufi = VertexFields;
                flags = new Dictionary<string, string>(); // Vertices don't have flags
            }
            else
                throw new NotSupportedException("Unsupported MapElement type: " + type.Name);

            // Check for regular UDMF fields
            if (ufi.Where(f => f.Name == name).FirstOrDefault() != null)
                return true;

            // Check for flags
            if (flags.ContainsKey(name))
                return true;

            return false;
        }

        #endregion
    }
}
