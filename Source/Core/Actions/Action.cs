
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
using System.Collections.Generic;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Actions
{
    public class Action
    {
        #region ================== Variables

        // Description

        // Shortcut key

        // Shortcut options

        // Delegate
        private readonly List<ActionDelegate> begindelegates;
        private readonly List<ActionDelegate> enddelegates;

        #endregion

        #region ================== Properties

        public string Name { get; }
        public string ShortName { get; }
        public string Category { get; }
        public string Title { get; }
        public string Description { get; }
        public bool RegisterToast { get; }
        public int ShortcutKey { get; private set; }
        public int ShortcutMask { get; }
        public int DefaultShortcutKey { get; }
        public bool AllowKeys { get; }
        public bool AllowMouse { get; }
        public bool AllowScroll { get; }
        public bool DisregardShift { get; }
        public bool DisregardControl { get; }
        public bool DisregardAlt { get; } //mxd
        public bool Repeat { get; }
        public bool BeginBound { get { return begindelegates.Count > 0; } }
        public bool EndBound { get { return enddelegates.Count > 0; } }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal Action(Configuration cfg, string name, string shortname, int key)
        {
            // Initialize
            this.Name = name;
            this.ShortName = shortname;
            this.Title = cfg.ReadSetting(shortname + ".title", "[" + name + "]");
            this.Category = cfg.ReadSetting(shortname + ".category", "");
            this.Description = cfg.ReadSetting(shortname + ".description", "");
            this.RegisterToast = cfg.ReadSetting(shortname + ".registertoast", false);
            this.AllowKeys = cfg.ReadSetting(shortname + ".allowkeys", true);
            this.AllowMouse = cfg.ReadSetting(shortname + ".allowmouse", true);
            this.AllowScroll = cfg.ReadSetting(shortname + ".allowscroll", false);
            this.DisregardShift = cfg.ReadSetting(shortname + ".disregardshift", false);
            this.DisregardControl = cfg.ReadSetting(shortname + ".disregardcontrol", false);
            this.DisregardAlt = cfg.ReadSetting(shortname + ".disregardalt", false); //mxd
            this.Repeat = cfg.ReadSetting(shortname + ".repeat", false);
            this.DefaultShortcutKey = cfg.ReadSetting(shortname + ".default", 0);
            this.begindelegates = new List<ActionDelegate>();
            this.enddelegates = new List<ActionDelegate>();

            ShortcutMask = DisregardShift ? (int)Keys.Shift : 0;
            if (DisregardControl) ShortcutMask |= (int)Keys.Control;
            if (DisregardAlt) ShortcutMask |= (int)Keys.Alt; //mxd

            ShortcutMask = ~ShortcutMask;

            if (key == -1)
            {
                this.ShortcutKey = -1;
            }
            else
            {
                this.ShortcutKey = key & ShortcutMask;
            }
        }

        #endregion

        #region ================== Static Methods

        // This returns the shortcut key description for a key
        public static string GetShortcutKeyDesc(int key)
        {
            KeysConverter conv = new KeysConverter();
            string ctrlprefix = "";

            // When key is 0, then return an empty string
            if (key == 0) return "";

            // Split the key in Control and Button
            int ctrl = key & ((int)Keys.Control | (int)Keys.Shift | (int)Keys.Alt);
            int button = key & ~((int)Keys.Control | (int)Keys.Shift | (int)Keys.Alt);

            // When the button is a control key, then remove the control itsself
            if ((button == (int)Keys.ControlKey) || (button == (int)Keys.ShiftKey) || (button == (int)Keys.Alt))
            {
                ctrl = 0;
                key = key & ~((int)Keys.Control | (int)Keys.Shift | (int)Keys.Alt);
            }

            //mxd. Determine control prefix
            if (ctrl != 0)
            {
                if ((key & (int)Keys.Control) != 0) ctrlprefix += "Ctrl+";
                if ((key & (int)Keys.Alt) != 0) ctrlprefix += "Alt+";
                if ((key & (int)Keys.Shift) != 0) ctrlprefix += "Shift+";
            }

            // Check if button is special
            switch (button)
            {
                // Scroll down
                case (int)SpecialKeys.MScrollDown:

                    // Make string representation
                    return ctrlprefix + "ScrollDown";

                // Scroll up
                case (int)SpecialKeys.MScrollUp:

                    // Make string representation
                    return ctrlprefix + "ScrollUp";

                case (int)SpecialKeys.MScrollLeft:

                    //
                    return ctrlprefix + "ScrollLeft";

                case (int)SpecialKeys.MScrollRight:

                    //
                    return ctrlprefix + "ScrollRight";

                // Keys that would otherwise have odd names
                case (int)Keys.Oemtilde: return ctrlprefix + "~";
                case (int)Keys.OemMinus: return ctrlprefix + "-";
                case (int)Keys.Oemplus: return ctrlprefix + "+";
                case (int)Keys.Subtract: return ctrlprefix + "NumPad-";
                case (int)Keys.Add: return ctrlprefix + "NumPad+";
                case (int)Keys.Decimal: return ctrlprefix + "NumPad.";
                case (int)Keys.Multiply: return ctrlprefix + "NumPad*";
                case (int)Keys.Divide: return ctrlprefix + "NumPad/";
                case (int)Keys.OemOpenBrackets: return ctrlprefix + "[";
                case (int)Keys.OemCloseBrackets: return ctrlprefix + "]";
                case (int)Keys.Oem1: return ctrlprefix + ";";
                case (int)Keys.Oem7: return ctrlprefix + "'";
                case (int)Keys.Oemcomma: return ctrlprefix + ",";
                case (int)Keys.OemPeriod: return ctrlprefix + ".";
                case (int)Keys.OemQuestion: return ctrlprefix + "?";
                case (int)Keys.Oem5: return ctrlprefix + "\\";
                case (int)Keys.Capital: return ctrlprefix + "CapsLock";
                case (int)Keys.Back: return ctrlprefix + "Backspace";

                default:

                    // Use standard key-string conversion
                    return conv.ConvertToString(key);
            }
        }

        //mxd. This returns the shortcut key description for an action name
        public static string GetShortcutKeyDesc(string actionName)
        {
            Action a = General.Actions.GetActionByName(actionName);
            if (a.ShortcutKey == 0) return a.Title + " (not bound to a key)";
            return GetShortcutKeyDesc(a.ShortcutKey);
        }

        #endregion

        #region ================== Methods

        // This invokes the action
        public void Invoke()
        {
            this.Begin();
            this.End();
        }

        // This sets a new key for the action
        internal void SetShortcutKey(int key)
        {
            // Make it so.
            this.ShortcutKey = key & ShortcutMask;
        }

        // This binds a delegate to this action
        internal void BindBegin(ActionDelegate method)
        {
            begindelegates.Add(method);
        }

        // This removes a delegate from this action
        internal void UnbindBegin(ActionDelegate method)
        {
            begindelegates.Remove(method);
        }

        // This binds a delegate to this action
        internal void BindEnd(ActionDelegate method)
        {
            enddelegates.Add(method);
        }

        // This removes a delegate from this action
        internal void UnbindEnd(ActionDelegate method)
        {
            enddelegates.Remove(method);
        }

        // This raises events for this action
        internal void Begin()
        {
            General.Plugins.OnActionBegin(this);

            // Method bound?
            if (begindelegates.Count > 0)
            {
                // Copy delegates list
                List<ActionDelegate> delegateslist = new List<ActionDelegate>(begindelegates);

                // Invoke all the delegates
                General.Actions.Current = this;
                General.Actions.ResetExclusiveRequest();
                foreach (ActionDelegate ad in delegateslist) ad.Invoke();
                General.Actions.ResetExclusiveRequest();
                General.Actions.Current = null;
            }
        }

        // This raises events for this action
        internal void End()
        {
            // Method bound?
            if (enddelegates.Count > 0)
            {
                // Copy delegates list
                List<ActionDelegate> delegateslist = new List<ActionDelegate>(enddelegates);

                // Invoke all the delegates
                General.Actions.Current = this;
                General.Actions.ResetExclusiveRequest();
                foreach (ActionDelegate ad in delegateslist) ad.Invoke();
                General.Actions.ResetExclusiveRequest();
                General.Actions.Current = null;
            }

            General.Plugins.OnActionEnd(this);
        }

        // This checks if the action qualifies for a key combination
        public bool KeyMatches(int pressedkey)
        {
            return ShortcutKey == (pressedkey & ShortcutMask);
        }

        #endregion
    }
}
