
#region ================== Copyright (c) 2010 Pascal vd Heiden

/*
 * Copyright (c) 2010 Pascal vd Heiden
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

using CodeImp.DoomBuilder.Map;
using System.Collections.Generic;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.CommentsPanel
{
    public class CommentInfo
    {
        // Properties
        public string Comment { get; }
        public List<MapElement> Elements { get; private set; }
        public DataGridViewRow Row { get; set; }

        // Constructor
        public CommentInfo(string comment, MapElement e)
        {
            this.Comment = comment;
            this.Elements = new List<MapElement>();
            this.AddElement(e);
            this.Row = null;
        }

        // This adds an element
        public void AddElement(MapElement e)
        {
            this.Elements.Add(e);
        }

        // This replaces the elements with those from another
        public void ReplaceElements(CommentInfo other)
        {
            this.Elements = other.Elements;
        }
    }
}
