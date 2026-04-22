

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


using CodeImp.DoomBuilder.Map;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.BuilderModes
{
    [FindReplace("Sidedef Index", BrowseButton = false)]
    internal class FindSidedefNumber : BaseFindSidedef
    {

        //mxd
        public override bool CanReplace()
        {
            return false;
        }

        // This is called to perform a search (and replace)
        // Returns a list of items to show in the results list
        // replacewith is null when not replacing
        public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
        {
            List<FindReplaceObject> objs = new List<FindReplaceObject>();

            // Interpret the number given
            int index;
            if (int.TryParse(value, out index))
            {
                Sidedef sd = General.Map.Map.GetSidedefByIndex(index);
                if (sd != null) objs.Add(new FindReplaceObject(sd, "Sidedef " + index));
            }

            return objs.ToArray();
        }
    }
}
