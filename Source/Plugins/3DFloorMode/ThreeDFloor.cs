#region ================== Copyright (c) 2014 Boris Iwanski

/*
 * Copyright (c) 2014 Boris Iwanski
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

using CodeImp.DoomBuilder.BuilderModes;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CodeImp.DoomBuilder.ThreeDFloorMode
{
    public class ThreeDFloor
    {
        private Vector3D floorslope;
        private Vector3D ceilingslope;
        public static Rectangle controlsectorarea = new Rectangle(-512, 512, 512, -512);

        public Sector Sector { get; private set; }
        public List<Sector> TaggedSectors { get; set; }
        public List<Sector> SectorsToTag { get; set; }
        public List<Sector> SectorsToUntag { get; set; }
        public string BorderTexture { get; set; }
        public string TopFlat { get; set; }
        public string BottomFlat { get; set; }
        public int Type { get; set; }
        public int Flags { get; set; }
        public int Alpha { get; set; }
        public int Brightness { get; set; }
        public int TopHeight { get; set; }
        public int BottomHeight { get; set; }
        public bool IsNew { get; set; }
        public int UDMFTag { get; set; }
        public List<int> Tags { get; set; }
        public Vector3D FloorSlope { get { return floorslope; } set { floorslope = value; } }
        public double FloorSlopeOffset { get; set; }
        public Vector3D CeilingSlope { get { return ceilingslope; } set { ceilingslope = value; } }
        public double CeilingSlopeOffset { get; set; }
        public LinedefProperties LinedefProperties { get; }
        public SectorProperties SectorProperties { get; }

        public ThreeDFloor()
        {
            Sector = null;
            TaggedSectors = new List<Sector>();
            TopFlat = General.Settings.DefaultCeilingTexture;
            BottomFlat = General.Settings.DefaultFloorTexture;
            TopHeight = General.Settings.DefaultCeilingHeight;
            BottomHeight = General.Settings.DefaultFloorHeight;
            BorderTexture = General.Settings.DefaultTexture;
            Type = 1;
            Flags = 0;
            Tags = new List<int>();
            floorslope = new Vector3D(0.0f, 0.0f, 0.0f);
            FloorSlopeOffset = 0.0f;
            ceilingslope = new Vector3D(0.0f, 0.0f, 0.0f);
            CeilingSlopeOffset = 0.0f;

            LinedefProperties = null;
            SectorProperties = null;

            Alpha = 255;
        }

        public ThreeDFloor(Sector sector) : this(sector, General.Map.Map.Sectors)
        {
            // Nothing extra do do here
        }

        public ThreeDFloor(Sector sector, IEnumerable<Sector> potentialsectors)
        {
            if (sector == null)
                throw new Exception("Sector can't be null");

            this.Sector = sector;
            TaggedSectors = new List<Sector>();
            TopFlat = sector.CeilTexture;
            BottomFlat = sector.FloorTexture;
            TopHeight = sector.CeilHeight;
            BottomHeight = sector.FloorHeight;
            Brightness = sector.Brightness;
            Tags = new List<int>();
            floorslope = sector.FloorSlope;
            FloorSlopeOffset = sector.FloorSlopeOffset;
            ceilingslope = sector.CeilSlope;
            CeilingSlopeOffset = sector.CeilSlopeOffset;

            foreach (Sidedef sd in sector.Sidedefs)
            {
                if (sd.Line.Action == 160)
                {
                    BorderTexture = sd.MiddleTexture;
                    UDMFTag = sd.Line.Args[0];
                    Type = sd.Line.Args[1];
                    Flags = sd.Line.Args[2];
                    Alpha = sd.Line.Args[3];
                    LinedefProperties = new LinedefProperties(sd.Line);
                    SectorProperties = new SectorProperties(sector);

                    foreach (Sector s in BuilderPlug.GetSectorsByTag(potentialsectors, sd.Line.Args[0]))
                    {
                        if (!TaggedSectors.Contains(s))
                            TaggedSectors.Add(s);
                    }
                }
            }
        }

        public void BindTag(int tag, LinedefProperties ldprops)
        {
            Linedef line = null;

            // try to find an line without an action
            foreach (Sidedef sd in Sector.Sidedefs)
            {
                if (sd.Line.Action == 0 && sd.Line.Tag == 0 && line == null)
                    line = sd.Line;

                // if a line of the control sector already has the tag
                // nothing has to be done
                if (sd.Line.Args[0] == tag)
                {
                    return;
                }
            }

            // no lines without an action, so a line has to get split
            // find the longest line to split
            if (line == null)
            {
                line = Sector.Sidedefs.First().Line;

                foreach (Sidedef sd in Sector.Sidedefs)
                {
                    if (sd.Line.Length > line.Length)
                        line = sd.Line;
                }

                // Lines may not have a length of less than 1 after splitting
                if (line.Length / 2 < 1)
                    throw new Exception("Can't split more lines in Sector " + line.Front.Sector.Index.ToString() + ".");

                Vertex v = General.Map.Map.CreateVertex(line.Line.GetCoordinatesAt(0.5f));
                v.SnapToAccuracy();

                line = line.Split(v);

                General.Map.Map.Update();
                General.Interface.RedrawDisplay();
            }

            if (ldprops != null)
                ldprops.Apply(new List<Linedef>() { line }, false);

            line.Action = 160;
            line.Args[0] = tag;
            line.Args[1] = Type;
            line.Args[2] = Flags;
            line.Args[3] = Alpha;
        }

        public void UpdateGeometry()
        {
            if (Sector == null)
                throw new Exception("3D floor has no geometry");

            Sector.CeilHeight = TopHeight;
            Sector.FloorHeight = BottomHeight;
            Sector.SetCeilTexture(TopFlat);
            Sector.SetFloorTexture(BottomFlat);
            Sector.Brightness = Brightness;
            Sector.Tags = Tags;
            Sector.FloorSlope = floorslope;
            Sector.FloorSlopeOffset = FloorSlopeOffset;
            Sector.CeilSlope = ceilingslope;
            Sector.CeilSlopeOffset = CeilingSlopeOffset;

            foreach (Sidedef sd in Sector.Sidedefs)
            {
                sd.SetTextureMid(BorderTexture);

                if (sd.Line.Action == 160)
                {
                    // We need to update the linedef's args, but we can't do it directly because otherwise their state will not be saved for the undo snapshot,
                    // so we're using the linedef's update method
                    sd.Line.Update(sd.Line.GetFlags(), sd.Line.RawFlags, sd.Line.Activate, sd.Line.Tags, sd.Line.Action, new int[] { sd.Line.Args[0], Type, Flags, Alpha, sd.Line.Args[4] });
                }
            }
        }

        public bool CreateGeometry(List<int> tagblacklist, List<DrawnVertex> alldrawnvertices)
        {
            int newtag;

            return CreateGeometry(tagblacklist, alldrawnvertices, null, null, false, out newtag);
        }

        public bool CreateGeometry(List<int> tagblacklist, List<DrawnVertex> alldrawnvertices, LinedefProperties ldprops, SectorProperties sectorprops, bool forcenewtag, out int newtag)
        {
            List<Vertex> vertices = new List<Vertex>();
            Vector3D slopetopthingpos = new Vector3D(0, 0, 0);
            Vector3D slopebottomthingpos = new Vector3D(0, 0, 0);
            Line2D slopeline = new Line2D(0, 0, 0, 0);

            newtag = -1;

            // We need 5 vertices to draw the control sector
            if (alldrawnvertices.Count < 5)
            {
                General.Interface.DisplayStatus(StatusType.Warning, "Could not draw new sector: not enough vertices");
                return false;
            }

            // Get the first 5 vertices in the list and also remove them from the list, so that creating further
            // control sectors won't use them
            List<DrawnVertex> drawnvertices = alldrawnvertices.GetRange(0, 5);
            alldrawnvertices.RemoveRange(0, 5);

            // drawnvertices = BuilderPlug.Me.ControlSectorArea.GetNewControlSectorVertices();

            if (Tools.DrawLines(drawnvertices) == false)
            {
                General.Interface.DisplayStatus(StatusType.Warning, "Could not draw new sector");
                return false;
            }

            Sector = General.Map.Map.GetMarkedSectors(true)[0];

            if (sectorprops != null)
                sectorprops.Apply(new List<Sector>() { Sector }, false);

            Sector.FloorHeight = BottomHeight;
            Sector.CeilHeight = TopHeight;
            Sector.SetFloorTexture(BottomFlat);
            Sector.SetCeilTexture(TopFlat);
            Sector.Brightness = Brightness;
            Sector.FloorSlope = floorslope;
            Sector.FloorSlopeOffset = FloorSlopeOffset;
            Sector.CeilSlope = ceilingslope;
            Sector.CeilSlopeOffset = CeilingSlopeOffset;

            foreach (Sidedef sd in Sector.Sidedefs)
            {
                sd.Line.Front.SetTextureMid(BorderTexture);
            }

            if (!Sector.Fields.ContainsKey("user_managed_3d_floor"))
                Sector.Fields.Add("user_managed_3d_floor", new UniValue(UniversalType.Boolean, true));

            Sector.Fields["comment"] = new UniValue(UniversalType.String, "[!]DO NOT DELETE! This sector is managed by the 3D floor plugin.");

            // With multiple tag support in UDMF only one tag is needed, so bind it right away
            if (General.Map.UDMF == true && General.Map.Config.SectorMultiTag)
            {
                if (IsNew || forcenewtag)
                {
                    newtag = UDMFTag = BuilderPlug.Me.ControlSectorArea.GetNewSectorTag(tagblacklist);
                    tagblacklist.Add(UDMFTag);
                }

                BindTag(UDMFTag, ldprops);
            }

            return true;
        }

        public void Cleanup()
        {
            int taggedLines = 0;

            foreach (Sidedef sd in Sector.Sidedefs)
            {
                if (sd.Line.Action == 160 && BuilderPlug.GetSectorsByTag(sd.Line.Args[0]).Count == 0)
                {
                    sd.Line.Action = 0;

                    for (int i = 0; i < 5; i++)
                        sd.Line.Args[i] = 0;
                }

                if (sd.Line.Action != 0)
                    taggedLines++;
            }

            if (taggedLines == 0)
            {
                DeleteControlSector(Sector);
            }
        }

        private void DeleteControlSector(Sector sector)
        {
            if (sector == null)
                return;

            General.Map.Map.BeginAddRemove();

            // Get all the linedefs
            List<Linedef> lines = new List<Linedef>(sector.Sidedefs.Count);
            foreach (Sidedef side in sector.Sidedefs) lines.Add(side.Line);


            // Dispose the sector
            sector.Dispose();

            // Check all the lines
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                // If the line has become orphaned, remove it
                if ((lines[i].Front == null) && (lines[i].Back == null))
                {
                    // Remove line
                    lines[i].Dispose();
                }
                else
                {
                    // If the line only has a back side left, flip the line and sides
                    if ((lines[i].Front == null) && (lines[i].Back != null))
                    {
                        lines[i].FlipVertices();
                        lines[i].FlipSidedefs();
                    }

                    // Check textures.
                    if (lines[i].Front.MiddleRequired() && (lines[i].Front.MiddleTexture.Length == 0 || lines[i].Front.MiddleTexture == "-"))
                    {
                        if (lines[i].Front.HighTexture.Length > 0 && lines[i].Front.HighTexture != "-")
                        {
                            lines[i].Front.SetTextureMid(lines[i].Front.HighTexture);
                        }
                        else if (lines[i].Front.LowTexture.Length > 0 && lines[i].Front.LowTexture != "-")
                        {
                            lines[i].Front.SetTextureMid(lines[i].Front.LowTexture);
                        }
                    }

                    // Do we still need high/low textures?
                    lines[i].Front.RemoveUnneededTextures(false);

                    // Update sided flags
                    lines[i].ApplySidedFlags();
                }
            }

            General.Map.Map.EndAddRemove();
        }

        public void DeleteControlSector()
        {
            DeleteControlSector(Sector);
        }
    }
}
