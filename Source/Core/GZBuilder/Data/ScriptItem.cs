using System;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.GZBuilder.Data
{
    internal sealed class ScriptItem : Object
    {
        private readonly List<string> argnames;

        internal string Name { get; }
        internal int Index { get; }
        internal int CursorPosition { get; }
        internal bool IsInclude { get; }
        internal bool HasCustomName { get; }
        internal bool Skip { get; } // Used to not display the script in the list of scripts in the action control

        // Constructor for misc usage
        internal ScriptItem(string name, int cursorposition, bool isinclude)
        {
            this.Name = name;
            this.argnames = new List<string>();
            this.Index = int.MinValue;
            this.CursorPosition = cursorposition;
            this.IsInclude = isinclude;
            this.HasCustomName = true;
            this.Skip = false;
        }

        // Constructor for numbered script
        internal ScriptItem(int index, string name, List<string> argnames, int cursorposition, bool isinclude, bool customname, bool skip)
        {
            this.Name = name;
            this.argnames = argnames;
            this.Index = index;
            this.CursorPosition = cursorposition;
            this.IsInclude = isinclude;
            this.HasCustomName = customname;
            this.Skip = skip;
        }

        // Constructor for named script
        internal ScriptItem(string name, List<string> argnames, int cursorposition, bool isinclude, bool skip)
        {
            this.Name = name;
            this.argnames = argnames;
            this.Index = int.MinValue;
            this.CursorPosition = cursorposition;
            this.IsInclude = isinclude;
            this.HasCustomName = true;
            this.Skip = skip;
        }

        internal static int SortByIndex(ScriptItem i1, ScriptItem i2)
        {
            if (i1.IsInclude && !i2.IsInclude) return 1;
            if (!i1.IsInclude && i2.IsInclude) return -1;
            if (i1.Index > i2.Index) return 1;
            if (i1.Index == i2.Index) return 0;
            return -1;
        }

        internal static int SortByName(ScriptItem i1, ScriptItem i2)
        {
            if (i1.IsInclude && !i2.IsInclude) return 1;
            if (!i1.IsInclude && i2.IsInclude) return -1;

            if (i1.Name == i2.Name) return 0;
            if (i1.Name.Length == 0) return -1;
            if (i2.Name.Length == 0) return 1;
            if (i1.Name.ToUpper()[0] > i2.Name.ToUpper()[0]) return 1;
            if (i1.Name.ToUpper()[0] == i2.Name.ToUpper()[0])
            {
                int len = Math.Min(i1.Name.Length, i2.Name.Length);
                for (int i = 0; i < len; i++)
                {
                    if (i1.Name.ToUpper()[i] > i2.Name.ToUpper()[i]) return 1;
                    if (i1.Name.ToUpper()[i] < i2.Name.ToUpper()[i]) return -1;
                }
                if (i1.Name.Length > i2.Name.Length) return 1;
                return -1;
            }
            return -1;
        }

        // God awful, but will do...
        // [ZZ] for backwards compatibility with broken maxed's code
        internal string[] GetArgumentsDescriptions(int action)
        {
            int first;
            return GetArgumentsDescriptions(action, out first);
        }
        internal string[] GetArgumentsDescriptions(int action, out int first)
        {
            string[] result = new[] { Index == int.MinValue ? "Script Name" : "Script Number", string.Empty, string.Empty, string.Empty, string.Empty };
            first = 0;
            switch (action)
            {
                case 80:        //ACS_Execute (script, map, s_arg1, s_arg2, s_arg3)
                case 226: //ACS_ExecuteAlways (script, map, s_arg1, s_arg2, s_arg3)
                    argnames.CopyTo(0, result, 2, argnames.Count < 3 ? argnames.Count : 3);
                    first = 2;
                    break;

                case 83:     //ACS_LockedExecute (script, map, s_arg1, s_arg2, lock)
                case 85: //ACS_LockedExecuteDoor (script, map, s_arg1, s_arg2, lock)
                    argnames.CopyTo(0, result, 2, argnames.Count < 2 ? argnames.Count : 2);
                    first = 2;
                    break;

                case 84: //ACS_ExecuteWithResult (script, s_arg1, s_arg2, s_arg3, s_arg4)
                    argnames.CopyTo(0, result, 1, argnames.Count < 4 ? argnames.Count : 4);
                    first = 1;
                    break;

                case 81:   //ACS_Suspend (script, map)
                case 82: //ACS_Terminate (script, map)
                    return result;

                default:
                    return new[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
            }

            return result;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
