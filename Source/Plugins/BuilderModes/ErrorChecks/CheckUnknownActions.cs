using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;
using System.Threading;

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
    [ErrorChecker("Check unknown actions/effects", true, 50)]
    public class CheckUnknownActions : ErrorChecker
    {
        private const int PROGRESS_STEP = 1000;

        // Constructor
        public CheckUnknownActions()
        {
            int count = DoomBuilder.General.Map.Map.Sectors.Count + DoomBuilder.General.Map.Map.Linedefs.Count;
            if (DoomBuilder.General.Map.FormatInterface.HasThingAction) count += DoomBuilder.General.Map.Map.Things.Count;

            // Total progress is done when all relevant map elements are checked
            SetTotalProgress(count / PROGRESS_STEP);
        }

        // This runs the check
        public override void Run()
        {
            int progress = 0;
            int stepprogress = 0;

            // Go for all Linedefs
            foreach (Linedef l in DoomBuilder.General.Map.Map.Linedefs)
            {
                // Check action...
                if (l.Action != 0 && !DoomBuilder.General.Map.Config.LinedefActions.ContainsKey(l.Action)
                   && !GameConfiguration.IsGeneralized(l.Action))
                {
                    SubmitResult(new ResultUnknownLinedefAction(l));
                }

                // Handle thread interruption
                try { Thread.Sleep(0); } catch (ThreadInterruptedException) { return; }

                // We are making progress!
                if ((++progress / PROGRESS_STEP) > stepprogress)
                {
                    stepprogress = progress / PROGRESS_STEP;
                    AddProgress(1);
                }
            }

            // Go for all Sectors
            foreach (Sector s in DoomBuilder.General.Map.Map.Sectors)
            {
                // Check action...
                if (s.Effect != 0 && !DoomBuilder.General.Map.Config.SectorEffects.ContainsKey(s.Effect)
                   && !GameConfiguration.IsGeneralizedSectorEffect(s.Effect))
                {
                    SubmitResult(new ResultUnknownSectorEffect(s));
                }

                // Handle thread interruption
                try { Thread.Sleep(0); } catch (ThreadInterruptedException) { return; }

                // We are making progress!
                if ((++progress / PROGRESS_STEP) > stepprogress)
                {
                    stepprogress = progress / PROGRESS_STEP;
                    AddProgress(1);
                }
            }

            // Go for all things?
            if (!DoomBuilder.General.Map.FormatInterface.HasThingAction) return;
            foreach (Thing t in DoomBuilder.General.Map.Map.Things)
            {
                // Check action...
                if (t.Action != 0 && !DoomBuilder.General.Map.Config.LinedefActions.ContainsKey(t.Action)
                   && !GameConfiguration.IsGeneralized(t.Action))
                {
                    SubmitResult(new ResultUnknownThingAction(t));
                }

                // Handle thread interruption
                try { Thread.Sleep(0); } catch (ThreadInterruptedException) { return; }

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
