
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
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.GZBuilder.Data;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;

#endregion

namespace CodeImp.DoomBuilder.Config
{
    public class ConfigurationInfo : IComparable<ConfigurationInfo>
    {
        #region ================== Constants

        private const string MODE_DISABLED_KEY = "disabled";
        private const string MODE_ENABLED_KEY = "enabled";

        // The { and } are invalid key names in a configuration so this ensures this string is unique
        private const string MISSING_NODEBUILDER = "{missing nodebuilder}";
        private readonly string[] LINEDEF_COLOR_PRESET_FLAGS_SEPARATOR = new[] { "^" }; //mxd

        #endregion

        #region ================== Variables

        private string settingskey;
        private List<ThingsFilter> thingsfilters;

        #endregion

        #region ================== Properties

        public string Name { get; private set; }
        public string Filename { get; private set; }
        public string DefaultLumpName { get; }
        public string NodebuilderSave { get; internal set; }
        public string NodebuilderTest { get; internal set; }
        public string FormatInterface { get; private set; } //mxd
        public string DefaultScriptCompiler { get; } //mxd
        internal DataLocationList Resources { get; private set; }
        internal Configuration Configuration { get; private set; } //mxd
        public bool Enabled { get; internal set; } //mxd
        public bool Changed { get; internal set; } //mxd

        //mxd
        public string TestProgramName { get { return TestEngines[CurrentEngineIndex].TestProgramName; } internal set { TestEngines[CurrentEngineIndex].TestProgramName = value; } }
        public string TestProgram { get { return TestEngines[CurrentEngineIndex].TestProgram; } internal set { TestEngines[CurrentEngineIndex].TestProgram = value; } }
        public string TestParameters { get { return TestEngines[CurrentEngineIndex].TestParameters; } internal set { TestEngines[CurrentEngineIndex].TestParameters = value; } }
        public bool TestShortPaths { get { return TestEngines[CurrentEngineIndex].TestShortPaths; } internal set { TestEngines[CurrentEngineIndex].TestShortPaths = value; } }
        public bool TestLinuxPaths { get { return TestEngines[CurrentEngineIndex].TestLinuxPaths; } internal set { TestEngines[CurrentEngineIndex].TestLinuxPaths = value; } }
        public int TestSkill { get { return TestEngines[CurrentEngineIndex].TestSkill; } internal set { TestEngines[CurrentEngineIndex].TestSkill = value; } }
        public string TestAdditionalParameters { get { return TestEngines[CurrentEngineIndex].AdditionalParameters; } internal set { TestEngines[CurrentEngineIndex].AdditionalParameters = value; } }
        public bool CustomParameters { get { return TestEngines[CurrentEngineIndex].CustomParameters; } internal set { TestEngines[CurrentEngineIndex].CustomParameters = value; } }
        public List<EngineInfo> TestEngines { get; internal set; }
        public int CurrentEngineIndex { get; internal set; }
        public LinedefColorPreset[] LinedefColorPresets { get; internal set; }

