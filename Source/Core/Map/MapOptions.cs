
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

using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;

#endregion

namespace CodeImp.DoomBuilder.Map
{
    public sealed class MapOptions
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // Map configuration
        private readonly Configuration mapconfig;

        // Game configuration

        // Map header name

        // Strict pathes loading?

        // Additional resources

        //mxd. View settings for opened script files, resources and lumps

        // mxd. Script compiler

        //mxd. Sector drawing options

        //mxd. Sector drawing overrides

        //mxd.

        //mxd. Position and scale

        #endregion

        #region ================== Properties

        internal string ConfigFile { get; set; }
        internal DataLocationList Resources { get; }
        internal bool StrictPatches { get; set; }
        internal Dictionary<string, ScriptDocumentSettings> ScriptDocumentSettings { get; } //mxd
        internal string ScriptCompiler { get; set; } //mxd
        internal string PreviousName { get; set; }
        internal string CurrentName
        {
            get { return LevelName; }

            set
            {
                // Change the name, but keep previous name
                if (LevelName != value)
                {
                    if (string.IsNullOrEmpty(PreviousName)) PreviousName = LevelName;
                    LevelName = value;
                }
            }
        }
        internal bool LevelNameChanged { get { return !string.IsNullOrEmpty(PreviousName) && PreviousName != LevelName; } } //mxd

        public string LevelName { get; private set; }
        public Dictionary<int, string> TagLabels { get; internal set; } //mxd 

        //mxd. Sector drawing options
        public string DefaultTopTexture { get; set; }
        public string DefaultWallTexture { get; set; }
        public string DefaultBottomTexture { get; set; }
        public string DefaultFloorTexture { get; set; }
        public string DefaultCeilingTexture { get; set; }
        public int CustomBrightness { get; set; }
        public int CustomFloorHeight { get; set; }
        public int CustomCeilingHeight { get; set; }

        //mxd. Sector drawing overrides
        public bool OverrideFloorTexture { get; set; }
        public bool OverrideCeilingTexture { get; set; }
        public bool OverrideTopTexture { get; set; }
        public bool OverrideMiddleTexture { get; set; }
        public bool OverrideBottomTexture { get; set; }
        public bool OverrideFloorHeight { get; set; }
        public bool OverrideCeilingHeight { get; set; }
        public bool OverrideBrightness { get; set; }

        //mxd
        public bool UseLongTextureNames { get; set; }

        //mxd. Position and scale
        public Vector2D ViewPosition { get; }
        public float ViewScale { get; }

