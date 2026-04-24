

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


using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Windows;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.FindReplace
{
    [FindReplace("Thing Angle", BrowseButton = true)]
    internal class FindThingAngle : BaseFindThing
    {

        public override Presentation RenderPresentation { get { return Presentation.Things; } }
        public override Image BrowseImage { get { return Properties.Resources.Angle; } }

        // This is called when the browse button is pressed
        public override string Browse(string initialvalue)
        {
            int initangle;
            int.TryParse(initialvalue, out initangle);
            return AngleForm.ShowDialog(Form.ActiveForm, initangle).ToString();
        }


        // This is called to perform a search (and replace)
        // Returns a list of items to show in the results list
        // replacewith is null when not replacing
        public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
        {
            List<FindReplaceObject> objs = new List<FindReplaceObject>();

            // Interpret the replacement
            int replaceangle = 0;
            if (replace)
            {
                // If it cannot be interpreted, set replacewith to null (not replacing at all)
                if (!int.TryParse(replacewith, out replaceangle)) replacewith = null;
                if (replacewith == null)
                {
                    MessageBox.Show("Invalid replace value for this search type!", "Find and Replace", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return objs.ToArray();
                }
            }

            // Interpret the number given
            int angle;
            if (int.TryParse(value, out angle))
            {
                // Where to search?
                ICollection<Thing> list = withinselection ? DoomBuilder.General.Map.Map.GetSelectedThings(true) : DoomBuilder.General.Map.Map.Things;

                // Go for all things
                foreach (Thing t in list)
                {
                    // Match?
                    if (Angle2D.RealToDoom(t.Angle) == angle)
                    {
                        // Replace
                        if (replace) t.Rotate(Angle2D.DoomToReal(replaceangle));

                        // Add to list
                        ThingTypeInfo ti = DoomBuilder.General.Map.Data.GetThingInfo(t.Type);
                        objs.Add(new FindReplaceObject(t, "Thing " + t.Index + " (" + ti.Title + ")"));
                    }
                }
            }

            return objs.ToArray();
        }
    }
}
