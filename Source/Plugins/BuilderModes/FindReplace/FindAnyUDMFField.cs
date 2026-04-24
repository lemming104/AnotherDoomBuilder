
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
    [FindReplace("Any UDMF Field", BrowseButton = false)]
    internal class FindAnyUDMFField : BaseFindUDMFField
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

            List<MapElement> list = new List<MapElement>();

            if (withinselection)
            {
                list.AddRange(DoomBuilder.General.Map.Map.GetSelectedSectors(true));
                list.AddRange(DoomBuilder.General.Map.Map.GetSelectedLinedefs(true));

                foreach (Linedef ld in DoomBuilder.General.Map.Map.GetSelectedLinedefs(true))
                {
                    if (ld.Front != null && !ld.Front.IsDisposed)
                        list.Add(ld.Front);

                    if (ld.Back != null && !ld.Back.IsDisposed)
                        list.Add(ld.Back);
                }

                list.AddRange(DoomBuilder.General.Map.Map.GetSelectedThings(true));
                list.AddRange(DoomBuilder.General.Map.Map.GetSelectedVertices(true));
            }
            else
            {
                list.AddRange(DoomBuilder.General.Map.Map.Sectors);
                list.AddRange(DoomBuilder.General.Map.Map.Linedefs);
                list.AddRange(DoomBuilder.General.Map.Map.Sidedefs);
                list.AddRange(DoomBuilder.General.Map.Map.Things);
                list.AddRange(DoomBuilder.General.Map.Map.Vertices);
            }

            return GetObjects(value, list);
        }
    }
}