        internal ICollection<ThingsFilter> ThingsFilters { get { return thingsfilters; } }
        internal List<DefinedTextureSet> TextureSets { get; private set; }
        internal Dictionary<string, bool> EditModes { get; private set; }
        public string StartMode { get; internal set; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal ConfigurationInfo(Configuration cfg, string filename)
        {
            // Initialize
            this.Filename = filename;
            this.Configuration = cfg; //mxd
            this.settingskey = Path.GetFileNameWithoutExtension(filename).ToLower();

            // Load settings from game configuration
            this.Name = Configuration.ReadSetting("game", "<unnamed game>");
            this.DefaultLumpName = Configuration.ReadSetting("defaultlumpname", "");

            // Load settings from program configuration
            this.NodebuilderSave = General.Settings.ReadSetting("configurations." + settingskey + ".nodebuildersave", MISSING_NODEBUILDER);
            this.NodebuilderTest = General.Settings.ReadSetting("configurations." + settingskey + ".nodebuildertest", MISSING_NODEBUILDER);
            this.FormatInterface = Configuration.ReadSetting("formatinterface", "").ToLowerInvariant(); //mxd
            this.DefaultScriptCompiler = cfg.ReadSetting("defaultscriptcompiler", ""); //mxd
            this.Resources = new DataLocationList(General.Settings.Config, "configurations." + settingskey + ".resources");
            this.StartMode = General.Settings.ReadSetting("configurations." + settingskey + ".startmode", "VerticesMode");
            this.Enabled = General.Settings.ReadSetting("configurations." + settingskey + ".enabled", Configuration.ReadSetting("enabledbydefault", false)); //mxd

            //mxd. Read test engines
            TestEngines = new List<EngineInfo>();
            IDictionary list = General.Settings.ReadSetting("configurations." + settingskey + ".engines", new ListDictionary());
            CurrentEngineIndex = Math.Max(0, General.Settings.ReadSetting("configurations." + settingskey + ".currentengineindex", 0));

            // No engine list found? Use old engine properties
            if (list.Count == 0)
            {
                EngineInfo info = new EngineInfo();
                info.TestProgram = General.Settings.ReadSetting("configurations." + settingskey + ".testprogram", "");
                info.TestProgramName = General.Settings.ReadSetting("configurations." + settingskey + ".testprogramname", EngineInfo.DEFAULT_ENGINE_NAME);
                info.TestParameters = General.Settings.ReadSetting("configurations." + settingskey + ".testparameters", "");
                info.TestShortPaths = General.Settings.ReadSetting("configurations." + settingskey + ".testshortpaths", false);
                info.TestLinuxPaths = General.Settings.ReadSetting("configurations." + settingskey + ".testlinuxpaths", false);
                info.CustomParameters = General.Settings.ReadSetting("configurations." + settingskey + ".customparameters", false);
                info.TestSkill = General.Settings.ReadSetting("configurations." + settingskey + ".testskill", 3);
                info.AdditionalParameters = General.Settings.ReadSetting("configurations." + settingskey + ".additionalparameters", "");
                TestEngines.Add(info);
                CurrentEngineIndex = 0;
            }
            else
            {
                //read engines settings from config
                foreach (DictionaryEntry de in list)
                {
                    string path = "configurations." + settingskey + ".engines." + de.Key;
                    EngineInfo info = new EngineInfo();
                    info.TestProgram = General.Settings.ReadSetting(path + ".testprogram", "");
                    info.TestProgramName = General.Settings.ReadSetting(path + ".testprogramname", EngineInfo.DEFAULT_ENGINE_NAME);
                    info.TestParameters = General.Settings.ReadSetting(path + ".testparameters", "");
                    info.TestShortPaths = General.Settings.ReadSetting(path + ".testshortpaths", false);
                    info.TestLinuxPaths = General.Settings.ReadSetting(path + ".testlinuxpaths", false);
                    info.CustomParameters = General.Settings.ReadSetting(path + ".customparameters", false);
                    info.TestSkill = General.Settings.ReadSetting(path + ".testskill", 3);
                    info.AdditionalParameters = General.Settings.ReadSetting(path + ".additionalparameters", "");
                    TestEngines.Add(info);
                }

                if (CurrentEngineIndex >= TestEngines.Count) CurrentEngineIndex = 0;
            }

            //mxd. read custom linedef colors 
            List<LinedefColorPreset> colorPresets = new List<LinedefColorPreset>();
            list = General.Settings.ReadSetting("configurations." + settingskey + ".linedefcolorpresets", new ListDictionary());

            //no presets? add "classic" ones then.
            if (list.Count == 0)
            {
                colorPresets.Add(new LinedefColorPreset("Any action", PixelColor.FromColor(System.Drawing.Color.PaleGreen), -1, 0, new List<string>(), new List<string>(), true));
            }
            else
            {
                //read custom linedef colors from config
                foreach (DictionaryEntry de in list)
                {
                    string path = "configurations." + settingskey + ".linedefcolorpresets." + de.Key;
                    string presetname = General.Settings.ReadSetting(path + ".name", "Unnamed");
                    bool presetenabled = General.Settings.ReadSetting(path + ".enabled", true);
                    PixelColor color = PixelColor.FromInt(General.Settings.ReadSetting(path + ".color", -1));
                    int action = General.Settings.ReadSetting(path + ".action", 0);
                    int activation = General.Settings.ReadSetting(path + ".activation", 0);
                    List<string> flags = new List<string>();
                    flags.AddRange(General.Settings.ReadSetting(path + ".flags", "").Split(LINEDEF_COLOR_PRESET_FLAGS_SEPARATOR, StringSplitOptions.RemoveEmptyEntries));
                    List<string> restrictedFlags = new List<string>();
                    restrictedFlags.AddRange(General.Settings.ReadSetting(path + ".restrictedflags", "").Split(LINEDEF_COLOR_PRESET_FLAGS_SEPARATOR, StringSplitOptions.RemoveEmptyEntries));
                    LinedefColorPreset preset = new LinedefColorPreset(presetname, color, action, activation, flags, restrictedFlags, presetenabled);
                    colorPresets.Add(preset);
                }
            }
            LinedefColorPresets = colorPresets.ToArray();

            // Make list of things filters
            thingsfilters = new List<ThingsFilter>();
            IDictionary cfgfilters = General.Settings.ReadSetting("configurations." + settingskey + ".thingsfilters", new Hashtable());
            foreach (DictionaryEntry de in cfgfilters)
            {
                thingsfilters.Add(new ThingsFilter(General.Settings.Config, "configurations." + settingskey + ".thingsfilters." + de.Key));
            }

            // Make list of texture sets
            TextureSets = new List<DefinedTextureSet>();
            IDictionary sets = General.Settings.ReadSetting("configurations." + settingskey + ".texturesets", new Hashtable());
            foreach (DictionaryEntry de in sets)
            {
                TextureSets.Add(new DefinedTextureSet(General.Settings.Config, "configurations." + settingskey + ".texturesets." + de.Key));
            }

            // Make list of edit modes
            this.EditModes = new Dictionary<string, bool>(StringComparer.Ordinal);
            IDictionary modes = General.Settings.ReadSetting("configurations." + settingskey + ".editmodes", new Hashtable());
            foreach (DictionaryEntry de in modes)
            {
                if (de.Key.ToString().StartsWith(MODE_ENABLED_KEY))
                    EditModes.Add(de.Value.ToString(), true);
                else if (de.Key.ToString().StartsWith(MODE_DISABLED_KEY))
                    EditModes.Add(de.Value.ToString(), false);
            }
        }

        // Constructor
        private ConfigurationInfo()
        {
        }

        //mxd. Destructor
        ~ConfigurationInfo()
        {
            // biwa. There have been crash reports because of null references
            // https://github.com/jewalky/UltimateDoomBuilder/issues/251
            // https://github.com/jewalky/UltimateDoomBuilder/issues/352
            // https://github.com/jewalky/UltimateDoomBuilder/issues/514
            // Can't reproduce, but add a safeguard anyway.
            if (thingsfilters != null) foreach (ThingsFilter tf in thingsfilters) if (tf != null) tf.Dispose();
            if (TestEngines != null) foreach (EngineInfo ei in TestEngines) if (ei != null) ei.Dispose();
        }

        #endregion

        #region ================== Methods

        /// <summary>
        /// This returns the resource locations as configured.
        /// </summary>
        public DataLocationList GetResources()
        {
            return new DataLocationList(Resources);
        }

        // This compares it to other ConfigurationInfo objects
        public int CompareTo(ConfigurationInfo other)
        {
            // Compare
            return String.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        // This saves the settings to program configuration
        internal void SaveSettings()
        {
            //mxd
            General.Settings.WriteSetting("configurations." + settingskey + ".enabled", Enabled);
            if (!Changed) return;

            // Write to configuration
            General.Settings.WriteSetting("configurations." + settingskey + ".nodebuildersave", NodebuilderSave);
            General.Settings.WriteSetting("configurations." + settingskey + ".nodebuildertest", NodebuilderTest);

            //mxd. Test Engines
            General.Settings.WriteSetting("configurations." + settingskey + ".currentengineindex", CurrentEngineIndex);
            SaveTestEngines();

            //mxd. Custom linedef colors
            SaveLinedefColorPresets();

            General.Settings.WriteSetting("configurations." + settingskey + ".startmode", StartMode);
            Resources.WriteToConfig(General.Settings.Config, "configurations." + settingskey + ".resources");

            // Write filters to configuration
            General.Settings.DeleteSetting("configurations." + settingskey + ".thingsfilters");
            for (int i = 0; i < thingsfilters.Count; i++)
            {
                thingsfilters[i].WriteSettings(General.Settings.Config,
                    "configurations." + settingskey + ".thingsfilters.filter" + i.ToString(CultureInfo.InvariantCulture));
            }

            // Write texturesets to configuration
            General.Settings.DeleteSetting("configurations." + settingskey + ".texturesets"); //mxd
            for (int i = 0; i < TextureSets.Count; i++)
            {
                TextureSets[i].WriteToConfig(General.Settings.Config,
                    "configurations." + settingskey + ".texturesets.set" + i.ToString(CultureInfo.InvariantCulture));
            }

            // Write edit modes to configuration
            ListDictionary modeslist = new ListDictionary();
            int index = 0;
            foreach (KeyValuePair<string, bool> em in EditModes)
            {
                if (em.Value)
                    modeslist.Add(MODE_ENABLED_KEY + index.ToString(CultureInfo.InvariantCulture), em.Key);
                else
                    modeslist.Add(MODE_DISABLED_KEY + index.ToString(CultureInfo.InvariantCulture), em.Key);

                index++;
            }
            General.Settings.WriteSetting("configurations." + settingskey + ".editmodes", modeslist);
        }

        //mxd
        private void SaveTestEngines()
        {
            // Fill structure
            IDictionary resinfo = new ListDictionary();

            for (int i = 0; i < TestEngines.Count; i++)
            {
                IDictionary rlinfo = new ListDictionary();
                rlinfo.Add("testprogramname", TestEngines[i].TestProgramName);
                rlinfo.Add("testprogram", TestEngines[i].TestProgram);
                rlinfo.Add("testparameters", TestEngines[i].TestParameters);
                rlinfo.Add("testshortpaths", TestEngines[i].TestShortPaths);
                rlinfo.Add("testlinuxpaths", TestEngines[i].TestLinuxPaths);
                rlinfo.Add("customparameters", TestEngines[i].CustomParameters);
                rlinfo.Add("testskill", TestEngines[i].TestSkill);
                rlinfo.Add("additionalparameters", TestEngines[i].AdditionalParameters);

                // Add structure
                resinfo.Add("engine" + i.ToString(CultureInfo.InvariantCulture), rlinfo);
            }

            // Write to config
            General.Settings.Config.WriteSetting("configurations." + settingskey + ".engines", resinfo);
        }

        //mxd
        private void SaveLinedefColorPresets()
        {
            // Fill structure
            IDictionary resinfo = new ListDictionary();

            for (int i = 0; i < LinedefColorPresets.Length; i++)
            {
                IDictionary rlinfo = new ListDictionary();
                rlinfo.Add("name", LinedefColorPresets[i].Name);
                rlinfo.Add("enabled", LinedefColorPresets[i].Enabled);
                rlinfo.Add("color", LinedefColorPresets[i].Color.ToInt());
                rlinfo.Add("action", LinedefColorPresets[i].Action);
                rlinfo.Add("activation", LinedefColorPresets[i].Activation);
                rlinfo.Add("flags", string.Join(LINEDEF_COLOR_PRESET_FLAGS_SEPARATOR[0], LinedefColorPresets[i].Flags.ToArray()));
                rlinfo.Add("restrictedflags", string.Join(LINEDEF_COLOR_PRESET_FLAGS_SEPARATOR[0], LinedefColorPresets[i].RestrictedFlags.ToArray()));

                // Add structure
                resinfo.Add("preset" + i.ToString(CultureInfo.InvariantCulture), rlinfo);
            }

            // Write to config
            General.Settings.Config.WriteSetting("configurations." + settingskey + ".linedefcolorpresets", resinfo);
        }


        // String representation
        public override string ToString()
        {
            return Name;
        }

        // This clones the object
        internal ConfigurationInfo Clone()
        {
            ConfigurationInfo ci = new ConfigurationInfo();
            ci.Name = this.Name;
            ci.Filename = this.Filename;
            ci.settingskey = this.settingskey;
            ci.NodebuilderSave = this.NodebuilderSave;
            ci.NodebuilderTest = this.NodebuilderTest;
            ci.FormatInterface = this.FormatInterface; //mxd
            ci.Resources = new DataLocationList(this.Resources);

            //mxd
            ci.TestEngines = new List<EngineInfo>();
            foreach (EngineInfo info in TestEngines) ci.TestEngines.Add(new EngineInfo(info));
            ci.CurrentEngineIndex = this.CurrentEngineIndex;
            ci.LinedefColorPresets = new LinedefColorPreset[LinedefColorPresets.Length];
            for (int i = 0; i < LinedefColorPresets.Length; i++)
                ci.LinedefColorPresets[i] = new LinedefColorPreset(LinedefColorPresets[i]);

            ci.StartMode = this.StartMode;
            ci.Configuration = this.Configuration; //mxd
            ci.Enabled = this.Enabled; //mxd
            ci.Changed = this.Changed; //mxd
            ci.TextureSets = new List<DefinedTextureSet>();
            foreach (DefinedTextureSet s in this.TextureSets) ci.TextureSets.Add(s.Copy());
            ci.thingsfilters = new List<ThingsFilter>();
            foreach (ThingsFilter f in this.thingsfilters) ci.thingsfilters.Add(new ThingsFilter(f));
            ci.EditModes = new Dictionary<string, bool>(this.EditModes);
            return ci;
        }

        // This applies settings from an object
        internal void Apply(ConfigurationInfo ci)
        {
            this.Name = ci.Name;
            this.Filename = ci.Filename;
            this.settingskey = ci.settingskey;
            this.NodebuilderSave = ci.NodebuilderSave;
            this.NodebuilderTest = ci.NodebuilderTest;
            this.FormatInterface = ci.FormatInterface; //mxd
            this.CurrentEngineIndex = ci.CurrentEngineIndex; //mxd
            this.Resources = new DataLocationList(ci.Resources);

            //mxd
            this.TestEngines = new List<EngineInfo>();
            foreach (EngineInfo info in ci.TestEngines) TestEngines.Add(new EngineInfo(info));
            if (this.CurrentEngineIndex >= TestEngines.Count) this.CurrentEngineIndex = Math.Max(0, TestEngines.Count - 1);
            this.LinedefColorPresets = new LinedefColorPreset[ci.LinedefColorPresets.Length];
            for (int i = 0; i < ci.LinedefColorPresets.Length; i++)
                this.LinedefColorPresets[i] = new LinedefColorPreset(ci.LinedefColorPresets[i]);

            this.StartMode = ci.StartMode;
            this.Configuration = ci.Configuration; //mxd
            this.Enabled = ci.Enabled; //mxd
            this.Changed = ci.Changed;
            this.TextureSets = new List<DefinedTextureSet>();
            foreach (DefinedTextureSet s in ci.TextureSets) this.TextureSets.Add(s.Copy());
            this.thingsfilters = new List<ThingsFilter>();
            foreach (ThingsFilter f in ci.thingsfilters) this.thingsfilters.Add(new ThingsFilter(f));
            this.EditModes = new Dictionary<string, bool>(ci.EditModes);
        }

        // This applies the defaults
        internal void ApplyDefaults(GameConfiguration gameconfig)
        {
            // Some of the defaults can only be applied from game configuration
            if (gameconfig != null)
            {
                // No nodebuildes set?
                if (NodebuilderSave == MISSING_NODEBUILDER) NodebuilderSave = gameconfig.DefaultSaveCompiler;
                if (NodebuilderTest == MISSING_NODEBUILDER) NodebuilderTest = gameconfig.DefaultTestCompiler;

                // No texture sets?
                if (TextureSets.Count == 0)
                {
                    // Copy the default texture sets from the game configuration
                    foreach (DefinedTextureSet s in gameconfig.TextureSets)
                    {
                        // Add a copy to our list
                        TextureSets.Add(s.Copy());
                    }
                }

                // No things filters?
                if (thingsfilters.Count == 0)
                {
                    // Copy the things filters from game configuration
                    foreach (ThingsFilter f in gameconfig.ThingsFilters)
                        thingsfilters.Add(new ThingsFilter(f));
                }

                //mxd. Validate filters. Do it only for currently used ConfigInfo
                if (General.Map != null && General.Map.ConfigSettings == this)
                {
                    foreach (ThingsFilter f in thingsfilters) f.Validate();
                }
            }

            // Go for all available editing modes
            foreach (EditModeInfo info in General.Editing.ModesInfo)
            {
                // Is this a mode that is optional?
                if (info.IsOptional)
                {
                    // Add if not listed yet
                    if (!EditModes.ContainsKey(info.Type.FullName))
                        EditModes.Add(info.Type.FullName, info.Attributes.UseByDefault);
                }
            }
        }

        //mxd
        internal void PasteResourcesFrom(ConfigurationInfo source)
        {
            Resources = new DataLocationList(source.Resources);
            Changed = true;
        }

        //mxd
        internal void PasteTestEnginesFrom(ConfigurationInfo source)
        {
            CurrentEngineIndex = source.CurrentEngineIndex;
            TestEngines = new List<EngineInfo>();
            foreach (EngineInfo info in source.TestEngines) TestEngines.Add(new EngineInfo(info));
            if (CurrentEngineIndex >= TestEngines.Count) CurrentEngineIndex = Math.Max(0, TestEngines.Count - 1);
            Changed = true;
        }

        //mxd
        internal void PasteColorPresetsFrom(ConfigurationInfo source)
        {
            LinedefColorPresets = new LinedefColorPreset[source.LinedefColorPresets.Length];
            for (int i = 0; i < source.LinedefColorPresets.Length; i++)
                LinedefColorPresets[i] = new LinedefColorPreset(source.LinedefColorPresets[i]);
            Changed = true;
        }

        //mxd. Not all properties should be pasted
        internal void PasteFrom(ConfigurationInfo source)
        {
            NodebuilderSave = source.NodebuilderSave;
            NodebuilderTest = source.NodebuilderTest;
            CurrentEngineIndex = source.CurrentEngineIndex;
            Resources = new DataLocationList(source.Resources);

            TestEngines = new List<EngineInfo>();
            foreach (EngineInfo info in source.TestEngines)
                TestEngines.Add(new EngineInfo(info));
            if (CurrentEngineIndex >= TestEngines.Count) CurrentEngineIndex = Math.Max(0, TestEngines.Count - 1);
            LinedefColorPresets = new LinedefColorPreset[source.LinedefColorPresets.Length];
            for (int i = 0; i < source.LinedefColorPresets.Length; i++)
                LinedefColorPresets[i] = new LinedefColorPreset(source.LinedefColorPresets[i]);

            StartMode = source.StartMode;
            Changed = true;
            TextureSets = new List<DefinedTextureSet>();
            foreach (DefinedTextureSet s in source.TextureSets) TextureSets.Add(s.Copy());
            thingsfilters = new List<ThingsFilter>();
            foreach (ThingsFilter f in source.thingsfilters) thingsfilters.Add(new ThingsFilter(f));
            EditModes = new Dictionary<string, bool>(source.EditModes);
        }

        //mxd. This checks if given map name can cause problems
        internal bool ValidateMapName(string name)
        {
            // Get the map lump names
            IDictionary maplumpnames = Configuration.ReadSetting("maplumpnames", new Hashtable());

            // Check if given map name overlaps with maplumpnames defined for this game configuration
            foreach (DictionaryEntry ml in maplumpnames)
            {
                // Ignore the map header (it will not be found because the name is different)
                string lumpname = ml.Key.ToString().ToUpperInvariant();
                if (lumpname.Contains(name)) return false;
            }

            return true;
        }

        #endregion
    }
}
