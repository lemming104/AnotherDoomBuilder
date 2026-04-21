
#region ================== Copyright (c) 2007 Pascal vd Heiden

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

#endregion

#region ================== Namespaces

using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.Map
{
    public enum SectorFogMode //mxd
    {
        NONE,              // no fog
        CLASSIC,           // black fog when sector brightness < 243
        FOGDENSITY,        // sector uses "fogdensity" MAPINFO property
        OUTSIDEFOGDENSITY, // sector uses "outsidefogdensity" MAPINFO property
        FADE               // sector uses UDMF "fade" sector property
    }

    public sealed class Sector : SelectableElement, IMultiTaggedMapElement
    {
        #region ================== Variables

        // Map

        // List items
        private LinkedListNode<Sector> selecteditem;

        // Sidedefs
        private LinkedList<Sidedef> sidedefs;

        // Properties
        private int fixedindex;
        private int floorheight;
        private int ceilheight;
        private string floortexname;
        private string ceiltexname;
        private long longfloortexname;
        private long longceiltexname;
        private int effect;
        private List<int> tags; //mxd
        private int brightness;

        //mxd. UDMF properties

        // Cloning

        // Triangulation
        private bool updateneeded;
        private bool triangulationneeded;
        private RectangleF bbox;
        private readonly SurfaceEntryCollection surfaceentries;

        //mxd. Rendering
        private Color4 fogcolor;

        //mxd. Slopes
        private Vector3D floorslope;
        private double flooroffset;
        private Vector3D ceilslope;
        private double ceiloffset;

        #endregion

        #region ================== Properties

        public MapSet Map { get; private set; }
        public ICollection<Sidedef> Sidedefs { get { return sidedefs; } }

        /// <summary>
        /// An unique index that does not change when other sectors are removed.
        /// </summary>
        public int FixedIndex { get { return fixedindex; } }
        public int FloorHeight { get { return floorheight; } set { BeforePropsChange(); floorheight = value; } }
        public int CeilHeight { get { return ceilheight; } set { BeforePropsChange(); ceilheight = value; } }
        public string FloorTexture { get { return floortexname; } }
        public string CeilTexture { get { return ceiltexname; } }
        public long LongFloorTexture { get { return longfloortexname; } }
        public long LongCeilTexture { get { return longceiltexname; } }
        public bool HasSkyCeiling { get { return ceiltexname == General.Map.Config.SkyFlatName; } }
        public bool HasSkyFloor { get { return floortexname == General.Map.Config.SkyFlatName; } }

        internal Dictionary<string, bool> Flags { get; private set; } //mxd
        public int Effect { get { return effect; } set { BeforePropsChange(); effect = value; } }
        public int Tag { get { return tags[0]; } set { BeforePropsChange(); tags[0] = value; if ((value < General.Map.FormatInterface.MinTag) || (value > General.Map.FormatInterface.MaxTag)) throw new ArgumentOutOfRangeException("Tag", "Invalid tag number"); } } //mxd
        public List<int> Tags { get { return tags; } set { BeforePropsChange(); tags = value; } } //mxd
        public int Brightness { get { return brightness; } set { BeforePropsChange(); brightness = value; updateneeded = true; } }
        public bool UpdateNeeded { get { return updateneeded; } set { updateneeded |= value; triangulationneeded |= value; } }
        public RectangleF BBox { get { return bbox; } }
        internal Sector Clone { get; set; }
        internal int SerializedIndex { get; set; }
        public Triangulation Triangles { get; private set; }
        public FlatVertex[] FlatVertices { get; private set; }
        public ReadOnlyCollection<LabelPositionInfo> Labels { get; private set; }

        //mxd. Rednering
        public Color4 FogColor { get { return fogcolor; } }
        public SectorFogMode FogMode { get; private set; }
        public double Desaturation
        {
            get
            {
                if (General.Map.UDMF && Fields.ContainsKey("desaturation"))
                    return (double)Fields["desaturation"].Value;
                return 0f;
            }
        }

        //mxd. Slopes
        public Vector3D FloorSlope { get { return floorslope; } set { BeforePropsChange(); floorslope = value; updateneeded = true; } }
        public double FloorSlopeOffset { get { return flooroffset; } set { BeforePropsChange(); flooroffset = value; updateneeded = true; } }
        public Vector3D CeilSlope { get { return ceilslope; } set { BeforePropsChange(); ceilslope = value; updateneeded = true; } }
        public double CeilSlopeOffset { get { return ceiloffset; } set { BeforePropsChange(); ceiloffset = value; updateneeded = true; } }
        internal int LastProcessed { get; set; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal Sector(MapSet map, int listindex, int index)
        {
            // Initialize
            this.elementtype = MapElementType.SECTOR; //mxd
            this.Map = map;
            this.listindex = listindex;
            this.sidedefs = new LinkedList<Sidedef>();
            this.fixedindex = index;
            this.floortexname = "-";
            this.ceiltexname = "-";
            this.longfloortexname = MapSet.EmptyLongName;
            this.longceiltexname = MapSet.EmptyLongName;
            this.Flags = new Dictionary<string, bool>(StringComparer.Ordinal); //mxd
            this.tags = new List<int> { 0 }; //mxd
            this.updateneeded = true;
            this.triangulationneeded = true;
            this.Triangles = new Triangulation(); //mxd
            this.surfaceentries = new SurfaceEntryCollection();

            if (map == General.Map.Map)
                General.Map.UndoRedo.RecAddSector(this);

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Disposer
        public override void Dispose()
        {
            // Not already disposed?
            if (!isdisposed)
            {
                // Already set isdisposed so that changes can be prohibited
                isdisposed = true;

                // Dispose the sidedefs that are attached to this sector
                // because a sidedef cannot exist without reference to its sector.
                if (Map.AutoRemove)
                    foreach (Sidedef sd in sidedefs) sd.Dispose();
                else
                    foreach (Sidedef sd in sidedefs) sd.SetSectorP(null);

                if (Map == General.Map.Map)
                    General.Map.UndoRedo.RecRemSector(this);

                // Remove from main list
                Map.RemoveSector(listindex);

                // Register the index as free
                Map.AddSectorIndexHole(fixedindex);

                // Free surface entry
                General.Map.CRenderer2D.Surfaces.FreeSurfaces(surfaceentries);

                // Clean up
                sidedefs = null;
                Map = null;

                //mxd. Restore isdisposed so base classes can do their disposal job
                isdisposed = false;

                // Dispose base
                base.Dispose();
            }
        }

        #endregion

        #region ================== Management

        // Call this before changing properties
        protected override void BeforePropsChange()
        {
            if (Map == General.Map.Map)
                General.Map.UndoRedo.RecPrpSector(this);
        }

        // Serialize / deserialize (passive: this doesn't record)
        new internal void ReadWrite(IReadWriteStream s)
        {
            if (!s.IsWriting)
            {
                BeforePropsChange();
                updateneeded = true;
            }

            base.ReadWrite(s);

            //mxd
            if (s.IsWriting)
            {
                s.wInt(Flags.Count);

                foreach (KeyValuePair<string, bool> f in Flags)
                {
                    s.wString(f.Key);
                    s.wBool(f.Value);
                }
            }
            else
            {
                int c; s.rInt(out c);

                Flags = new Dictionary<string, bool>(c, StringComparer.Ordinal);
                for (int i = 0; i < c; i++)
                {
                    string t; s.rString(out t);
                    bool b; s.rBool(out b);
                    Flags.Add(t, b);
                }
            }

            s.rwInt(ref fixedindex);
            s.rwInt(ref floorheight);
            s.rwInt(ref ceilheight);
            s.rwString(ref floortexname);
            s.rwString(ref ceiltexname);
            s.rwLong(ref longfloortexname);
            s.rwLong(ref longceiltexname);
            s.rwInt(ref effect);
            s.rwInt(ref brightness);

            //mxd. (Re)store tags
            if (s.IsWriting)
            {
                s.wInt(tags.Count);
                foreach (int tag in tags) s.wInt(tag);
            }
            else
            {
                int c;
                s.rInt(out c);
                tags = new List<int>(c);
                for (int i = 0; i < c; i++)
                {
                    int t;
                    s.rInt(out t);
                    tags.Add(t);
                }
            }

            //mxd. Slopes
            s.rwDouble(ref flooroffset);
            s.rwVector3D(ref floorslope);
            s.rwDouble(ref ceiloffset);
            s.rwVector3D(ref ceilslope);
        }

        // After deserialization
        internal void PostDeserialize(MapSet map)
        {
            Triangles.PostDeserialize(map);
            updateneeded = true;
            triangulationneeded = true;
        }

        // This copies all properties to another sector
        public void CopyPropertiesTo(Sector s)
        {
            s.BeforePropsChange();

            // Copy properties
            s.ceilheight = ceilheight;
            s.ceiltexname = ceiltexname;
            s.longceiltexname = longceiltexname;
            s.floorheight = floorheight;
            s.floortexname = floortexname;
            s.longfloortexname = longfloortexname;
            s.effect = effect;
            s.tags = new List<int>(tags); //mxd
            s.Flags = new Dictionary<string, bool>(Flags); //mxd
            s.brightness = brightness;
            s.flooroffset = flooroffset; //mxd
            s.floorslope = floorslope; //mxd
            s.ceiloffset = ceiloffset; //mxd
            s.ceilslope = ceilslope; //mxd
            s.updateneeded = true;
            base.CopyPropertiesTo(s);
        }

        // This attaches a sidedef and returns the listitem
        internal LinkedListNode<Sidedef> AttachSidedefP(Sidedef sd)
        {
            updateneeded = true;
            triangulationneeded = true;
            return sidedefs.AddLast(sd);
        }

        // This detaches a sidedef
        internal void DetachSidedefP(LinkedListNode<Sidedef> l)
        {
            // Not disposing?
            if (!isdisposed)
            {
                // Remove sidedef
                updateneeded = true;
                triangulationneeded = true;
                sidedefs.Remove(l);

                // No more sidedefs left?
                if ((sidedefs.Count == 0) && Map.AutoRemove)
                {
                    // This sector is now useless, dispose it
                    this.Dispose();
                }
            }
        }

        // This triangulates the sector geometry
        internal void Triangulate()
        {
            if (updateneeded)
            {
                // Triangulate again?
                if (triangulationneeded || (Triangles == null))
                {
                    // Triangulate sector
                    Triangles = Triangulation.Create(this);
                    triangulationneeded = false;
                    updateneeded = true;

                    // Make label positions
                    Labels = Array.AsReadOnly(Tools.FindLabelPositions(this).ToArray());

                    // Number of vertices changed?
                    if (Triangles.Vertices.Count != surfaceentries.totalvertices)
                        General.Map.CRenderer2D.Surfaces.FreeSurfaces(surfaceentries);
                }
            }
        }

        // This makes new vertices as well as floor and ceiling surfaces
        internal void CreateSurfaces()
        {
            if (updateneeded)
            {
                // Brightness color
                int brightint = General.Map.Renderer2D.CalculateBrightness(brightness);

                // Make vertices
                FlatVertices = new FlatVertex[Triangles.Vertices.Count];
                for (int i = 0; i < Triangles.Vertices.Count; i++)
                {
                    FlatVertices[i].x = (float)Triangles.Vertices[i].x;
                    FlatVertices[i].y = (float)Triangles.Vertices[i].y;
                    FlatVertices[i].z = 1.0f;
                    FlatVertices[i].c = brightint;
                    FlatVertices[i].u = FlatVertices[i].x;
                    FlatVertices[i].v = FlatVertices[i].y;
                }

                // Create bounding box
                bbox = CreateBBox();

                // Make update info (this lets the plugin fill in texture coordinates and such)
                SurfaceUpdate updateinfo = new SurfaceUpdate(FlatVertices.Length, true, true);
                FlatVertices.CopyTo(updateinfo.floorvertices, 0);
                General.Plugins.OnSectorFloorSurfaceUpdate(this, ref updateinfo.floorvertices);
                FlatVertices.CopyTo(updateinfo.ceilvertices, 0);
                General.Plugins.OnSectorCeilingSurfaceUpdate(this, ref updateinfo.ceilvertices);
                updateinfo.floortexture = longfloortexname;
                updateinfo.ceiltexture = longceiltexname;
                updateinfo.hidden = IsFlagSet("hidden");
                updateinfo.desaturation = this.Desaturation;

                // Update surfaces
                General.Map.CRenderer2D.Surfaces.UpdateSurfaces(surfaceentries, updateinfo);

                // Updated
                updateneeded = false;
            }
        }

        // This updates the floor surface
        public void UpdateFloorSurface()
        {
            if (FlatVertices == null) return;

            // Create floor vertices
            SurfaceUpdate updateinfo = new SurfaceUpdate(FlatVertices.Length, true, false);
            FlatVertices.CopyTo(updateinfo.floorvertices, 0);
            General.Plugins.OnSectorFloorSurfaceUpdate(this, ref updateinfo.floorvertices);
            updateinfo.floortexture = longfloortexname;
            updateinfo.hidden = IsFlagSet("hidden");
            updateinfo.desaturation = this.Desaturation;

            // Update entry
            General.Map.CRenderer2D.Surfaces.UpdateSurfaces(surfaceentries, updateinfo);
            General.Map.CRenderer2D.Surfaces.UnlockBuffers();
        }

        // This updates the ceiling surface
        public void UpdateCeilingSurface()
        {
            if (FlatVertices == null) return;

            // Create ceiling vertices
            SurfaceUpdate updateinfo = new SurfaceUpdate(FlatVertices.Length, false, true);
            FlatVertices.CopyTo(updateinfo.ceilvertices, 0);
            General.Plugins.OnSectorCeilingSurfaceUpdate(this, ref updateinfo.ceilvertices);
            updateinfo.ceiltexture = longceiltexname;
            updateinfo.hidden = IsFlagSet("hidden");
            updateinfo.desaturation = this.Desaturation;

            // Update entry
            General.Map.CRenderer2D.Surfaces.UpdateSurfaces(surfaceentries, updateinfo);
            General.Map.CRenderer2D.Surfaces.UnlockBuffers();
        }

        // This updates the sector when changes have been made
        public void UpdateCache()
        {
            // Update if needed
            if (updateneeded)
            {
                Triangulate();

                CreateSurfaces();

                General.Map.CRenderer2D.Surfaces.UnlockBuffers();
            }
        }

        // Selected
        protected override void DoSelect()
        {
            base.DoSelect();
            selecteditem = Map.SelectedSectors.AddLast(this);
        }

        // Deselect
        protected override void DoUnselect()
        {
            base.DoUnselect();
            if (selecteditem.List != null) selecteditem.List.Remove(selecteditem);
            selecteditem = null;
        }

        // This removes UDMF stuff (mxd)
        internal void TranslateFromUDMF()
        {
            // Clear UDMF-related properties (but keep VirtualSectorField!)
            bool isvirtual = this.Fields.ContainsKey(MapSet.VirtualSectorField);
            this.Fields.Clear();
            if (isvirtual) this.Fields.Add(MapSet.VirtualSectorField, MapSet.VirtualSectorValue);
            this.Flags.Clear();
            this.FogMode = SectorFogMode.NONE;

            // Reset Slopes
            floorslope = new Vector3D();
            flooroffset = 0;
            ceilslope = new Vector3D();
            ceiloffset = 0;
        }

        #endregion

        #region ================== Methods

        // This checks and returns a flag without creating it
        public bool IsFlagSet(string flagname)
        {
            return Flags.ContainsKey(flagname) && Flags[flagname];
        }

        // This sets a flag
        public void SetFlag(string flagname, bool value)
        {
            if (!Flags.ContainsKey(flagname) || (IsFlagSet(flagname) != value))
            {
                BeforePropsChange();

                Flags[flagname] = value;

                // [XA] TODO: de-hardcode this special case thing
                if (flagname == "hidden")
                    updateneeded = true;
            }
        }

        // This returns a copy of the flags dictionary
        public Dictionary<string, bool> GetFlags()
        {
            return new Dictionary<string, bool>(Flags);
        }

        //mxd. This returns enabled flags
        public HashSet<string> GetEnabledFlags()
        {
            HashSet<string> result = new HashSet<string>();
            foreach (KeyValuePair<string, bool> group in Flags)
                if (group.Value) result.Add(group.Key);
            return result;
        }

        // This clears all flags
        public void ClearFlags()
        {
            BeforePropsChange();
            Flags.Clear();
        }

        // This checks if the given point is inside the sector polygon
        // See: http://paulbourke.net/geometry/polygonmesh/index.html#insidepoly
        public bool Intersect(Vector2D p) { return Intersect(p, true); }
        public bool Intersect(Vector2D p, bool countontopastrue)
        {
            //mxd. Check bounding box first
            if (p.x < bbox.Left || p.x > bbox.Right || p.y < bbox.Top || p.y > bbox.Bottom) return false;

            uint c = 0;
            Vector2D v1, v2;
            bool selfreferencing = true;

            // Go for all linedefs
            foreach (Sidedef sd in sidedefs)
            {
                // Get vertices
                v1 = sd.Line.Start.Position;
                v2 = sd.Line.End.Position;

                //mxd. On top of a vertex?
                if (p == v1 || p == v2) return countontopastrue;

                // If both sidedefs of any one line aren't referencing the same sector, then it's not a self-referencing sector
                if (sd.Other == null || (sd.Other != null && sd.Other.Sector != this))
                    selfreferencing = false;

                // Check for intersection
                if (v1.y != v2.y //mxd. If line is not horizontal...
                  && p.y > (v1.y < v2.y ? v1.y : v2.y) //mxd. ...And test point y intersects with the line y bounds...
                  && p.y <= (v1.y > v2.y ? v1.y : v2.y) //mxd
                  && (p.x < (v1.x < v2.x ? v1.x : v2.x) || (p.x <= (v1.x > v2.x ? v1.x : v2.x) //mxd. ...And test point x is to the left of the line, or is inside line x bounds and intersects it
                        && (v1.x == v2.x || p.x <= (((p.y - v1.y) * (v2.x - v1.x) / (v2.y - v1.y)) + v1.x)))))
                    c++; //mxd. ...Count the line as crossed
            }

            // Inside this polygon when we crossed odd number of polygon lines (for non-self-referencing sectors)
            if (!selfreferencing)
                return c % 2 != 0;
            else
                return c % 2 == 0;
        }

        // This creates a bounding box rectangle
        private RectangleF CreateBBox()
        {
            if (sidedefs.Count == 0) return new RectangleF(); //mxd

            // Setup
            double left = double.MaxValue;
            double top = double.MaxValue;
            double right = double.MinValue;
            double bottom = double.MinValue;

            HashSet<Vertex> processed = new HashSet<Vertex>(); //mxd

            foreach (Sidedef s in sidedefs)
            {
                //start...
                if (!processed.Contains(s.Line.Start))
                {
                    if (s.Line.Start.Position.x < left) left = s.Line.Start.Position.x;
                    if (s.Line.Start.Position.x > right) right = s.Line.Start.Position.x;
                    if (s.Line.Start.Position.y < top) top = s.Line.Start.Position.y;
                    if (s.Line.Start.Position.y > bottom) bottom = s.Line.Start.Position.y;
                    processed.Add(s.Line.Start);
                }

                //end...
                if (!processed.Contains(s.Line.End))
                {
                    if (s.Line.End.Position.x < left) left = s.Line.End.Position.x;
                    if (s.Line.End.Position.x > right) right = s.Line.End.Position.x;
                    if (s.Line.End.Position.y < top) top = s.Line.End.Position.y;
                    if (s.Line.End.Position.y > bottom) bottom = s.Line.End.Position.y;
                    processed.Add(s.Line.End);
                }
            }

            // Return rectangle
            return new RectangleF((float)left, (float)top, (float)(right - left), (float)(bottom - top));
        }

        //mxd
        public void UpdateBBox()
        {
            bbox = CreateBBox();
        }

        // This joins the sector with another sector
        // This sector will be disposed
        public void Join(Sector other)
        {
            // Any sidedefs to move?
            if (sidedefs.Count > 0)
            {
                // Change secter reference on my sidedefs
                // This automatically disposes this sector
                while (sidedefs != null)
                    sidedefs.First.Value.SetSector(other);
            }
            else
            {
                // No sidedefs attached
                // Dispose manually
                this.Dispose();
            }

            General.Map.IsChanged = true;
        }

        //mxd
        public static Geometry.Plane GetFloorPlane(Sector s)
        {
            if (General.Map.UDMF)
            {
                // UDMF Sector slope?
                if (s.FloorSlope.GetLengthSq() > 0 && !double.IsNaN(s.FloorSlopeOffset / s.FloorSlope.z))
                    return new Geometry.Plane(s.FloorSlope, s.FloorSlopeOffset);

                if (s.sidedefs.Count == 3)
                {
                    Geometry.Plane floor = new Geometry.Plane(new Vector3D(0, 0, 1), -s.FloorHeight);
                    Vector3D[] verts = new Vector3D[3];
                    bool sloped = false;
                    int index = 0;

                    // Check vertices
                    foreach (Sidedef sd in s.Sidedefs)
                    {
                        Vertex v = sd.IsFront ? sd.Line.End : sd.Line.Start;

                        //create "normal" vertices
                        verts[index] = new Vector3D(v.Position);

                        // Check floor
                        if (!double.IsNaN(v.ZFloor))
                        {
                            //vertex offset is absolute
                            verts[index].z = v.ZFloor;
                            sloped = true;
                        }
                        else
                        {
                            verts[index].z = floor.GetZ(v.Position);
                        }

                        index++;
                    }

                    // Have slope?
                    return sloped ? new Geometry.Plane(verts[0], verts[1], verts[2], true) : floor;
                }
            }

            // Have line slope?
            foreach (Sidedef side in s.sidedefs)
            {
                // Carbon copy of EffectLineSlope class here...
                if (side.Line.HasActionPlaneAlign() && ((side.Line.Args[0] == 1 && side == side.Line.Front) || side.Line.Args[0] == 2) && side.Other != null)
                {
                    Linedef l = side.Line;

                    // Find the vertex furthest from the line
                    Vertex foundv = null;
                    double founddist = -1.0f;
                    foreach (Sidedef sd in s.Sidedefs)
                    {
                        Vertex v = sd.IsFront ? sd.Line.Start : sd.Line.End;
                        double d = l.DistanceToSq(v.Position, false);
                        if (d > founddist)
                        {
                            foundv = v;
                            founddist = d;
                        }
                    }

                    Vector3D v1 = new Vector3D(l.Start.Position.x, l.Start.Position.y, side.Other.Sector.FloorHeight);
                    Vector3D v2 = new Vector3D(l.End.Position.x, l.End.Position.y, side.Other.Sector.FloorHeight);
                    Vector3D v3 = new Vector3D(foundv.Position.x, foundv.Position.y, s.FloorHeight);

                    return l.SideOfLine(v3) < 0.0f ? new Geometry.Plane(v1, v2, v3, true) : new Geometry.Plane(v2, v1, v3, true);
                }
            }

            //TODO: other types of slopes...

            // Normal (flat) floor plane
            return new Geometry.Plane(new Vector3D(0, 0, 1), -s.FloorHeight);
        }

        //mxd
        public static Geometry.Plane GetCeilingPlane(Sector s)
        {
            if (General.Map.UDMF)
            {
                // UDMF Sector slope?
                if (s.CeilSlope.GetLengthSq() > 0 && !double.IsNaN(s.CeilSlopeOffset / s.CeilSlope.z))
                    return new Geometry.Plane(s.CeilSlope, s.CeilSlopeOffset);

                if (s.sidedefs.Count == 3)
                {
                    Geometry.Plane ceiling = new Geometry.Plane(new Vector3D(0, 0, -1), s.CeilHeight);
                    Vector3D[] verts = new Vector3D[3];
                    bool sloped = false;
                    int index = 0;

                    // Check vertices
                    foreach (Sidedef sd in s.Sidedefs)
                    {
                        Vertex v = sd.IsFront ? sd.Line.End : sd.Line.Start;

                        //create "normal" vertices
                        verts[index] = new Vector3D(v.Position);

                        // Check floor
                        if (!double.IsNaN(v.ZCeiling))
                        {
                            //vertex offset is absolute
                            verts[index].z = v.ZCeiling;
                            sloped = true;
                        }
                        else
                        {
                            verts[index].z = ceiling.GetZ(v.Position);
                        }

                        index++;
                    }

                    // Have slope?
                    return sloped ? new Geometry.Plane(verts[0], verts[2], verts[1], false) : ceiling;
                }
            }

            // Have line slope?
            foreach (Sidedef side in s.sidedefs)
            {
                // Carbon copy of EffectLineSlope class here...
                if (side.Line.HasActionPlaneAlign() && ((side.Line.Args[1] == 1 && side == side.Line.Front) || side.Line.Args[1] == 2) && side.Other != null)
                {
                    Linedef l = side.Line;

                    // Find the vertex furthest from the line
                    Vertex foundv = null;
                    double founddist = -1.0f;
                    foreach (Sidedef sd in s.Sidedefs)
                    {
                        Vertex v = sd.IsFront ? sd.Line.Start : sd.Line.End;
                        double d = l.DistanceToSq(v.Position, false);
                        if (d > founddist)
                        {
                            foundv = v;
                            founddist = d;
                        }
                    }

                    Vector3D v1 = new Vector3D(l.Start.Position.x, l.Start.Position.y, side.Other.Sector.CeilHeight);
                    Vector3D v2 = new Vector3D(l.End.Position.x, l.End.Position.y, side.Other.Sector.CeilHeight);
                    Vector3D v3 = new Vector3D(foundv.Position.x, foundv.Position.y, s.CeilHeight);

                    return l.SideOfLine(v3) > 0.0f ? new Geometry.Plane(v1, v2, v3, false) : new Geometry.Plane(v2, v1, v3, false);
                }
            }

            //TODO: other types of slopes...

            // Normal (flat) ceiling plane
            return new Geometry.Plane(new Vector3D(0, 0, -1), s.CeilHeight);
        }

        /// <summary>
        /// Changes the sector's index to a new index.
        /// </summary>
        /// <param name="newindex">The new index to set</param>
        public void ChangeIndex(int newindex)
        {
            General.Map.UndoRedo.RecIndexSector(Index, newindex);
            Map?.ChangeSectorIndex(Index, newindex);
        }

        // String representation
        public override string ToString()
        {
#if DEBUG
            return "Sector " + listindex + (marked ? " (marked)" : ""); //mxd
#else
			return "Sector " + listindex;
#endif
        }

        #endregion

        #region ================== Changes

        //mxd. This updates all properties (Doom/Hexen version)
        public void Update(int hfloor, int hceil, string tfloor, string tceil, int effect, int tag, int brightness)
        {
            Update(hfloor, hceil, tfloor, tceil, effect, new Dictionary<string, bool>(StringComparer.Ordinal), new List<int> { tag }, brightness, 0, new Vector3D(), 0, new Vector3D());
        }

        //mxd. This updates all properties (UDMF version)
        public void Update(int hfloor, int hceil, string tfloor, string tceil, int effect, Dictionary<string, bool> flags, List<int> tags, int brightness, double flooroffset, Vector3D floorslope, double ceiloffset, Vector3D ceilslope)
        {
            BeforePropsChange();

            // Apply changes
            this.floorheight = hfloor;
            this.ceilheight = hceil;
            this.effect = effect;
            this.tags = new List<int>(tags); //mxd
            this.Flags = new Dictionary<string, bool>(flags); //mxd
            this.brightness = brightness;
            this.flooroffset = flooroffset; //mxd
            this.floorslope = floorslope; //mxd
            this.ceiloffset = ceiloffset; //mxd
            this.ceilslope = ceilslope; //mxd

            //mxd. Set ceil texture
            if (string.IsNullOrEmpty(tceil)) tceil = "-";
            ceiltexname = tceil;
            longceiltexname = Lump.MakeLongName(ceiltexname);

            //mxd. Set floor texture
            if (string.IsNullOrEmpty(tfloor)) tfloor = "-"; //mxd
            floortexname = tfloor;
            longfloortexname = Lump.MakeLongName(tfloor);

            //mxd. Map is changed
            General.Map.IsChanged = true;
            updateneeded = true;
        }

        // This sets texture
        public void SetFloorTexture(string name)
        {
            BeforePropsChange();

            if (string.IsNullOrEmpty(name)) name = "-"; //mxd
            floortexname = name;
            longfloortexname = Lump.MakeLongName(name);
            updateneeded = true;
            General.Map.IsChanged = true;
        }

        // This sets texture
        public void SetCeilTexture(string name)
        {
            BeforePropsChange();

            if (string.IsNullOrEmpty(name)) name = "-"; //mxd
            ceiltexname = name;
            longceiltexname = Lump.MakeLongName(name);
            updateneeded = true;
            General.Map.IsChanged = true;
        }

        //mxd
        public void UpdateFogColor()
        {
            if (General.Map.UDMF && Fields.ContainsKey("fadecolor"))
            {
                fogcolor = new Color4((int)Fields["fadecolor"].Value);
                FogMode = SectorFogMode.FADE;
            }
            // Sector uses outisde fog when it's ceiling is sky or Sector_Outside effect (87) is set
            else if (General.Map.Data.MapInfo.HasOutsideFogColor &&
                (HasSkyCeiling || (effect == 87 && General.Map.Config.SectorEffects.ContainsKey(effect))))
            {
                fogcolor = General.Map.Data.MapInfo.OutsideFogColor;
                FogMode = SectorFogMode.OUTSIDEFOGDENSITY;
            }
            else if (General.Map.Data.MapInfo.HasFadeColor)
            {
                fogcolor = General.Map.Data.MapInfo.FadeColor;
                FogMode = SectorFogMode.FOGDENSITY;
            }
            else
            {
                fogcolor = new Color4();
                FogMode = brightness < 248 ? SectorFogMode.CLASSIC : SectorFogMode.NONE;
            }
        }

        #endregion
    }
}
