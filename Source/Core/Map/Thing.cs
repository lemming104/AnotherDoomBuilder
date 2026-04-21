
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

using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.GZBuilder;
using CodeImp.DoomBuilder.GZBuilder.Data;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.VisualModes;
using System;
using System.Collections.Generic;
using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.Map
{
    public sealed class Thing : SelectableElement, ITaggedMapElement
    {
        #region ================== Constants

        public const int NUM_ARGS = 5;
        public static readonly HashSet<ThingRenderMode> AlignableRenderModes = new HashSet<ThingRenderMode>
        {
            ThingRenderMode.FLATSPRITE, ThingRenderMode.WALLSPRITE, ThingRenderMode.MODEL
        };

        #endregion

        #region ================== Variables

        // Map

        // Sector

        // List items
        private LinkedListNode<Thing> selecteditem;

        // Properties
        private int type;
        private GZGeneral.LightData dynamiclighttype;
        private Vector3D pos;
        private int angledoom;      // Angle as entered / stored in file
        private int tag;
        private int action;
        private double scaleX; //mxd
        private double scaleY; //mxd
        private SizeF spritescale; //mxd
        private int pitch; //mxd. Used in model rendering
        private int roll; //mxd. Used in model rendering

        //mxd. GZDoom rendering properties
        private bool rollsprite; //mxd

        // Configuration
        private PixelColor color;

        // biwa. This should only ever be used for temporary player starts for the "test from current position" action
        private bool recordundo;

        // Rendering

        #endregion

        #region ================== Properties

        public MapSet Map { get; private set; }
        public int Type { get { return type; } set { BeforePropsChange(); type = value; } } //mxd
        public GZGeneral.LightData DynamicLightType { get { return dynamiclighttype; } internal set { BeforePropsChange(); dynamiclighttype = value; } }
        public Vector3D Position { get { return pos; } }
        public double ScaleX { get { return scaleX; } } //mxd. This is UDMF property, not actual scale!
        public double ScaleY { get { return scaleY; } } //mxd. This is UDMF property, not actual scale!
        public int Pitch { get { return pitch; } } //mxd
        public double PitchRad { get; private set; }
        public int Roll { get { return roll; } } //mxd
        public double RollRad { get; private set; }
        public SizeF ActorScale { get { return spritescale; } } //mxd. Actor scale set in DECORATE
        public double Angle { get; private set; }
        public int AngleDoom { get { return angledoom; } }
        internal Dictionary<string, bool> Flags { get; private set; }
        public ushort RawFlags { get; private set; }
        public int Action { get { return action; } set { BeforePropsChange(); action = value; } }
        public int[] Args { get; private set; }
        public float Size { get; private set; }
        public float RenderSize { get; private set; }
        public float Height { get; private set; } //mxd
        public PixelColor Color { get { return color; } }
        public bool FixedSize { get; private set; }
        public int Tag { get { return tag; } set { BeforePropsChange(); tag = value; if ((tag < General.Map.FormatInterface.MinTag) || (tag > General.Map.FormatInterface.MaxTag)) throw new ArgumentOutOfRangeException("Tag", "Invalid tag number"); } }
        public Sector Sector { get; private set; }
        public ThingRenderMode RenderMode { get; private set; } //mxd
        public bool IsDirectional { get; private set; } //mxd
        public bool Highlighted { get; set; } //mxd
        internal int LastProcessed { get; set; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal Thing(MapSet map, int listindex, bool recordundo = true)
        {
            // Initialize
            this.elementtype = MapElementType.THING; //mxd
            this.Map = map;
            this.listindex = listindex;
            this.Flags = new Dictionary<string, bool>(StringComparer.Ordinal);
            this.Args = new int[NUM_ARGS];
            this.scaleX = 1.0f;
            this.scaleY = 1.0f;
            this.spritescale = new SizeF(1.0f, 1.0f);
            this.recordundo = recordundo;

            if (map == General.Map.Map && recordundo)
                General.Map.UndoRedo.RecAddThing(this);

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Disposer
        public override void Dispose()
        {
            // Not already disposed?
            if (!isdisposed)
            {
                if (Map == General.Map.Map && recordundo)
                    General.Map.UndoRedo.RecRemThing(this);

                // Remove from main list
                Map.RemoveThing(listindex);

                // Clean up
                Map = null;
                Sector = null;

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
                General.Map.UndoRedo.RecPrpThing(this);
        }

        // Serialize / deserialize
        new internal void ReadWrite(IReadWriteStream s)
        {
            if (!s.IsWriting) BeforePropsChange();

            base.ReadWrite(s);

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

            s.rwInt(ref type);
            s.rwVector3D(ref pos);
            s.rwInt(ref angledoom);
            s.rwInt(ref pitch); //mxd
            s.rwInt(ref roll); //mxd
            s.rwDouble(ref scaleX); //mxd
            s.rwDouble(ref scaleY); //mxd
            s.rwInt(ref tag);
            s.rwInt(ref action);
            for (int i = 0; i < NUM_ARGS; i++) s.rwInt(ref Args[i]);

            if (!s.IsWriting)
            {
                Angle = Angle2D.DoomToReal(angledoom);
                UpdateCache(); //mxd
            }
        }

        // This copies all properties to another thing
        public void CopyPropertiesTo(Thing t)
        {
            t.BeforePropsChange();

            // Copy properties
            t.type = type;
            t.dynamiclighttype = dynamiclighttype;
            t.Angle = Angle;
            t.angledoom = angledoom;
            t.roll = roll; //mxd
            t.pitch = pitch; //mxd
            t.RollRad = RollRad; //mxd
            t.PitchRad = PitchRad; //mxd
            t.scaleX = scaleX; //mxd
            t.scaleY = scaleY; //mxd
            t.spritescale = spritescale; //mxd
            t.pos = pos;
            t.Flags = new Dictionary<string, bool>(Flags);
            t.RawFlags = RawFlags;
            t.tag = tag;
            t.action = action;
            t.Args = (int[])Args.Clone();
            t.Size = Size;
            t.RenderSize = RenderSize;
            t.Height = Height; //mxd
            t.color = color;
            t.IsDirectional = IsDirectional;
            t.FixedSize = FixedSize;
            t.RenderMode = RenderMode; //mxd
            t.rollsprite = rollsprite; //mxd

            base.CopyPropertiesTo(t);
        }

        /// <summary>
        /// Updates the raw flag bit map from the flags dictionary. Has to be called before the flags in the game config changed. Has to be called in conjunction with UpdateFlagsFromRawFlags.
        /// </summary>
        internal void UpdateRawFlagsFromFlags()
        {
            foreach (KeyValuePair<string, bool> f in Flags)
            {
                if (ushort.TryParse(f.Key, out ushort fnum))
                {
                    // Set bit to 0
                    RawFlags &= (ushort)~fnum;

                    // Set bit if necessary
                    if (f.Value)
                        RawFlags |= fnum;
                }
            }
        }

        /// <summary>
        /// Updates the flags dictionary from the raw flags. Has to be called after the flags in the game config changed. Has to be called in conjunction with UpdateRawFlagsFromFlags.
        /// </summary>
        internal void UpdateFlagsFromRawFlags()
        {
            foreach (string fname in General.Map.Config.ThingFlags.Keys)
            {
                if (ushort.TryParse(fname, out ushort fnum))
                {
                    Flags[fname] = (RawFlags & fnum) == fnum;
                }
            }
        }

        // This determines which sector the thing is in and links it
        public void DetermineSector()
        {
            //mxd
            Sector = Map.GetSectorByCoordinates(pos);
        }

        /// <summary>
        /// Determines what sector a thing is in, given a blockmap
        /// </summary>
        /// <param name="blockmap">The blockmap to use</param>
        public void DetermineSector(BlockMap<BlockEntry> blockmap)
        {
            BlockEntry be = blockmap.GetBlockAt(pos);
            List<Sector> sectors = new List<Sector>(1);

            foreach (Sector s in be.Sectors)
                if (s.Intersect(pos))
                    sectors.Add(s);

            if (sectors.Count == 0)
            {
                Sector = null;
            }
            else if (sectors.Count == 1)
            {
                Sector = sectors[0];
            }
            else
            {
                // Having multiple intersections indicates that there are self-referencing sectors in this spot.
                // In this case we have to check which side of the nearest linedef pos is on, and then use that sector
                HashSet<Linedef> linedefs = new HashSet<Linedef>(sectors[0].Sidedefs.Count * sectors.Count);

                foreach (Sector s in sectors)
                    foreach (Sidedef sd in s.Sidedefs)
                        linedefs.Add(sd.Line);

                Linedef nearest = MapSet.NearestLinedef(linedefs, pos);
                double d = nearest.SideOfLine(pos);

                if (d <= 0.0 && nearest.Front != null)
                    Sector = nearest.Front.Sector;
                else if (nearest.Back != null)
                    Sector = nearest.Back.Sector;
                else
                    Sector = null;
            }
        }

        // This determines which sector the thing is in and links it
        public void DetermineSector(VisualBlockMap blockmap)
        {
            Sector = blockmap.GetSectorAt(pos);
        }

        // This translates the flags into UDMF fields
        internal void TranslateToUDMF()
        {
            // First make a single integer with all flags
            int bits = 0;
            int flagbit;
            foreach (KeyValuePair<string, bool> f in Flags)
                if (int.TryParse(f.Key, out flagbit) && f.Value) bits |= flagbit;

            // Now make the new flags
            Flags.Clear();
            foreach (FlagTranslation f in General.Map.Config.ThingFlagsTranslation)
            {
                // Flag found in bits?
                if ((bits & f.Flag) == f.Flag)
                {
                    // Add fields and remove bits
                    bits &= ~f.Flag;
                    for (int i = 0; i < f.Fields.Count; i++)
                        Flags[f.Fields[i]] = f.FieldValues[i];
                }
                else
                {
                    // Add fields with inverted value
                    for (int i = 0; i < f.Fields.Count; i++)
                        Flags[f.Fields[i]] = !f.FieldValues[i];
                }
            }
        }

        // This translates UDMF fields back into the normal flags
        internal void TranslateFromUDMF()
        {
            //mxd. Clear UDMF-related properties
            this.Fields.Clear();
            scaleX = 1.0f;
            scaleY = 1.0f;
            pitch = 0;
            PitchRad = 0;
            roll = 0;
            RollRad = 0;

            // Make copy of the flags
            Dictionary<string, bool> oldfields = new Dictionary<string, bool>(Flags);

            // Make the flags
            Flags.Clear();
            foreach (KeyValuePair<string, string> f in General.Map.Config.ThingFlags)
            {
                // Flag must be numeric
                int flagbit;
                if (int.TryParse(f.Key, out flagbit))
                {
                    foreach (FlagTranslation ft in General.Map.Config.ThingFlagsTranslation)
                    {
                        if (ft.Flag == flagbit)
                        {
                            // Only set this flag when the fields match
                            bool fieldsmatch = true;
                            for (int i = 0; i < ft.Fields.Count; i++)
                            {
                                if (!oldfields.ContainsKey(ft.Fields[i]) || (oldfields[ft.Fields[i]] != ft.FieldValues[i]))
                                {
                                    fieldsmatch = false;
                                    break;
                                }
                            }

                            // Field match? Then add the flag.
                            if (fieldsmatch)
                            {
                                Flags.Add(f.Key, true);
                                break;
                            }
                        }
                    }
                }
            }
        }

        // Selected
        protected override void DoSelect()
        {
            base.DoSelect();
            selecteditem = Map.SelectedThings.AddLast(this);
        }

        // Deselect
        protected override void DoUnselect()
        {
            base.DoUnselect();
            if (selecteditem.List != null) selecteditem.List.Remove(selecteditem);
            selecteditem = null;
        }

        #endregion

        #region ================== Changes

        // This moves the thing
        // NOTE: This does not update sector! (call DetermineSector)
        public void Move(Vector3D newpos)
        {
            if (newpos != pos)
            {
                BeforePropsChange();

                // Change position
                this.pos = newpos;

                if (type != General.Map.Config.Start3DModeThingType)
                    General.Map.IsChanged = true;
            }
        }

        // This moves the thing
        // NOTE: This does not update sector! (call DetermineSector)
        public void Move(Vector2D newpos)
        {
            Vector3D p = new Vector3D(newpos.x, newpos.y, pos.z);

            if (p != pos)
            {
                BeforePropsChange();

                // Change position
                this.pos = p;

                if (type != General.Map.Config.Start3DModeThingType)
                    General.Map.IsChanged = true;
            }
        }

        // This moves the thing
        // NOTE: This does not update sector! (call DetermineSector)
        public void Move(double x, double y, double zoffset)
        {
            Move(new Vector3D(x, y, zoffset));
        }

        // This rotates the thing
        public void Rotate(double newangle)
        {
            BeforePropsChange();

            // Change angle
            this.Angle = newangle;
            this.angledoom = Angle2D.RealToDoom(newangle);

            if (type != General.Map.Config.Start3DModeThingType)
                General.Map.IsChanged = true;
        }

        // This rotates the thing
        public void Rotate(int newangle)
        {
            BeforePropsChange();

            // Change angle
            Angle = Angle2D.DoomToReal(newangle);
            angledoom = newangle;

            if (type != General.Map.Config.Start3DModeThingType)
                General.Map.IsChanged = true;
        }

        //mxd
        public void SetPitch(int newpitch)
        {
            BeforePropsChange();

            pitch = General.ClampAngle(newpitch);

            switch (RenderMode)
            {
                case ThingRenderMode.MODEL:
                    double pmult = General.Map.Config.BuggyModelDefPitch ? 1 : -1;
                    ModelData md = General.Map.Data.ModeldefEntries[type];
                    if (md.InheritActorPitch || md.UseActorPitch)
                        PitchRad = Angle2D.DegToRad(pmult * (md.InheritActorPitch ? -pitch : pitch));
                    else
                        PitchRad = 0;
                    break;

                case ThingRenderMode.FLATSPRITE:
                    PitchRad = Angle2D.DegToRad(pitch);
                    break;

                default:
                    PitchRad = 0;
                    break;
            }

            if (type != General.Map.Config.Start3DModeThingType)
                General.Map.IsChanged = true;
        }

        //mxd
        public void SetRoll(int newroll)
        {
            BeforePropsChange();

            roll = General.ClampAngle(newroll);
            RollRad = (rollsprite || (RenderMode == ThingRenderMode.MODEL && General.Map.Data.ModeldefEntries[type].UseActorRoll))
                ? Angle2D.DegToRad(roll) : 0;

            if (type != General.Map.Config.Start3DModeThingType)
                General.Map.IsChanged = true;
        }

        //mxd
        public void SetScale(double scalex, double scaley)
        {
            BeforePropsChange();

            scaleX = scalex;
            scaleY = scaley;

            if (type != General.Map.Config.Start3DModeThingType)
                General.Map.IsChanged = true;
        }

        // This updates all properties
        // NOTE: This does not update sector! (call DetermineSector)
        public void Update(int type, double x, double y, double zoffset, int angle, int pitch, int roll, double scaleX, double scaleY,
                           Dictionary<string, bool> flags, ushort rawflags, int tag, int action, int[] args)
        {
            // Apply changes
            this.type = type;
            this.Angle = Angle2D.DoomToReal(angle);
            this.angledoom = angle;
            this.pitch = pitch; //mxd
            this.roll = roll; //mxd
            this.scaleX = scaleX == 0 ? 1.0f : scaleX; //mxd
            this.scaleY = scaleY == 0 ? 1.0f : scaleY; //mxd
            this.Flags = new Dictionary<string, bool>(flags);
            this.RawFlags = rawflags;
            this.tag = tag;
            this.action = action;
            this.Args = new int[NUM_ARGS];
            args.CopyTo(this.Args, 0);
            this.Move(x, y, zoffset);

            UpdateCache(); //mxd
        }

        // This updates the settings from configuration
        public void UpdateConfiguration()
        {
            // Lookup settings
            ThingTypeInfo ti = General.Map.Data.GetThingInfo(type);

            // Apply size
            dynamiclighttype = GZGeneral.GetGZLightTypeByClass(ti.Actor);
            if (dynamiclighttype == null)
                dynamiclighttype = ti.DynamicLightType;
            //General.ErrorLogger.Add(ErrorType.Warning, string.Format("thing dynamiclighttype is {0}; class is {1}", dynamiclighttype, ti.Actor.ClassName));
            Size = ti.Radius;
            RenderSize = ti.RenderRadius;
            Height = ti.Height; //mxd
            FixedSize = ti.FixedSize;
            spritescale = ti.SpriteScale; //mxd

            //mxd. Apply radius and height overrides?
            for (int i = 0; i < ti.Args.Length; i++)
            {
                if (ti.Args[i] == null) continue;
                if (ti.Args[i].Type == (int)UniversalType.ThingRadius && Args[i] > 0)
                    Size = Args[i];
                else if (ti.Args[i].Type == (int)UniversalType.ThingHeight && Args[i] > 0)
                    Height = Args[i];
            }

            // Color valid?
            if ((ti.Color >= 0) && (ti.Color < ColorCollection.NUM_THING_COLORS))
            {
                // Apply color
                color = General.Colors.Colors[ti.Color + ColorCollection.THING_COLORS_OFFSET];
            }
            else
            {
                // Unknown thing color
                color = General.Colors.Colors[ColorCollection.THING_COLORS_OFFSET];
            }

            IsDirectional = ti.Arrow; //mxd
            RenderMode = ti.RenderMode; //mxd
            rollsprite = ti.RollSprite; //mxd
            UpdateCache(); //mxd
        }

        //mxd. This checks if the thing has model override and whether pitch/roll values should be used
        internal void UpdateCache()
        {
            if (General.Map.Data == null) return;

            // Check if the thing has model override
            if (General.Map.Data.ModeldefEntries.ContainsKey(type))
            {
                ModelData md = General.Map.Data.ModeldefEntries[type];
                if ((md.LoadState == ModelLoadState.None && General.Map.Data.ProcessModel(type)) || md.LoadState != ModelLoadState.None)
                    RenderMode = General.Map.Data.ModeldefEntries[type].IsVoxel ? ThingRenderMode.VOXEL : ThingRenderMode.MODEL;
            }
            else // reset rendermode if we SUDDENLY became a sprite out of a model. otherwise it crashes violently.
            {
                ThingTypeInfo ti = General.Map.Data.GetThingInfo(Type);
                RenderMode = (ti != null) ? ti.RenderMode : ThingRenderMode.NORMAL;
            }

            // Update radian versions of pitch and roll
            switch (RenderMode)
            {
                case ThingRenderMode.MODEL:
                    float pmult = General.Map.Config.BuggyModelDefPitch ? 1 : -1;
                    ModelData md = General.Map.Data.ModeldefEntries[type];
                    RollRad = md.UseActorRoll ? Angle2D.DegToRad(roll) : 0;
                    PitchRad = (md.InheritActorPitch || md.UseActorPitch) ? Angle2D.DegToRad(pmult * (md.InheritActorPitch ? -pitch : pitch)) : 0;
                    break;

                case ThingRenderMode.FLATSPRITE:
                    RollRad = Angle2D.DegToRad(roll);
                    PitchRad = Angle2D.DegToRad(pitch);
                    break;

                case ThingRenderMode.WALLSPRITE:
                    RollRad = Angle2D.DegToRad(roll);
                    PitchRad = 0;
                    break;

                case ThingRenderMode.NORMAL:
                    RollRad = rollsprite ? Angle2D.DegToRad(roll) : 0;
                    PitchRad = 0;
                    break;

                case ThingRenderMode.VOXEL:
                    RollRad = 0;
                    PitchRad = 0;
                    break;

                default: throw new NotImplementedException("Unknown ThingRenderMode");
            }
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

        // This snaps the vertex to the grid
        public void SnapToGrid()
        {
            // Calculate nearest grid coordinates
            this.Move(General.Map.Grid.SnappedToGrid(pos));
        }

        // This snaps the vertex to the map format accuracy
        public void SnapToAccuracy()
        {
            SnapToAccuracy(true);
        }

        // This snaps the vertex to the map format accuracy
        public void SnapToAccuracy(bool usepreciseposition)
        {
            // Round the coordinates
            Vector3D newpos = new Vector3D(Math.Round(pos.x, usepreciseposition ? General.Map.FormatInterface.VertexDecimals : 0),
                                           Math.Round(pos.y, usepreciseposition ? General.Map.FormatInterface.VertexDecimals : 0),
                                           Math.Round(pos.z, usepreciseposition ? General.Map.FormatInterface.VertexDecimals : 0));
            this.Move(newpos);
        }

        // This returns the distance from given coordinates
        public double DistanceToSq(Vector2D p)
        {
            return Vector2D.DistanceSq(p, pos);
        }

        // This returns the distance from given coordinates
        public double DistanceTo(Vector2D p)
        {
            return Vector2D.Distance(p, pos);
        }

        /// <summary>
        /// Changes the thing's index to a new index.
        /// </summary>
        /// <param name="newindex">The new index to set</param>
        public void ChangeIndex(int newindex)
        {
            General.Map.UndoRedo.RecIndexThing(Index, newindex);
            Map?.ChangeThingIndex(Index, newindex);
        }

        #endregion
    }
}
