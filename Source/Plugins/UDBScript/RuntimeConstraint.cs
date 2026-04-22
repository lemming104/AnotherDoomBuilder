
/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */


using Jint;
using System.Diagnostics;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.UDBScript
{
    class RuntimeConstraint : Constraint
    {

        private const long CHECK_MILLISECONDS = 5000;

        private Stopwatch stopwatch;

        public RuntimeConstraint(Stopwatch stopwatch)
        {
            this.stopwatch = stopwatch;
        }

        public override void Reset()
        {
        }

        /// <summary>
        /// Checks how long the script has been running and asks the user if it should abort or keep running
        /// </summary>
        public override void Check()
        {
            if (stopwatch.ElapsedMilliseconds > CHECK_MILLISECONDS)
            {
                DialogResult result = MessageBox.Show("The script has been running for some time, want to stop it?", "Script", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                    throw new UserScriptAbortException();
                else
                {
                    stopwatch.Restart();
                }
            }
        }
    }
}
