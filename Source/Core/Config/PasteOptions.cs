
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

#endregion

namespace CodeImp.DoomBuilder.Config
{
    public class PasteOptions
    {
        #region ================== Constants

        public const int TAGS_KEEP = 0;
        public const int TAGS_RENUMBER = 1;
        public const int TAGS_REMOVE = 2;

        #endregion

        #region ================== Variables


        #endregion

        #region ================== Properties

        public int ChangeTags { get; set; }
        public bool RemoveActions { get; set; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        public PasteOptions()
        {
        }

        // Copy Constructor
        public PasteOptions(PasteOptions p)
        {
            this.ChangeTags = p.ChangeTags;
            this.RemoveActions = p.RemoveActions;
        }

        #endregion

        #region ================== Methods

        // Make a copy
        public PasteOptions Copy()
        {
            return new PasteOptions(this);
        }

        // This reads from configuration
        internal void ReadConfiguration(Configuration cfg, string path)
        {
            ChangeTags = cfg.ReadSetting(path + ".changetags", 0);
            RemoveActions = cfg.ReadSetting(path + ".removeactions", false);
        }

        // This writes to configuration
        internal void WriteConfiguration(Configuration cfg, string path)
        {
            cfg.WriteSetting(path + ".changetags", ChangeTags);
            cfg.WriteSetting(path + ".removeactions", RemoveActions);
        }

        #endregion
    }
}
