using CodeImp.DoomBuilder.GZBuilder;
using CodeImp.DoomBuilder.Map;
using System;
using System.Threading;

namespace CodeImp.DoomBuilder.BuilderModes.ErrorChecks
{
    [ErrorChecker("Check unknown ACS scripts", true, 50)]
    public class CheckUnknownScripts : ErrorChecker
    {

        private const int PROGRESS_STEP = 1000;

        // Only possible in Hexen/UDMF map formats
        public override bool SkipCheck { get { return !DoomBuilder.General.Map.UDMF && !DoomBuilder.General.Map.HEXEN; } }

        public CheckUnknownScripts()
        {
            // Total progress is done when all things are checked
            SetTotalProgress((DoomBuilder.General.Map.Map.Things.Count + DoomBuilder.General.Map.Map.Linedefs.Count) / PROGRESS_STEP);
        }

        // This runs the check
        public override void Run()
        {
            int progress = 0;
            int stepprogress = 0;

            // Go for all linedefs
            foreach (Linedef l in DoomBuilder.General.Map.Map.Linedefs)
            {
                bool isacsscript = Array.IndexOf(GZGeneral.ACS_SPECIALS, l.Action) != -1;
                bool isnamedacsscript = isacsscript && DoomBuilder.General.Map.UDMF && l.Fields.ContainsKey("arg0str");

                if (isnamedacsscript)
                {
                    string scriptname = l.Fields.GetValue("arg0str", string.Empty);
                    if (!DoomBuilder.General.Map.ScriptNameExists(scriptname))
                        SubmitResult(new ResultUnknownLinedefScript(l, true));
                }
                else if (isacsscript && !DoomBuilder.General.Map.ScriptNumberExists(l.Args[0]))
                {
                    SubmitResult(new ResultUnknownLinedefScript(l, false));
                }

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

            // Go for all things
            foreach (Thing t in DoomBuilder.General.Map.Map.Things)
            {
                bool isacsscript = Array.IndexOf(GZGeneral.ACS_SPECIALS, t.Action) != -1;
                bool isnamedacsscript = isacsscript && DoomBuilder.General.Map.UDMF && t.Fields.ContainsKey("arg0str");

                if (isnamedacsscript)
                {
                    string scriptname = t.Fields.GetValue("arg0str", string.Empty);
                    if (!DoomBuilder.General.Map.ScriptNameExists(scriptname))
                        SubmitResult(new ResultUnknownThingScript(t, true));
                }
                else if (isacsscript && !DoomBuilder.General.Map.ScriptNumberExists(t.Args[0]))
                {
                    SubmitResult(new ResultUnknownThingScript(t, false));
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
