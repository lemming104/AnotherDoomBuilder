

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
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.FindReplace
{
    [FindReplace("Thing Tag", BrowseButton = false)]
    internal class FindThingTag : BaseFindThing
    {

        public override Presentation RenderPresentation { get { return Presentation.Things; } }

        // This is called to test if the item should be displayed
        public override bool DetermineVisiblity()
        {
            return DoomBuilder.General.Map.FormatInterface.HasThingTag;
        }

        // This is called to perform a search (and replace)
        // Returns a list of items to show in the results list
        // replacewith is null when not replacing
        public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
        {
            List<FindReplaceObject> objs = new List<FindReplaceObject>();

            // Interpret the replacement
            int replacetag = 0;
            if (replace)
            {
                // If it cannot be interpreted, set replacewith to null (not replacing at all)
                if (!int.TryParse(replacewith, out replacetag)) replacewith = null;
                if (replacetag < 0) replacewith = null;
                if (replacetag > Int16.MaxValue) replacewith = null;
                if (replacewith == null)
                {
                    MessageBox.Show("Invalid replace value for this search type!", "Find and Replace", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return objs.ToArray();
                }
            }

            // Interpret the number given
            int tag;
            if (int.TryParse(value, out tag))
            {
                // Where to search?
                ICollection<Thing> list = withinselection ? DoomBuilder.General.Map.Map.GetSelectedThings(true) : DoomBuilder.General.Map.Map.Things;

                // Go for all things
                foreach (Thing t in list)
                {
                    // Match?
                    if (t.Tag == tag)
                    {
                        // Replace
                        if (replace) t.Tag = replacetag;

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
