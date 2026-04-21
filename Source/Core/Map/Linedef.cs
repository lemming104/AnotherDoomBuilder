
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
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.IO;
using System;
using System.Collections.Generic;
using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.Map
{
    public sealed class Linedef : SelectableElement, IMultiTaggedMapElement
    {
        #region ================== Constants

        public const double SIDE_POINT_DISTANCE = 0.01;
        public const int NUM_ARGS = 5;

        #endregion

        #region ================== Variables

        // Map

        // List items
        private LinkedListNode<Linedef> startvertexlistitem;
        private LinkedListNode<Linedef> endvertexlistitem;
        private LinkedListNode<Linedef> selecteditem;

        // Vertices

        // Sidedefs

        // Cache
        private bool updateneeded;
        private double lengthsqinv;
        private RectangleF rect;

        // Properties
        private int action;
        private int activate;
        private List<int> tags; //mxd

        // Clone

        // Rendering

        #endregion

        #region ================== Properties

        public MapSet Map { get; private set; }
        public Vertex Start { get; private set; }
        public Vertex End { get; private set; }
        public Sidedef Front { get; private set; }
        public Sidedef Back { get; private set; }
        public Line2D Line { get { return new Line2D(Start.Position, End.Position); } }
        internal Dictionary<string, bool> Flags { get; private set; }
        public ushort RawFlags { get; private set; }
        public int Action { get { return action; } set { BeforePropsChange(); action = value; UpdateColorPreset(); } }
        public int Activate { get { return activate; } set { BeforePropsChange(); activate = value; UpdateColorPreset(); } }

        public int Tag { get { return tags[0]; } set { BeforePropsChange(); tags[0] = value; if ((value < General.Map.FormatInterface.MinTag) || (value > General.Map.FormatInterface.MaxTag)) throw new ArgumentOutOfRangeException("Tag", "Invalid tag number"); } } //mxd
        public List<int> Tags { get { return tags; } set { BeforePropsChange(); tags = value; } } //mxd
        public double LengthSq { get; private set; }
        public double Length { get; private set; }
        public double LengthInv { get; private set; }
        public double Angle { get; private set; }
        public int AngleDeg { get { return (int)(Angle * Angle2D.PIDEG); } }
        public RectangleF Rect { get { return rect; } }
        public int[] Args { get; private set; }
        internal int SerializedIndex { get; set; }
        internal int LastProcessed { get; set; }
        internal bool FrontInterior { get; set; }
        internal bool ImpassableFlag { get; private set; }
        internal int ColorPresetIndex { get; private set; } //mxd
        internal bool ExtraFloorFlag; //mxd

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal Linedef(MapSet map, int listindex, Vertex start, Vertex end)
        {
            // Initialize
            this.elementtype = MapElementType.LINEDEF; //mxd
            this.Map = map;
            this.listindex = listindex;
            this.updateneeded = true;
            this.Args = new int[NUM_ARGS];
            this.tags = new List<int> { 0 }; //mxd
            this.Flags = new Dictionary<string, bool>(StringComparer.Ordinal);
            this.ColorPresetIndex = -1;//mxd

            // Attach to vertices
            this.Start = start;
            this.startvertexlistitem = start.AttachLinedefP(this);
            this.End = end;
            this.endvertexlistitem = end.AttachLinedefP(this);

            if (map == General.Map.Map)
                General.Map.UndoRedo.RecAddLinedef(this);

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

                // Dispose sidedefs
                if ((Front != null) && Map.AutoRemove) Front.Dispose(); else AttachFrontP(null);
                if ((Back != null) && Map.AutoRemove) Back.Dispose(); else AttachBackP(null);

                if (Map == General.Map.Map)
                    General.Map.UndoRedo.RecRemLinedef(this);

                // Remove from main list
                Map.RemoveLinedef(listindex);

                // Detach from vertices
                if (startvertexlistitem != null) Start.DetachLinedefP(startvertexlistitem);
                startvertexlistitem = null;
                Start = null;
                if (endvertexlistitem != null) End.DetachLinedefP(endvertexlistitem);
                endvertexlistitem = null;
                End = null;

                // Clean up
                Start = null;
                End = null;
                Front = null;
                Back = null;
                Map = null;

                //mxd. Restore isdisposed so base classes can do their disposal job
                isdisposed = false;

                // Clean up base
                base.Dispose();
            }
        }

        #endregion

        #region ================== Management

        // Call this before changing properties
        protected override void BeforePropsChange()
        {
            if (Map == General.Map.Map)
                General.Map.UndoRedo.RecPrpLinedef(this);
        }

        // Serialize / deserialize (passive: doesn't record)
        new internal void ReadWrite(IReadWriteStream s)
        {
            if (!s.IsWriting)
            {
                BeforePropsChange();
                updateneeded = true;
            }

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

            s.rwInt(ref action);
            s.rwInt(ref activate);

            //mxd
            if (s.IsWriting)
            {
                s.wInt(tags.Count);
                foreach (int tag in tags) s.wInt(tag);
            }
            else
            {
                int c; s.rInt(out c);
                tags = new List<int>(c);
                for (int i = 0; i < c; i++)
                {
                    int t; s.rInt(out t);
                    tags.Add(t);
                }
            }

            for (int i = 0; i < NUM_ARGS; i++) s.rwInt(ref Args[i]);

            //mxd
            if (!s.IsWriting) UpdateColorPreset();
        }

        // This sets new start vertex
        public void SetStartVertex(Vertex v)
        {
            if (Map == General.Map.Map)
                General.Map.UndoRedo.RecRefLinedefStart(this);

            // Change start
            if (startvertexlistitem != null) Start.DetachLinedefP(startvertexlistitem);
            startvertexlistitem = null;
            Start = v;
            if (Start != null) startvertexlistitem = Start.AttachLinedefP(this);
            this.updateneeded = true;
        }

        // This sets new end vertex
        public void SetEndVertex(Vertex v)
        {
            if (Map == General.Map.Map)
                General.Map.UndoRedo.RecRefLinedefEnd(this);

            // Change end
            if (endvertexlistitem != null) End.DetachLinedefP(endvertexlistitem);
            endvertexlistitem = null;
            End = v;
            if (End != null) endvertexlistitem = End.AttachLinedefP(this);
            this.updateneeded = true;
        }

        // This detaches a vertex
        internal void DetachVertexP(Vertex v)
        {
            if (v == Start)
            {
                if (startvertexlistitem != null) Start.DetachLinedefP(startvertexlistitem);
                startvertexlistitem = null;
                Start = null;
            }
            else if (v == End)
            {
                if (endvertexlistitem != null) End.DetachLinedefP(endvertexlistitem);
                endvertexlistitem = null;
                End = null;
            }
            else
                throw new Exception("Specified Vertex is not attached to this Linedef.");
        }

        // This copies all properties to another line
        public void CopyPropertiesTo(Linedef l)
        {
            l.BeforePropsChange();

            // Copy properties
            l.action = action;
            l.Args = (int[])Args.Clone();
            l.Flags = new Dictionary<string, bool>(Flags);
            l.RawFlags = RawFlags;
            l.tags = new List<int>(tags); //mxd
            l.updateneeded = true;
            l.activate = activate;
            l.ImpassableFlag = ImpassableFlag;
            l.UpdateColorPreset();//mxd
            base.CopyPropertiesTo(l);
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

            // In Hexen format line activations are also stored in the flags bitmap, so take that into account here as well. Set bits to 0 first, then set bit if necessary.
            if (General.Map.Config.LinedefActivates.Count > 0)
            {
                foreach (LinedefActivateInfo lai in General.Map.Config.LinedefActivates)
                {
                    if (lai.Index > 0)
                    {
                        // Set bit to 0
                        RawFlags &= (ushort)~lai.Index;
                    }
                }

                // Set bit if necessary
                if (activate > 0)
                    RawFlags |= (ushort)activate;
            }
        }

        /// <summary>
        /// Updates the flags dictionary from the raw flags. Has to be called after the flags in the game config changed. Has to be called in conjunction with UpdateRawFlagsFromFlags.
        /// </summary>
        internal void UpdateFlagsFromRawFlags()
        {
            foreach (string fname in General.Map.Config.LinedefFlags.Keys)
            {
                if (ushort.TryParse(fname, out ushort fnum))
                {
                    Flags[fname] = (RawFlags & fnum) == fnum;
                }
            }
        }

        // This attaches a sidedef on the front
        internal void AttachFront(Sidedef s)
        {
            if (Map == General.Map.Map)
                General.Map.UndoRedo.RecRefLinedefFront(this);

            // Attach and recalculate
            AttachFrontP(s);
        }

        // Passive version, does not record the change
        internal void AttachFrontP(Sidedef s)
        {
            // Attach and recalculate
            Front = s;
            if (Front != null) Front.SetLinedefP(this);
            updateneeded = true;
        }

        // This attaches a sidedef on the back
        internal void AttachBack(Sidedef s)
        {
            if (Map == General.Map.Map)
                General.Map.UndoRedo.RecRefLinedefBack(this);

            // Attach and recalculate
            AttachBackP(s);
        }

        // Passive version, does not record the change
        internal void AttachBackP(Sidedef s)
        {
            // Attach and recalculate
            Back = s;
            if (Back != null) Back.SetLinedefP(this);
            updateneeded = true;
        }

        // This detaches a sidedef from the front
        internal void DetachSidedefP(Sidedef s)
        {
            // Sidedef is on the front?
            if (Front == s)
            {
                // Remove sidedef reference
                if (Front != null) Front.SetLinedefP(null);
                Front = null;
                updateneeded = true;
            }
            // Sidedef is on the back?
            else if (Back == s)
            {
                // Remove sidedef reference
                if (Back != null) Back.SetLinedefP(null);
                Back = null;
                updateneeded = true;
            }
            //else throw new Exception("Specified Sidedef is not attached to this Linedef.");
        }

        // This updates the line when changes have been made
        public void UpdateCache()
        {
            // Update if needed
            if (updateneeded)
            {
                // Delta vector
                Vector2D delta = End.Position - Start.Position;

                // Recalculate values
                LengthSq = delta.GetLengthSq();
                Length = Math.Sqrt(LengthSq);
                if (Length > 0.0) LengthInv = 1.0 / Length; else LengthInv = 1.0 / 0.0000000001;
                if (LengthSq > 0.0) lengthsqinv = 1.0 / LengthSq; else lengthsqinv = 1.0 / 0.0000000001;
                Angle = delta.GetAngle();
                double l = Math.Min(Start.Position.x, End.Position.x);
                double t = Math.Min(Start.Position.y, End.Position.y);
                double r = Math.Max(Start.Position.x, End.Position.x);
                double b = Math.Max(Start.Position.y, End.Position.y);
                rect = new RectangleF((float)l, (float)t, (float)(r - l), (float)(b - t));

                // Cached flags
                ImpassableFlag = IsFlagSet(General.Map.Config.ImpassableFlag);

                //mxd. Color preset
                UpdateColorPreset();

                // Updated
                updateneeded = false;
            }
        }

        // This flags the line needs an update because it moved
        public void NeedUpdate()
        {
            // Update this line
            updateneeded = true;

            // Update sectors as well
            if (Front != null) Front.Sector.UpdateNeeded = true;
            if (Back != null) Back.Sector.UpdateNeeded = true;
        }

        // This translates the flags and activations into UDMF fields
        internal void TranslateToUDMF(Type previousmapformatinterfacetype)
        {
            // First make a single integer with all bits from activation and flags
            int bits = activate;
            int flagbit;
            foreach (KeyValuePair<string, bool> f in Flags)
                if (int.TryParse(f.Key, out flagbit) && f.Value) bits |= flagbit;

            // Now make the new flags
            Flags.Clear();

            //mxd. Add default activation flag if needed
            if (action != 0 && activate == 0 && !string.IsNullOrEmpty(General.Map.Config.DefaultLinedefActivationFlag))
                Flags[General.Map.Config.DefaultLinedefActivationFlag] = true;

            foreach (FlagTranslation f in General.Map.Config.LinedefFlagsTranslation)
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
                    {
                        if (!Flags.ContainsKey(f.Fields[i])) //mxd
                            Flags[f.Fields[i]] = !f.FieldValues[i];
                    }
                }
            }

            //mxd. Hexen -> UDMF action translation. Hardcoded for now...
            if (previousmapformatinterfacetype == typeof(HexenMapSetIO))
            {
                switch (Action)
                {
                    case 121: //Line_SetIdentification
                              //Convert arg0 to tag
                        tags[0] = Args[0] + (Args[4] * 256);

                        //Convert arg1 to flags
                        ConvertArgToFlags(1);

                        //clear action and arguments
                        action = 0;
                        for (int i = 0; i < Args.Length; i++) Args[i] = 0;
                        break;

                    case 208: //TranslucentLine
                              //Convert arg0 to tag
                        tags[0] = Args[0];

                        //Convert arg3 to flags
                        ConvertArgToFlags(3);
                        break;

                    case 1: ConvertArgToTag(3, true); break; //Polyobj_StartLine
                    case 5: ConvertArgToTag(4, true); break; //Polyobj_ExplicitLine
                    case 181: ConvertArgToTag(2, true); break; //Plane_Align
                    case 215: ConvertArgToTag(0, true); break; //Teleport_Line
                    case 222: ConvertArgToTag(0, false); break; //Scroll_Texture_Model

                    case 160: //Sector_3DFloor
                              // Convert to UDMF
                        if ((Args[1] & 8) == 8) // arg4 is LineID?
                        {
                            tags[0] = Args[4];
                            Args[1] &= ~8; // Unset flag
                        }
                        else // It's sector's HiTag then
                        {
                            Args[0] += Args[4] * 256;
                        }

                        // Clear arg
                        Args[4] = 0;
                        break;
                }
            }

            //mxd. Update cached flags
            ImpassableFlag = IsFlagSet(General.Map.Config.ImpassableFlag);

            // Update color preset
            UpdateColorPreset();
        }

        // This translates UDMF fields back into the normal flags and activations
        internal void TranslateFromUDMF()
        {
            //mxd. Clear UDMF-related properties
            this.Fields.Clear();
            ExtraFloorFlag = false;

            // Make copy of the flags
            Dictionary<string, bool> oldfields = new Dictionary<string, bool>(Flags);

            // Make the flags
            Flags.Clear();
            foreach (KeyValuePair<string, string> f in General.Map.Config.LinedefFlags)
            {
                // Flag must be numeric
                int flagbit;
                if (int.TryParse(f.Key, out flagbit))
                {
                    foreach (FlagTranslation ft in General.Map.Config.LinedefFlagsTranslation)
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

            // Make the activation
            foreach (LinedefActivateInfo a in General.Map.Config.LinedefActivates)
            {
                bool foundactivation = false;
                foreach (FlagTranslation ft in General.Map.Config.LinedefFlagsTranslation)
                {
                    if (ft.Flag == a.Index)
                    {
                        // Only set this activation when the fields match
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
                            activate = a.Index;
                            foundactivation = true;
                            break;
                        }
                    }
                }
                if (foundactivation) break;
            }

            //mxd. UDMF -> Hexen action translation. Hardcoded for now...
            if (General.Map.FormatInterface is HexenMapSetIO)
            {
                switch (action)
                {
                    case 208: //TranslucentLine
                              //Convert tag to arg0
                        if (tags[0] < General.Map.FormatInterface.MinArgument || tags[0] > General.Map.FormatInterface.MaxArgument)
                        {
                            string message = "Linedef " + Index + ": unable to convert Tag (" + tags[0] + ") to LineID because it's outside of supported argument range [" + General.Map.FormatInterface.MinArgument + ".." + General.Map.FormatInterface.MaxArgument + "].";
                            General.ErrorLogger.Add(new MapElementErrorItem(ErrorType.Warning, this, message));
                        }
                        else
                        {
                            Args[0] = tags[0];
                        }

                        //Convert flags to arg3
                        ConvertFlagsToArg(oldfields, 3);
                        break;

                    case 1: ConvertTagToArg(3); break; //Polyobj_StartLine
                    case 5: ConvertTagToArg(4); break; //Polyobj_ExplicitLine
                    case 181: ConvertTagToArg(2); break; //Plane_Align
                    case 215: ConvertTagToArg(0); break; //Teleport_Line
                    case 222: ConvertTagToArg(0); break; //Scroll_Texture_Model

                    case 160: //Sector_3DFloor
                        if (Args[0] > General.Map.FormatInterface.MaxArgument) // Split sector tag?
                        {
                            int hitag = Args[0] / 256;
                            int lotag = Args[0] % 256;

                            Args[0] = lotag;
                            Args[4] = hitag;

                            if (tags[0] != 0)
                            {
                                string message = "Linedef " + Index + ": unable to convert Tag (" + tags[0] + ") to LineID, because target sector tag (arg0) is greater than " + General.Map.FormatInterface.MaxArgument + ".";
                                General.ErrorLogger.Add(new MapElementErrorItem(ErrorType.Warning, this, message));
                            }
                        }
                        else if (Args[0] < General.Map.FormatInterface.MinArgument)
                        {
                            string message = "Linedef " + Index + ": unable to convert arg0 (" + Args[0] + "), because it's outside of supported argument range [" + General.Map.FormatInterface.MinArgument + ".." + General.Map.FormatInterface.MaxArgument + "].";
                            General.ErrorLogger.Add(new MapElementErrorItem(ErrorType.Warning, this, message));
                        }
                        else if (tags[0] > General.Map.FormatInterface.MinArgument) // Convert to LineID?
                        {
                            if (tags[0] > General.Map.FormatInterface.MaxArgument)
                            {
                                string message = "Linedef " + Index + ": unable to convert Tag (" + tags[0] + ") to LineID, because linedef tag is greater than " + General.Map.FormatInterface.MaxArgument + ".";
                                General.ErrorLogger.Add(new MapElementErrorItem(ErrorType.Warning, this, message));
                            }
                            else
                            {
                                Args[4] = tags[0];
                                Args[1] |= 8; // Add "Use arg4 as LineID" flag
                            }
                        }
                        break;

                    default: // Convert tag to Line_SetIdentification?
                        if (tags[0] > General.Map.FormatInterface.MaxArgument)
                        {
                            if (action != 0)
                            {
                                string message = "Linedef " + Index + ": unable to convert Tag (" + tags[0] + ") to LineID, because linedef already has an action.";
                                General.ErrorLogger.Add(new MapElementErrorItem(ErrorType.Warning, this, message));
                            }
                            else // Convert to Line_SetIdentification
                            {
                                int hiid = tags[0] / 256;
                                int loid = tags[0] % 256;

                                action = 121;
                                Args[0] = loid;
                                Args[4] = hiid;
                                ConvertFlagsToArg(oldfields, 1);
                            }
                        }
                        else if (tags[0] < General.Map.FormatInterface.MinArgument)
                        {
                            string message = "Linedef " + Index + ": unable to convert Tag (" + tags[0] + ") to LineID, because it's outside of supported argument range [" + General.Map.FormatInterface.MinArgument + ".." + General.Map.FormatInterface.MaxArgument + "].";
                            General.ErrorLogger.Add(new MapElementErrorItem(ErrorType.Warning, this, message));
                        }
                        break;
                }

                // Clear tag
                tags[0] = 0;
            }

            //mxd. Update cached flags
            ImpassableFlag = IsFlagSet(General.Map.Config.ImpassableFlag);

            // Update color preset
            UpdateColorPreset();
        }

        //mxd
        private void ConvertArgToTag(int argnum, bool cleararg)
        {
            // Convert arg to tag
            tags[0] = Args[argnum];

            // Clear obsolete arg
            if (cleararg) Args[argnum] = 0;
        }

        //mxd
        private void ConvertTagToArg(int argnum)
        {
            if (tags[0] < General.Map.FormatInterface.MinArgument || tags[0] > General.Map.FormatInterface.MaxArgument)
            {
                General.ErrorLogger.Add(ErrorType.Warning, "Linedef " + Index + ": unable to convert Tag (" + tags[0] + ") to LineID because it's outside of supported argument range [" + General.Map.FormatInterface.MinArgument + ".." + General.Map.FormatInterface.MaxArgument + "].");
            }
            else
            {
                Args[argnum] = tags[0];
            }
        }

        //mxd
        private void ConvertArgToFlags(int argnum)
        {
            if (Args[argnum] == 0) return;

            // Convert to flags
            if ((Args[argnum] & 1) == 1) Flags["zoneboundary"] = true;
            if ((Args[argnum] & 2) == 2) Flags["jumpover"] = true;
            if ((Args[argnum] & 4) == 4) Flags["blockfloaters"] = true;
            if ((Args[argnum] & 8) == 8) Flags["clipmidtex"] = true;
            if ((Args[argnum] & 16) == 16) Flags["wrapmidtex"] = true;
            if ((Args[argnum] & 32) == 32) Flags["midtex3d"] = true;
            if ((Args[argnum] & 64) == 64) Flags["checkswitchrange"] = true;
            if ((Args[argnum] & 128) == 128) Flags["firstsideonly"] = true;

            // Clear obsolete arg
            Args[argnum] = 0;
        }

        //mxd
        private void ConvertFlagsToArg(Dictionary<string, bool> oldflags, int argnum)
        {
            int bits = 0;
            if (oldflags.ContainsKey("zoneboundary") && oldflags["zoneboundary"]) bits &= 1;
            if (oldflags.ContainsKey("jumpover") && oldflags["jumpover"]) bits &= 2;
            if (oldflags.ContainsKey("blockfloaters") && oldflags["blockfloaters"]) bits &= 4;
            if (oldflags.ContainsKey("clipmidtex") && oldflags["clipmidtex"]) bits &= 8;
            if (oldflags.ContainsKey("wrapmidtex") && oldflags["wrapmidtex"]) bits &= 16;
            if (oldflags.ContainsKey("midtex3d") && oldflags["midtex3d"]) bits &= 32;
            if (oldflags.ContainsKey("checkswitchrange") && oldflags["checkswitchrange"]) bits &= 64;
            if (oldflags.ContainsKey("firstsideonly") && oldflags["firstsideonly"]) bits &= 128;

            // Set arg
            Args[argnum] = bits;
        }

        // Selected
        protected override void DoSelect()
        {
            base.DoSelect();
            selecteditem = Map.SelectedLinedefs.AddLast(this);
        }

        // Deselect
        protected override void DoUnselect()
        {
            base.DoUnselect();
            if (selecteditem.List != null) selecteditem.List.Remove(selecteditem);
            selecteditem = null;
        }

        #endregion

        #region ================== Methods

        // Plane Align (181) (see http://zdoom.org/wiki/Plane_Align
        public bool HasActionPlaneAlign()
        {
            return Action > 0 && General.Map.Config.GetLinedefActionInfo(Action).Id?.ToLowerInvariant() == "plane_align";
        }

        // Determine if this line defines the sky upper texture transferred to a sector.
        public bool HasSkyTransfer()
        {
            return HasSkyTransferStaticInit() ||
                General.Map.Config.GetLinedefActionInfo(Action).ErrorCheckerExemptions.RequiresUpperTexture;
        }

        // Determine if this line uses Static_Init that mimics MBF's sky transfer linedef specials.
        // This also enables an optional lower texture to be shown during a lightning weather effect.
        public bool HasSkyTransferStaticInit()
        {
            return General.Map.Config.GetLinedefActionInfo(Action).Id?.ToLowerInvariant() == "static_init" &&
                Args[1] == 255;
        }

        // Determine if this line and another line are associated by action and tag.
        public bool IsAssociatedWith(Linedef ld)
        {
            LinedefActionInfo actioninfo;

            return this != ld &&
                Action > 0 &&
                ld.Tag > 0 &&
                tags.Contains(ld.Tag) &&
                (actioninfo = General.Map.Config.GetLinedefActionInfo(Action)).LineToLineTag &&
                (!actioninfo.LineToLineSameAction || Action == ld.Action);
        }

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

                // Cached flags
                if (flagname == General.Map.Config.ImpassableFlag) ImpassableFlag = value;

                //mxd
                UpdateColorPreset();
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
            ImpassableFlag = false;

            //mxd
            UpdateColorPreset();
        }

        // This flips the linedef's vertex attachments
        public void FlipVertices()
        {
            // make sure the start/end vertices are not automatically
            // deleted if they do not belong to any other line
            General.Map.Map.AutoRemove = false;

            // Flip vertices
            Vertex oldstart = Start;
            Vertex oldend = End;
            SetStartVertex(oldend);
            SetEndVertex(oldstart);

            General.Map.Map.AutoRemove = true;

            // For drawing, the interior now lies on the other side
            FrontInterior = !FrontInterior;

            // Update required (angle changed)
            NeedUpdate();
            General.Map.IsChanged = true;
        }

        // This flips the sidedefs
        public void FlipSidedefs()
        {
            // Flip sidedefs
            Sidedef oldfront = Front;
            Sidedef oldback = Back;
            AttachFront(oldback);
            AttachBack(oldfront);

            General.Map.IsChanged = true;
        }

        // This returns a point for testing on one side
        public Vector2D GetSidePoint(bool front)
        {
            Vector2D n = new Vector2D();
            n.x = (End.Position.x - Start.Position.x) * LengthInv * SIDE_POINT_DISTANCE;
            n.y = (End.Position.y - Start.Position.y) * LengthInv * SIDE_POINT_DISTANCE;

            if (front)
            {
                n.x = -n.x;
                n.y = -n.y;
            }

            Vector2D p = new Vector2D();
            p.x = Start.Position.x + ((End.Position.x - Start.Position.x) * 0.5) - n.y;
            p.y = Start.Position.y + ((End.Position.y - Start.Position.y) * 0.5) + n.x;

            return p;
        }

        // This returns a point in the middle of the line
        public Vector2D GetCenterPoint()
        {
            return Start.Position + ((End.Position - Start.Position) * 0.5);
        }

        // This applies single/double sided flags
        public void ApplySidedFlags()
        {
            // Doublesided?
            if ((Front != null) && (Back != null))
            {
                // Apply or remove flags for doublesided line
                SetFlag(General.Map.Config.SingleSidedFlag, false);
                SetFlag(General.Map.Config.DoubleSidedFlag, true);
            }
            else
            {
                // Apply or remove flags for singlesided line
                SetFlag(General.Map.Config.SingleSidedFlag, true);
                SetFlag(General.Map.Config.DoubleSidedFlag, false);
            }

            General.Map.IsChanged = true;
        }

        // This returns all points at which the line intersects with the grid
        public List<Vector2D> GetGridIntersections()
        {
            return GetGridIntersections(0.0);
        }


        public List<Vector2D> GetGridIntersections(double gridrotation, double gridoriginx = 0.0, double gridoriginy = 0.0)
        {
            return GetGridIntersections(new Vector2D(), gridrotation, gridoriginx, gridoriginy);
        }

        // This returns all points at which the line intersects with the grid
        public List<Vector2D> GetGridIntersections(Vector2D gridoffset, double gridrotation = 0.0, double gridoriginx = 0.0, double gridoriginy = 0.0)
        {
            List<Vector2D> coords = new List<Vector2D>();
            Vector2D v = new Vector2D();
            double minx, maxx, miny, maxy;
            bool reversex, reversey;

            Vector2D v1 = Start.Position;
            Vector2D v2 = End.Position;

            bool transformed = Math.Abs(gridrotation) > 1e-4 || Math.Abs(gridoriginx) > 1e-4 || Math.Abs(gridoriginy) > 1e-4;
            if (transformed)
            {
                v1 = (v1 - new Vector2D(gridoriginx, gridoriginy)).GetRotated(-gridrotation);
                v2 = (v2 - new Vector2D(gridoriginx, gridoriginy)).GetRotated(-gridrotation);
            }

            if (v1.x > v2.x)
            {
                minx = v2.x;
                maxx = v1.x;
                reversex = true;
            }
            else
            {
                minx = v1.x;
                maxx = v2.x;
                reversex = false;
            }

            if (v1.y > v2.y)
            {
                miny = v2.y;
                maxy = v1.y;
                reversey = true;
            }
            else
            {
                miny = v1.y;
                maxy = v2.y;
                reversey = false;
            }

            // Go for all vertical grid lines in between line start and end
            double gx = General.Map.Grid.GetHigher(minx) + gridoffset.x;
            if (gx < maxx)
            {
                for (; gx < maxx; gx += General.Map.Grid.GridSizeF)
                {
                    // Add intersection point at this x coordinate
                    double u = (gx - minx) / (maxx - minx);
                    if (reversex) u = 1.0 - u;
                    v.x = gx;
                    v.y = v1.y + ((v2.y - v1.y) * u);
                    coords.Add(v);
                }
            }

            // Go for all horizontal grid lines in between line start and end
            double gy = General.Map.Grid.GetHigher(miny) + gridoffset.y;
            if (gy < maxy)
            {
                for (; gy < maxy; gy += General.Map.Grid.GridSizeF)
                {
                    // Add intersection point at this y coordinate
                    double u = (gy - miny) / (maxy - miny);
                    if (reversey) u = 1.0 - u;
                    v.x = v1.x + ((v2.x - v1.x) * u);
                    v.y = gy;
                    coords.Add(v);
                }
            }

            if (transformed)
            {
                for (int i = 0; i < coords.Count; i++)
                {
                    coords[i] = coords[i].GetRotated(gridrotation) + new Vector2D(gridoriginx, gridoriginy);
                }
            }

            // Profit
            return coords;
        }

        // This returns the closest coordinates ON the line
        public Vector2D NearestOnLine(Vector2D pos)
        {
            double u = Line2D.GetNearestOnLine(Start.Position, End.Position, pos);
            if (u < 0.0) u = 0.0; else if (u > 1.0) u = 1.0;
            return Line2D.GetCoordinatesAt(Start.Position, End.Position, u);
        }

        // This returns the shortest distance from given coordinates to line
        public double SafeDistanceToSq(Vector2D p, bool bounded)
        {
            Vector2D v1 = Start.Position;
            Vector2D v2 = End.Position;

            // Calculate intersection offset
            double u = (((p.x - v1.x) * (v2.x - v1.x)) + ((p.y - v1.y) * (v2.y - v1.y))) * lengthsqinv;

            // Limit intersection offset to the line
            if (bounded)
            {
                // We really don't want u to be 0 or 1, because that'd mean the distance will be measured
                // from the vertices, which will result in linedefs being equally far away. We still need 
                // special handling for linedefs that are shorter than 1 mu (which is possible in UDMF)
                // Detailed explanation here: https://github.com/jewalky/GZDoom-Builder-Bugfix/issues/307
                if (LengthInv > 1.0)
                {
                    u = Math.Max(0, Math.Min(1.0, u));
                }
                else
                {
                    u = Math.Max(LengthInv, Math.Min(1.0 - LengthInv, u));
                }
            }

            /*
			// Calculate intersection point
			Vector2D i = v1 + u * (v2 - v1);

			// Return distance between intersection and point
			// which is the shortest distance to the line
			float ldx = p.x - i.x;
			float ldy = p.y - i.y;
			*/

            // ano - let's check to see if we can do the previous faster without using operator overloading and etc
            // the answer: running it  int.MaxValue / 64 times it tended to be around 100ms faster
            double ldx = p.x - (v1.x + (u * (v2.x - v1.x)));
            double ldy = p.y - (v1.y + (u * (v2.y - v1.y)));
            return (ldx * ldx) + (ldy * ldy);
        }

        // This returns the shortest distance from given coordinates to line
        public double SafeDistanceTo(Vector2D p, bool bounded)
        {
            return Math.Sqrt(SafeDistanceToSq(p, bounded));
        }

        // This returns the shortest distance from given coordinates to line
        public double DistanceToSq(Vector2D p, bool bounded)
        {
            Vector2D v1 = Start.Position;
            Vector2D v2 = End.Position;

            // Calculate intersection offset
            double u = (((p.x - v1.x) * (v2.x - v1.x)) + ((p.y - v1.y) * (v2.y - v1.y))) * lengthsqinv;

            // Limit intersection offset to the line
            if (bounded) if (u < 0.0) u = 0.0; else if (u > 1.0) u = 1.0;

            // Calculate intersection point
            Vector2D i = v1 + (u * (v2 - v1));

            // Return distance between intersection and point
            // which is the shortest distance to the line
            double ldx = p.x - i.x;
            double ldy = p.y - i.y;
            return (ldx * ldx) + (ldy * ldy);
        }

        // This returns the shortest distance from given coordinates to line
        public double DistanceTo(Vector2D p, bool bounded)
        {
            return Math.Sqrt(DistanceToSq(p, bounded));
        }

        // This tests on which side of the line the given coordinates are
        // returns < 0 for front (right) side, > 0 for back (left) side and 0 if on the line
        public double SideOfLine(Vector2D p)
        {
            Vector2D v1 = Start.Position;
            Vector2D v2 = End.Position;

            // Calculate and return side information
            return ((p.y - v1.y) * (v2.x - v1.x)) - ((p.x - v1.x) * (v2.y - v1.y));
        }

        // This splits this line by vertex v
        // Returns the new line resulting from the split, or null when it failed
        public Linedef Split(Vertex v)
        {
            Sidedef nsd;

            // Copy linedef and change vertices
            Linedef nl = Map.CreateLinedef(v, End);
            if (nl == null) return null;
            CopyPropertiesTo(nl);
            SetEndVertex(v);
            nl.Selected = this.Selected;
            nl.marked = this.marked;
            nl.ExtraFloorFlag = this.ExtraFloorFlag; //mxd

            // Copy front sidedef if exists
            if (Front != null)
            {
                nsd = Map.CreateSidedef(nl, true, Front.Sector);
                if (nsd == null) return null;
                Front.CopyPropertiesTo(nsd);
                nsd.Marked = Front.Marked;
            }

            // Copy back sidedef if exists
            if (Back != null)
            {
                nsd = Map.CreateSidedef(nl, false, Back.Sector);
                if (nsd == null) return null;
                Back.CopyPropertiesTo(nsd);
                nsd.Marked = Back.Marked;
            }

            //mxd
            AdjustSplitCoordinates(this, nl, General.Settings.SplitLineBehavior);

            // Return result
            General.Map.IsChanged = true;
            return nl;
        }

        // This joins the line with another line
        // This line will be disposed
        // Returns false when the operation could not be completed
        public bool Join(Linedef other)
        {
            // Check which lines were 2 sided
            bool otherwas2s = (other.Front != null) && (other.Back != null);
            bool thiswas2s = (this.Front != null) && (this.Back != null);

            // Get sector references
            Sector otherfs = other.Front != null ? other.Front.Sector : null;
            Sector otherbs = other.Back != null ? other.Back.Sector : null;
            Sector thisfs = this.Front != null ? this.Front.Sector : null;
            Sector thisbs = this.Back != null ? this.Back.Sector : null;

            // This line has no sidedefs?
            if ((thisfs == null) && (thisbs == null))
            {
                // We have no sidedefs, so we have no influence
                // Nothing to change on the other line
            }
            // Other line has no sidedefs?
            else if ((otherfs == null) && (otherbs == null))
            {
                // The other has no sidedefs, so it has no influence
                // Copy my sidedefs to the other
                if (this.Start == other.Start)
                {
                    if (!JoinChangeSidedefs(other, true, Front)) return false;
                    if (!JoinChangeSidedefs(other, false, Back)) return false;
                }
                else
                {
                    if (!JoinChangeSidedefs(other, false, Front)) return false;
                    if (!JoinChangeSidedefs(other, true, Back)) return false;
                }

                // Copy my properties to the other
                this.CopyPropertiesTo(other);
            }
            else
            {
                // Compare front sectors
                if ((otherfs != null) && (otherfs == thisfs))
                {
                    // Copy textures
                    if (other.Front != null) other.Front.AddTexturesTo(this.Back);
                    if (this.Front != null) this.Front.AddTexturesTo(other.Back);

                    // Change sidedefs?
                    if (!JoinChangeSidedefs(other, true, Back)) return false;
                }
                // Compare back sectors
                else if ((otherbs != null) && (otherbs == thisbs))
                {
                    // Copy textures
                    if (other.Back != null) other.Back.AddTexturesTo(this.Front);
                    if (this.Back != null) this.Back.AddTexturesTo(other.Front);

                    // Change sidedefs?
                    if (!JoinChangeSidedefs(other, false, Front)) return false;
                }
                // Compare front and back
                else if ((otherfs != null) && (otherfs == thisbs))
                {
                    // Copy textures
                    if (other.Front != null) other.Front.AddTexturesTo(this.Front);
                    if (this.Back != null) this.Back.AddTexturesTo(other.Back);

                    // Change sidedefs?
                    if (!JoinChangeSidedefs(other, true, Front)) return false;
                }
                // Compare back and front
                else if ((otherbs != null) && (otherbs == thisfs))
                {
                    // Copy textures
                    if (other.Back != null) other.Back.AddTexturesTo(this.Back);
                    if (this.Front != null) this.Front.AddTexturesTo(other.Front);

                    // Change sidedefs?
                    if (!JoinChangeSidedefs(other, false, Back)) return false;
                }
                else
                {
                    // Other line single sided?
                    if (other.Back == null)
                    {
                        // This line with its back to the other?
                        if (this.Start == other.End)
                        {
                            // Copy textures
                            if (this.Back != null) this.Back.AddTexturesTo(other.Front);

                            // Change sidedefs?
                            if (!JoinChangeSidedefs(other, false, Front)) return false;
                        }
                        else
                        {
                            // Copy textures
                            if (this.Front != null) this.Front.AddTexturesTo(other.Front);

                            // Change sidedefs?
                            if (!JoinChangeSidedefs(other, false, Back)) return false;
                        }
                    }
                    // This line single sided?
                    else if (this.Back == null)
                    {
                        // Other line with its back to this?
                        if (other.Start == this.End)
                        {
                            if (otherbs == null)
                            {
                                // Copy textures
                                if (other.Back != null) other.Back.AddTexturesTo(this.Front);

                                // Change sidedefs
                                if (!JoinChangeSidedefs(other, false, Front)) return false;
                            }
                        }
                        else
                        {
                            if (otherfs == null)
                            {
                                // Copy textures
                                if (other.Front != null) other.Front.AddTexturesTo(this.Front);

                                // Change sidedefs
                                if (!JoinChangeSidedefs(other, true, Front)) return false;
                            }
                        }
                    }
                    else
                    {
                        // This line with its back to the other?
                        if (this.Start == other.End)
                        {
                            // Copy textures
                            if (other.Back != null) other.Back.AddTexturesTo(this.Front);
                            if (this.Back != null) this.Back.AddTexturesTo(other.Front);

                            // Change sidedefs
                            if (!JoinChangeSidedefs(other, false, Front)) return false;
                        }
                        // Both lines face the same way
                        else
                        {
                            // Copy textures
                            if (other.Back != null) other.Back.AddTexturesTo(this.Back);
                            if (this.Front != null) this.Front.AddTexturesTo(other.Front);

                            // Change sidedefs
                            if (!JoinChangeSidedefs(other, false, Back)) return false;
                        }
                    }
                }

                // Apply single/double sided flags if the double-sided-ness changed
                if ((!otherwas2s && other.Front != null && other.Back != null) ||
                     (otherwas2s && (other.Front == null || other.Back == null)))
                    other.ApplySidedFlags();

                // Remove unneeded textures
                if (other.Front != null) other.Front.RemoveUnneededTextures(!(otherwas2s && thiswas2s));
                if (other.Back != null) other.Back.RemoveUnneededTextures(!(otherwas2s && thiswas2s));
            }

            // If either of the two lines was selected, keep the other selected
            if (this.Selected) other.Selected = true;
            if (this.marked) other.marked = true;

            // I got killed by the other.
            this.Dispose();
            General.Map.IsChanged = true;
            return true;
        }

        // This changes sidedefs (used for joining lines)
        // target:		The linedef on which to remove or create a new sidedef
        // front:		Side on which to remove or create the sidedef (true for front side)
        // newside:		The side from which to copy the properties to the new sidedef.
        //				If this is null, no sidedef will be created (only removed)
        // Returns false when the operation could not be completed.
        private bool JoinChangeSidedefs(Linedef target, bool front, Sidedef newside)
        {
            // Change sidedefs
            if (front)
            {
                if (target.Front != null) target.Front.Dispose();
            }
            else
            {
                if (target.Back != null) target.Back.Dispose();
            }

            if (newside != null)
            {
                Sidedef sd = Map.CreateSidedef(target, front, newside.Sector);
                if (sd == null) return false;
                newside.CopyPropertiesTo(sd);
                sd.Marked = newside.Marked;
            }

            return true;
        }

        //mxd
        internal void UpdateColorPreset()
        {
            for (int i = 0; i < General.Map.ConfigSettings.LinedefColorPresets.Length; i++)
            {
                if (General.Map.ConfigSettings.LinedefColorPresets[i].Matches(this))
                {
                    ColorPresetIndex = i;
                    return;
                }
            }
            ColorPresetIndex = -1;
        }

        /// <summary>
        /// Changes the linedef's index to a new index.
        /// </summary>
        /// <param name="newindex">The new index to set</param>
        public void ChangeIndex(int newindex)
        {
            General.Map.UndoRedo.RecIndexLinedef(Index, newindex);
            Map?.ChangeLindefIndex(Index, newindex);
        }

        // String representation
        public override string ToString()
        {
#if DEBUG
            string starttext = Start != null ? " (" + Start : string.Empty;
            string endtext = End != null ? ", " + End + ")" : string.Empty;
            return "Linedef " + listindex + (marked ? " (marked)" : "") + starttext + endtext; //mxd
#else
			return "Linedef " + listindex;
#endif
        }

        #endregion

        #region ================== Changes

        // This updates all properties
        public void Update(Dictionary<string, bool> flags, ushort rawflags, int activate, List<int> tags, int action, int[] args)
        {
            BeforePropsChange();

            // Apply changes
            this.Flags = new Dictionary<string, bool>(flags);
            this.RawFlags = rawflags;
            this.tags = new List<int>(tags); //mxd
            this.activate = activate;
            this.action = action;
            this.Args = new int[NUM_ARGS];
            args.CopyTo(this.Args, 0);
            this.updateneeded = true;
        }

        // mxd. Moved here from BuilderModes.BuilderPlug
        // This adjusts texture coordinates for splitted lines according to the user preferences
        private static void AdjustSplitCoordinates(Linedef oldline, Linedef newline, SplitLineBehavior splitlinebehavior)
        {
            switch (splitlinebehavior)
            {
                case SplitLineBehavior.Interpolate:
                    //Make texture offset adjustments
                    if (oldline.Back != null)
                    {
                        if ((oldline.Back.MiddleRequired() && oldline.Back.LongMiddleTexture != MapSet.EmptyLongName) || oldline.Back.HighRequired() || oldline.Back.LowRequired())
                        {
                            int distance = (int)Vector2D.Distance(newline.Start.Position, newline.End.Position);
                            if (General.Map.UDMF && General.Map.Config.UseLocalSidedefTextureOffsets)
                            {
                                if (distance != 0) oldline.Back.SetUdmfTextureOffsetX(distance);
                            }
                            else
                            {
                                oldline.Back.OffsetX += distance;
                            }
                        }
                    }

                    if (newline.Front != null && (newline.Front.MiddleRequired() || newline.Front.LongMiddleTexture != MapSet.EmptyLongName || newline.Front.HighRequired() || newline.Front.LowRequired()))
                    {
                        int distance = (int)Vector2D.Distance(oldline.Start.Position, oldline.End.Position);
                        if (General.Map.UDMF && General.Map.Config.UseLocalSidedefTextureOffsets)
                        {
                            if (distance != 0) newline.Front.SetUdmfTextureOffsetX(distance);
                        }
                        else
                        {
                            newline.Front.OffsetX += distance;
                        }
                    }

                    //Clamp texture coordinates
                    if ((oldline.Front != null) && (newline.Front != null))
                    {
                        //get texture
                        ImageData texture = null;

                        if (newline.Front.MiddleRequired() && newline.Front.LongMiddleTexture != MapSet.EmptyLongName && General.Map.Data.GetTextureExists(newline.Front.LongMiddleTexture))
                            texture = General.Map.Data.GetTextureImage(newline.Front.MiddleTexture);
                        else if (newline.Front.HighRequired() && newline.Front.LongHighTexture != MapSet.EmptyLongName && General.Map.Data.GetTextureExists(newline.Front.LongHighTexture))
                            texture = General.Map.Data.GetTextureImage(newline.Front.HighTexture);
                        else if (newline.Front.LowRequired() && newline.Front.LongLowTexture != MapSet.EmptyLongName && General.Map.Data.GetTextureExists(newline.Front.LongLowTexture))
                            texture = General.Map.Data.GetTextureImage(newline.Front.LowTexture);

                        //clamp offsetX
                        if (texture != null && texture.IsImageLoaded)
                            newline.Front.OffsetX %= texture.Width;
                    }

                    if ((oldline.Back != null) && (newline.Back != null))
                    {
                        //get texture
                        ImageData texture = null;

                        if (newline.Back.MiddleRequired() && newline.Back.LongMiddleTexture != MapSet.EmptyLongName && General.Map.Data.GetTextureExists(newline.Back.LongMiddleTexture))
                            texture = General.Map.Data.GetTextureImage(newline.Back.MiddleTexture);
                        else if (newline.Back.HighRequired() && newline.Back.LongHighTexture != MapSet.EmptyLongName && General.Map.Data.GetTextureExists(newline.Back.LongHighTexture))
                            texture = General.Map.Data.GetTextureImage(newline.Back.HighTexture);
                        else if (newline.Back.LowRequired() && newline.Back.LongLowTexture != MapSet.EmptyLongName && General.Map.Data.GetTextureExists(newline.Back.LongLowTexture))
                            texture = General.Map.Data.GetTextureImage(newline.Back.LowTexture);

                        //clamp offsetX
                        if (texture != null && texture.IsImageLoaded)
                            newline.Back.OffsetX %= texture.Width;
                    }

                    break;

                case SplitLineBehavior.CopyXY:
                    if ((oldline.Front != null) && (newline.Front != null))
                    {
                        newline.Front.OffsetX = oldline.Front.OffsetX;
                        newline.Front.OffsetY = oldline.Front.OffsetY;

                        //mxd. Copy UDMF offsets as well
                        if (General.Map.UDMF && General.Map.Config.UseLocalSidedefTextureOffsets)
                        {
                            UniFields.SetFloat(newline.Front.Fields, "offsetx_top", oldline.Front.Fields.GetValue("offsetx_top", 0.0));
                            UniFields.SetFloat(newline.Front.Fields, "offsetx_mid", oldline.Front.Fields.GetValue("offsetx_mid", 0.0));
                            UniFields.SetFloat(newline.Front.Fields, "offsetx_bottom", oldline.Front.Fields.GetValue("offsetx_bottom", 0.0));

                            UniFields.SetFloat(newline.Front.Fields, "offsety_top", oldline.Front.Fields.GetValue("offsety_top", 0.0));
                            UniFields.SetFloat(newline.Front.Fields, "offsety_mid", oldline.Front.Fields.GetValue("offsety_mid", 0.0));
                            UniFields.SetFloat(newline.Front.Fields, "offsety_bottom", oldline.Front.Fields.GetValue("offsety_bottom", 0.0));
                        }
                    }

                    if ((oldline.Back != null) && (newline.Back != null))
                    {
                        newline.Back.OffsetX = oldline.Back.OffsetX;
                        newline.Back.OffsetY = oldline.Back.OffsetY;

                        //mxd. Copy UDMF offsets as well
                        if (General.Map.UDMF && General.Map.Config.UseLocalSidedefTextureOffsets)
                        {
                            UniFields.SetFloat(newline.Back.Fields, "offsetx_top", oldline.Back.Fields.GetValue("offsetx_top", 0.0));
                            UniFields.SetFloat(newline.Back.Fields, "offsetx_mid", oldline.Back.Fields.GetValue("offsetx_mid", 0.0));
                            UniFields.SetFloat(newline.Back.Fields, "offsetx_bottom", oldline.Back.Fields.GetValue("offsetx_bottom", 0.0));

                            UniFields.SetFloat(newline.Back.Fields, "offsety_top", oldline.Back.Fields.GetValue("offsety_top", 0.0));
                            UniFields.SetFloat(newline.Back.Fields, "offsety_mid", oldline.Back.Fields.GetValue("offsety_mid", 0.0));
                            UniFields.SetFloat(newline.Back.Fields, "offsety_bottom", oldline.Back.Fields.GetValue("offsety_bottom", 0.0));
                        }
                    }
                    break;

                case SplitLineBehavior.ResetXCopyY:
                    if ((oldline.Front != null) && (newline.Front != null))
                    {
                        newline.Front.OffsetX = 0;
                        newline.Front.OffsetY = oldline.Front.OffsetY;

                        //mxd. Reset UDMF X offset as well
                        if (General.Map.UDMF && General.Map.Config.UseLocalSidedefTextureOffsets)
                        {
                            UniFields.SetFloat(newline.Front.Fields, "offsetx_top", 0.0);
                            UniFields.SetFloat(newline.Front.Fields, "offsetx_mid", 0.0);
                            UniFields.SetFloat(newline.Front.Fields, "offsetx_bottom", 0.0);
                        }
                    }

                    if ((oldline.Back != null) && (newline.Back != null))
                    {
                        newline.Back.OffsetX = 0;
                        newline.Back.OffsetY = oldline.Back.OffsetY;

                        //mxd. Reset UDMF X offset and copy Y offset as well
                        if (General.Map.UDMF && General.Map.Config.UseLocalSidedefTextureOffsets)
                        {
                            UniFields.SetFloat(newline.Back.Fields, "offsetx_top", 0.0);
                            UniFields.SetFloat(newline.Back.Fields, "offsetx_mid", 0.0);
                            UniFields.SetFloat(newline.Back.Fields, "offsetx_bottom", 0.0);

                            UniFields.SetFloat(newline.Back.Fields, "offsety_top", oldline.Back.Fields.GetValue("offsety_top", 0.0));
                            UniFields.SetFloat(newline.Back.Fields, "offsety_mid", oldline.Back.Fields.GetValue("offsety_mid", 0.0));
                            UniFields.SetFloat(newline.Back.Fields, "offsety_bottom", oldline.Back.Fields.GetValue("offsety_bottom", 0.0));
                        }
                    }
                    break;

                case SplitLineBehavior.ResetXY:
                    if (newline.Front != null)
                    {
                        newline.Front.OffsetX = 0;
                        newline.Front.OffsetY = 0;

                        if (General.Map.UDMF && General.Map.Config.UseLocalSidedefTextureOffsets)
                        {
                            UniFields.SetFloat(newline.Front.Fields, "offsetx_top", 0.0);
                            UniFields.SetFloat(newline.Front.Fields, "offsetx_mid", 0.0);
                            UniFields.SetFloat(newline.Front.Fields, "offsetx_bottom", 0.0);

                            UniFields.SetFloat(newline.Front.Fields, "offsety_top", 0.0);
                            UniFields.SetFloat(newline.Front.Fields, "offsety_mid", 0.0);
                            UniFields.SetFloat(newline.Front.Fields, "offsety_bottom", 0.0);
                        }
                    }

                    if (newline.Back != null)
                    {
                        newline.Back.OffsetX = 0;
                        newline.Back.OffsetY = 0;

                        if (General.Map.UDMF && General.Map.Config.UseLocalSidedefTextureOffsets)
                        {
                            UniFields.SetFloat(newline.Back.Fields, "offsetx_top", 0.0);
                            UniFields.SetFloat(newline.Back.Fields, "offsetx_mid", 0.0);
                            UniFields.SetFloat(newline.Back.Fields, "offsetx_bottom", 0.0);

                            UniFields.SetFloat(newline.Back.Fields, "offsety_top", 0.0);
                            UniFields.SetFloat(newline.Back.Fields, "offsety_mid", 0.0);
                            UniFields.SetFloat(newline.Back.Fields, "offsety_bottom", 0.0);
                        }
                    }
                    break;
            }
        }

        #endregion
    }
}
