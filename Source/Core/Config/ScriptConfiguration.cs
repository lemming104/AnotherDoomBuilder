
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
using ScintillaNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#endregion

namespace CodeImp.DoomBuilder.Config
{
    //mxd
    // WARNING: if you add items here you also *must* add icons to the ScriptEditorPanel control (it's in Core\Controls\Scripting)!
    //          Icons required are for the script file itself, for the close group folder, and open group folder.
    //          The icon must also be placed at the correct position (i.e. not just at the end), so that the order is correct.
    //          Since this is apparently only used for the script editor (where nothing but DIALOG and ACS can be edited anymore) you
    //          you can apparently just use UNKNOWN for the script type in classes derived from ZDTextParser
    public enum ScriptType
    {
        UNKNOWN,
        ACS,
        MODELDEF,
        DECORATE,
        GLDEFS,
        SNDSEQ,
        MAPINFO,
        VOXELDEF,
        TEXTURES,
        ANIMDEFS,
        REVERBS,
        TERRAIN,
        X11R6RGB,
        CVARINFO,
        SNDINFO,
        LOCKDEFS,
        MENUDEF,
        SBARINFO,
        USDF,
        GAMEINFO,
        KEYCONF,
        FONTDEFS,
        ZSCRIPT,
        DECALDEF
    }

    public class ScriptConfiguration : IComparable<ScriptConfiguration>
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // Compiler settings

        // Editor settings

        // Collections
        private readonly Dictionary<string, string> keywords;
        private readonly Dictionary<string, string> lowerkeywords;
        private readonly List<string> keywordkeyssorted; //mxd
        private readonly List<string> constants;
        private readonly Dictionary<string, string> lowerconstants;
        private readonly List<string> properties; //mxd
        private readonly Dictionary<string, string> lowerproperties; //mxd
        private readonly Dictionary<string, string[]> snippets; //mxd
        private readonly HashSet<string> snippetkeyssorted; //mxd

        #endregion

        #region ================== Properties

        // Compiler settings
        public CompilerInfo Compiler { get; }
        public string Parameters { get; }
        public string ResultLump { get; }

        // Editor settings
        public string Description { get; }
        public int CodePage { get; }
        public string[] Extensions { get; }
        public bool CaseSensitive { get; }
        public int InsertCase { get; }
        public Lexer Lexer { get; }
        public string KeywordHelp { get; }
        public string FunctionOpen { get; }
        public string FunctionClose { get; }
        public string CodeBlockOpen { get; } //mxd
        public string CodeBlockClose { get; } //mxd
        public string ArrayOpen { get; } //mxd
        public string ArrayClose { get; } //mxd
        public string ArgumentDelimiter { get; }
        public string Terminator { get; }
        public string ExtraWordCharacters { get; } //mxd
        public ScriptType ScriptType { get; } //mxd

        // Collections
        public ICollection<string> Keywords { get { return keywordkeyssorted; } }
        public ICollection<string> Properties { get { return properties; } } //mxd
        public ICollection<string> Constants { get { return constants; } }
        public ICollection<string> Snippets { get { return snippetkeyssorted; } } //mxd
        public HashSet<char> BraceChars { get; } //mxd

        #endregion

        #region ================== Constructor / Disposer

        // This creates the default script configuration
        // that is used for documents of unknown type
        internal ScriptConfiguration()
        {
            // Initialize
            this.keywords = new Dictionary<string, string>(StringComparer.Ordinal);
            this.constants = new List<string>();
            this.properties = new List<string>(); //mxd
            this.lowerkeywords = new Dictionary<string, string>(StringComparer.Ordinal);
            this.lowerconstants = new Dictionary<string, string>(StringComparer.Ordinal);
            this.lowerproperties = new Dictionary<string, string>(StringComparer.Ordinal); //mxd
            this.keywordkeyssorted = new List<string>(); //mxd
            this.snippets = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase); //mxd
            this.snippetkeyssorted = new HashSet<string>(); //mxd
            this.BraceChars = new HashSet<char>(); //mxd

