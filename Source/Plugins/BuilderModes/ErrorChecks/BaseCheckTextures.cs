
/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * Copyright (c) 2019 Boris Iwanski
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */


using CodeImp.DoomBuilder.Map;
using System;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
    public abstract class BaseCheckTextures : ErrorChecker
    {

        private const int PROGRESS_STEP = 1000;

        protected Dictionary<int, Flags3DFloor> sector3dfloors;
        protected ActionFloorLowerToLowestTextures floorlowertolowest;
        protected ActionFloorRaiseToNextHigherTextures floorraisetonexthigher;
        protected ActionFloorRaiseToHighestTextures floorraisetohighest;

        // Constructor
        public BaseCheckTextures()
        {
            // Total progress is done when all lines are checked
            SetTotalProgress(DoomBuilder.General.Map.Map.Sidedefs.Count / PROGRESS_STEP);

            sector3dfloors = new Dictionary<int, Flags3DFloor>();
            floorlowertolowest = new ActionFloorLowerToLowestTextures();
            floorraisetonexthigher = new ActionFloorRaiseToNextHigherTextures();
            floorraisetohighest = new ActionFloorRaiseToHighestTextures();
        }

        [Flags]
        protected enum Flags3DFloor
        {
            UseUpper = 1,
            UseLower = 2,
            RenderInside = 4
        }

        // Create a cache of sectors that have 3D floors, with their flags relevant to the error checker
        protected void Build3DFloorCache()
        {
            // Skip if linedef action 160 isn't the ZDoom 3D floor special.
            if (DoomBuilder.General.Map.Config.GetLinedefActionInfo(160).Id != "Sector_Set3dFloor")
            {
                return;
            }

            foreach (Linedef ld in DoomBuilder.General.Map.Map.Linedefs)
            {
                if (ld.Action == 160)
                {
                    if ((ld.Args[1] & 4) == 4) // Type render inside
                    {
                        if (!sector3dfloors.ContainsKey(ld.Args[0]))
                            sector3dfloors.Add(ld.Args[0], Flags3DFloor.RenderInside);
                    }

                    if ((ld.Args[2] & 16) == 16) // Flag use upper
                    {
                        if (!sector3dfloors.ContainsKey(ld.Args[0]))
                            sector3dfloors.Add(ld.Args[0], Flags3DFloor.UseUpper);
                        else
                            sector3dfloors[ld.Args[0]] |= Flags3DFloor.UseUpper;
                    }

                    if ((ld.Args[2] & 32) == 32) // Flag use lower
                    {
                        if (!sector3dfloors.ContainsKey(ld.Args[0]))
                            sector3dfloors.Add(ld.Args[0], Flags3DFloor.UseLower);
                        else
                            sector3dfloors[ld.Args[0]] |= Flags3DFloor.UseLower;
                    }
                }
            }
        }
    }
}
