

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


using CodeImp.DoomBuilder.IO;

namespace CodeImp.DoomBuilder.Config
{
    public class PasteOptions
    {

        public const int TAGS_KEEP = 0;
        public const int TAGS_RENUMBER = 1;
        public const int TAGS_REMOVE = 2;

        private int changetags;             // See TAGS_ constants
        private bool removeactions;

        public int ChangeTags { get { return changetags; } set { changetags = value; } }
        public bool RemoveActions { get { return removeactions; } set { removeactions = value; } }

        // Constructor
        public PasteOptions()
        {
        }

        // Copy Constructor
        public PasteOptions(PasteOptions p)
        {
            this.changetags = p.changetags;
            this.removeactions = p.removeactions;
        }

        // Make a copy
        public PasteOptions Copy()
        {
            return new PasteOptions(this);
        }

        // This reads from configuration
        internal void ReadConfiguration(Configuration cfg, string path)
        {
            changetags = cfg.ReadSetting(path + ".changetags", 0);
            removeactions = cfg.ReadSetting(path + ".removeactions", false);
        }

        // This writes to configuration
        internal void WriteConfiguration(Configuration cfg, string path)
        {
            cfg.WriteSetting(path + ".changetags", changetags);
            cfg.WriteSetting(path + ".removeactions", removeactions);
        }
    }
}
