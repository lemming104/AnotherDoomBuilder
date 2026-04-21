
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
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Plugins;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder
{
    public static class General
    {
        #region ================== API Declarations and Mono compatibility

#if MONO_WINFORMS
		public static void ApplyMonoListViewFix(System.Windows.Forms.ListView listview)
		{
			if (listview.View == System.Windows.Forms.View.List)
			{
				listview.View = System.Windows.Forms.View.SmallIcon;
			}
		}
		
		public static void ApplyDataGridViewFix(System.Windows.Forms.DataGridView gridview)
		{
			if (gridview.RowsDefaultCellStyle != null && gridview.RowsDefaultCellStyle.Padding != new System.Windows.Forms.Padding(0,0,0,0))
			{
				gridview.RowsDefaultCellStyle.Padding = new System.Windows.Forms.Padding(0,0,0,0);
			}
		}
#else
        public static void ApplyMonoListViewFix(System.Windows.Forms.ListView listview) { }
        public static void ApplyDataGridViewFix(System.Windows.Forms.DataGridView gridview) { }
#endif

#if NO_WIN32

	internal static void InvokeUIActions(MainForm mainform)
    {
		// This implementation really should work universally, but it seemed to hang sometimes on Windows.
		// Let's hope the mono implementation of Winforms works better.
		mainform.Invoke(new System.Action(() => { mainform.ProcessQueuedUIActions(); }));
	}

	internal static bool MessageBeep(MessageBeepType type)
	{
		System.Media.SystemSounds.Beep.Play();
		return true;
	}

	internal static bool LockWindowUpdate(IntPtr hwnd)
	{
		// This can be safely ignored. It is a performance/flicker optimization. It might not even be needed on Windows anymore.
		return true;
	}

	internal unsafe static void ZeroPixels(PixelColor* pixels, int size)
	{
		var transparent = new PixelColor(0,0,0,0);
		for (int i = 0; i < size; i++)
			pixels[i] = transparent;
	}

	internal static void SetComboBoxItemHeight(ComboBox combobox, int height)
	{
		// Only used by FieldsEditorControl. Not sure what its purpose is, might only be visual adjustment that isn't strictly needed?
	}

#else
        [DllImport("user32.dll")]
        internal static extern bool LockWindowUpdate(IntPtr hwnd);

        [DllImport("kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        static extern void ZeroMemory(IntPtr dest, int size);

        internal unsafe static void ZeroPixels(PixelColor* pixels, int size) { ZeroMemory(new IntPtr(pixels), size * sizeof(PixelColor)); }

        [DllImport("user32.dll", EntryPoint = "SendMessage", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        static extern int SendMessage(IntPtr hwnd, uint Msg, IntPtr wParam, IntPtr lParam);

        internal static void SetComboBoxItemHeight(ComboBox combobox, int height)
        {
            SendMessage(combobox.Handle, General.CB_SETITEMHEIGHT, new IntPtr(-1), new IntPtr(height));
        }

        [DllImport("user32.dll", EntryPoint = "PostMessage", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        static extern int PostMessage(IntPtr hwnd, uint Msg, IntPtr wParam, IntPtr lParam);

        internal static void InvokeUIActions(MainForm mainform)
        {
            PostMessage(mainform.Handle, General.WM_UIACTION, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MessageBeep(MessageBeepType type);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string longpath, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortpath, uint buffersize);
#endif

        #endregion

        #region ================== Constants

        // SendMessage API
        internal const int WM_USER = 0x400;
        internal const int WM_UIACTION = WM_USER + 1;
        internal const int WM_SYSCOMMAND = 0x112;
        internal const int WM_MOUSEHWHEEL = 0x020E; // [ZZ]
        internal const int WM_MOUSEWHEEL = 0x20A;
        internal const int SC_KEYMENU = 0xF100;
        internal const int CB_SETITEMHEIGHT = 0x153;
        //internal const int CB_SHOWDROPDOWN = 0x14F;
        //internal const int EM_GETSCROLLPOS = WM_USER + 221;
        //internal const int EM_SETSCROLLPOS = WM_USER + 222;
        //internal const int SB_HORZ = 0;
        //internal const int SB_VERT = 1;
        //internal const int SB_CTL = 2;
        //internal const int SIF_RANGE = 0x1;
        //internal const int SIF_PAGE = 0x2;
        //internal const int SIF_POS = 0x4;
        //internal const int SIF_DISABLENOSCROLL = 0x8;
        //internal const int SIF_TRACKPOS = 0x16;
        //internal const int SIF_ALL = SIF_RANGE + SIF_PAGE + SIF_POS + SIF_TRACKPOS;

        // Files and Folders
        private const string LEGACY_SETTINGS_FILE = "GZBuilder.cfg"; // To make transision from GZDB* easier
        private const string SETTINGS_FILE = "UDBuilder.cfg";
        private const string DEFAULT_SETTINGS_FILE = "UDBuilder.default.cfg"; //mxd
        private const string SETTINGS_DIR = "Doom Builder";
        private const string LOG_FILE = "UDBuilder.log";
        private const string GAME_CONFIGS_DIR = "Configurations";
        private const string COMPILERS_DIR = "Compilers";
        private const string PLUGINS_DIR = "Plugins";
        private const string SCRIPTS_DIR = "Scripting";
        private const string SCREENSHOTS_DIR = "Screenshots"; //mxd
        private const string SNIPPETS_DIR = "Snippets"; //mxd
        private const string MAP_RESTORE_DIR = "Restore"; //mxd
        private const string SPRITES_DIR = "Sprites";
        private const string TEXTURES_DIR = "Textures"; //mxd
        private const string HELP_FILE = "Refmanual.chm";

        #endregion

        #region ================== Variables

        // Files and Folders
        private static string scriptspath;

        // Main objects

        //private static Mutex appmutex;

        // Configurations

        // States

        // Command line arguments
        private static string[] cmdargs;
        private static DataLocationList autoloadresources;
        private static bool portablemode; //mxd

        //misc
        private static readonly Random random = new Random(); //mxd

        // Toasts

        // Autosaving

        #endregion

        #region ================== Properties

        public static Assembly ThisAssembly { get; private set; }
        public static string AppPath { get; private set; }
        public static string TempPath { get; private set; }
        public static string ConfigsPath { get; private set; }
        internal static string SettingsPath { get; private set; } //mxd
        internal static string MapRestorePath { get; private set; } //mxd
        internal static string LogFile { get; private set; } //mxd
        public static string CompilersPath { get; private set; }
        public static string PluginsPath { get; private set; }
        public static string SpritesPath { get; private set; }
        internal static string TexturesPath { get; private set; } //mxd
        public static string SnippetsPath { get; private set; } //mxd
        public static string DefaultScreenshotsPath { get; private set; } //mxd
        public static ICollection<string> CommandArgs { get { return Array.AsReadOnly(cmdargs); } }
        internal static MainForm MainWindow { get; private set; }
        public static IMainForm Interface { get { return MainWindow; } }
        public static ProgramConfiguration Settings { get; private set; }
        public static ColorCollection Colors { get; private set; }
        internal static List<ConfigurationInfo> Configs { get; private set; }
        internal static List<NodebuilderInfo> Nodebuilders { get; private set; }
        internal static List<CompilerInfo> Compilers { get; private set; }
        internal static Dictionary<string, ScriptConfiguration> ScriptConfigs { get; private set; }
        internal static Dictionary<string, ScriptConfiguration> CompiledScriptConfigs { get; private set; } //mxd
        public static MapManager Map { get; private set; }
        public static ActionManager Actions { get; private set; }
        public static HintsManager Hints { get; private set; } //mxd
        internal static PluginManager Plugins { get; private set; }
        public static bool DebugBuild { get; private set; }
        public static TypesManager Types { get; private set; }
        public static string AutoLoadFile { get; private set; }
        public static string AutoLoadMap { get; private set; }
        public static string AutoLoadConfig { get; private set; }
        public static string AutoLoadScriptConfig { get; private set; }
        public static bool AutoLoadStrictPatches { get; private set; }
        public static DataLocationList AutoLoadResources { get { return new DataLocationList(autoloadresources); } }
        public static bool DelayMainWindow { get; private set; }
        public static bool NoSettings { get; private set; }
        public static bool DebugRenderDevice { get; private set; }
        public static EditingManager Editing { get; private set; }
        public static ErrorLogger ErrorLogger { get; private set; }
        public static string CommitHash { get; private set; } //mxd
        public static ToastManager ToastManager { get; private set; }
        internal static AutoSaver AutoSaver { get; private set; }

        #endregion

        #region ================== Configurations

        /// <summary>
        /// Checks if a given game configuration file exists.
        /// </summary>
        /// <param name="filename">The file name of the game configuration file.</param>
        /// <returns>true if the game configuration exists, false if it doesn't</returns>
        internal static bool ConfigurationInfoExist(string filename) => Configs.Any(ci => string.Compare(Path.GetFileNameWithoutExtension(ci.Filename), Path.GetFileNameWithoutExtension(filename), true) == 0);

        // This returns the game configuration info by filename
        internal static ConfigurationInfo GetConfigurationInfo(string filename)
        {
            // Go for all config infos
            foreach (ConfigurationInfo ci in Configs)
            {
                // Check if filename matches
                if (string.Compare(Path.GetFileNameWithoutExtension(ci.Filename),
                                  Path.GetFileNameWithoutExtension(filename), true) == 0)
                {
                    // Return this info
                    return ci;
                }
            }

            // None found
            return null;
        }

        // This loads and returns a game configuration
        private static Configuration LoadGameConfiguration(string filename)
        {
            // Make the full filepathname
            string filepathname = Path.Combine(ConfigsPath, filename);

            // Load configuration
            try
            {
                // Try loading the configuration
                Configuration cfg = new Configuration(filepathname, true);

                // Check for erors
                if (cfg.ErrorResult)
                {
                    // Error in configuration
                    ErrorLogger.Add(ErrorType.Error, "Unable to load the game configuration file \"" + filename + "\". " +
                                                     "Error in file \"" + cfg.ErrorFile + "\" near line " + cfg.ErrorLine + ": " + cfg.ErrorDescription);
                    return null;
                }
                // Check if this is a Doom Builder 2 config
                if (cfg.ReadSetting("type", "") != "Doom Builder 2 Game Configuration")
                {
                    // Old configuration
                    ErrorLogger.Add(ErrorType.Error, "Unable to load the game configuration file \"" + filename + "\". " +
                                                     "This configuration is not a Doom Builder 2 game configuration.");
                    return null;
                }

                // Return config
                return cfg;
            }
            catch (Exception e)
            {
                // Unable to load configuration
                ErrorLogger.Add(ErrorType.Error, "Unable to load the game configuration file \"" + filename + "\". " + e.GetType().Name + ": " + e.Message);
                General.WriteLog(e.StackTrace);
                return null;
            }
        }

        // This loads all game configurations
        private static void LoadAllGameConfigurations()
        {
            // Display status
            MainWindow.DisplayStatus(StatusType.Busy, "Loading game configurations...");

            // Make array
            Configs = new List<ConfigurationInfo>();

            // Go for all cfg files in the configurations directory
            string[] filenames = Directory.GetFiles(ConfigsPath, "*.cfg", SearchOption.TopDirectoryOnly);

            foreach (string filepath in filenames)
            {
                // Check if it can be loaded
                Configuration cfg = LoadGameConfiguration(Path.GetFileName(filepath));
                if (cfg != null)
                {
                    string fullfilename = Path.GetFileName(filepath);
                    ConfigurationInfo cfginfo = new ConfigurationInfo(cfg, fullfilename);

                    // Add to lists
                    General.WriteLogLine("Registered game configuration \"" + cfginfo.Name + "\" from \"" + fullfilename + "\"");
                    Configs.Add(cfginfo);
                }
            }

            // Sort the configs
            Configs.Sort();
        }

        // This loads all nodebuilder configurations
        private static void LoadAllNodebuilderConfigurations()
        {
            // Display status
            MainWindow.DisplayStatus(StatusType.Busy, "Loading nodebuilder configurations...");

            // Make array
            Nodebuilders = new List<NodebuilderInfo>();

            // Go for all cfg files in the compilers directory
            string[] filenames = Directory.GetFiles(CompilersPath, "*.cfg", SearchOption.AllDirectories);
            foreach (string filepath in filenames)
            {
                try
                {
                    // Try loading the configuration
                    Configuration cfg = new Configuration(filepath, true);

                    // Check for erors
                    if (cfg.ErrorResult)
                    {
                        // Error in configuration
                        ErrorLogger.Add(ErrorType.Error, "Unable to load the compiler configuration file \"" + Path.GetFileName(filepath) + "\". " +
                                                         "Error in file \"" + cfg.ErrorFile + "\" near line " + cfg.ErrorLine + ": " + cfg.ErrorDescription);
                    }
                    else
                    {
                        // Get structures
                        IDictionary builderslist = cfg.ReadSetting("nodebuilders", new Hashtable());
                        foreach (DictionaryEntry de in builderslist)
                        {
                            // Check if this is a structure
                            if (de.Value is IDictionary)
                            {
                                try
                                {
                                    // Make nodebuilder info
                                    Nodebuilders.Add(new NodebuilderInfo(Path.GetFileName(filepath), de.Key.ToString(), cfg));
                                }
                                catch (Exception e)
                                {
                                    // Unable to load configuration
                                    ErrorLogger.Add(ErrorType.Error, "Unable to load the nodebuilder configuration \"" + de.Key + "\" from \"" + Path.GetFileName(filepath) + "\". Error: " + e.Message);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Unable to load configuration
                    ErrorLogger.Add(ErrorType.Error, "Unable to load the compiler configuration file \"" + Path.GetFileName(filepath) + "\".");
                }
            }

            // Sort the list
            Nodebuilders.Sort();
        }

        // This loads all script configurations
        private static void LoadAllScriptConfigurations()
        {
            // Display status
            MainWindow.DisplayStatus(StatusType.Busy, "Loading script configurations...");

            // Make collection
            ScriptConfigs = new Dictionary<string, ScriptConfiguration>(StringComparer.Ordinal);
            CompiledScriptConfigs = new Dictionary<string, ScriptConfiguration>(StringComparer.Ordinal); //mxd

            // Go for all cfg files in the scripts directory
            string[] filenames = Directory.GetFiles(scriptspath, "*.cfg", SearchOption.TopDirectoryOnly);
            foreach (string filepath in filenames)
            {
                try
                {
                    // Try loading the configuration
                    Configuration cfg = new Configuration(filepath, true);

                    // Check for erors
                    if (cfg.ErrorResult)
                    {
                        // Error in configuration
                        ErrorLogger.Add(ErrorType.Error, "Unable to load the script configuration file \"" + Path.GetFileName(filepath) + "\". " +
                                                        "Error in file \"" + cfg.ErrorFile + "\" near line " + cfg.ErrorLine + ": " + cfg.ErrorDescription);
                    }
                    else
                    {
                        try
                        {
                            // Make script configuration
                            ScriptConfiguration scfg = new ScriptConfiguration(cfg);
                            string filename = Path.GetFileName(filepath);
                            ScriptConfigs.Add(filename.ToLowerInvariant(), scfg);

                            //mxd. Store acc compilers in a separate dictionary
                            if (scfg.ScriptType == ScriptType.ACS)
                                CompiledScriptConfigs.Add(filename.ToLowerInvariant(), scfg);
                        }
                        catch (Exception e)
                        {
                            // Unable to load configuration
                            ErrorLogger.Add(ErrorType.Error, "Unable to load the script configuration \"" + Path.GetFileName(filepath) + "\". Error: " + e.Message);
                        }
                    }
                }
                catch (Exception e)
                {
                    // Unable to load configuration
                    ErrorLogger.Add(ErrorType.Error, "Unable to load the script configuration file \"" + Path.GetFileName(filepath) + "\". Error: " + e.Message);
                    General.WriteLogLine(e.StackTrace);
                }
            }
        }

        // This loads all compiler configurations
        private static void LoadAllCompilerConfigurations()
        {
            Dictionary<string, CompilerInfo> addedcompilers = new Dictionary<string, CompilerInfo>(StringComparer.Ordinal);

            // Display status
            MainWindow.DisplayStatus(StatusType.Busy, "Loading compiler configurations...");

            // Make array
            Compilers = new List<CompilerInfo>();

            // Go for all cfg files in the compilers directory
            string[] filenames = Directory.GetFiles(CompilersPath, "*.cfg", SearchOption.AllDirectories);
            foreach (string filepath in filenames)
            {
                try
                {
                    // Try loading the configuration
                    Configuration cfg = new Configuration(filepath, true);

                    // Check for erors
                    if (cfg.ErrorResult)
                    {
                        // Error in configuration
                        ErrorLogger.Add(ErrorType.Error, "Unable to load the compiler configuration file \"" + Path.GetFileName(filepath) + "\". " +
                                                         "Error in file \"" + cfg.ErrorFile + "\" near line " + cfg.ErrorLine + ": " + cfg.ErrorDescription);
                    }
                    else
                    {
                        // Get structures
                        IDictionary compilerslist = cfg.ReadSetting("compilers", new Hashtable());
                        foreach (DictionaryEntry de in compilerslist)
                        {
                            // Check if this is a structure
                            if (de.Value is IDictionary)
                            {
                                // Make compiler info
                                CompilerInfo info = new CompilerInfo(Path.GetFileName(filepath), de.Key.ToString(), Path.GetDirectoryName(filepath), cfg);
                                if (!addedcompilers.ContainsKey(info.Name))
                                {
                                    Compilers.Add(info);
                                    addedcompilers.Add(info.Name, info);
                                }
                                else
                                {
                                    ErrorLogger.Add(ErrorType.Error, "Compiler \"" + info.Name + "\" is defined more than once. The first definition in " + addedcompilers[info.Name].FileName + " will be used.");
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    // Unable to load configuration
                    ErrorLogger.Add(ErrorType.Error, "Unable to load the compiler configuration file \"" + Path.GetFileName(filepath) + "\". " + e.GetType().Name + ": " + e.Message);
                    General.WriteLogLine(e.StackTrace);
                }
            }
        }

        // This returns a nodebuilder by name
        internal static NodebuilderInfo GetNodebuilderByName(string name)
        {
            // Go for all nodebuilders
            foreach (NodebuilderInfo n in Nodebuilders)
            {
                // Name matches?
                if (n.Name == name) return n;
            }

            // Cannot find that nodebuilder
            return null;
        }

        /// <summary>
        /// Saves the program's configuration
        /// </summary>
        internal static void SaveSettings()
        {
            // Save settings configuration
            if (!General.NoSettings)
                General.Settings.Save(Path.Combine(SettingsPath, SETTINGS_FILE));
        }

        /// <summary>
        /// Saves the game configuration settings, like engine, resources etc.
        /// </summary>
        internal static void SaveGameSettings()
        {
            // Save game configuration settings
            if (Configs != null) foreach (ConfigurationInfo ci in Configs) ci.SaveSettings();
        }

        #endregion

        #region ================== Startup

        // Main program entry
        [STAThread]
        internal static void Main(string[] args)
        {
            // Determine states
#if DEBUG
            DebugBuild = true;
#else
				debugbuild = false;
				//mxd. Custom exception dialog.
				AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
				Application.ThreadException += Application_ThreadException;
#endif

            // Enable OS visual styles
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false); //mxd
                                                                  //Application.DoEvents();		// This must be here to work around a .NET bug

            //mxd. Set CultureInfo
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            // biwa. If the default culture for threads is not set it'll screw with the culture
            // in the FileSystemWatcher thread, which can result in incorrect string outputs
            // See: https://github.com/jewalky/UltimateDoomBuilder/issues/858
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            // Set current thread name
            Thread.CurrentThread.Name = "Main Application";

            // Application is running
            //appmutex = new Mutex(false, "gzdoombuilder"); //"doombuilder2"

            // Get a reference to this assembly
            ThisAssembly = Assembly.GetExecutingAssembly();

            // Find application path
            AppPath = Path.GetDirectoryName(Application.ExecutablePath); //mxd. What was the point of using Uri here (other than to prevent lauching from a shared folder)?

            // Parse command-line arguments
            ParseCommandLineArgs(args);

            // Setup directories
            TempPath = Path.GetTempPath();
            SettingsPath = portablemode ? AppPath : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SETTINGS_DIR); //mxd
            MapRestorePath = Path.Combine(SettingsPath, MAP_RESTORE_DIR);
            ConfigsPath = Path.Combine(AppPath, GAME_CONFIGS_DIR);
            CompilersPath = Path.Combine(AppPath, COMPILERS_DIR);
            PluginsPath = Path.Combine(AppPath, PLUGINS_DIR);
            scriptspath = Path.Combine(AppPath, SCRIPTS_DIR);
            SnippetsPath = Path.Combine(AppPath, SNIPPETS_DIR); //mxd
            DefaultScreenshotsPath = Path.Combine(AppPath, SCREENSHOTS_DIR).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar); //mxd
            SpritesPath = Path.Combine(AppPath, SPRITES_DIR);
            TexturesPath = Path.Combine(AppPath, TEXTURES_DIR); //mxd
            LogFile = Path.Combine(SettingsPath, LOG_FILE);

            // Make program settings directory if missing
            if (!portablemode && !Directory.Exists(SettingsPath)) Directory.CreateDirectory(SettingsPath);

            //mxd. Get git commit hash
            var hashes = (AssemblyHashAttribute[])ThisAssembly.GetCustomAttributes(typeof(AssemblyHashAttribute), false);
            if (hashes.Length == 1)
            {
                CommitHash = hashes[0].CommitHash;
            }
            else
            {
                WriteLogLine("Unable to determine commit hash. Missing AssemblyHashAttribute?");
                CommitHash = "0000000";
            }

            // Remove the previous log file and start logging
            if (File.Exists(LogFile)) File.Delete(LogFile);
            string platform = Environment.Is64BitProcess ? "x64" : "x86";
            General.WriteLogLine("Ultimate Doom Builder R" + ThisAssembly.GetName().Version.Revision + " (" + platform + ", " + CommitHash + ") startup"); //mxd
            General.WriteLogLine("Application path:        \"" + AppPath + "\"");
            General.WriteLogLine("Temporary path:          \"" + TempPath + "\"");
            General.WriteLogLine("Local settings path:     \"" + SettingsPath + "\"");
            General.WriteLogLine("Command-line arguments:  \"" + string.Join(" ", args) + "\""); //mxd

            // Load configuration
            General.WriteLogLine("Loading program configuration...");
            Settings = new ProgramConfiguration();
            string defaultsettingsfile = Path.Combine(AppPath, DEFAULT_SETTINGS_FILE);
            string usersettingsfile = NoSettings ? defaultsettingsfile : Path.Combine(SettingsPath, SETTINGS_FILE);
            string legacysettingsfile = NoSettings ? String.Empty : Path.Combine(SettingsPath, LEGACY_SETTINGS_FILE);

            if (Settings.Load(usersettingsfile, defaultsettingsfile, legacysettingsfile))
            {
                // Create error logger
                ErrorLogger = new ErrorLogger();

                // Create action manager
                Actions = new ActionManager();

                // Bind static methods to actions
                General.Actions.BindMethods(typeof(General));

                //mxd. Create hints manager
                Hints = new HintsManager();

                // Initialize static classes
                MapSet.Initialize();

                // Create main window
                General.WriteLogLine("Loading main interface window...");
                MainWindow = new MainForm();
                MainWindow.SetupInterface();
                MainWindow.UpdateInterface();
                MainWindow.UpdateThingsFilters();

                if (!DelayMainWindow)
                {
                    // Show main window
                    General.WriteLogLine("Showing main interface window...");
                    MainWindow.Show();
                    MainWindow.Update();
                }

                // Create the toast manager after the main windows, but before plugins are loaded,
                // since the plugins can register toasts. Also register toasts for the core
                ToastManager = new ToastManager(MainWindow.Display);
                RegisterToasts();

                // Load plugin manager
                General.WriteLogLine("Loading plugins...");
                Plugins = new PluginManager();
                Plugins.LoadAllPlugins();

                // Register toasts from actions. This has to be done after all plugins are loaded
                ToastManager.RegisterActions();
                ToastManager.LoadSettings(Settings.Config);

                // Load game configurations
                General.WriteLogLine("Loading game configurations...");
                LoadAllGameConfigurations();

                // Create editing modes
                General.WriteLogLine("Creating editing modes manager...");
                Editing = new EditingManager();

                // Now that all settings have been combined (core & plugins) apply the defaults
                General.WriteLogLine("Applying configuration settings...");
                Actions.ApplyDefaultShortcutKeys();
                MainWindow.ApplyShortcutKeys();
                foreach (ConfigurationInfo info in Configs) info.ApplyDefaults(null);

                // Load compiler configurations
                General.WriteLogLine("Loading compiler configurations...");
                LoadAllCompilerConfigurations();

                // Load nodebuilder configurations
                General.WriteLogLine("Loading nodebuilder configurations...");
                LoadAllNodebuilderConfigurations();

                // Load script configurations
                General.WriteLogLine("Loading script configurations...");
                LoadAllScriptConfigurations();

                // Load color settings
                General.WriteLogLine("Loading color settings...");
                Colors = new ColorCollection(Settings.Config);

                // Create types manager
                General.WriteLogLine("Creating types manager...");
                Types = new TypesManager();

                // Do auto map loading when window is delayed
                if (DelayMainWindow) MainWindow.PerformAutoMapLoading();

                // All done
                General.WriteLogLine("Startup done");
                MainWindow.DisplayReady();

                // Show any errors if preferred
                if (ErrorLogger.IsErrorAdded)
                {
                    MainWindow.DisplayStatus(StatusType.Warning, "There were errors during program startup!");
                    if (!DelayMainWindow && General.Settings.ShowErrorsWindow) MainWindow.ShowErrors();
                }

                //mxd. Check enabled game configuration
                bool noneenabled = true;
                for (int i = 0; i < Configs.Count; i++)
                {
                    if (Configs[i].Enabled)
                    {
                        noneenabled = false;
                        break;
                    }
                }

                if (noneenabled)
                {
                    if (MessageBox.Show("No game configurations are currently enabled.\nPlease enable at least one game configuration", "Warning", MessageBoxButtons.OK) == DialogResult.OK)
                        MainWindow.ShowConfiguration();
                }

                //mxd. Check backup files
                if (Directory.Exists(MapRestorePath))
                {
                    foreach (string backup in Directory.GetFiles(MapRestorePath, "*.restore"))
                    {
                        // Remove if created more than a month ago
                        if ((DateTime.Now - File.GetLastWriteTime(backup)).TotalDays > 30)
                        {
                            File.Delete(backup);
                            WriteLogLine("Removed \"" + backup + "\" map backup.");
                        }
                    }
                }

                //mxd. Check for updates?
#if !NO_UPDATER
                if (General.Settings.CheckForUpdates) UpdateChecker.PerformCheck(false);
#endif

                // Prepare autosaving
                AutoSaver = new AutoSaver();

                // Run application from the main window
                Application.Run(MainWindow);
            }
            else
            {
                // Terminate
                Terminate(false);
            }
        }

        private static void RegisterToasts()
        {
            ToastManager.RegisterToast("resourcewarningsanderrors", "Resource warnings and errors", "When there are errors or warning while (re)loading the resources");
            ToastManager.RegisterToast("autosave", "Autosave", "Notifications related to autosaving");
        }

        // This parses the command line arguments
        private static void ParseCommandLineArgs(string[] args)
        {
            autoloadresources = new DataLocationList();

            // Keep a copy
            cmdargs = args;

            // Make a queue so we can parse the values from left to right
            Queue<string> argslist = new Queue<string>(args);

            // Parse list
            while (argslist.Count > 0)
            {
                // Get next arg
                string curarg = argslist.Dequeue();

                // Delay window?
                if (string.Compare(curarg, "-DELAYWINDOW", true) == 0)
                {
                    // Delay showing the main window
                    DelayMainWindow = true;
                }
                // No settings?
                else if (string.Compare(curarg, "-NOSETTINGS", true) == 0)
                {
                    // Don't load or save program settings
                    NoSettings = true;
                }
                // Map name info?
                else if (string.Compare(curarg, "-MAP", true) == 0)
                {
                    // Store next arg as map name information
                    AutoLoadMap = argslist.Dequeue()?.ToUpperInvariant();
                }
                // Config name info?
                else if ((string.Compare(curarg, "-CFG", true) == 0) ||
                        (string.Compare(curarg, "-CONFIG", true) == 0))
                {
                    // Store next arg as config filename information
                    AutoLoadConfig = argslist.Dequeue();
                }
                // Script config? (picks the compiler configuration to use)
                else if (string.Compare(curarg, "-SCRIPTCONFIG", true) == 0)
                {
                    // Store next arg as the script configuration name, being the configuration's file name.
                    AutoLoadScriptConfig = argslist.Dequeue().ToLowerInvariant();
                }
                // Strict patches rules?
                else if (string.Compare(curarg, "-STRICTPATCHES", true) == 0)
                {
                    AutoLoadStrictPatches = true;
                }
                //mxd. Portable mode?
                else if (string.Compare(curarg, "-PORTABLE", true) == 0)
                {
                    // Can we write stuff to apppath?
                    try
                    {
                        WindowsIdentity identity = WindowsIdentity.GetCurrent();
                        if (identity != null)
                        {
                            WindowsPrincipal principal = new WindowsPrincipal(identity);
                            DirectorySecurity security = Directory.GetAccessControl(AppPath);
                            AuthorizationRuleCollection authrules = security.GetAccessRules(true, true, typeof(SecurityIdentifier));

                            foreach (FileSystemAccessRule accessrule in authrules)
                            {
                                SecurityIdentifier id = accessrule.IdentityReference as SecurityIdentifier;
                                if (id == null || !principal.IsInRole(id)) continue;
                                if ((FileSystemRights.WriteData & accessrule.FileSystemRights) != FileSystemRights.WriteData) continue;

                                if (accessrule.AccessControlType == AccessControlType.Allow)
                                {
                                    portablemode = true;
                                }
                                else if (accessrule.AccessControlType == AccessControlType.Deny)
                                {
                                    //Deny usually overrides any Allow
                                    portablemode = false;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception) { }

                    // Warn the user?
                    if (!portablemode) ShowWarningMessage("Failed to enable portable mode.\nMake sure you have write premission for \"" + AppPath + "\" directory.", MessageBoxButtons.OK);
                }
                // Resource?
                else if (string.Compare(curarg, "-RESOURCE", true) == 0)
                {
                    DataLocation dl = new DataLocation();

                    // Parse resource type
                    string resourcetype = argslist.Dequeue();
                    if (string.Compare(resourcetype, "WAD", true) == 0)
                        dl.type = DataLocation.RESOURCE_WAD;
                    else if (string.Compare(resourcetype, "DIR", true) == 0)
                        dl.type = DataLocation.RESOURCE_DIRECTORY;
                    else if (string.Compare(resourcetype, "PK3", true) == 0)
                        dl.type = DataLocation.RESOURCE_PK3;
                    else
                    {
                        General.WriteLogLine("Unexpected resource type \"" + resourcetype + "\" in program parameters. Expected \"wad\", \"dir\" or \"pk3\".");
                        break;
                    }

                    // We continue parsing args until an existing filename is found
                    // all other arguments must be one of the optional keywords.
                    while (string.IsNullOrEmpty(dl.location))
                    {
                        curarg = argslist.Dequeue();

                        if ((string.Compare(curarg, "ROOTTEXTURES", true) == 0) &&
                           (dl.type == DataLocation.RESOURCE_DIRECTORY))
                        {
                            // Load images in the root directory of the resource as textures
                            dl.option1 = true;
                        }
                        else if ((string.Compare(curarg, "ROOTFLATS", true) == 0) &&
                                (dl.type == DataLocation.RESOURCE_DIRECTORY))
                        {
                            // Load images in the root directory of the resource as flats
                            dl.option2 = true;
                        }
                        else if ((string.Compare(curarg, "STRICTPATCHES", true) == 0) &&
                                (dl.type == DataLocation.RESOURCE_WAD))
                        {
                            // Use strict rules for patches
                            dl.option1 = true;
                        }
                        else if (string.Compare(curarg, "NOTEST", true) == 0)
                        {
                            // Exclude this resource from testing parameters
                            dl.notfortesting = true;
                        }
                        else
                        {
                            // This must be an existing file, or it is an invalid argument
                            if (dl.type == DataLocation.RESOURCE_DIRECTORY)
                            {
                                if (Directory.Exists(curarg))
                                    dl.location = curarg;
                            }
                            else
                            {
                                if (File.Exists(curarg))
                                    dl.location = curarg;
                            }

                            if (string.IsNullOrEmpty(dl.location))
                            {
                                General.WriteLogLine("Unexpected argument \"" + curarg + "\" in program parameters. Expected a valid resource option or a resource filename.");
                                break;
                            }
                        }
                    }

                    // Add resource to list
                    if (!string.IsNullOrEmpty(dl.location))
                        autoloadresources.Add(dl);
                }
                else if (string.Compare(curarg, "-DEBUGRENDERDEVICE", true) == 0)
                {
                    DebugRenderDevice = true;
                }
                // Every other arg
                else
                {
                    // No command to load file yet?
                    if (AutoLoadFile == null)
                    {
                        // Check if this is a file we can load
                        if (File.Exists(curarg))
                        {
                            // Load this file!
                            AutoLoadFile = curarg.Trim();
                        }
                        else
                        {
                            // Note in the log that we cannot find this file
                            General.WriteLogLine("Cannot find the specified file \"" + curarg + "\"");
                        }
                    }
                }
            }
        }

        // This cancels automatic map loading
        internal static void CancelAutoMapLoad()
        {
            AutoLoadFile = null;
        }

        #endregion

        #region ================== Terminate

        // This is for plugins to use
        public static void Exit(bool properexit)
        {
            // Plugin wants to exit nicely?
            if (properexit)
            {
                // Close dialog forms first
                while ((Form.ActiveForm != MainWindow) && (Form.ActiveForm != null))
                    Form.ActiveForm.Close();

                // Close main window
                MainWindow.Close();
            }
            else
            {
                // Terminate, no questions asked
                Terminate(true);
            }
        }

        // This terminates the program
        internal static void Terminate(bool properexit)
        {
            // Terminate properly?
            if (properexit)
            {
                General.WriteLogLine("Termination requested");

                // Unbind static methods from actions
                General.Actions.UnbindMethods(typeof(General));

                // Save colors
                if (Colors != null) Colors.SaveColors(Settings.Config);

                // Save action controls
                Actions.SaveSettings();

                // Save game settings
                SaveGameSettings();

                // Save program configuration
                SaveSettings();

                // Clean up
                if (Map != null) { Map.Dispose(); Map = null; }
                if (Editing != null) { Editing.Dispose(); Editing = null; }
                if (Plugins != null) { Plugins.Dispose(); Plugins = null; }
                if (MainWindow != null) { MainWindow.Dispose(); MainWindow = null; }
                if (Actions != null) { Actions.Dispose(); Actions = null; }
                if (Types != null) { Types.Dispose(); Types = null; }

                // Application ends here and now
                General.WriteLogLine("Termination done");
                Application.Exit();
            }
            else
            {
                // Just end now
                General.WriteLogLine("Immediate program termination");
                Application.Exit();
            }

            // Die.
            Process.GetCurrentProcess().Kill();
        }

        #endregion

        #region ================== Management

        // This creates a new map
        [BeginAction("newmap")]
        internal static void NewMap()
        {
            MapOptions newoptions = new MapOptions();

            // Cancel volatile mode, if any
            Editing.DisengageVolatileMode();

            // Ask the user to save changes (if any)
            if (AskSaveMap())
            {
                // Open map options dialog
                MapOptionsForm optionswindow = new MapOptionsForm(newoptions, true);
                if (optionswindow.ShowDialog(MainWindow) == DialogResult.OK)
                {
                    // Display status
                    MainWindow.DisplayStatus(StatusType.Busy, "Creating new map...");
                    Cursor.Current = Cursors.WaitCursor;

                    // Clear the display
                    MainWindow.ClearDisplay();
                    MainWindow.RemoveHintsDocker(); //mxd

                    // Trash the current map, if any
                    if (Map != null) Map.Dispose();

                    // Let the plugins know
                    Plugins.OnMapNewBegin();

                    // Clear old errors (mxd)
                    ErrorLogger.Clear();

                    // Create map manager with given options
                    Map = new MapManager();
                    if (Map.InitializeNewMap(newoptions))
                    {
                        Settings.FindDefaultDrawSettings(); //mxd

                        // Let the plugins know
                        Plugins.OnMapNewEnd();

                        // All done
                        MainWindow.SetupInterface();
                        MainWindow.RedrawDisplay();
                        MainWindow.UpdateThingsFilters();
                        MainWindow.UpdateLinedefColorPresets(); //mxd
                        MainWindow.UpdateInterface();
                        MainWindow.AddHintsDocker(); //mxd
                        MainWindow.UpdateGZDoomPanel(); //mxd
                        MainWindow.HideInfo(); //mxd
                    }
                    else
                    {
                        // Unable to create map manager
                        Map.Dispose();
                        Map = null;

                        // Show splash logo on display
                        MainWindow.ShowSplashDisplay();
                    }

                    if (ErrorLogger.IsErrorAdded)
                    {
                        // Show any errors if preferred
                        MainWindow.DisplayStatus(StatusType.Warning, "There were errors during loading!");
                        if (!DelayMainWindow && Settings.ShowErrorsWindow) MainWindow.ShowErrors();
                    }
                    else
                        MainWindow.DisplayReady();

                    //mxd. Also reset the clock...
                    MainWindow.ResetClock();

                    Cursor.Current = Cursors.Default;
                }
            }
        }

        // This closes the current map
        [BeginAction("closemap")]
        internal static void ActionCloseMap() { CloseMap(); }
        internal static bool CloseMap()
        {
            // Cancel volatile mode, if any
            Editing.DisengageVolatileMode();

            // Ask the user to save changes (if any)
            if (AskSaveMap())
            {
                // Display status
                MainWindow.DisplayStatus(StatusType.Busy, "Closing map...");
                WriteLogLine("Unloading map...");
                Cursor.Current = Cursors.WaitCursor;

                // Trash the current map
                if (Map != null) Map.Dispose();
                Map = null;

                // Clear errors
                ErrorLogger.Clear();

                //mxd. Clear Console
#if DEBUG
                DebugConsole.Clear();
#endif

                // Show splash logo on display
                MainWindow.ShowSplashDisplay();

                // Done
                Cursor.Current = Cursors.Default;
                Editing.UpdateCurrentEditModes();
                MainWindow.SetupInterface();
                MainWindow.RedrawDisplay();
                MainWindow.HideInfo();
                MainWindow.UpdateThingsFilters();
                //mxd
                MainWindow.UpdateLinedefColorPresets();
                MainWindow.RemoveHintsDocker();
                MainWindow.UpdateGZDoomPanel();
                MainWindow.UpdateInterface();
                MainWindow.DisplayReady();
                WriteLogLine("Map unload done");
                return true;
            }
            else
            {
                // User cancelled
                return false;
            }
        }

        // This loads a map from file
        [BeginAction("openmap")]
        internal static void OpenMap()
        {
            // Cancel volatile mode, if any
            Editing.DisengageVolatileMode();

            // Open map file dialog
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Title = "Open Map";

#if NO_WIN32
			// No easy way to have case-insesitivity for non-Windows platforms
			openfile.Filter = "Doom WAD Files (*.wad)|*.wad;*.Wad;*.wAd;*.WAd;*.waD;*.WaD;*.wAD;*.WAD";
#else
            openfile.Filter = "Doom WAD Files (*.wad)|*.wad";
#endif

            if (!string.IsNullOrEmpty(Settings.LastUsedMapFolder) && Directory.Exists(Settings.LastUsedMapFolder)) //mxd
            {
                openfile.RestoreDirectory = true;
                openfile.InitialDirectory = Settings.LastUsedMapFolder;
            }
            openfile.AddExtension = false;
            openfile.CheckFileExists = true;
            openfile.Multiselect = false;
            openfile.ValidateNames = true;
            if (openfile.ShowDialog(MainWindow) == DialogResult.OK)
            {
                // Update main window
                MainWindow.Update();

                // Open map file
                OpenMapFile(openfile.FileName, null);
            }

            openfile.Dispose();
        }

        //mxd. This loads a different map from same wad file without reloading resources
        [BeginAction("openmapincurrentwad")]
        internal static void OpenMapInCurrentWad()
        {
            if (Map == null || string.IsNullOrEmpty(Map.FilePathName) || !File.Exists(Map.FilePathName))
            {
                Interface.DisplayStatus(StatusType.Warning, "Unable to open map from current WAD!");
                return;
            }

            // Cancel volatile mode, if any
            Editing.DisengageVolatileMode();

            // Ask the user to save changes (if any)
            if (!AskSaveMap()) return;

            // Open map options dialog
            ChangeMapForm changemapwindow = new ChangeMapForm(Map.FilePathName, Map.Options);
            if (changemapwindow.ShowDialog(MainWindow) != DialogResult.OK) return;

            // Display status
            MainWindow.DisplayStatus(StatusType.Busy, "Switching to map \"" + changemapwindow.Options.CurrentName + "\"...");
            WriteLogLine("Switching to map \"" + changemapwindow.Options.CurrentName + "\"...");

            Cursor.Current = Cursors.WaitCursor;

            // Let the plugins know
            Plugins.OnMapCloseBegin();

            // Clear the display
            MainWindow.ClearDisplay();
            MainWindow.RemoveHintsDocker(); //mxd

            //mxd. Close the script editor
            Map.CloseScriptEditor(false);

            // Let the plugins know
            Plugins.OnMapCloseEnd();
            Plugins.OnMapOpenBegin();

            // Clear old errors
            ErrorLogger.Clear();

            if (!Map.InitializeSwitchMap(changemapwindow.Options)) return;

            // Clear undo history
            Map.UndoRedo.ClearAllUndos();
            Map.UndoRedo.ClearAllRedos();

            Settings.FindDefaultDrawSettings(); //mxd

            // Let the plugins know
            Plugins.OnMapOpenEnd();

            // All done
            MainWindow.SetupInterface();
            MainWindow.RedrawDisplay();
            MainWindow.UpdateThingsFilters();
            MainWindow.UpdateLinedefColorPresets(); //mxd
            MainWindow.UpdateInterface();
            MainWindow.HideInfo();
            MainWindow.AddHintsDocker(); //mxd
            MainWindow.UpdateGZDoomPanel(); //mxd

            if (ErrorLogger.IsErrorAdded)
            {
                // Show any errors if preferred
                MainWindow.DisplayStatus(StatusType.Warning, "There were errors during loading!");
                if (!DelayMainWindow && Settings.ShowErrorsWindow)
                    MainWindow.ShowErrors();
            }
            else
            {
                MainWindow.DisplayReady();
            }

            Cursor.Current = Cursors.Default;
        }

        // This opens the specified file
        internal static void OpenMapFile(string filename, MapOptions options)
        {
            // Cancel volatile mode, if any
            Editing.DisengageVolatileMode();

            // Ask the user to save changes (if any)
            if (AskSaveMap())
            {
                // Open map options dialog
                OpenMapOptionsForm openmapwindow = options != null ? new OpenMapOptionsForm(filename, options) : new OpenMapOptionsForm(filename);

                if (openmapwindow.ShowDialog(MainWindow) == DialogResult.OK)
                    OpenMapFileWithOptions(filename, openmapwindow.Options);
            }
        }

        // This opens the specified file without dialog
        internal static void OpenMapFileWithOptions(string filename, MapOptions options)
        {
            // Display status
            MainWindow.DisplayStatus(StatusType.Busy, "Opening map file...");
            Cursor.Current = Cursors.WaitCursor;

            // Clear the display
            MainWindow.ClearDisplay();
            MainWindow.RemoveHintsDocker(); //mxd

            // Trash the current map, if any
            if (Map != null) Map.Dispose();

            // Let the plugins know
            Plugins.OnMapOpenBegin();

            // mxd. Clear old errors
            ErrorLogger.Clear();

            // Create map manager with given options
            Map = new MapManager();
            if (Map.InitializeOpenMap(filename, options))
            {
                // Add recent file
                MainWindow.AddRecentFile(filename);

                //mxd
                MainWindow.UpdateGZDoomPanel();
                Settings.LastUsedMapFolder = Path.GetDirectoryName(filename);
                Settings.FindDefaultDrawSettings();

                // Let the plugins know
                Plugins.OnMapOpenEnd();

                // All done
                MainWindow.SetupInterface();
                MainWindow.UpdateThingsFilters();
                MainWindow.UpdateLinedefColorPresets(); //mxd
                MainWindow.UpdateInterface();
                MainWindow.HideInfo();
                MainWindow.AddHintsDocker(); //mxd

                //mxd. Center map in screen or on stored coordinates. Done here to avoid the view jerking around when updating the interface.
                ClassicMode mode = Editing.Mode as ClassicMode;
                if (mode != null)
                {
                    if (options != null && options.ViewPosition.IsFinite() && !float.IsNaN(options.ViewScale))
                        mode.CenterOnCoordinates(options.ViewPosition, options.ViewScale);
                    else
                        mode.CenterInScreen();
                }

                MainWindow.RedrawDisplay();
            }
            else
            {
                // Unable to create map manager
                Map.Dispose();
                Map = null;

                // Show splash logo on display
                MainWindow.ShowSplashDisplay();
            }

            if (ErrorLogger.IsErrorAdded)
            {
                // Show any errors if preferred
                MainWindow.DisplayStatus(StatusType.Warning, "There were errors during loading!");
                if (!DelayMainWindow && Settings.ShowErrorsWindow) MainWindow.ShowErrors();
            }
            else
                MainWindow.DisplayReady();

            Cursor.Current = Cursors.Default;
        }

        // This saves the current map
        // Returns tre when saved, false when cancelled or failed
        [BeginAction("savemap")]
        internal static void ActionSaveMap() { SaveMap(); }
        internal static bool SaveMap()
        {
            if (Map == null) return false;
            bool result = false;

            // Cancel volatile mode, if any
            Editing.DisengageVolatileMode();

            // Check if a wad file is known
            if (string.IsNullOrEmpty(Map.FilePathName))
            {
                // Call to SaveMapAs
                result = SaveMapAs();
            }
            else
            {
                //mxd. Do we need to save the map?
                if (!Map.MapSaveRequired(Map.FilePathName, SavePurpose.Normal))
                {
                    // Still save settings file
                    result = Map.SaveSettingsFile(Map.FilePathName);

                    // Display status
                    MainWindow.DisplayStatus(StatusType.Info, "Map is up to date. Updated map settings file.");

                    // All done
                    MainWindow.UpdateInterface();
                    return result;
                }

                // Display status
                MainWindow.DisplayStatus(StatusType.Busy, "Saving map file...");
                Cursor.Current = Cursors.WaitCursor;

                // Set this to false so we can see if errors are added
                ErrorLogger.IsErrorAdded = false;

                // Save the map
                Plugins.OnMapSaveBegin(SavePurpose.Normal);
                if (Map.SaveMap(Map.FilePathName, SavePurpose.Normal))
                {
                    // Add recent file
                    MainWindow.AddRecentFile(Map.FilePathName);
                    result = true;
                }
                Plugins.OnMapSaveEnd(SavePurpose.Normal);

                // All done
                MainWindow.UpdateInterface();

                if (ErrorLogger.IsErrorAdded)
                {
                    // Show any errors if preferred
                    MainWindow.DisplayStatus(StatusType.Warning, "There were errors during saving!");
                    if (!DelayMainWindow && Settings.ShowErrorsWindow) MainWindow.ShowErrors();
                }
                else if (result)
                {
                    MainWindow.DisplayStatus(StatusType.Info, "Map saved in " + Map.FileTitle + ".");
                }
                else
                {
                    MainWindow.DisplayStatus(StatusType.Info, "Map saving cancelled."); //mxd
                }

                Cursor.Current = Cursors.Default;
            }

            //mxd. Also reset the clock...
            MainWindow.ResetClock();

            return result;
        }


        // This saves the current map as a different file
        // Returns tre when saved, false when cancelled or failed
        [BeginAction("savemapas")]
        internal static void ActionSaveMapAs() { SaveMapAs(); }
        internal static bool SaveMapAs()
        {
            if (Map == null) return false;
            bool result = false;

            // Cancel volatile mode, if any
            Editing.DisengageVolatileMode();

            // Show save as dialog
            SaveFileDialog savefile = new SaveFileDialog();
#if NO_WIN32
			// No easy way to have case-insesitivity for non-Windows platforms
			savefile.Filter = "Doom WAD Files (*.wad)|*.wad;*.Wad;*.wAd;*.WAd;*.waD;*.WaD;*.wAD;*.WAD";
#else
            savefile.Filter = "Doom WAD Files (*.wad)|*.wad";
#endif
            savefile.Title = "Save Map As";
            savefile.AddExtension = true;
            savefile.CheckPathExists = true;
            savefile.OverwritePrompt = true;
            savefile.ValidateNames = true;
            savefile.FileName = Map.FileTitle; //mxd
            if (Map.FilePathName.Length > 0) //mxd
            {
                savefile.RestoreDirectory = true;
                savefile.InitialDirectory = Path.GetDirectoryName(Map.FilePathName);
            }

            if (savefile.ShowDialog(MainWindow) == DialogResult.OK)
            {
                // Check if we're saving to the same file as the original.
                // Because some muppets use Save As even when saving to the same file.
                string currentfilename = (Map.FilePathName.Length > 0) ? Path.GetFullPath(Map.FilePathName).ToLowerInvariant() : "";
                string savefilename = Path.GetFullPath(savefile.FileName).ToLowerInvariant();
                if (currentfilename == savefilename)
                {
                    SaveMap();
                }
                else
                {
                    // Display status
                    MainWindow.DisplayStatus(StatusType.Busy, "Saving map file...");
                    Cursor.Current = Cursors.WaitCursor;

                    // Set this to false so we can see if errors are added
                    ErrorLogger.IsErrorAdded = false;

                    // Save the map
                    Plugins.OnMapSaveBegin(SavePurpose.AsNewFile);
                    if (Map.SaveMap(savefile.FileName, SavePurpose.AsNewFile))
                    {
                        // Add recent file
                        MainWindow.AddRecentFile(Map.FilePathName);
                        Settings.LastUsedMapFolder = Path.GetDirectoryName(Map.FilePathName); //mxd
                        result = true;
                    }
                    Plugins.OnMapSaveEnd(SavePurpose.AsNewFile);

                    // All done
                    MainWindow.UpdateInterface();

                    if (ErrorLogger.IsErrorAdded)
                    {
                        // Show any errors if preferred
                        MainWindow.DisplayStatus(StatusType.Warning, "There were errors during saving!");
                        if (!DelayMainWindow && Settings.ShowErrorsWindow) MainWindow.ShowErrors();
                    }
                    else if (result)
                    {
                        MainWindow.DisplayStatus(StatusType.Info, "Map saved in " + Map.FileTitle + ".");
                    }
                    else
                    {
                        MainWindow.DisplayStatus(StatusType.Info, "Map saving cancelled."); //mxd
                    }

                    Cursor.Current = Cursors.Default;
                }
            }

            savefile.Dispose();

            //mxd. Also reset the clock...
            MainWindow.ResetClock();

            // Make sure to give focus to the display, otherwise it will become unresponsive to keyboard input
            // when executed from visual mode
            Interface.FocusDisplay();

            return result;
        }


        // This saves the current map as a different file
        // Returns tre when saved, false when cancelled or failed
        [BeginAction("savemapinto")]
        internal static void ActionSaveMapInto() { SaveMapInto(); }
        internal static bool SaveMapInto()
        {
            if (Map == null) return false;
            bool result = false;

            // Cancel volatile mode, if any
            Editing.DisengageVolatileMode();

            // Show save as dialog
            SaveFileDialog savefile = new SaveFileDialog();
#if NO_WIN32
			// No easy way to have case-insesitivity for non-Windows platforms
			savefile.Filter = "Doom WAD Files (*.wad)|*.wad;*.Wad;*.wAd;*.WAd;*.waD;*.WaD;*.wAD;*.WAD";
#else
            savefile.Filter = "Doom WAD Files (*.wad)|*.wad";
#endif
            savefile.Title = "Save Map Into";
            savefile.AddExtension = true;
            savefile.CheckPathExists = true;
            savefile.OverwritePrompt = false;
            savefile.ValidateNames = true;
            if (savefile.ShowDialog(MainWindow) == DialogResult.OK)
            {
                // Display status
                MainWindow.DisplayStatus(StatusType.Busy, "Saving map file...");
                Cursor.Current = Cursors.WaitCursor;

                // Set this to false so we can see if errors are added
                ErrorLogger.IsErrorAdded = false;

                // Save the map
                Plugins.OnMapSaveBegin(SavePurpose.IntoFile);
                if (Map.SaveMap(savefile.FileName, SavePurpose.IntoFile))
                {
                    // Add recent file
                    MainWindow.AddRecentFile(Map.FilePathName);
                    result = true;
                }
                Plugins.OnMapSaveEnd(SavePurpose.IntoFile);

                // All done
                MainWindow.UpdateInterface();

                if (ErrorLogger.IsErrorAdded)
                {
                    // Show any errors if preferred
                    MainWindow.DisplayStatus(StatusType.Warning, "There were errors during saving!");
                    if (!DelayMainWindow && Settings.ShowErrorsWindow) MainWindow.ShowErrors();
                }
                else if (result)
                {
                    MainWindow.DisplayStatus(StatusType.Info, "Map saved in " + Map.FileTitle + ".");
                }
                else
                {
                    MainWindow.DisplayStatus(StatusType.Info, "Map saving cancelled."); //mxd
                }

                Cursor.Current = Cursors.Default;
            }

            savefile.Dispose();

            //mxd. Also reset the clock...
            MainWindow.ResetClock();

            return result;
        }

        // This asks to save the map if needed
        // Returns false when action was cancelled
        internal static bool AskSaveMap()
        {
            if (Map == null)
                return true;

            bool returnvalue;

            // Map open and not saved?
            if (Map.IsChanged)
            {
                // Ask to save changes
                DialogResult result = MessageBox.Show(MainWindow, "Do you want to save changes to " + Map.FileTitle + " (" + Map.Options.CurrentName + ")?", Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // Save map
                    if (SaveMap())
                    {
                        // Ask to save changes to scripts
                        returnvalue = Map.AskSaveScriptChanges();
                    }
                    else
                    {
                        // Failed to save map
                        returnvalue = false;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    // Abort
                    returnvalue = false;
                }
                else
                {
                    // Ask to save changes to scripts
                    returnvalue = Map.AskSaveScriptChanges();
                }
            }
            else
            {
                // Ask to save changes to scripts
                returnvalue = Map.AskSaveScriptChanges();
            }

            // Make sure to give focus to the display, otherwise it will become unresponsive to keyboard input
            // when executed from visual mode
            Interface.FocusDisplay();

            return returnvalue;
        }

        #endregion

        #region ================== Debug

        // This shows a major failure
        public static void Fail(string message)
        {
            WriteLogLine("FAIL: " + message);
#if DEBUG
            Debug.Fail(message);
#else
			//mxd. Lets notify the user about our Epic Failure before crashing...
			ShowErrorMessage(message, MessageBoxButtons.OK);
#endif
            Terminate(false);
        }

        // This outputs log information
        public static void WriteLogLine(string line)
        {
            lock (random)
            {
#if DEBUG
                // Output to consoles
                Console.WriteLine(line);
                DebugConsole.WriteLine(DebugMessageType.LOG, line); //mxd
#endif
                // Write to log file
                try { File.AppendAllText(LogFile, line + Environment.NewLine); }
                catch (Exception) { }
            }
        }

        // This outputs log information
        public static void WriteLog(string text)
        {
            lock (random)
            {
#if DEBUG
                // Output to consoles
                Console.Write(text);
                DebugConsole.Write(DebugMessageType.LOG, text);
#endif

                // Write to log file
                try { File.AppendAllText(LogFile, text); }
                catch (Exception) { }
            }
        }

        #endregion

        #region ================== Tools

        // This swaps two pointers
        public static void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }

        // This calculates the bits needed for a number
        public static int BitsForInt(int v)
        {
            int[] LOGTABLE = new[] {
              0, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3,
              4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
              5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
              5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
              6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
              6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
              6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
              6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
              7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7 };

            int r;  // r will be lg(v)
            int t, tt;

            if (Int2Bool(tt = v >> 16))
            {
                r = Int2Bool(t = tt >> 8) ? 24 + LOGTABLE[t] : 16 + LOGTABLE[tt];
            }
            else
            {
                r = Int2Bool(t = v >> 8) ? 8 + LOGTABLE[t] : LOGTABLE[v];
            }

            return r;
        }

        // This clamps a value
        public static float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(min, value), max);
        }

        // This clamps a value
        public static double Clamp(double value, double min, double max)
        {
            return Math.Min(Math.Max(min, value), max);
        }

        // This clamps a value
        public static int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(min, value), max);
        }

        // This clamps a value
        public static byte Clamp(byte value, byte min, byte max)
        {
            return Math.Min(Math.Max(min, value), max);
        }

        //mxd. This clamps angle between 0 and 359
        public static int ClampAngle(int angle)
        {
            angle %= 360;
            if (angle < 0) angle += 360;
            return angle;
        }

        //mxd. This clamps angle between 0 and 359
        public static float ClampAngle(float angle)
        {
            angle %= 360;
            if (angle < 0) angle += 360;
            return angle;
        }

        // This clamps angle between 0 and 359
        public static double ClampAngle(double angle)
        {
            angle %= 360;
            if (angle < 0) angle += 360;
            return angle;
        }

        //mxd
        public static int Random(int min, int max)
        {
            return random.Next(min, max + 1); //because max is never rolled
        }

        //mxd
        public static float Random(float min, float max)
        {
            return (float)Math.Round(min + ((max - min) * random.NextDouble()), 2);
        }

        public static double Random(double min, double max)
        {
            return Math.Round(min + ((max - min) * random.NextDouble()), 2);
        }

        // This returns an element from a collection by index
        public static T GetByIndex<T>(ICollection<T> collection, int index)
        {
            IEnumerator<T> e = collection.GetEnumerator();
            for (int i = -1; i < index; i++) e.MoveNext();
            return e.Current;
        }

        // This returns the next power of 2
        /*public static int NextPowerOf2(int v)
		{
			int p = 0;

			// Continue increasing until higher than v
			while(Math.Pow(2, p) < v) p++;

			// Return power
			return (int)Math.Pow(2, p);
		}*/

        //mxd. This returns the next power of 2. Taken from http://bits.stephan-brumme.com/roundUpToNextPowerOfTwo.html
        public static int NextPowerOf2(int x)
        {
            x--;
            x |= x >> 1;  // handle  2 bit numbers
            x |= x >> 2;  // handle  4 bit numbers
            x |= x >> 4;  // handle  8 bit numbers
            x |= x >> 8;  // handle 16 bit numbers
            x |= x >> 16; // handle 32 bit numbers
            x++;

            return x;
        }

        // Convert bool to integer
        internal static int Bool2Int(bool v)
        {
            return v ? 1 : 0;
        }

        // Convert integer to bool
        internal static bool Int2Bool(int v)
        {
            return v != 0;
        }

        // This shows a message and logs the message
        public static DialogResult ShowErrorMessage(string message, MessageBoxButtons buttons)
        {
            return ShowErrorMessage(message, buttons, true);
        }

        // This shows a message and logs the message
        public static DialogResult ShowErrorMessage(string message, MessageBoxButtons buttons, bool log)
        {
            //mxd. Log the message?
            if (log) WriteLogLine(message);

            // Use normal cursor
            Cursor oldcursor = Cursor.Current;
            Cursor.Current = Cursors.Default;

            // Show message
            IWin32Window window = null;
            if ((Form.ActiveForm != null) && Form.ActiveForm.Visible) window = Form.ActiveForm;
            DialogResult result = MessageBox.Show(window, message, Application.ProductName, buttons, MessageBoxIcon.Error);

            // Restore old cursor
            Cursor.Current = oldcursor;

            // Return result
            return result;
        }

        // This shows a message and logs the message
        public static DialogResult ShowWarningMessage(string message, MessageBoxButtons buttons)
        {
            return ShowWarningMessage(message, buttons, MessageBoxDefaultButton.Button1, true);
        }

        // This shows a message and logs the message
        public static DialogResult ShowWarningMessage(string message, MessageBoxButtons buttons, MessageBoxDefaultButton defaultbutton)
        {
            return ShowWarningMessage(message, buttons, defaultbutton, true);
        }

        // This shows a message and logs the message
        public static DialogResult ShowWarningMessage(string message, MessageBoxButtons buttons, MessageBoxDefaultButton defaultbutton, bool log)
        {
            //mxd. Log the message?
            if (log) WriteLogLine(message);

            // Use normal cursor
            Cursor oldcursor = Cursor.Current;
            Cursor.Current = Cursors.Default;

            // Show message
            IWin32Window window = null;
            if ((Form.ActiveForm != null) && Form.ActiveForm.Visible) window = Form.ActiveForm;
            DialogResult result = MessageBox.Show(window, message, Application.ProductName, buttons, MessageBoxIcon.Warning, defaultbutton);

            // Restore old cursor
            Cursor.Current = oldcursor;

            // Return result
            return result;
        }

        // This shows the reference manual
        public static void ShowHelp(string pagefile)
        {
            ShowHelp(pagefile, HELP_FILE);
        }

        // This shows the reference manual
        public static void ShowHelp(string pagefile, string chmfile)
        {
            // Check if the file can be found in the root
            string filepathname = Path.Combine(AppPath, chmfile);
            if (!File.Exists(filepathname))
            {
                // Check if the file exists in the plugins directory
                filepathname = Path.Combine(PluginsPath, chmfile);
                if (!File.Exists(filepathname))
                {
                    // Fail
                    WriteLogLine("ERROR: Can't find the help file \"" + chmfile + "\"");
                    return;
                }
            }

            // Show help file
            Help.ShowHelp(MainWindow, filepathname, HelpNavigator.Topic, pagefile);
        }

        // This returns a unique temp filename
        internal static string MakeTempFilename(string tempdir)
        {
            return MakeTempFilename(tempdir, "tmp");
        }

        // This returns a unique temp filename
        internal static string MakeTempFilename(string tempdir, string extension)
        {
            string filename;
            const string chars = "abcdefghijklmnopqrstuvwxyz1234567890";

            do
            {
                // Generate a filename
                filename = "";
                for (int i = 0; i < 8; i++) filename += chars[Random(0, chars.Length - 1)];
                filename = Path.Combine(tempdir, filename + "." + extension);
            }
            // Continue while file is not unique
            while (File.Exists(filename) || Directory.Exists(filename));

            // Return the filename
            return filename;
        }

        // This returns a unique temp directory name
        internal static string MakeTempDirname()
        {
            string dirname;
            const string chars = "abcdefghijklmnopqrstuvwxyz1234567890";

            do
            {
                // Generate a filename
                dirname = "";
                for (int i = 0; i < 8; i++) dirname += chars[Random(0, chars.Length - 1)];
                dirname = Path.Combine(TempPath, dirname);
            }
            // Continue while file is not unique
            while (File.Exists(dirname) || Directory.Exists(dirname));

            // Return the filename
            return dirname;
        }

        // This shows an image in a panel either zoomed or centered depending on size
        public static void DisplayZoomedImage(Panel panel, Image image)
        {
            // Image not null?
            if (image != null)
            {
                // Set the image
                panel.BackgroundImage = image;

                // Display zoomed
                panel.BackgroundImageLayout = ImageLayout.Zoom;
            }
        }

        // This calculates the new rectangle when one is scaled into another keeping aspect ratio
        public static RectangleF MakeZoomedRect(Size source, RectangleF target)
        {
            return MakeZoomedRect(new SizeF(source.Width, source.Height), target);
        }

        // This calculates the new rectangle when one is scaled into another keeping aspect ratio
        public static RectangleF MakeZoomedRect(Size source, Rectangle target)
        {
            return MakeZoomedRect(new SizeF(source.Width, source.Height),
                                  new RectangleF(target.Left, target.Top, target.Width, target.Height));
        }

        // This calculates the new rectangle when one is scaled into another keeping aspect ratio
        public static RectangleF MakeZoomedRect(SizeF source, RectangleF target)
        {
            float scale;

            // Image fits?
            if ((source.Width <= target.Width) && (source.Height <= target.Height))
            {
                // Just center
                scale = 1.0f;
            }
            // Image is wider than tall?
            else if ((source.Width - target.Width) > (source.Height - target.Height))
            {
                // Scale down by width
                scale = target.Width / source.Width;
            }
            else
            {
                // Scale down by height
                scale = target.Height / source.Height;
            }

            // Return centered and scaled
            return new RectangleF(target.Left + ((target.Width - (source.Width * scale)) * 0.5f),
                                  target.Top + ((target.Height - (source.Height * scale)) * 0.5f),
                                  source.Width * scale, source.Height * scale);
        }

        // This opens a URL in the default browser
        public static void OpenWebsite(string url)
        {
            // [ZZ] note: it may break. no idea why it was done like it was done.
            string url2 = url.ToLowerInvariant();
            if (!url2.StartsWith("http://") && !url2.StartsWith("https://") && !url2.StartsWith("ftp://") && !url2.StartsWith("mailto:"))
                return;
            System.Diagnostics.Process.Start(url);
            /*

			RegistryKey key = null;
			Process p = null;
			string browser;

			try
			{
				// Get the registry key where default browser is stored
				key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

				// Trim off quotes
				browser = key.GetValue(null).ToString().ToLower().Replace("\"", "");

				// String doesnt end in EXE?
				if(!browser.EndsWith("exe"))
				{
					// Get rid of everything after the ".exe"
					browser = browser.Substring(0, browser.LastIndexOf(".exe") + 4);
				}
			}
			finally
			{
				// Clean up
				if(key != null) key.Close();
			}

			try
			{
				// Fork a process
				p = new Process();
				p.StartInfo.FileName = browser;
				p.StartInfo.Arguments = url;
				p.Start();
			}
			catch(Exception) { }

			// Clean up
			if(p != null) p.Dispose();*/
        }

        // This returns the short path name for a file
        public static string GetShortFilePath(string longpath)
        {
#if NO_WIN32
			return longpath;
#else
            const int maxlen = 256;
            StringBuilder shortname = new StringBuilder(maxlen);
            GetShortPathName(longpath, shortname, maxlen);
            return shortname.ToString();
#endif
        }

        public static string GetLinuxFilePath(string longpath)
        {
            string linuxpath;
            linuxpath = longpath.Replace('\\', '/');
            string wineprefix = Environment.GetEnvironmentVariable("WINEPREFIX");

            if (linuxpath.Substring(0, 2) == "C:")
            {
                linuxpath = wineprefix + "/drive_c" + linuxpath.Substring(2);
            }
            else if (linuxpath.Substring(0, 2) == "Z:")
            {
                linuxpath = linuxpath.Substring(2);
            }
            return linuxpath;
        }

        //mxd
        internal static ScriptConfiguration GetScriptConfiguration(ScriptType type)
        {
            if (type == ScriptType.ACS)
            {
                // Return map-defined compiler
                string compiler = !string.IsNullOrEmpty(Map.Options.ScriptCompiler) ? Map.Options.ScriptCompiler : Map.ConfigSettings.DefaultScriptCompiler;
                foreach (KeyValuePair<string, ScriptConfiguration> group in ScriptConfigs)
                {
                    if (group.Key == compiler) return group.Value;
                }
            }
            else
            {
                // Just pick the first one from the list
                foreach (ScriptConfiguration cfg in ScriptConfigs.Values)
                {
                    if (cfg.ScriptType == type) return cfg;
                }
            }

            return null;
        }

        //mxd
        public static bool CheckWritePremissions(string path)
        {
            try
            {
                string testFile = path + "/GZDBWriteTest.tmp";
                if (File.Exists(testFile))
                    File.Delete(testFile);
                FileStream fs = File.OpenWrite(testFile);
                fs.Close();
                File.Delete(testFile);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<Assembly> GetPluginAssemblies()
        {
            return Plugins.GetPluginAssemblies();
        }

        #endregion

        #region ==================  mxd. Uncaught exceptions handling

        // In some cases the program can remain operational after these
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            try
            {
                // Try handling it in user-friendy way...
                ExceptionDialog dlg = new ExceptionDialog(e);
                dlg.Setup();
                if (dlg.ShowDialog() == DialogResult.Cancel) Terminate(false);
            }
            catch
            {
                string exceptionmsg;

                // Try getting exception details...
                try { exceptionmsg = "Fatal Windows Forms error occurred: " + e.Exception.Message + "\n\nStack Trace:\n" + e.Exception.StackTrace; }
                catch (Exception exc) { exceptionmsg = "Failed to get initial exception details: " + exc.Message + "\n\nStack Trace:\n" + exc.StackTrace; }

                // Try logging it...
                try { WriteLogLine(exceptionmsg); } catch { }

                // Try displaying it to the user...
                try { MessageBox.Show(exceptionmsg, "Fatal Windows Forms Error", MessageBoxButtons.OK, MessageBoxIcon.Stop); }
                finally { Process.GetCurrentProcess().Kill(); }
            }
        }

        // These are usually unrecoverable
        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                // Try handling it in user-friendy way...
                ExceptionDialog dlg = new ExceptionDialog(e);
                dlg.Setup();
                if (dlg.ShowDialog() == DialogResult.Cancel) Terminate(false);
            }
            catch
            {
                string exceptionmsg;

                // Try getting exception details...
                try
                {
                    Exception ex = (Exception)e.ExceptionObject;
                    exceptionmsg = "Fatal Non-UI error:\n" + ex.Message + "\n\nStack Trace:\n" + ex.StackTrace;
                }
                catch (Exception exc)
                {
                    exceptionmsg = "Failed to get initial exception details:\n" + exc.Message + "\n\nStack Trace:\n" + exc.StackTrace;
                }

                // Try logging it...
                try { WriteLogLine(exceptionmsg); } catch { }

                // Try displaying it to the user...
                try { MessageBox.Show(exceptionmsg, "Fatal Non-UI Error", MessageBoxButtons.OK, MessageBoxIcon.Stop); }
                finally { Process.GetCurrentProcess().Kill(); }
            }
        }

        #endregion

    }
}

