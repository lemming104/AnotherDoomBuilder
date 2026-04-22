
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

namespace CodeImp.DoomBuilder.BuilderModes
{
    [FindReplace("Linedef UDMF Field", BrowseButton = false)]
    internal class FindLinedefUDMFField : BaseFindUDMFField
    {

        public override bool CanReplace()
        {
            return false;
        }

        public override bool DetermineVisiblity()
        {
            return General.Map.UDMF;
        }

        public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new FindReplaceObject[] { };

            ICollection<MapElement> list = withinselection ? new List<MapElement>(General.Map.Map.GetSelectedLinedefs(true)) : (ICollection<MapElement>)General.Map.Map.Linedefs;

            return GetObjects(value, list);
        }
    }
}
