

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
using System.Threading;

namespace CodeImp.DoomBuilder.BuilderModes
{
    [ErrorChecker("Check unknown flats", true, 40)]
    public class CheckUnknownFlats : ErrorChecker
    {

        private const int PROGRESS_STEP = 1000;

        // Constructor
        public CheckUnknownFlats()
        {
            // Total progress is done when all sectors are checked
            SetTotalProgress(General.Map.Map.Sectors.Count / PROGRESS_STEP);
        }

        // This runs the check
        public override void Run()
        {
            int progress = 0;
            int stepprogress = 0;

            // Go for all the sectors
            foreach (Sector s in General.Map.Map.Sectors)
            {
                // Check floor texture
                if (s.LongFloorTexture != MapSet.EmptyLongName && !General.Map.Data.GetFlatExists(s.FloorTexture))
                    SubmitResult(new ResultUnknownFlat(s, false));

                // Check ceiling texture
                if (s.LongCeilTexture != MapSet.EmptyLongName && !General.Map.Data.GetFlatExists(s.CeilTexture))
                    SubmitResult(new ResultUnknownFlat(s, true));

                // Handle thread interruption
                try { Thread.Sleep(0); }
                catch (ThreadInterruptedException) { return; }

                // We are making progress!
                if ((++progress / PROGRESS_STEP) > stepprogress)
                {
                    stepprogress = progress / PROGRESS_STEP;
                    AddProgress(1);
                }
            }
        }
    }
}
