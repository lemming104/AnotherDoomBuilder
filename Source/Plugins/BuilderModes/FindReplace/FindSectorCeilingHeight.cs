using CodeImp.DoomBuilder.Map;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.FindReplace
{
    [FindReplace("Sector Height (Ceiling)", BrowseButton = false)]
    internal class FindSectorCeilingHeight : BaseFindSector
    {

        // This is called to perform a search (and replace)
        // Returns a list of items to show in the results list
        // replacewith is null when not replacing
        public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
        {
            List<FindReplaceObject> objs = new List<FindReplaceObject>();

            // Interpret the replacement
            int replaceheight = 0;
            if (replace)
            {
                // If it cannot be interpreted, set replacewith to null (not replacing at all)
                if (!int.TryParse(replacewith, out replaceheight)) replacewith = null;
                if (replacewith == null)
                {
                    MessageBox.Show("Invalid replace value for this search type!", "Find and Replace", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return objs.ToArray();
                }
            }

            // Interpret the number given
            int height;
            if (int.TryParse(value, out height))
            {
                // Where to search?
                ICollection<Sector> list = withinselection ? DoomBuilder.General.Map.Map.GetSelectedSectors(true) : DoomBuilder.General.Map.Map.Sectors;

                // Go for all sectors
                foreach (Sector s in list)
                {
                    // Height matches?
                    if (s.CeilHeight == height)
                    {
                        // Replace
                        if (replace) s.CeilHeight = replaceheight;

                        objs.Add(new FindReplaceObject(s, "Sector " + s.Index));
                    }
                }
            }

            //refresh map
            if (replace)
            {
                DoomBuilder.General.Map.Map.Update();
                DoomBuilder.General.Map.IsChanged = true;
            }

            return objs.ToArray();
        }
    }
}
