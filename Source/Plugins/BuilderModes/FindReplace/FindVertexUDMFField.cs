
/*
 * Copyright (c) 2021 Boris Iwanski
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */


using CodeImp.DoomBuilder.Map;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.BuilderModes.FindReplace
{
    [FindReplace("Vertex UDMF Field", BrowseButton = false)]
    internal class FindVertexUDMFField : BaseFindUDMFField
    {

        public override bool CanReplace()
        {
            return false;
        }

        public override bool DetermineVisiblity()
        {
            return DoomBuilder.General.Map.UDMF;
        }

        public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new FindReplaceObject[] { };

            ICollection<MapElement> list = withinselection ? new List<MapElement>(DoomBuilder.General.Map.Map.GetSelectedVertices(true)) : (ICollection<MapElement>)DoomBuilder.General.Map.Map.Vertices;

            return GetObjects(value, list);
        }
    }
}
