using CodeImp.DoomBuilder.Config;
using System;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Controls.Scripting
{
    internal class ScriptIconsManager
    {
        internal int ScriptTypeIconsOffset { get; }
        internal int ScriptGroupIconsOffset { get; }
        internal int ScriptGroupOpenIconsOffset { get; }
        public ImageList Icons { get; }

        public ScriptIconsManager(ImageList icons)
        {
            this.Icons = icons;

            int numicons = Enum.GetNames(typeof(ScriptType)).Length;
            ScriptGroupOpenIconsOffset = icons.Images.Count - numicons;
            ScriptGroupIconsOffset = ScriptGroupOpenIconsOffset - numicons;
            ScriptTypeIconsOffset = ScriptGroupIconsOffset - numicons;
        }

        public int GetResourceIcon(int datalocationtype)
        {
            return datalocationtype;
        }

        public int GetScriptIcon(ScriptType type)
        {
            int scripttype = (int)type + ScriptTypeIconsOffset;
            if (scripttype >= ScriptGroupIconsOffset) scripttype = ScriptTypeIconsOffset;
            return scripttype;
        }

        public int GetScriptFolderIcon(ScriptType type, bool opened)
        {
            int scripttype = (int)type;
            if (scripttype >= ScriptGroupIconsOffset - ScriptTypeIconsOffset)
                scripttype = ScriptTypeIconsOffset;

            if (opened) return ScriptGroupOpenIconsOffset + scripttype;
            return ScriptGroupIconsOffset + scripttype;
        }
    }
}