            // Settings
            Lexer = Lexer.Null;
            CaseSensitive = false;
            CodePage = 65001;
            Parameters = "";
            ResultLump = "";
            InsertCase = 0;
            KeywordHelp = "";
            FunctionOpen = "";
            FunctionClose = "";
            CodeBlockOpen = ""; //mxd
            CodeBlockClose = ""; //mxd
            ArrayOpen = ""; //mxd
            ArrayClose = ""; //mxd
            ArgumentDelimiter = "";
            Terminator = "";
            Description = "Plain text";
            ScriptType = ScriptType.UNKNOWN; //mxd
            ExtraWordCharacters = ""; //mxd
            Extensions = new[] { "txt" };
        }

        // Constructor
        internal ScriptConfiguration(Configuration cfg)
        {
            // Initialize
            this.keywords = new Dictionary<string, string>(StringComparer.Ordinal);
            this.constants = new List<string>();
            this.properties = new List<string>(); //mxd
            this.lowerkeywords = new Dictionary<string, string>(StringComparer.Ordinal);
            this.lowerconstants = new Dictionary<string, string>(StringComparer.Ordinal);
            this.lowerproperties = new Dictionary<string, string>(StringComparer.Ordinal); //mxd
            this.keywordkeyssorted = new List<string>(); //mxd
            this.snippets = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase); //mxd
            this.snippetkeyssorted = new HashSet<string>(); //mxd
            this.BraceChars = new HashSet<char>(); //mxd

            // Read settings
            Description = cfg.ReadSetting("description", "Untitled script");
            CodePage = cfg.ReadSetting("codepage", 0);
            string extensionsstring = cfg.ReadSetting("extensions", "");
            string compilername = cfg.ReadSetting("compiler", "");
            Parameters = cfg.ReadSetting("parameters", "");
            ResultLump = cfg.ReadSetting("resultlump", "");
            CaseSensitive = cfg.ReadSetting("casesensitive", true);
            InsertCase = cfg.ReadSetting("insertcase", 0);
            Lexer = (Lexer)cfg.ReadSetting("lexer", (int)Lexer.Container);
            KeywordHelp = cfg.ReadSetting("keywordhelp", "");
            FunctionOpen = cfg.ReadSetting("functionopen", "");
            FunctionClose = cfg.ReadSetting("functionclose", "");
            CodeBlockOpen = cfg.ReadSetting("codeblockopen", ""); //mxd
            CodeBlockClose = cfg.ReadSetting("codeblockclose", ""); //mxd
            ArrayOpen = cfg.ReadSetting("arrayopen", ""); //mxd
            ArrayClose = cfg.ReadSetting("arrayclose", ""); //mxd
            ArgumentDelimiter = cfg.ReadSetting("argumentdelimiter", "");
            Terminator = cfg.ReadSetting("terminator", "");
            ExtraWordCharacters = cfg.ReadSetting("extrawordchars", ""); //mxd

            //mxd. Get script type...
            string scripttypestr = cfg.ReadSetting("scripttype", string.Empty);
            if (!string.IsNullOrEmpty(scripttypestr))
            {
                List<string> typenames = new List<string>(Enum.GetNames(typeof(ScriptType)));
                int pos = typenames.IndexOf(scripttypestr.ToUpperInvariant());
                if (pos == -1)
                {
                    ScriptType = ScriptType.UNKNOWN;
                    General.ErrorLogger.Add(ErrorType.Warning, "Unknown script type \"" + scripttypestr.ToUpperInvariant() + "\" in \"" + Description + "\" script configuration.");
                }
                else
                {
                    ScriptType = (ScriptType)pos;
                }
            }
            else
            {
                ScriptType = ScriptType.UNKNOWN;
            }

            //mxd. Make braces array
            if (!string.IsNullOrEmpty(FunctionOpen)) BraceChars.Add(FunctionOpen[0]);
            if (!string.IsNullOrEmpty(FunctionClose)) BraceChars.Add(FunctionClose[0]);
            if (!string.IsNullOrEmpty(CodeBlockOpen)) BraceChars.Add(CodeBlockOpen[0]);
            if (!string.IsNullOrEmpty(CodeBlockClose)) BraceChars.Add(CodeBlockClose[0]);
            if (!string.IsNullOrEmpty(ArrayOpen)) BraceChars.Add(ArrayOpen[0]);
            if (!string.IsNullOrEmpty(ArrayClose)) BraceChars.Add(ArrayClose[0]);

            // Make extensions array
            Extensions = extensionsstring.Split(',');
            for (int i = 0; i < Extensions.Length; i++) Extensions[i] = Extensions[i].Trim();

            // Load keywords
            IDictionary dic = cfg.ReadSetting("keywords", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                string keyword = de.Key.ToString();
                if (keywords.ContainsKey(keyword)) //mxd
                {
                    General.ErrorLogger.Add(ErrorType.Warning, "Keyword \"" + keyword + "\" is double defined in \"" + Description + "\" script configuration.");
                    continue;
                }

                keywords[keyword] = de.Value.ToString();
                lowerkeywords[keyword.ToLowerInvariant()] = keyword;
                keywordkeyssorted.Add(keyword); //mxd
            }

            //mxd. Sort keywords lookup
            keywordkeyssorted.Sort();

            //mxd. Load properties
            dic = cfg.ReadSetting("properties", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                string property = de.Key.ToString();
                if (lowerproperties.ContainsValue(property)) //mxd
                {
                    General.ErrorLogger.Add(ErrorType.Warning, "Property \"" + property + "\" is double defined in \"" + Description + "\" script configuration.");
                    continue;
                }

                properties.Add(property);
                lowerproperties[property.ToLowerInvariant()] = property;
            }

            //mxd
            properties.Sort();

            // Load constants
            dic = cfg.ReadSetting("constants", new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                string constant = de.Key.ToString();
                if (lowerconstants.ContainsValue(constant)) //mxd
                {
                    General.ErrorLogger.Add(ErrorType.Warning, "Constant \"" + constant + "\" is double defined in \"" + Description + "\" script configuration.");
                    continue;
                }

                constants.Add(constant);
                lowerconstants[constant.ToLowerInvariant()] = constant;
            }

            //mxd
            constants.Sort();

            //mxd. Load Snippets
            string snippetsdir = cfg.ReadSetting("snippetsdir", "");
            if (!string.IsNullOrEmpty(snippetsdir))
            {
                string snippetspath = Path.Combine(General.SnippetsPath, snippetsdir);
                if (Directory.Exists(snippetspath))
                {
                    string[] files = Directory.GetFiles(snippetspath, "*.txt", SearchOption.TopDirectoryOnly);
                    List<string> sortedkeys = new List<string>();

                    foreach (string file in files)
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        if (string.IsNullOrEmpty(name))
                        {
                            General.ErrorLogger.Add(ErrorType.Warning, "Failed to load snippet \"" + file + "\" for \"" + Description + "\" script configuration.");
                        }
                        else
                        {
                            if (name.Contains(" ")) name = name.Replace(' ', '_');
                            string[] lines = File.ReadAllLines(file);
                            if (lines.Length > 0)
                            {
                                snippets.Add(name, lines);
                                sortedkeys.Add(name);
                            }
                            else
                            {
                                General.ErrorLogger.Add(ErrorType.Warning, "Failed to load snippet \"" + file + "\" for \"" + Description + "\" script configuration: file is empty!");
                            }
                        }
                    }

                    //mxd. Sort snippets lookup
                    sortedkeys.Sort();
                    snippetkeyssorted = new HashSet<string>(sortedkeys, StringComparer.OrdinalIgnoreCase);
                }
            }

            // Compiler specified?
            if (compilername.Length > 0)
            {
                // Find compiler
                foreach (CompilerInfo c in General.Compilers)
                {
                    // Compiler name matches?
                    if (c.Name == compilername)
                    {
                        // Apply compiler
                        this.Compiler = c;
                        break;
                    }
                }

                // No compiler found?
                if (this.Compiler == null) throw new Exception("Compiler \"" + compilername + "\" is not defined");
            }
        }

        #endregion

        #region ================== Methods

        // This returns the correct case for a keyword
        // Returns the same keyword as the input when it cannot be found
        public string GetKeywordCase(string keyword)
        {
            if (lowerkeywords.ContainsKey(keyword.ToLowerInvariant()))
                return lowerkeywords[keyword.ToLowerInvariant()];
            else
                return keyword;
        }

        // This returns the correct case for a constant
        // Returns the same constant as the input when it cannot be found
        public string GetConstantCase(string constant)
        {
            if (lowerconstants.ContainsKey(constant.ToLowerInvariant()))
                return lowerconstants[constant.ToLowerInvariant()];
            else
                return constant;
        }

        // This returns true when the given word is a keyword
        public bool IsKeyword(string keyword)
        {
            return lowerkeywords.ContainsKey(keyword.ToLowerInvariant());
        }

        // This returns true when the given word is a contant
        public bool IsConstant(string constant)
        {
            return lowerconstants.ContainsKey(constant.ToLowerInvariant());
        }

        // This returns the function definition for a keyword
        // Returns null when no function definition exists
        // NOTE: The keyword parameter is case-sensitive!
        public string GetFunctionDefinition(string keyword)
        {
            if (keywords.ContainsKey(keyword))
                return keywords[keyword];
            else
                return null;
        }

        //mxd
        public string[] GetSnippet(string name)
        {
            return snippetkeyssorted.Contains(name) ? snippets[name] : null;
        }

        // This sorts by description
        public int CompareTo(ScriptConfiguration other)
        {
            return string.Compare(this.Description, other.Description, true);
        }

        //mxd
        public override string ToString()
        {
            return Description;
        }

        #endregion
    }
}
