
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

using CodeImp.DoomBuilder.Compilers;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data.Scripting;
using CodeImp.DoomBuilder.GZBuilder.Data;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Properties;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Windows;
using ScintillaNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
    internal enum ScriptStyleType
    {
        PlainText = 0,
        Keyword = 1,
        Constant = 2,
        Comment = 3,
        Literal = 4,
        LineNumber = 5,
        String = 6, //mxd
        Include = 7, //mxd
        Property = 8, //mxd
    }

    public partial class ScriptEditorControl : UserControl
    {
        #region ================== Enums

        // Index for registered images
        internal enum ImageIndex
        {
            ScriptConstant = 0,
            ScriptKeyword = 1,
            ScriptError = 2,
            ScriptSnippet = 3, //mxd
            ScriptProperty = 4, //mxd
        }

        #endregion

        #region ================== Constants

        private const string LEXERS_RESOURCE = "Lexers.cfg";
        private const int HIGHLIGHT_INDICATOR = 8; //mxd. Indicators 0-7 could be in use by a lexer so we'll use indicator 8 to highlight words.
        private const string ENTRY_POSITION_MARKER = "[EP]"; //mxd
        private const string LINE_BREAK_MARKER = "[LB]"; //mxd

        const int SCI_ENSUREVISIBLEENFORCEPOLICY = 2234;
        const int SCI_SETBACKSPACEUNINDENTS = 2262;
        const int SCI_SETTABINDENTS = 2260;

        #endregion

        #region ================== Delegates / Events

        public delegate void ExplicitSaveTabDelegate();
        public delegate void OpenScriptBrowserDelegate();
        public delegate void OpenFindReplaceDelegate();
        public delegate bool FindNextDelegate();
        public delegate bool FindPreviousDelegate(); //mxd
        public delegate bool FindNextWrapAroundDelegate(FindReplaceOptions options); //mxd
        public delegate bool FindPreviousWrapAroundDelegate(FindReplaceOptions options); //mxd
        public delegate void GoToLineDelegate(); //mxd
        public delegate void CompileScriptDelegate(); //mxd

        public event ExplicitSaveTabDelegate OnExplicitSaveTab;
        public event OpenScriptBrowserDelegate OnOpenScriptBrowser;
        public event OpenFindReplaceDelegate OnOpenFindAndReplace;
        public event FindNextDelegate OnFindNext;
        public event FindPreviousDelegate OnFindPrevious; //mxd
        public event FindNextWrapAroundDelegate OnFindNextWrapAround; //mxd
        public event FindPreviousWrapAroundDelegate OnFindPreviousWrapAround; //mxd
        public new event EventHandler OnTextChanged; //mxd
        public event EventHandler OnFunctionBarDropDown; //mxd
        public event GoToLineDelegate OnGoToLine; //mxd
        public event CompileScriptDelegate OnCompileScript; //mxd

        #endregion

        #region ================== Variables

        // Script configuration
        private ScriptConfiguration scriptconfig;

        //mxd. Handles script type-specific stuff
        private ScriptHandler handler;

        // Style translation from Scintilla style to ScriptStyleType
        private Dictionary<int, ScriptStyleType> stylelookup;

        // Current position information
        private int linenumbercharlength; //mxd. Current max number of chars in the line number
        private int lastcaretpos; //mxd. Used in brace matching
        private int caretoffset; //mxd. Used to modify caret position after autogenerating stuff
        private bool expandcodeblock; //mxd. More gross hacks
        private string highlightedword; //mxd

        //mxd. Event propagation
        private bool preventchanges;

        #endregion

        #region ================== Properties

        public bool IsChanged { get { return Scintilla.Modified; } }
        public int SelectionStart { get { return Scintilla.SelectionStart; } set { Scintilla.SelectionStart = value; } }
        public int SelectionEnd { get { return Scintilla.SelectionEnd; } set { Scintilla.SelectionEnd = value; } }
        public new string Text { get { return Scintilla.Text; } set { Scintilla.Text = value; } } //mxd
        public string SelectedText { get { return Scintilla.SelectedText; } } //mxd
        public bool ShowWhitespace { get { return Scintilla.ViewWhitespace != WhitespaceMode.Invisible; } set { Scintilla.ViewWhitespace = value ? WhitespaceMode.VisibleAlways : WhitespaceMode.Invisible; } }
        public bool WrapLongLines { get { return Scintilla.WrapMode != WrapMode.None; } set { Scintilla.WrapMode = value ? WrapMode.Char : WrapMode.None; } }
        internal Scintilla Scintilla { get; private set; } //mxd
        internal static Encoding Encoding { get; } = Encoding.GetEncoding(1251); //mxd

        #endregion

        #region ================== Contructor / Disposer

        // Constructor
        public ScriptEditorControl()
        {
            // Initialize
            InitializeComponent();

            // Script editor properties
            //TODO: use ScintillaNET properties instead when they become available
            Scintilla.DirectMessage(SCI_SETBACKSPACEUNINDENTS, new IntPtr(1), IntPtr.Zero);
            //scriptedit.DirectMessage(SCI_SETMOUSEDOWNCAPTURES, new IntPtr(1), IntPtr.Zero);
            Scintilla.DirectMessage(SCI_SETTABINDENTS, new IntPtr(1), IntPtr.Zero);

            // Symbol margin
            Scintilla.Margins[0].Type = MarginType.Symbol;
            Scintilla.Margins[0].Width = 20;
            Scintilla.Margins[0].Mask = 1 << (int)ImageIndex.ScriptError; // Error marker only
            Scintilla.Margins[0].Cursor = MarginCursor.Arrow;
            Scintilla.Margins[0].Sensitive = true;

            // Line numbers margin
            if (General.Settings.ScriptShowLineNumbers)
            {
                Scintilla.Margins[1].Type = MarginType.Number;
                Scintilla.Margins[1].Width = 16;
            }
            Scintilla.Margins[1].Mask = 0; // No markers here

            // Spacing margin
            Scintilla.Margins[2].Type = MarginType.Symbol;
            Scintilla.Margins[2].Width = 5;
            Scintilla.Margins[2].Cursor = MarginCursor.Arrow;
            Scintilla.Margins[2].Mask = 0; // No markers here

            // Images
            RegisterAutoCompleteImage(ImageIndex.ScriptConstant, Resources.ScriptConstant);
            RegisterAutoCompleteImage(ImageIndex.ScriptKeyword, Resources.ScriptKeyword);
            RegisterAutoCompleteImage(ImageIndex.ScriptSnippet, Resources.ScriptSnippet); //mxd
            RegisterAutoCompleteImage(ImageIndex.ScriptProperty, Resources.ScriptProperty); //mxd
            RegisterMarkerImage(ImageIndex.ScriptError, Resources.ScriptError);

            // These key combinations put odd characters in the script. Let's disable them
            Scintilla.AssignCmdKey(Keys.Control | Keys.Q, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.W, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.E, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.R, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.I, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.P, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.G, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.H, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.K, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.B, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.N, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.Q, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.W, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.E, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.R, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.Y, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.O, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.P, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.A, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.S, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.D, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.F, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.G, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.H, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.K, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.Z, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.X, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.C, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.V, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.B, Command.Null);
            Scintilla.AssignCmdKey(Keys.Control | Keys.Shift | Keys.N, Command.Null);
        }

        #endregion

        #region ================== Public methods

        // This launches keyword help website
        public bool LaunchKeywordHelp()
        {
            string helpsite = scriptconfig.KeywordHelp;
            string currentword = GetCurrentWord();
            if (!string.IsNullOrEmpty(currentword) && (currentword.Length > 1) && !string.IsNullOrEmpty(helpsite))
            {
                currentword = scriptconfig.GetKeywordCase(currentword);
                helpsite = helpsite.Replace("%K", currentword);
                General.OpenWebsite(helpsite);
                return true;
            }

            return !string.IsNullOrEmpty(helpsite); //mxd
        }

        // This replaces the selection with the given text
        public void ReplaceSelection(string replacement)
        {
            Scintilla.ReplaceSelection(replacement); //mxd TODO: encoding check/conversion?
        }

        // This moves the caret to a given line and ensures the line is visible
        public void MoveToLine(int linenumber)
        {
            //mxd. Safety required
            linenumber = General.Clamp(linenumber, 0, Scintilla.Lines.Count);

            Scintilla.Lines[linenumber].Goto();
            EnsureLineVisible(linenumber);
            Scintilla.SetEmptySelection(Scintilla.Lines[linenumber].Position);
        }

        // This makes sure a line is visible
        public void EnsureLineVisible(int linenumber)
        {
            int caretpos = Scintilla.CurrentPosition;

            // Determine target lines range
            int startline = Math.Max(0, linenumber - 4);
            int endline = Math.Min(Scintilla.Lines.Count, Math.Max(linenumber, linenumber + Scintilla.LinesOnScreen - 6));

            // Go to target line
            Scintilla.DirectMessage(SCI_ENSUREVISIBLEENFORCEPOLICY, (IntPtr)startline, IntPtr.Zero); // Unfold the whole text block if needed
            Scintilla.ShowLines(startline, endline);

            // We may want to do some scrolling...
            if (Scintilla.FirstVisibleLine >= startline)
                Scintilla.Lines[startline].Goto();
            else if (Scintilla.FirstVisibleLine + Scintilla.LinesOnScreen <= endline)
                Scintilla.Lines[endline].Goto();

            // We don't want to change caret position
            Scintilla.CurrentPosition = caretpos;
        }

        //mxd
        private void SelectAndShow(int startpos, int endpos)
        {
            // Select the result
            int startline = Scintilla.LineFromPosition(startpos);
            int endline = Scintilla.LineFromPosition(endpos);

            // Go to target line
            Scintilla.DirectMessage(SCI_ENSUREVISIBLEENFORCEPOLICY, (IntPtr)startline, IntPtr.Zero); // Unfold the whole text block if needed
            Scintilla.ShowLines(startline, endline);
            Scintilla.GotoPosition(startpos);

            // We may want to do some extra scrolling...
            if (startline > 1 && Scintilla.FirstVisibleLine >= startline - 1)
                Scintilla.Lines[startline - 1].Goto();
            else if (endline < Scintilla.Lines.Count - 1 && Scintilla.FirstVisibleLine + Scintilla.LinesOnScreen <= endline + 1)
                Scintilla.Lines[endline + 1].Goto();

            // Update selection
            Scintilla.SelectionStart = startpos;
            Scintilla.SelectionEnd = endpos;
        }

        // This returns the line for a position
        public int LineFromPosition(int position)
        {
            return Scintilla.LineFromPosition(position);
        }

        // This clears all marks
        public void ClearMarks()
        {
            Scintilla.MarkerDeleteAll((int)ImageIndex.ScriptError);
        }

        // This adds a mark on the given line
        public void AddMark(int linenumber)
        {
            Scintilla.Lines[linenumber].MarkerAdd((int)ImageIndex.ScriptError);
        }

        // This refreshes the style setup
        public void RefreshStyle()
        {
            // Re-setup with the same config
            SetupStyles(scriptconfig);
        }

        // This sets up the script editor with a script configuration
        public void SetupStyles(ScriptConfiguration config)
        {
            //mxd. Update script handler
            handler = General.Types.GetScriptHandler(config.ScriptType);
            handler.Initialize(this, config);

            //mxd
            functionbar.Enabled = config.ScriptType != ScriptType.UNKNOWN;

            Configuration lexercfg = new Configuration();

            // Make collections
            stylelookup = new Dictionary<int, ScriptStyleType>();

            // Keep script configuration
            scriptconfig = config;

            // Find a resource named Lexers.cfg
            string[] resnames = General.ThisAssembly.GetManifestResourceNames();
            foreach (string rn in resnames)
            {
                // Found one?
                if (rn.EndsWith(LEXERS_RESOURCE, StringComparison.OrdinalIgnoreCase))
                {
                    // Get a stream from the resource
                    Stream lexersdata = General.ThisAssembly.GetManifestResourceStream(rn);
                    if (lexersdata != null)
                    {
                        StreamReader lexersreader = new StreamReader(lexersdata, Encoding.ASCII);

                        // Load configuration from stream
                        lexercfg.InputConfiguration(lexersreader.ReadToEnd());

                        // Done with the resource
                        lexersreader.Dispose();
                    }

                    //mxd. We are done here
                    break;
                }
            }

            //mxd. Reset document slyle
            Scintilla.ClearDocumentStyle();
            Scintilla.StyleResetDefault();

            // Check if specified lexer exists and set the lexer to use
            string lexername = "lexer" + (int)scriptconfig.Lexer;
            if (!lexercfg.SettingExists(lexername)) throw new InvalidOperationException("Unknown lexer " + scriptconfig.Lexer + " specified in script configuration!");
            Scintilla.Lexer = scriptconfig.Lexer;

            //mxd. Set extra word chars?
            if (!string.IsNullOrEmpty(scriptconfig.ExtraWordCharacters))
                Scintilla.WordChars += scriptconfig.ExtraWordCharacters;

            // Set the default style and settings
            Scintilla.Styles[Style.Default].Font = General.Settings.ScriptFontName;
            Scintilla.Styles[Style.Default].Size = General.Settings.ScriptFontSize;
            Scintilla.Styles[Style.Default].Bold = General.Settings.ScriptFontBold;
            Scintilla.Styles[Style.Default].Italic = false;
            Scintilla.Styles[Style.Default].Underline = false;
            Scintilla.Styles[Style.Default].Case = StyleCase.Mixed;
            Scintilla.Styles[Style.Default].ForeColor = General.Colors.PlainText.ToColor();
            Scintilla.Styles[Style.Default].BackColor = General.Colors.ScriptBackground.ToColor();
            Scintilla.CaretPeriod = SystemInformation.CaretBlinkTime;
            Scintilla.CaretForeColor = General.Colors.ScriptBackground.Inverse().ToColor();

            // Set tabulation settings
            Scintilla.UseTabs = General.Settings.ScriptUseTabs;
            Scintilla.TabWidth = General.Settings.ScriptTabWidth;
            //scriptedit.IndentWidth = General.Settings.ScriptTabWidth; // Equals to TabWidth by default
            //TODO: use ScintillaNET properties instead when they become available
            Scintilla.DirectMessage(SCI_SETTABINDENTS, new IntPtr(1), IntPtr.Zero);
            Scintilla.DirectMessage(SCI_SETBACKSPACEUNINDENTS, new IntPtr(1), IntPtr.Zero);

            // This applies the default style to all styles
            Scintilla.StyleClearAll();

            // Set the code page to use. [mxd] No longer needed?
            //scriptedit.CodePage = scriptconfig.CodePage;

            //mxd. We can't change Font or Size here because this will screw displayed tab width (because it's based on character width)...
            // Set the default to something normal (this is used by the autocomplete list)
            //scriptedit.Styles[Style.Default].Font = this.Font.Name;
            Scintilla.Styles[Style.Default].Bold = this.Font.Bold;
            Scintilla.Styles[Style.Default].Italic = this.Font.Italic;
            Scintilla.Styles[Style.Default].Underline = this.Font.Underline;
            //scriptedit.Styles[Style.Default].Size = (int)Math.Round(this.Font.SizeInPoints);

            // Set style for linenumbers and margins
            Scintilla.Styles[Style.LineNumber].BackColor = General.Colors.ScriptBackground.ToColor();
            Scintilla.SetFoldMarginColor(true, General.Colors.ScriptFoldBackColor.ToColor());
            Scintilla.SetFoldMarginHighlightColor(true, General.Colors.ScriptFoldBackColor.ToColor());
            for (int i = 25; i < 32; i++)
            {
                Scintilla.Markers[i].SetForeColor(General.Colors.ScriptFoldBackColor.ToColor());
                Scintilla.Markers[i].SetBackColor(General.Colors.ScriptFoldForeColor.ToColor());
            }

            //mxd. Set style for (mis)matching braces
            Scintilla.Styles[Style.BraceLight].BackColor = General.Colors.ScriptBraceHighlight.ToColor();
            Scintilla.Styles[Style.BraceBad].BackColor = General.Colors.ScriptBadBraceHighlight.ToColor();

            //mxd. Set whitespace color
            Scintilla.SetWhitespaceForeColor(true, General.Colors.ScriptWhitespace.ToColor());

            //mxd. Set selection colors
            Scintilla.SetSelectionForeColor(true, General.Colors.ScriptSelectionForeColor.ToColor());
            Scintilla.SetSelectionBackColor(true, General.Colors.ScriptSelectionBackColor.ToColor());

            // Clear all keywords
            for (int i = 0; i < 9; i++) Scintilla.SetKeywords(i, null);

            // Now go for all elements in the lexer configuration
            // We are looking for the numeric keys, because these are the
            // style index to set and the value is our ScriptStyleType
            IDictionary dic = lexercfg.ReadSetting(lexername, new Hashtable());
            foreach (DictionaryEntry de in dic)
            {
                // Check if this is a numeric key
                int stylenum;
                if (int.TryParse(de.Key.ToString(), out stylenum))
                {
                    // Add style to lookup table
                    stylelookup.Add(stylenum, (ScriptStyleType)(int)de.Value);

                    // Apply color to style
                    int colorindex;
                    ScriptStyleType type = (ScriptStyleType)(int)de.Value;
                    switch (type)
                    {
                        case ScriptStyleType.PlainText: colorindex = ColorCollection.PLAINTEXT; break;
                        case ScriptStyleType.Comment: colorindex = ColorCollection.COMMENTS; break;
                        case ScriptStyleType.Constant: colorindex = ColorCollection.CONSTANTS; break;
                        case ScriptStyleType.Keyword: colorindex = ColorCollection.KEYWORDS; break;
                        case ScriptStyleType.LineNumber: colorindex = ColorCollection.LINENUMBERS; break;
                        case ScriptStyleType.Literal: colorindex = ColorCollection.LITERALS; break;
                        case ScriptStyleType.String: colorindex = ColorCollection.STRINGS; break;
                        case ScriptStyleType.Include: colorindex = ColorCollection.INCLUDES; break;
                        case ScriptStyleType.Property: colorindex = ColorCollection.PROPERTIES; break;
                        default: colorindex = ColorCollection.PLAINTEXT; break;
                    }

                    Scintilla.Styles[stylenum].ForeColor = General.Colors.Colors[colorindex].ToColor();
                }
            }

            //mxd. Set keywords
            handler.SetKeywords(lexercfg, lexername);

            // Setup folding (https://github.com/jacobslusser/ScintillaNET/wiki/Automatic-Code-Folding)
            if (General.Settings.ScriptShowFolding && (scriptconfig.Lexer == Lexer.Cpp || (int)scriptconfig.Lexer == 35)) // 35 - custom CPP case insensitive style lexer
            {
                // Instruct the lexer to calculate folding
                Scintilla.SetProperty("fold", "1");
                Scintilla.SetProperty("fold.compact", "0"); // 1 = folds blank lines
                Scintilla.SetProperty("fold.comment", "1"); // Enable block comment folding
                Scintilla.SetProperty("fold.preprocessor", "1"); // Enable #region folding
                Scintilla.SetFoldFlags(FoldFlags.LineAfterContracted); // Draw line below if not expanded

                // Configure a margin to display folding symbols
                Scintilla.Margins[2].Type = MarginType.Symbol;
                Scintilla.Margins[2].Mask = Marker.MaskFolders;
                Scintilla.Margins[2].Sensitive = true;
                Scintilla.Margins[2].Width = 12;

                // Configure folding markers with respective symbols
                Scintilla.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
                Scintilla.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
                Scintilla.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
                Scintilla.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
                Scintilla.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
                Scintilla.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
                Scintilla.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

                // Enable automatic folding
                Scintilla.AutomaticFold = AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change;
            }
            else
            {
                // Disable folding
                Scintilla.SetProperty("fold", "0");
                Scintilla.SetProperty("fold.compact", "0");

                Scintilla.Margins[2].Type = MarginType.Symbol;
                Scintilla.Margins[2].Mask = 0; // No markers here
                Scintilla.Margins[2].Sensitive = false;
                Scintilla.Margins[2].Width = 5;

                Scintilla.AutomaticFold = AutomaticFold.None;
            }

            // Rearrange the layout
            this.PerformLayout();
        }

        // This returns the current word (where the caret is at)
        public string GetCurrentWord()
        {
            return GetWordAt(Scintilla.CurrentPosition);
        }

        // This returns the word at the given position
        public string GetWordAt(int position)
        {
            return Scintilla.GetWordFromPosition(position);
        }

        // Save one undo checkpoint when making many changes in the editor.
        public void UndoTransaction(Action callback)
        {
            Scintilla.BeginUndoAction();

            try
            {
                callback.Invoke();
            }
            finally
            {
                Scintilla.EndUndoAction();
            }
        }

        // Perform undo
        public void Undo()
        {
            Scintilla.Undo();
        }

        // Perform redo
        public void Redo()
        {
            Scintilla.Redo();
        }

        // This clears all undo levels
        public void ClearUndoRedo()
        {
            Scintilla.EmptyUndoBuffer();
        }

        //mxd. This marks the current document as unmodified
        public void SetSavePoint()
        {
            Scintilla.SetSavePoint();
        }

        // Perform cut
        public void Cut()
        {
            Scintilla.Cut();
        }

        // Perform copy
        public void Copy()
        {
            Scintilla.Copy();
        }

        // Perform paste
        public void Paste()
        {
            Scintilla.Paste();
        }

        // This steals the focus (use with care!)
        public void GrabFocus()
        {
            Scintilla.Focus();
        }

        public byte[] GetText()
        {
            return Encoding.GetBytes(Scintilla.Text); //mxd TODO: other encodings?..
        }

        public void SetText(byte[] text)
        {
            Scintilla.Text = Encoding.GetString(text); //mxd TODO: other encodings?..
        }

        //mxd
        public void InsertSnippet(string[] lines)
        {
            // Insert the snippet
            List<string> processedlines = new List<string>(lines.Length);
            int curline = Scintilla.LineFromPosition(Scintilla.SelectionStart);
            int indent = Scintilla.Lines[Scintilla.CurrentLine].Indentation;
            string tabs = Environment.NewLine + GetIndentationString(indent);
            string spaces = new String(' ', General.Settings.ScriptTabWidth);
            string[] linebreak = { LINE_BREAK_MARKER };
            int entrypos = -1;
            int entryline = -1;

            // Process line breaks
            foreach (string line in lines)
            {
                if (line.IndexOf(linebreak[0], StringComparison.Ordinal) != -1)
                {
                    if (General.Settings.ScriptAllmanStyle)
                        processedlines.AddRange(line.Split(linebreak, StringSplitOptions.RemoveEmptyEntries));
                    else
                        processedlines.Add(line.Replace(linebreak[0], " "));
                }
                else
                {
                    processedlines.Add(line);
                }
            }

            // Process special chars, try to find entry position marker
            for (int i = 0; i < processedlines.Count; i++)
            {
                if (!Scintilla.UseTabs) processedlines[i] = processedlines[i].Replace("\t", spaces);

                // Check if we have the [EP] marker
                if (entrypos == -1)
                {
                    int pos = processedlines[i].IndexOf(ENTRY_POSITION_MARKER, StringComparison.OrdinalIgnoreCase);
                    if (pos != -1)
                    {
                        processedlines[i] = processedlines[i].Remove(pos, 4);
                        entryline = curline + i;
                        entrypos = processedlines[i].Length - pos;
                    }
                }
            }

            // Replace the text
            string text = string.Join(tabs, processedlines.ToArray());
            Scintilla.SelectionStart = Scintilla.WordStartPosition(Scintilla.CurrentPosition, true);
            Scintilla.SelectionEnd = Scintilla.WordEndPosition(Scintilla.CurrentPosition, true);
            Scintilla.ReplaceSelection(text);

            // Move the cursor if we had the [EP] marker
            if (entrypos != -1)
            {
                // Count from the end of the line, because I don't see a reliable way to count indentation chars...
                int pos = Scintilla.Lines[entryline].EndPosition - entrypos;
                if (Scintilla.Lines[entryline].Text.EndsWith(Environment.NewLine)) pos -= 2;
                Scintilla.SetEmptySelection(pos);
            }
        }

        //mxd. Find next result
        public bool FindNext(FindReplaceOptions options, bool useselectionstart)
        {
            if (string.IsNullOrEmpty(options.FindText)) return false;

            // Find next match/abort when trying to replace in read-only tab
            if (Scintilla.ReadOnly && options.ReplaceWith != null)
            {
                if (options.SearchMode != FindReplaceSearchMode.CURRENT_FILE && OnFindNextWrapAround != null)
                    return OnFindNextWrapAround(options);
                return false;
            }

            int startpos = useselectionstart ? Math.Min(Scintilla.SelectionStart, Scintilla.SelectionEnd) : Math.Max(Scintilla.SelectionStart, Scintilla.SelectionEnd);

            // Search the document
            Scintilla.TargetStart = startpos;
            Scintilla.TargetEnd = Scintilla.TextLength;
            Scintilla.SearchFlags = options.CaseSensitive ? SearchFlags.MatchCase : SearchFlags.None;
            if (options.WholeWord) Scintilla.SearchFlags |= SearchFlags.WholeWord;

            int result = Scintilla.SearchInTarget(options.FindText);

            // Wrap around?
            if (result == -1)
            {
                if (options.WrapAroundDisabled)
                    return false;

                if (options.SearchMode != FindReplaceSearchMode.CURRENT_FILE
                    && OnFindNextWrapAround != null && OnFindNextWrapAround(options))
                {
                    return true;
                }

                Scintilla.TargetStart = 0;
                Scintilla.TargetEnd = startpos;
                result = Scintilla.SearchInTarget(options.FindText);
            }

            // Found something
            if (result != -1)
            {
                // Select the result
                SelectAndShow(result, result + options.FindText.Length);

                // Update extra highlights
                HighlightWord(options.FindText);

                // All done
                return true;
            }

            // Nothing found...
            return false;
        }

        //mxd. Find previous result
        public bool FindPrevious(FindReplaceOptions options)
        {
            if (string.IsNullOrEmpty(options.FindText)) return false;

            // Find previous match/abort when trying to replace in read-only tab
            if (Scintilla.ReadOnly && options.ReplaceWith != null)
            {
                if (options.SearchMode != FindReplaceSearchMode.CURRENT_FILE && OnFindPreviousWrapAround != null)
                    return OnFindPreviousWrapAround(options);
                return false;
            }

            int endpos = Math.Max(0, Math.Min(Scintilla.SelectionStart, Scintilla.SelectionEnd) - 1);

            // Search the document
            Scintilla.TargetStart = endpos;
            Scintilla.TargetEnd = 0;
            Scintilla.SearchFlags = options.CaseSensitive ? SearchFlags.MatchCase : SearchFlags.None;
            if (options.WholeWord) Scintilla.SearchFlags |= SearchFlags.WholeWord;

            int result = Scintilla.SearchInTarget(options.FindText);

            // Wrap around?
            if (result == -1)
            {
                if (options.SearchMode != FindReplaceSearchMode.CURRENT_FILE
                    && OnFindPreviousWrapAround != null && OnFindPreviousWrapAround(options))
                {
                    return true;
                }

                Scintilla.TargetStart = Scintilla.TextLength;
                Scintilla.TargetEnd = endpos;
                result = Scintilla.SearchInTarget(options.FindText);
            }

            // Found something
            if (result != -1)
            {
                // Select the result
                SelectAndShow(result, result + options.FindText.Length);

                // Update extra highlights
                HighlightWord(options.FindText);

                // All done
                return true;
            }

            // Nothing found...
            return false;
        }

        //mxd. (Un)indents selection
        public void IndentSelection(bool indent)
        {
            // Get selected range of lines
            int startline = Scintilla.LineFromPosition(Scintilla.SelectionStart);
            int endline = Scintilla.LineFromPosition(Scintilla.SelectionEnd);

            for (int i = startline; i < endline + 1; i++)
            {
                Scintilla.Lines[i].Indentation += indent ? General.Settings.ScriptTabWidth : -General.Settings.ScriptTabWidth;
            }
        }

        //mxd
        public void DuplicateLine()
        {
            //scriptedit.DirectMessage(NativeMethods.SCI_LINEDUPLICATE);

            // Do it manually instead of using Scintilla's builtin variant to avoid triggering InsertCheck event, 
            // which conatains only "\r\n" text, which in turn messes with our scriptedit_InsertCheck handler logic
            // resulting in extra indentation...
            var curline = Scintilla.Lines[Scintilla.CurrentLine];
            Scintilla.InsertText(curline.EndPosition, curline.Text);

            // Offset selection by line length
            Scintilla.SetEmptySelection(curline.EndPosition + (Scintilla.SelectionStart - curline.Position));
        }

        //mxd
        internal List<CompilerError> UpdateNavigator(ScriptDocumentTab tab)
        {
            List<CompilerError> result = new List<CompilerError>();

            // Just clear the navigator when current tab has no text
            if (Scintilla.Text.Length == 0)
            {
                functionbar.Items.Clear();
                functionbar.Enabled = false;
                return result;
            }

            // Store currently selected item name
            string prevtext = functionbar.Text;

            // Repopulate FunctionBar
            result = handler.UpdateFunctionBarItems(tab, new MemoryStream(GetText()), functionbar);

            // Put some text in the navigator (but don't actually trigger selection event)
            functionbar.Enabled = functionbar.Items.Count > 0;
            if (functionbar.Items.Count > 0)
            {
                preventchanges = true;

                // Put the text back if we still have the corresponding item
                if (!string.IsNullOrEmpty(prevtext))
                {
                    foreach (var item in functionbar.Items)
                    {
                        if (item.ToString() == prevtext)
                        {
                            functionbar.Text = item.ToString();
                            break;
                        }
                    }
                }

                // No dice. Use the first item
                if (string.IsNullOrEmpty(functionbar.Text))
                    functionbar.Text = functionbar.Items[0].ToString();

                preventchanges = false;
            }

            return result;
        }

        #endregion

        #region ================== Utility methods

        // This returns the ScriptStyleType for a given Scintilla style
        internal ScriptStyleType GetScriptStyle(int scintillastyle)
        {
            return stylelookup.ContainsKey(scintillastyle) ? stylelookup[scintillastyle] : ScriptStyleType.PlainText;
        }

        // This registers an image for the autocomplete list
        private void RegisterAutoCompleteImage(ImageIndex index, Bitmap image)
        {
            // Register image
            Scintilla.RegisterRgbaImage((int)index, image);
        }

        // This registers an image for the markes list
        private void RegisterMarkerImage(ImageIndex index, Bitmap image)
        {
            // Register image
            Scintilla.Markers[(int)index].DefineRgbaImage(image);
            Scintilla.Markers[(int)index].Symbol = MarkerSymbol.RgbaImage;
        }

        //mxd
        private string GetIndentationString(int indent)
        {
            if (Scintilla.UseTabs)
            {
                string indentstr = string.Empty;
                int numtabs = indent / Scintilla.TabWidth;
                if (numtabs > 0) indentstr = new string('\t', numtabs);

                // Mixed padding? Add spaces
                if (numtabs * Scintilla.TabWidth < indent)
                {
                    int numspaces = indent - (numtabs * Scintilla.TabWidth);
                    indentstr += new string(' ', numspaces);
                }

                return indentstr;
            }
            else
            {
                return new string(' ', indent);
            }
        }

        //mxd. https://github.com/jacobslusser/ScintillaNET/wiki/Find-and-Highlight-Words
        private void HighlightWord(string text)
        {
            // Remove all uses of our indicator
            Scintilla.IndicatorCurrent = HIGHLIGHT_INDICATOR;
            Scintilla.IndicatorClearRange(0, Scintilla.TextLength);

            // Update indicator appearance
            Scintilla.Indicators[HIGHLIGHT_INDICATOR].Style = IndicatorStyle.RoundBox;
            Scintilla.Indicators[HIGHLIGHT_INDICATOR].Under = true;
            Scintilla.Indicators[HIGHLIGHT_INDICATOR].ForeColor = General.Colors.ScriptIndicator.ToColor();
            Scintilla.Indicators[HIGHLIGHT_INDICATOR].OutlineAlpha = 50;
            Scintilla.Indicators[HIGHLIGHT_INDICATOR].Alpha = 30;

            // Search the document
            Scintilla.TargetStart = 0;
            Scintilla.TargetEnd = Scintilla.TextLength;
            Scintilla.SearchFlags = SearchFlags.WholeWord;

            while (Scintilla.SearchInTarget(text) != -1)
            {
                //mxd. Don't mark currently selected word
                if (Scintilla.SelectionStart != Scintilla.TargetStart && Scintilla.SelectionEnd != Scintilla.TargetEnd)
                {
                    // Mark the search results with the current indicator
                    Scintilla.IndicatorFillRange(Scintilla.TargetStart, Scintilla.TargetEnd - Scintilla.TargetStart);
                }

                // Search the remainder of the document
                Scintilla.TargetStart = Scintilla.TargetEnd;
                Scintilla.TargetEnd = Scintilla.TextLength;
            }
        }

        //mxd. Handle keyboard shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keydata)
        {
            // F3 for Find Next
            if (keydata == Keys.F3)
            {
                if (OnFindNext != null) OnFindNext();
                return true;
            }

            //mxd. F2 for Find Previous
            if (keydata == Keys.F2)
            {
                if (OnFindPrevious != null) OnFindPrevious();
                return true;
            }

            //mxd. F5 for Compile Script
            if (keydata == Keys.F5)
            {
                if (OnCompileScript != null) OnCompileScript();
                return true;
            }

            // CTRL+F for find & replace
            if (keydata == (Keys.Control | Keys.F))
            {
                if (OnOpenFindAndReplace != null) OnOpenFindAndReplace();
                return true;
            }

            // CTRL+G for go to line
            if (keydata == (Keys.Control | Keys.G))
            {
                if (OnGoToLine != null) OnGoToLine();
                return true;
            }

            // CTRL+S for save
            if (!Scintilla.ReadOnly && keydata == (Keys.Control | Keys.S))
            {
                if (OnExplicitSaveTab != null) OnExplicitSaveTab();
                return true;
            }

            // CTRL+O for open
            if (keydata == (Keys.Control | Keys.O))
            {
                if (OnOpenScriptBrowser != null) OnOpenScriptBrowser();
                return true;
            }

            // CTRL+Space to autocomplete
            if (!Scintilla.ReadOnly && keydata == (Keys.Control | Keys.Space))
            {
                // Hide call tip if any
                Scintilla.CallTipCancel();

                // Show autocomplete
                handler.ShowAutoCompletionList();
                return true;
            }

            //mxd. Tab to expand code snippet. Do it only when the text cursor is at the end of a keyword.
            if (!Scintilla.ReadOnly && keydata == Keys.Tab && !Scintilla.AutoCActive)
            {
                string curword = GetCurrentWord().ToLowerInvariant();
                if (scriptconfig.Snippets.Contains(curword) && Scintilla.CurrentPosition == Scintilla.WordEndPosition(Scintilla.CurrentPosition, true))
                {
                    InsertSnippet(scriptconfig.GetSnippet(curword));
                    return true;
                }
            }

            //mxd. Skip text insert when "save screenshot" action keys are pressed
            Actions.Action[] actions = General.Actions.GetActionsByKey((int)keydata);
            foreach (Actions.Action action in actions)
            {
                if (action.ShortName == "savescreenshot" || action.ShortName == "saveeditareascreenshot") return true;
            }

            // Pass to base
            return base.ProcessCmdKey(ref msg, keydata);
        }

        #endregion

        #region ================== Events

        // Layout needs to be re-organized
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            // With or without functions bar?
            if (functionbar.Visible)
            {
                scriptpanel.Top = functionbar.Bottom + 6;
                scriptpanel.Height = this.ClientSize.Height - scriptpanel.Top;
            }
            else
            {
                scriptpanel.Top = 0;
                scriptpanel.Height = this.ClientSize.Height;
            }
        }

        //mxd. Script text changed
        private void scriptedit_TextChanged(object sender, EventArgs e)
        {
            // Line number margin width needs changing?
            int curlinenumbercharlength = Scintilla.Lines.Count.ToString().Length;

            // Calculate the width required to display the last line number
            // and include some padding for good measure.
            if (curlinenumbercharlength != linenumbercharlength)
            {
                const int padding = 2;
                Scintilla.Margins[1].Width = Scintilla.TextWidth(Style.LineNumber, new string('9', curlinenumbercharlength + 1)) + padding;
                linenumbercharlength = curlinenumbercharlength;
            }

            if (OnTextChanged != null) OnTextChanged(this, EventArgs.Empty);
        }

        //mxd
        private void scriptedit_CharAdded(object sender, CharAddedEventArgs e)
        {
            // Hide call tip if any
            Scintilla.CallTipCancel();

            // Offset caret if needed
            if (caretoffset != 0)
            {
                Scintilla.SetEmptySelection(Scintilla.SelectionStart + caretoffset);
                caretoffset = 0;
                if (!expandcodeblock) return;
            }

            // Move CodeBlockOpen to the new line?
            if (expandcodeblock)
            {
                if (Scintilla.CurrentLine > 0)
                {
                    string linetext = Scintilla.Lines[Scintilla.CurrentLine - 1].Text;
                    int blockopenpos = string.IsNullOrEmpty(scriptconfig.CodeBlockOpen) ? -1 : linetext.LastIndexOf(scriptconfig.CodeBlockOpen, StringComparison.Ordinal);
                    if (blockopenpos != -1)
                    {
                        // Do it only if initial line doesn't start with CodeBlockOpen
                        string linestart = linetext.Substring(0, blockopenpos).Trim();
                        if (linestart.Length > 0)
                        {
                            Scintilla.InsertText(Scintilla.Lines[Scintilla.CurrentLine - 1].Position + blockopenpos,
                                Environment.NewLine + GetIndentationString(Scintilla.Lines[Scintilla.CurrentLine - 1].Indentation));
                        }
                    }
                }

                expandcodeblock = false;
                return;
            }

            // Auto-match braces
            if (General.Settings.ScriptAutoCloseBrackets)
            {
                //TODO: Auto-match quotes
                bool endpos = Scintilla.CurrentPosition == Scintilla.TextLength;
                if (!string.IsNullOrEmpty(scriptconfig.CodeBlockOpen) && e.Char == scriptconfig.CodeBlockOpen[0] && !string.IsNullOrEmpty(scriptconfig.CodeBlockClose) &&
                    (endpos || (char)Scintilla.GetCharAt(Scintilla.CurrentPosition + 1) != scriptconfig.CodeBlockClose[0]))
                {
                    Scintilla.InsertText(Scintilla.CurrentPosition, scriptconfig.CodeBlockClose);
                    return;
                }

                if (!string.IsNullOrEmpty(scriptconfig.FunctionOpen) && e.Char == scriptconfig.FunctionOpen[0] && !string.IsNullOrEmpty(scriptconfig.FunctionClose) &&
                    (endpos || (char)Scintilla.GetCharAt(Scintilla.CurrentPosition + 1) != scriptconfig.FunctionClose[0]))
                {
                    Scintilla.InsertText(Scintilla.CurrentPosition, scriptconfig.FunctionClose);
                    return;
                }

                if (!string.IsNullOrEmpty(scriptconfig.ArrayOpen) && e.Char == scriptconfig.ArrayOpen[0] && !string.IsNullOrEmpty(scriptconfig.ArrayClose) &&
                    (endpos || (char)Scintilla.GetCharAt(Scintilla.CurrentPosition + 1) != scriptconfig.ArrayClose[0]))
                {
                    Scintilla.InsertText(Scintilla.CurrentPosition, scriptconfig.ArrayClose);
                    return;
                }
            }

            if (!Scintilla.ReadOnly && General.Settings.ScriptAutoShowAutocompletion)
            {
                // Display the autocompletion list
                handler.ShowAutoCompletionList();
            }
        }

        //mxd
        private void scriptedit_UpdateUI(object sender, UpdateUIEventArgs e)
        {
            // If a word is selected, highlight the same words
            if (Scintilla.SelectedText != highlightedword)
            {
                // Highlight only when whole word is selected
                if (!string.IsNullOrEmpty(Scintilla.SelectedText) && Scintilla.GetWordFromPosition(Scintilla.SelectionStart) == Scintilla.SelectedText)
                {
                    HighlightWord(Scintilla.SelectedText);
                }
                else
                {
                    // Clear highlight
                    Scintilla.IndicatorCurrent = HIGHLIGHT_INDICATOR;
                    Scintilla.IndicatorClearRange(0, Scintilla.TextLength);
                }

                highlightedword = Scintilla.SelectedText;
            }

            // Has the caret changed position?
            int caretpos = Scintilla.CurrentPosition;
            if (lastcaretpos != caretpos && scriptconfig.BraceChars.Count > 0)
            {
                // Perform brace matching (https://github.com/jacobslusser/ScintillaNET/wiki/Brace-Matching)
                lastcaretpos = caretpos;
                int bracepos1 = -1;

                // Is there a brace to the left or right?
                if (caretpos > 0 && scriptconfig.BraceChars.Contains((char)Scintilla.GetCharAt(caretpos - 1)))
                    bracepos1 = caretpos - 1;
                else if (scriptconfig.BraceChars.Contains((char)Scintilla.GetCharAt(caretpos)))
                    bracepos1 = caretpos;

                if (bracepos1 > -1)
                {
                    // Find the matching brace
                    int bracepos2 = Scintilla.BraceMatch(bracepos1);
                    if (bracepos2 == Scintilla.InvalidPosition)
                        Scintilla.BraceBadLight(bracepos1);
                    else
                        Scintilla.BraceHighlight(bracepos1, bracepos2);
                }
                else
                {
                    // Turn off brace matching
                    Scintilla.BraceHighlight(Scintilla.InvalidPosition, Scintilla.InvalidPosition);
                }
            }
        }

        //mxd
        private void scriptedit_InsertCheck(object sender, InsertCheckEventArgs e)
        {
            // Do we want auto-indentation?
            if (!expandcodeblock && General.Settings.ScriptAutoIndent && e.Text == "\r\n")
            {
                // Get current line indentation up to the cursor position
                string linetext = Scintilla.Lines[Scintilla.CurrentLine].Text;
                int selectionpos = Scintilla.SelectionStart - Scintilla.Lines[Scintilla.CurrentLine].Position;
                int indent = 0;
                for (int i = 0; i < selectionpos; i++)
                {
                    switch (linetext[i])
                    {
                        case ' ': indent++; break;
                        case '\t': indent += Scintilla.TabWidth; break;
                        default: i = selectionpos; break; // break the loop
                    }
                }

                // Store initial indentation
                int initialindent = indent;

                // Need to increase indentation? We do this when:
                // 1. Line contains '{' and '}' and the cursor is between them
                // 2. Line either doesn't contain '}', or it's before '{', or the line contains '{' and the cursor is after it 
                int blockopenpos = string.IsNullOrEmpty(scriptconfig.CodeBlockOpen) ? -1 : linetext.LastIndexOf(scriptconfig.CodeBlockOpen, selectionpos, StringComparison.Ordinal);
                int blockclosepos = string.IsNullOrEmpty(scriptconfig.CodeBlockOpen) ? -1 : linetext.IndexOf(scriptconfig.CodeBlockClose, selectionpos, StringComparison.Ordinal);

                // Add indentation when the cursor is between { and }
                bool addindent = blockopenpos != -1 && blockopenpos < selectionpos && (blockclosepos == -1 || (blockopenpos < blockclosepos && blockclosepos >= selectionpos));
                if (addindent) indent += Scintilla.TabWidth;

                // Calculate indentation
                string indentstr = GetIndentationString(indent);

                // Move CodeBlockOpen to the new line? (will be applied in scriptedit_CharAdded)
                expandcodeblock = General.Settings.ScriptAllmanStyle;

                // Offset closing block char?
                if (addindent && blockclosepos != -1)
                {
                    string initialindentstr = GetIndentationString(initialindent);
                    indentstr += Environment.NewLine + initialindentstr;

                    // Offset cursor position (will be performed in scriptedit_CharAdded)
                    caretoffset = -(initialindentstr.Length + Environment.NewLine.Length);
                }

                // Apply new indentation
                e.Text += indentstr;
            }
        }

        //mxd
        private void scriptedit_AutoCCompleted(object sender, AutoCSelectionEventArgs e)
        {
            // Expand snippet?
            string[] lines = scriptconfig.GetSnippet(e.Text);
            if (lines != null)
            {
                InsertSnippet(lines);
            }
            else
            {
                string definition = scriptconfig.GetFunctionDefinition(e.Text);
                if (!string.IsNullOrEmpty(definition))
                {
                    int entrypos = definition.IndexOf(ENTRY_POSITION_MARKER, StringComparison.OrdinalIgnoreCase);

                    // Replace inserted text with expanded version?
                    if (e.Text.StartsWith("$") || entrypos != -1)
                    {
                        // Remove the marker
                        if (entrypos != -1) definition = definition.Remove(entrypos, 4);

                        // Replace insterted text with expanded comment
                        int startpos = Scintilla.WordStartPosition(Scintilla.CurrentPosition, true);
                        Scintilla.SelectionStart = startpos;
                        Scintilla.SelectionEnd = Scintilla.WordEndPosition(Scintilla.CurrentPosition, true);
                        Scintilla.ReplaceSelection(definition);

                        // Update caret position
                        if (entrypos != -1) Scintilla.SetEmptySelection(startpos + entrypos);
                    }
                }
            }
        }

        //mxd
        private void functionbar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!preventchanges && functionbar.SelectedItem is ScriptItem)
            {
                ScriptItem si = (ScriptItem)functionbar.SelectedItem;
                EnsureLineVisible(LineFromPosition(si.CursorPosition));
                Scintilla.SelectionStart = si.CursorPosition;
                Scintilla.SelectionEnd = si.CursorPosition;

                // Focus to the editor!
                Scintilla.Focus();
            }
        }

        private void functionbar_DropDown(object sender, EventArgs e)
        {
            if (OnFunctionBarDropDown != null) OnFunctionBarDropDown(sender, e);
        }

        #endregion

        #region ================== Context menu Events

        private void contextmenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            menuundo.Enabled = Scintilla.CanUndo;
            menuredo.Enabled = Scintilla.CanRedo;

            bool cancopy = Scintilla.SelectionEnd > Scintilla.SelectionStart;
            menucut.Enabled = cancopy;
            menucopy.Enabled = cancopy;
            menupaste.Enabled = Scintilla.CanPaste;
            menudelete.Enabled = cancopy;

            menufindusages.Enabled = !string.IsNullOrEmpty(Scintilla.GetWordFromPosition(Scintilla.SelectionStart));
        }

        private void menuundo_Click(object sender, EventArgs e)
        {
            Scintilla.Undo();
        }

        private void menuredo_Click(object sender, EventArgs e)
        {
            Scintilla.Redo();
        }

        private void menucut_Click(object sender, EventArgs e)
        {
            Scintilla.Cut();
        }

        private void menucopy_Click(object sender, EventArgs e)
        {
            Scintilla.Copy();
        }

        private void menupaste_Click(object sender, EventArgs e)
        {
            Scintilla.Paste();
        }

        private void menudelete_Click(object sender, EventArgs e)
        {
            Scintilla.DeleteRange(Scintilla.SelectionStart, Scintilla.SelectionEnd - Scintilla.SelectionStart);
        }

        private void menuduplicateline_Click(object sender, EventArgs e)
        {
            DuplicateLine();
        }

        private void menuselectall_Click(object sender, EventArgs e)
        {
            Scintilla.SelectAll();
        }

        #endregion

    }
}