        public ExternalCommandSettings ReloadResourcePreCommand { get; internal set; }
        public ExternalCommandSettings ReloadResourcePostCommand { get; internal set; }
        public ExternalCommandSettings TestPreCommand { get; internal set; }
        public ExternalCommandSettings TestPostCommand { get; internal set; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal MapOptions()
        {
            // Initialize
            this.PreviousName = "";
            this.LevelName = "";
            this.ConfigFile = "";
            this.StrictPatches = false;
            this.Resources = new DataLocationList();
            this.mapconfig = new Configuration(true);
            this.ScriptDocumentSettings = new Dictionary<string, ScriptDocumentSettings>(StringComparer.OrdinalIgnoreCase); //mxd
            this.ScriptCompiler = ""; //mxd
            this.TagLabels = new Dictionary<int, string>(); //mxd
            this.ViewPosition = new Vector2D(float.NaN, float.NaN); //mxd
            this.ViewScale = float.NaN; //mxd

            ReloadResourcePreCommand = new ExternalCommandSettings();
            ReloadResourcePostCommand = new ExternalCommandSettings();
            TestPreCommand = new ExternalCommandSettings();
            TestPostCommand = new ExternalCommandSettings();

            //mxd. Sector drawing options
            this.CustomBrightness = 196;
            this.CustomCeilingHeight = 128;
        }

        // Constructor to load from Doom Builder Map Settings Configuration
        internal MapOptions(Configuration cfg, string mapname, bool longtexturenamessupported)
        {
            // Initialize
            this.PreviousName = "";
            this.LevelName = mapname;
            this.StrictPatches = General.Int2Bool(cfg.ReadSetting("strictpatches", 0));
            this.ConfigFile = cfg.ReadSetting("gameconfig", "");
            this.Resources = new DataLocationList();
            this.mapconfig = new Configuration(true);
            this.ScriptDocumentSettings = new Dictionary<string, ScriptDocumentSettings>(StringComparer.OrdinalIgnoreCase); //mxd

            // Read map configuration
            this.mapconfig.Root = cfg.ReadSetting("maps." + mapname, new Hashtable());

            //mxd. Read Tag Labels
            this.TagLabels = new Dictionary<int, string>();
            ListDictionary tagLabelsData = (ListDictionary)this.mapconfig.ReadSetting("taglabels", new ListDictionary());

            foreach (DictionaryEntry tagLabelsEntry in tagLabelsData)
            {
                int tag = 0;
                string label = string.Empty;

                foreach (DictionaryEntry entry in (ListDictionary)tagLabelsEntry.Value)
                {
                    switch ((string)entry.Key)
                    {
                        case "tag": tag = (int)entry.Value; break;
                        case "label": label = (string)entry.Value; break;
                    }
                }

                if (tag != 0 && !string.IsNullOrEmpty(label))
                    TagLabels.Add(tag, label);
            }

            //mxd. Script compiler
            ScriptCompiler = this.mapconfig.ReadSetting("scriptcompiler", string.Empty);

            //mxd. Read Sector drawing options
            DefaultFloorTexture = this.mapconfig.ReadSetting("defaultfloortexture", string.Empty);
            DefaultCeilingTexture = this.mapconfig.ReadSetting("defaultceiltexture", string.Empty);
            DefaultTopTexture = this.mapconfig.ReadSetting("defaulttoptexture", string.Empty);
            DefaultWallTexture = this.mapconfig.ReadSetting("defaultwalltexture", string.Empty);
            DefaultBottomTexture = this.mapconfig.ReadSetting("defaultbottomtexture", string.Empty);
            CustomBrightness = General.Clamp(this.mapconfig.ReadSetting("custombrightness", 196), 0, 255);
            CustomFloorHeight = this.mapconfig.ReadSetting("customfloorheight", 0);
            CustomCeilingHeight = this.mapconfig.ReadSetting("customceilheight", 128);

            //mxd. Read Sector drawing overrides
            OverrideFloorTexture = this.mapconfig.ReadSetting("overridefloortexture", false);
            OverrideCeilingTexture = this.mapconfig.ReadSetting("overrideceiltexture", false);
            OverrideTopTexture = this.mapconfig.ReadSetting("overridetoptexture", false);
            OverrideMiddleTexture = this.mapconfig.ReadSetting("overridemiddletexture", false);
            OverrideBottomTexture = this.mapconfig.ReadSetting("overridebottomtexture", false);
            OverrideFloorHeight = this.mapconfig.ReadSetting("overridefloorheight", false);
            OverrideCeilingHeight = this.mapconfig.ReadSetting("overrideceilheight", false);
            OverrideBrightness = this.mapconfig.ReadSetting("overridebrightness", false);

            //mxd
            UseLongTextureNames = longtexturenamessupported && this.mapconfig.ReadSetting("uselongtexturenames", false);

            // Load the pre and post commands
            ReloadResourcePreCommand = new ExternalCommandSettings(mapconfig, "reloadresourceprecommand");
            ReloadResourcePostCommand = new ExternalCommandSettings(mapconfig, "reloadresourcepostcommand");
            TestPreCommand = new ExternalCommandSettings(mapconfig, "testprecommand");
            TestPostCommand = new ExternalCommandSettings(mapconfig, "testpostcommand");

            //mxd. Position and scale
            float vpx = this.mapconfig.ReadSetting("viewpositionx", float.NaN);
            float vpy = this.mapconfig.ReadSetting("viewpositiony", float.NaN);
            if (!float.IsNaN(vpx) && !float.IsNaN(vpy)) ViewPosition = new Vector2D(vpx, vpy);
            ViewScale = this.mapconfig.ReadSetting("viewscale", float.NaN);

            // Resources
            IDictionary reslist = this.mapconfig.ReadSetting("resources", new Hashtable());
            foreach (DictionaryEntry mp in reslist)
            {
                // Item is a structure?
                IDictionary resinfo = mp.Value as IDictionary;
                if (resinfo != null)
                {
                    // Create resource
                    DataLocation res = new DataLocation();

                    // Copy information from Configuration to ResourceLocation
                    if (resinfo.Contains("type") && (resinfo["type"] is int)) res.type = (int)resinfo["type"];
                    if (resinfo.Contains("location") && (resinfo["location"] is string)) res.location = (string)resinfo["location"];
                    if (resinfo.Contains("textures") && (resinfo["textures"] is bool)) res.option1 = (bool)resinfo["textures"];
                    if (resinfo.Contains("flats") && (resinfo["flats"] is bool)) res.option2 = (bool)resinfo["flats"];
                    if (resinfo.Contains("notfortesting") && (resinfo["notfortesting"] is int)) res.notfortesting = Convert.ToBoolean(resinfo["notfortesting"]);

                    // Add resource
                    AddResource(res);
                }
            }

            //mxd. Read script documents settings
            IDictionary sflist = this.mapconfig.ReadSetting("scriptdocuments", new Hashtable());
            foreach (DictionaryEntry mp in sflist)
            {
                // Item is a structure?
                IDictionary scfinfo = mp.Value as IDictionary;
                if (scfinfo != null)
                {
                    ScriptDocumentSettings settings = ReadScriptDocumentSettings(scfinfo);
                    if (!string.IsNullOrEmpty(settings.Filename)) ScriptDocumentSettings[settings.Filename] = settings;
                }
            }
        }

        #endregion

        #region ================== Methods

        // This makes the path prefix for the given assembly
        private static string GetPluginPathPrefix(Assembly asm)
        {
            Plugin p = General.Plugins.FindPluginByAssembly(asm);
            return "plugins." + p.Name.ToLowerInvariant() + ".";
        }

        // ReadPluginSetting
        public string ReadPluginSetting(string setting, string defaultsetting) { return mapconfig.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public int ReadPluginSetting(string setting, int defaultsetting) { return mapconfig.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public float ReadPluginSetting(string setting, float defaultsetting) { return mapconfig.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public double ReadPluginSetting(string setting, double defaultsetting) { return mapconfig.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public short ReadPluginSetting(string setting, short defaultsetting) { return mapconfig.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public long ReadPluginSetting(string setting, long defaultsetting) { return mapconfig.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public bool ReadPluginSetting(string setting, bool defaultsetting) { return mapconfig.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public byte ReadPluginSetting(string setting, byte defaultsetting) { return mapconfig.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }
        public IDictionary ReadPluginSetting(string setting, IDictionary defaultsetting) { return mapconfig.ReadSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, defaultsetting); }

        // ReadPluginSetting with specific plugin
        public string ReadPluginSetting(string pluginname, string setting, string defaultsetting) { return mapconfig.ReadSetting(pluginname.ToLowerInvariant() + "." + setting, defaultsetting); }
        public int ReadPluginSetting(string pluginname, string setting, int defaultsetting) { return mapconfig.ReadSetting(pluginname.ToLowerInvariant() + "." + setting, defaultsetting); }
        public float ReadPluginSetting(string pluginname, string setting, float defaultsetting) { return mapconfig.ReadSetting(pluginname.ToLowerInvariant() + "." + setting, defaultsetting); }
        public double ReadPluginSetting(string pluginname, string setting, double defaultsetting) { return mapconfig.ReadSetting(pluginname.ToLowerInvariant() + "." + setting, defaultsetting); }
        public short ReadPluginSetting(string pluginname, string setting, short defaultsetting) { return mapconfig.ReadSetting(pluginname.ToLowerInvariant() + "." + setting, defaultsetting); }
        public long ReadPluginSetting(string pluginname, string setting, long defaultsetting) { return mapconfig.ReadSetting(pluginname.ToLowerInvariant() + "." + setting, defaultsetting); }
        public bool ReadPluginSetting(string pluginname, string setting, bool defaultsetting) { return mapconfig.ReadSetting(pluginname.ToLowerInvariant() + "." + setting, defaultsetting); }
        public byte ReadPluginSetting(string pluginname, string setting, byte defaultsetting) { return mapconfig.ReadSetting(pluginname.ToLowerInvariant() + "." + setting, defaultsetting); }
        public IDictionary ReadPluginSetting(string pluginname, string setting, IDictionary defaultsetting) { return mapconfig.ReadSetting(pluginname.ToLowerInvariant() + "." + setting, defaultsetting); }

        // WritePluginSetting
        public bool WritePluginSetting(string setting, object settingvalue) { return mapconfig.WriteSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting, settingvalue); }

        // DeletePluginSetting
        public bool DeletePluginSetting(string setting) { return mapconfig.DeleteSetting(GetPluginPathPrefix(Assembly.GetCallingAssembly()) + setting); }

        // This stores the map options in a configuration
        internal void WriteConfiguration(string settingsfile)
        {
            Configuration wadcfg;

            // Write resources to config
            Resources.WriteToConfig(mapconfig, "resources");

            //mxd. Save selection groups
            General.Map.Map.WriteSelectionGroups(mapconfig);

            //mxd. Save Tag Labels
            if (TagLabels.Count > 0)
            {
                ListDictionary tagLabelsData = new ListDictionary();
                int counter = 1;

                foreach (KeyValuePair<int, string> group in TagLabels)
                {
                    ListDictionary data = new ListDictionary();
                    data.Add("tag", group.Key);
                    data.Add("label", group.Value);
                    tagLabelsData.Add("taglabel" + counter, data);
                    counter++;
                }

                mapconfig.WriteSetting("taglabels", tagLabelsData);
            }

            //mxd. Write Sector drawing options
            mapconfig.WriteSetting("defaultfloortexture", DefaultFloorTexture);
            mapconfig.WriteSetting("defaultceiltexture", DefaultCeilingTexture);
            mapconfig.WriteSetting("defaulttoptexture", DefaultTopTexture);
            mapconfig.WriteSetting("defaultwalltexture", DefaultWallTexture);
            mapconfig.WriteSetting("defaultbottomtexture", DefaultBottomTexture);
            mapconfig.WriteSetting("custombrightness", CustomBrightness);
            mapconfig.WriteSetting("customfloorheight", CustomFloorHeight);
            mapconfig.WriteSetting("customceilheight", CustomCeilingHeight);

            //mxd. Write Sector drawing overrides
            mapconfig.WriteSetting("overridefloortexture", OverrideFloorTexture);
            mapconfig.WriteSetting("overrideceiltexture", OverrideCeilingTexture);
            mapconfig.WriteSetting("overridetoptexture", OverrideTopTexture);
            mapconfig.WriteSetting("overridemiddletexture", OverrideMiddleTexture);
            mapconfig.WriteSetting("overridebottomtexture", OverrideBottomTexture);
            mapconfig.WriteSetting("overridefloorheight", OverrideFloorHeight);
            mapconfig.WriteSetting("overrideceilheight", OverrideCeilingHeight);
            mapconfig.WriteSetting("overridebrightness", OverrideBrightness);

            //mxd
            mapconfig.WriteSetting("uselongtexturenames", UseLongTextureNames);

            //mxd. Position and scale
            mapconfig.WriteSetting("viewpositionx", General.Map.Renderer2D.OffsetX);
            mapconfig.WriteSetting("viewpositiony", General.Map.Renderer2D.OffsetY);
            mapconfig.WriteSetting("viewscale", General.Map.Renderer2D.Scale);

            //mxd. Write script compiler
            if (!string.IsNullOrEmpty(ScriptCompiler))
                mapconfig.WriteSetting("scriptcompiler", ScriptCompiler);

            // Write grid settings
            General.Map.Grid.WriteToConfig(mapconfig, "grid");

            //mxd. Write script document settings to config
            int sdcounter = 0;
            mapconfig.DeleteSetting("scriptdocuments");
            foreach (ScriptDocumentSettings settings in ScriptDocumentSettings.Values)
                WriteScriptDocumentSettings(mapconfig, "scriptdocuments.document" + sdcounter++, settings);

            // Write pre and post commands
            ReloadResourcePreCommand.WriteSettings(mapconfig, "reloadresourceprecommand");
            ReloadResourcePostCommand.WriteSettings(mapconfig, "reloadresourcepostcommand");
            TestPreCommand.WriteSettings(mapconfig, "testprecommand");
            TestPostCommand.WriteSettings(mapconfig, "testpostcommand");

            // Load the file or make a new file
            if (File.Exists(settingsfile))
                wadcfg = new Configuration(settingsfile, true);
            else
                wadcfg = new Configuration(true);

            // Write configuration type information
            wadcfg.WriteSetting("type", "Doom Builder Map Settings Configuration");
            wadcfg.WriteSetting("gameconfig", ConfigFile);
            wadcfg.WriteSetting("strictpatches", General.Bool2Int(StrictPatches));

            // Update the settings file with this map configuration
            wadcfg.WriteSetting("maps." + LevelName, mapconfig.Root);

            // Save file
            wadcfg.SaveConfiguration(settingsfile);
        }

        //mxd
        private static ScriptDocumentSettings ReadScriptDocumentSettings(IDictionary scfinfo)
        {
            ScriptDocumentSettings settings = new ScriptDocumentSettings { FoldLevels = new Dictionary<int, HashSet<int>>() };

            // Copy information from Configuration to ScriptDocumentSaveSettings
            if (scfinfo.Contains("filename") && (scfinfo["filename"] is string)) settings.Filename = (string)scfinfo["filename"];
            if (scfinfo.Contains("hash"))
            {
                // Configuration will parse the value as int if it's inside int type bounds.
                if (scfinfo["hash"] is int) settings.Hash = (int)scfinfo["hash"];
                else if (scfinfo["hash"] is long) settings.Hash = (long)scfinfo["hash"];
            }
            if (scfinfo.Contains("resource") && (scfinfo["resource"] is string)) settings.ResourceLocation = (string)scfinfo["resource"];
            if (scfinfo.Contains("tabtype") && (scfinfo["tabtype"] is int)) settings.TabType = (ScriptDocumentTabType)scfinfo["tabtype"];
            if (scfinfo.Contains("scripttype") && (scfinfo["scripttype"] is int)) settings.ScriptType = (ScriptType)scfinfo["scripttype"];
            if (scfinfo.Contains("caretposition") && (scfinfo["caretposition"] is int)) settings.CaretPosition = (int)scfinfo["caretposition"];
            if (scfinfo.Contains("firstvisibleline") && (scfinfo["firstvisibleline"] is int)) settings.FirstVisibleLine = (int)scfinfo["firstvisibleline"];
            if (scfinfo.Contains("activetab") && (scfinfo["activetab"] is bool)) settings.IsActiveTab = (bool)scfinfo["activetab"];
            if (scfinfo.Contains("foldlevels") && (scfinfo["foldlevels"] is string))
            {
                // 1:12,13,14;2:21,43,36
                string foldstr = (string)scfinfo["foldlevels"];

                // Convert string to dictionary
                if (!string.IsNullOrEmpty(foldstr))
                {
                    //TODO: add all kinds of warnings?
                    string[] foldlevels = foldstr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string foldlevel in foldlevels)
                    {
                        // 1:12,13,14
                        string[] parts = foldlevel.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 2) continue;

                        int fold;
                        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out fold)) continue;
                        if (settings.FoldLevels.ContainsKey(fold)) continue;

                        string[] linenumbersstr = parts[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (linenumbersstr.Length == 0) continue;

                        HashSet<int> linenumbers = new HashSet<int>();
                        foreach (string linenumber in linenumbersstr)
                        {
                            int linenum;
                            if (int.TryParse(linenumber, NumberStyles.Integer, CultureInfo.InvariantCulture, out linenum))
                                linenumbers.Add(linenum);
                        }

                        if (linenumbers.Count != linenumbersstr.Length) continue;

                        // Add to collection
                        settings.FoldLevels.Add(fold, new HashSet<int>(linenumbers));
                    }
                }
            }

            return settings;
        }

        //mxd
        private static void WriteScriptDocumentSettings(Configuration mapconfig, string prefix, ScriptDocumentSettings settings)
        {
            // Store data
            ListDictionary data = new ListDictionary();
            data.Add("filename", settings.Filename);
            data.Add("hash", settings.Hash);
            data.Add("resource", settings.ResourceLocation);
            data.Add("tabtype", (int)settings.TabType);
            data.Add("scripttype", (int)settings.ScriptType);
            if (settings.CaretPosition > 0) data.Add("caretposition", settings.CaretPosition);
            if (settings.FirstVisibleLine > 0) data.Add("firstvisibleline", settings.FirstVisibleLine);
            if (settings.IsActiveTab) data.Add("activetab", true);

            // Convert dictionary to string
            List<string> foldlevels = new List<string>();
            foreach (KeyValuePair<int, HashSet<int>> group in settings.FoldLevels)
            {
                List<string> linenums = new List<string>(group.Value.Count);
                foreach (int i in group.Value) linenums.Add(i.ToString());
                foldlevels.Add(group.Key + ":" + string.Join(",", linenums.ToArray()));
            }

            // Add to collection
            if (foldlevels.Count > 0) data.Add("foldlevels", string.Join(";", foldlevels.ToArray()));

            // Write to config
            mapconfig.WriteSetting(prefix, data);
        }

        // This adds a resource location and returns the index where the item was added
        internal int AddResource(DataLocation res)
        {
            // Get a fully qualified path
            res.location = Path.GetFullPath(res.location);

            // Go for all items in the list
            for (int i = 0; i < Resources.Count; i++)
            {
                // Check if location is already added
                if (Path.GetFullPath(Resources[i].location) == res.location)
                {
                    // Update the item in the list
                    Resources[i] = res;
                    return i;
                }
            }

            // Add to list
            Resources.Add(res);
            return Resources.Count - 1;
        }

        /// <summary>
        /// This returns the resource locations as configured.
        /// </summary>
        public DataLocationList GetResources()
        {
            return new DataLocationList(Resources);
        }

        // This clears all reasource
        internal void ClearResources()
        {
            // Clear list
            Resources.Clear();
        }

        // This removes a resource by index
        internal void RemoveResource(int index)
        {
            // Remove the item
            Resources.RemoveAt(index);
        }

        // This copies resources from a list
        internal void CopyResources(DataLocationList fromlist)
        {
            // Clear this list
            Resources.Clear();
            Resources.AddRange(fromlist);
        }

        // This loads the grid settings
        internal void ApplyGridSettings()
        {
            General.Map.Grid.ReadFromConfig(mapconfig, "grid");
        }

        //mxd. This reads stored selection groups from the map configuration
        internal void ReadSelectionGroups()
        {
            General.Map.Map.ReadSelectionGroups(mapconfig);
        }

        // This displays the current map name
        public override string ToString()
        {
            return LevelName;
        }

        // This returns the UDMF field type
        internal int GetUniversalFieldType(string elementname, string fieldname, int defaulttype)
        {
            // Check if the field type is set in the game configuration
            int type = General.Map.Config.ReadSetting("universalfields." + elementname + "." + fieldname + ".type", -1);
            if (type == -1)
            {
                // Read from map configuration
                type = mapconfig.ReadSetting("fieldtypes." + elementname + "." + fieldname, defaulttype);
            }

            return type;
        }

        // This stores the UDMF field type
        internal void SetUniversalFieldType(string elementname, string fieldname, int type)
        {
            // Check if the type of this field is not set in the game configuration
            if (General.Map.Config.ReadSetting("universalfields." + elementname + "." + fieldname + ".type", -1) == -1)
            {
                // Write type to map configuration
                mapconfig.WriteSetting("fieldtypes." + elementname + "." + fieldname, type);
            }
        }

        // This removes all UDMF field types
        internal void ForgetUniversalFieldTypes()
        {
            mapconfig.DeleteSetting("fieldtypes");
        }

        #endregion
    }
}
