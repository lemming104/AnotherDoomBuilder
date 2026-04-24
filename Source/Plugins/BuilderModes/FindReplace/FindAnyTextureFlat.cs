

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


using CodeImp.DoomBuilder.BuilderModes.General;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderModes.FindReplace
{
    [FindReplace("Any Texture or Flat", BrowseButton = true)]
    internal class FindAnyTextureFlat : FindReplaceType
    {

        public override Image BrowseImage { get { return Properties.Resources.List_Images; } }
        public override string UsageHint
        {
            get
            {
                return "Supported wildcards:" + Environment.NewLine
                    + "* - zero or more characters" + Environment.NewLine
                    + "? - one character";
            }
        }

        //mxd. 
        public override bool CanReplace()
        {
            return DoomBuilder.General.Map.Config.MixTexturesFlats;
        }

        // This is called when the browse button is pressed
        public override string Browse(string initialvalue)
        {
            return DoomBuilder.General.Interface.BrowseTexture(BuilderPlug.Me.FindReplaceForm, initialvalue);
        }


        // This is called to perform a search (and replace)
        // Returns a list of items to show in the results list
        // replacewith is null when not replacing
        public override FindReplaceObject[] Find(string value, bool withinselection, bool replace, string replacewith, bool keepselection)
        {
            List<FindReplaceObject> objs = new List<FindReplaceObject>();

            // Interpret the replacement
            if (replace && (string.IsNullOrEmpty(replacewith) || replacewith.Length > DoomBuilder.General.Map.Config.MaxTextureNameLength))
            {
                MessageBox.Show("Invalid replace value for this search type!", "Find and Replace", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return objs.ToArray();
            }

            // Interpret the find
            bool isregex = value.IndexOf('*') != -1 || value.IndexOf('?') != -1; //mxd
            MatchingTextureSet set = new MatchingTextureSet(new Collection<string> { value.Trim() }); //mxd

            // Where to search?
            ICollection<Sector> seclist = withinselection ? DoomBuilder.General.Map.Map.GetSelectedSectors(true) : DoomBuilder.General.Map.Map.Sectors;
            ICollection<Sidedef> sidelist = withinselection ? DoomBuilder.General.Map.Map.GetSidedefsFromSelectedLinedefs(true) : DoomBuilder.General.Map.Map.Sidedefs;

            // Go for all sectors
            foreach (Sector s in seclist)
            {
                // Flat matches?
                if (set.IsMatch(s.CeilTexture))
                {
                    // Replace and add to list
                    if (replace) s.SetCeilTexture(replacewith);
                    objs.Add(new FindReplaceObject(s, "Sector " + s.Index + " (ceiling)" + (isregex ? " - " + s.CeilTexture : null)));
                }

                if (set.IsMatch(s.FloorTexture))
                {
                    // Replace and add to list
                    if (replace) s.SetFloorTexture(replacewith);
                    objs.Add(new FindReplaceObject(s, "Sector " + s.Index + " (floor)" + (isregex ? " - " + s.FloorTexture : null)));
                }
            }

            // Go for all sidedefs
            foreach (Sidedef sd in sidelist)
            {
                string side = sd.IsFront ? "front" : "back";

                if (set.IsMatch(sd.HighTexture) && (value != "-" || sd.HighRequired()))
                {
                    // Replace and add to list
                    if (replace) sd.SetTextureHigh(replacewith);
                    objs.Add(new FindReplaceObject(sd, "Sidedef " + sd.Index + " (" + side + ", high)" + (isregex ? " - " + sd.HighTexture : null)));
                }

                if (set.IsMatch(sd.MiddleTexture) && (value != "-" || sd.MiddleRequired()))
                {
                    // Replace and add to list
                    if (replace) sd.SetTextureMid(replacewith);
                    objs.Add(new FindReplaceObject(sd, "Sidedef " + sd.Index + " (" + side + ", middle)" + (isregex ? " - " + sd.MiddleTexture : null)));
                }

                if (set.IsMatch(sd.LowTexture) && (value != "-" || sd.LowRequired()))
                {
                    // Replace and add to list
                    if (replace) sd.SetTextureLow(replacewith);
                    objs.Add(new FindReplaceObject(sd, "Sidedef " + sd.Index + " (" + side + ", low)" + (isregex ? " - " + sd.LowTexture : null)));
                }
            }

            // When replacing, make sure we keep track of used textures
            if (replace)
            {
                DoomBuilder.General.Map.Data.UpdateUsedTextures();
                DoomBuilder.General.Map.Map.Update(); //mxd. And don't forget to update the view itself
                DoomBuilder.General.Map.IsChanged = true;
            }

            return objs.ToArray();
        }

        // This is called when a specific object is selected from the list
        public override void ObjectSelected(FindReplaceObject[] selection)
        {
            if (selection.Length == 1)
            {
                ZoomToSelection(selection);
                if (selection[0].Object is Sector)
                    DoomBuilder.General.Interface.ShowSectorInfo(selection[0].Sector);
                else if (selection[0].Object is Sidedef)
                    DoomBuilder.General.Interface.ShowLinedefInfo(selection[0].Sidedef.Line);
            }
            else
                DoomBuilder.General.Interface.HideInfo();

            DoomBuilder.General.Map.Map.ClearAllSelected();
            foreach (FindReplaceObject obj in selection)
            {
                if (obj.Object is Sector)
                    obj.Sector.Selected = true;
                else if (obj.Object is Sidedef)
                    obj.Sidedef.Line.Selected = true;
            }
        }

        // Render selection
        public override void PlotSelection(IRenderer2D renderer, FindReplaceObject[] selection)
        {
            foreach (FindReplaceObject o in selection)
            {
                if (o.Object is Sector)
                {
                    foreach (Sidedef sd in o.Sector.Sidedefs)
                    {
                        renderer.PlotLinedef(sd.Line, DoomBuilder.General.Colors.Selection);
                    }
                }
                else if (o.Object is Sidedef)
                {
                    renderer.PlotLinedef(o.Sidedef.Line, DoomBuilder.General.Colors.Selection);
                }
            }
        }

        //mxd
        public override void RenderOverlaySelection(IRenderer2D renderer, FindReplaceObject[] selection)
        {
            if (!DoomBuilder.General.Settings.UseHighlight) return;

            int color = DoomBuilder.General.Colors.Selection.WithAlpha(64).ToInt();
            foreach (FindReplaceObject o in selection)
            {
                if (o.Object is Sector) renderer.RenderHighlight(o.Sector.FlatVertices, color);
            }
        }

        // Edit objects
        public override void EditObjects(FindReplaceObject[] selection)
        {
            List<Sector> sectors = new List<Sector>(selection.Length);
            List<Linedef> lines = new List<Linedef>(selection.Length);
            foreach (FindReplaceObject o in selection)
            {
                if (o.Object is Sector)
                    sectors.Add(o.Sector);
                else
                    lines.Add(o.Sidedef.Line);
            }
            if (sectors.Count > 0) DoomBuilder.General.Interface.ShowEditSectors(sectors);
            if (lines.Count > 0) DoomBuilder.General.Interface.ShowEditLinedefs(lines);
        }
    }
}
