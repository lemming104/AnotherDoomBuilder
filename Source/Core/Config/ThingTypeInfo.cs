
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

using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Dehacked;
using CodeImp.DoomBuilder.GZBuilder;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.ZDoom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

#endregion

namespace CodeImp.DoomBuilder.Config
{
    public struct SpriteFrameInfo //mxd
    {
        public string Sprite;
        public long SpriteLongName;
        public bool Mirror;
    }

    public class ThingTypeInfo : INumberedTitle, IComparable<ThingTypeInfo>
    {
        #region ================== Constants

        public const int THING_BLOCKING_NONE = 0;
        public const int THING_BLOCKING_FULL = 1;
        public const int THING_BLOCKING_HEIGHT = 2;
        public const int THING_ERROR_NONE = 0;
        public const int THING_ERROR_INSIDE = 1;
        public const int THING_ERROR_INSIDE_STUCK = 2;
        private const float THING_FIXED_SIZE = 14f; //mxd

        #endregion

        #region ================== Variables

        // Properties
        private SizeF spritescale;
        private readonly bool locksprite; //mxd
        private Dictionary<string, Dictionary<string, string>> flagsrename; //mxd. <MapSetIOName, <flag, title>>
        private List<string> adduniversalfields;

        //mxd. GZDoom rendering properties

        //mxd. Ambinent sound info

        // [ZZ] GZDoom inheritance data (DECORATE and ZScript). used for dynamic lighting.
        private GZGeneral.LightData dynamiclighttype = null;

        // [ZZ] optional thing is a thing that can have nonexistent sprite. this is currently only used for Skulltag things.

        #endregion

        #region ================== Properties

        public int Index { get; }
        public string Title { get; internal set; } //mxd. Added setter
        public string Sprite { get; private set; }
        public SpriteFrameInfo[] SpriteFrame { get; private set; }
        public ActorStructure Actor { get; private set; }
        public int Color { get; private set; }
        public double Alpha { get; private set; } //mxd
        public byte AlphaByte { get; private set; } //mxd
        public string RenderStyle { get; private set; } //mxd
        public bool Bright { get; private set; } //mxd
        public bool Arrow { get; private set; }
        public float Radius { get; private set; }
        public float RenderRadius { get; private set; }
        public float Height { get; private set; }
        public double DistanceCheckSq { get; private set; } //mxd
        public bool Hangs { get; private set; }
        public int Blocking { get; private set; }
        public int ErrorCheck { get; private set; }
        public bool FixedSize { get; }
        public bool FixedRotation { get; } //mxd
        public ThingCategory Category { get; }
        public ArgumentInfo[] Args { get; }
        public bool IsKnown { get; }
        public bool IsNull { get { return Index == 0; } }
        public bool IsObsolete { get; private set; } //mxd
        public string ObsoleteMessage { get; private set; } //mxd
        public bool AbsoluteZ { get; }
        public bool XYBillboard { get; private set; } //mxd
        public SizeF SpriteScale { get { return spritescale; } }
        public string ClassName { get; private set; } //mxd. Need this to add model overrides for things defined in configs
        public string LightName { get; private set; } //mxd
        public Dictionary<string, string> FlagsRename { get { return flagsrename.ContainsKey(General.Map.Config.FormatInterface) ? flagsrename[General.Map.Config.FormatInterface] : null; } } //mxd

        //mxd. GZDoom rendering properties
        public ThingRenderMode RenderMode { get; private set; }
        public bool RollSprite { get; private set; }
        public bool RollCenter { get; private set; }

        public int ThingLink { get; }

        //mxd. Ambinent sound info
        public AmbientSoundInfo AmbientSound { get; internal set; }

        // [ZZ] GZDoom inheritance data
        public GZGeneral.LightData DynamicLightType { get { return dynamiclighttype; } set { if (dynamiclighttype == null) dynamiclighttype = value; } }

        // [ZZ]
        public bool Optional { get; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal ThingTypeInfo(int index)
        {
            // Initialize
            this.Index = index;
            this.Category = null;
            this.Actor = null;
            this.Title = "<" + index.ToString(CultureInfo.InvariantCulture) + ">";
            this.Sprite = DataManager.INTERNAL_PREFIX + "unknownthing";
            this.ClassName = string.Empty; //mxd
            this.Color = 0;
            this.Alpha = 1f; //mxd
            this.AlphaByte = 255; //mxd
            this.RenderStyle = "normal"; //mxd
            this.Bright = false; //mxd
            this.Arrow = true;
            this.Radius = 10f;
            this.RenderRadius = 10f;
            this.Height = 20f;
            this.DistanceCheckSq = double.MaxValue;
            this.Hangs = false;
            this.Blocking = 0;
            this.ErrorCheck = 0;
            this.spritescale = new SizeF(1.0f, 1.0f);
            this.FixedSize = false;
            this.FixedRotation = false; //mxd
            this.SpriteFrame = new[] { new SpriteFrameInfo { Sprite = Sprite, SpriteLongName = Lump.MakeLongName(Sprite, true) } }; //mxd
            this.Args = new ArgumentInfo[Linedef.NUM_ARGS];
            this.IsKnown = false;
            this.AbsoluteZ = false;
            this.XYBillboard = false;
            this.locksprite = false; //mxd
            this.flagsrename = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase); //mxd
            this.ThingLink = 0;
            this.Optional = false; // [ZZ]
            this.adduniversalfields = new List<string>();

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Constructor
        internal ThingTypeInfo(ThingCategory cat, int index, Configuration cfg, IDictionary<string, EnumList> enums)
        {
            string key = index.ToString(CultureInfo.InvariantCulture);

            // Initialize
            this.Index = index;
            this.Category = cat;
            this.Args = new ArgumentInfo[Linedef.NUM_ARGS];
            this.IsKnown = true;
            this.Actor = null;
            this.Bright = false; //mxd
            this.DistanceCheckSq = double.MaxValue;

            // Read properties
            this.Title = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".title", "<" + key + ">");
            this.Sprite = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".sprite", cat.Sprite);
            this.Color = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".color", cat.Color);
            this.Alpha = General.Clamp(cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".alpha", cat.Alpha), 0f, 1f); //mxd
            this.AlphaByte = (byte)(this.Alpha * 255); //mxd
            this.RenderStyle = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".renderstyle", cat.RenderStyle).ToLower(); //mxd
            this.Arrow = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".arrow", cat.Arrow) != 0;
            this.Radius = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".width", cat.Radius);
            this.Height = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".height", cat.Height);
            this.Hangs = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".hangs", cat.Hangs) != 0;
            this.Blocking = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".blocking", cat.Blocking);
            this.ErrorCheck = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".error", cat.ErrorCheck);
            this.FixedSize = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".fixedsize", cat.FixedSize);
            this.FixedRotation = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".fixedrotation", cat.FixedRotation); //mxd
            this.AbsoluteZ = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".absolutez", cat.AbsoluteZ);
            float sscale = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".spritescale", cat.SpriteScale);
            this.spritescale = new SizeF(sscale, sscale);
            this.locksprite = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".locksprite", false); //mxd
            this.ClassName = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".class", String.Empty); //mxd
            this.ThingLink = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".thinglink", 0);

            // Read universal fields that should be added to this thing type
            adduniversalfields = new List<string>();
            IDictionary adduniversalfieldsdic = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".adduniversalfields", new Hashtable());
            foreach (DictionaryEntry de in adduniversalfieldsdic)
            {
                string addname = de.Key.ToString().ToLowerInvariant();
                if (!adduniversalfields.Contains(addname))
                    adduniversalfields.Add(addname);
            }

            //mxd. Read flagsrename
            this.flagsrename = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            IDictionary maindic = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".flagsrename", new Hashtable());
            foreach (DictionaryEntry de in maindic)
            {
                string ioname = de.Key.ToString().ToLowerInvariant();
                switch (ioname)
                {
                    case "doommapsetio":
                    case "hexenmapsetio":
                    case "universalmapsetio":
                        IDictionary flagdic = de.Value as IDictionary;
                        if (flagdic == null) continue;
                        flagsrename.Add(ioname, new Dictionary<string, string>());
                        foreach (DictionaryEntry fe in flagdic)
                            flagsrename[ioname].Add(fe.Key.ToString(), fe.Value.ToString());
                        break;

                    default: throw new NotImplementedException("Unsupported MapSetIO");
                }
            }

            // Read the args
            for (int i = 0; i < Linedef.NUM_ARGS; i++)
                this.Args[i] = new ArgumentInfo(cfg, "thingtypes." + cat.Name + "." + key, i, enums);

            // Safety
            if (this.Radius < 4f || this.FixedSize) this.Radius = THING_FIXED_SIZE;
            if (this.Hangs && this.AbsoluteZ) this.Hangs = false; //mxd

            //mxd. Create sprite frame
            this.SpriteFrame = new[] { new SpriteFrameInfo { Sprite = Sprite, SpriteLongName = Lump.MakeLongName(Sprite, true) } };

            // [ZZ] optional thing sprite.
            this.Optional = cfg.ReadSetting("thingtypes." + cat.Name + "." + key + ".optional", cat.Optional);

            // [ZZ] generate internal light data
            this.dynamiclighttype = GZGeneral.GetLightDataByNum(index);

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Constructor
        public ThingTypeInfo(ThingCategory cat, int index, string title)
        {
            // Initialize
            this.Index = index;
            this.Category = cat;
            this.Title = title;
            this.Actor = null;
            this.ClassName = string.Empty; //mxd
            this.IsKnown = true;
            this.Bright = false; //mxd
            this.DistanceCheckSq = double.MaxValue;
            this.Args = new ArgumentInfo[Linedef.NUM_ARGS];
            for (int i = 0; i < Linedef.NUM_ARGS; i++) this.Args[i] = new ArgumentInfo(i);

            // Read properties
            this.Sprite = cat.Sprite;
            this.Color = cat.Color;
            this.Arrow = cat.Arrow != 0;
            this.Alpha = cat.Alpha; //mxd
            this.AlphaByte = (byte)(this.Alpha * 255); //mxd
            this.RenderStyle = cat.RenderStyle; //mxd
            this.Radius = cat.Radius;
            this.Height = cat.Height;
            this.Hangs = cat.Hangs != 0;
            this.Blocking = cat.Blocking;
            this.ErrorCheck = cat.ErrorCheck;
            this.FixedSize = cat.FixedSize;
            this.FixedRotation = cat.FixedRotation; //mxd
            this.AbsoluteZ = cat.AbsoluteZ;
            this.spritescale = new SizeF(cat.SpriteScale, cat.SpriteScale);
            this.locksprite = false; //mxd
            this.flagsrename = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase); //mxd
            this.adduniversalfields = new List<string>();

            // Safety
            if (this.Radius < 4f || this.FixedSize) this.Radius = THING_FIXED_SIZE;
            if (this.Hangs && this.AbsoluteZ) this.Hangs = false; //mxd

            //mxd. Create sprite frame
            this.SpriteFrame = new[] { new SpriteFrameInfo { Sprite = Sprite, SpriteLongName = Lump.MakeLongName(Sprite, true) } };

            this.Optional = false; // [ZZ]

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Constructor
        internal ThingTypeInfo(ThingCategory cat, ActorStructure actor)
        {
            // Initialize
            this.Index = actor.DoomEdNum;
            this.Category = cat;
            this.Title = "";
            this.Actor = actor;
            this.ClassName = actor.ClassName; //mxd
            this.IsKnown = true;
            this.Bright = false; //mxd
            this.DistanceCheckSq = double.MaxValue;
            this.Args = new ArgumentInfo[Linedef.NUM_ARGS];
            for (int i = 0; i < Linedef.NUM_ARGS; i++) this.Args[i] = new ArgumentInfo(i);

            // Read properties
            this.Sprite = cat.Sprite;
            this.Color = cat.Color;
            this.Alpha = cat.Alpha; //mxd
            this.AlphaByte = (byte)(this.Alpha * 255); //mxd
            this.RenderStyle = cat.RenderStyle; //mxd
            this.Arrow = cat.Arrow != 0;
            this.Radius = cat.Radius;
            this.Height = cat.Height;
            this.Hangs = cat.Hangs != 0;
            this.Blocking = cat.Blocking;
            this.ErrorCheck = cat.ErrorCheck;
            this.FixedSize = cat.FixedSize;
            this.FixedRotation = cat.FixedRotation; //mxd
            this.AbsoluteZ = cat.AbsoluteZ;
            this.spritescale = new SizeF(cat.SpriteScale, cat.SpriteScale);
            this.flagsrename = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase); //mxd
            this.adduniversalfields = new List<string>();

            // Safety
            if (this.Hangs && this.AbsoluteZ) this.Hangs = false; //mxd

            // Apply settings from actor
            ModifyByDecorateActor(actor);

            //mxd. Create sprite frame
            this.SpriteFrame = new[] { new SpriteFrameInfo { Sprite = Sprite, SpriteLongName = Lump.MakeLongName(Sprite, true) } };

            //
            this.Optional = false; // [ZZ]

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        //mxd. Constructor
        internal ThingTypeInfo(ThingCategory cat, ActorStructure actor, int index)
        {
            // Initialize
            this.Index = index;
            this.Category = cat;
            this.Title = "";
            this.Actor = actor;
            this.ClassName = actor.ClassName; //mxd
            this.IsKnown = true;
            this.Bright = false; //mxd
            this.DistanceCheckSq = double.MaxValue;
            this.Args = new ArgumentInfo[Linedef.NUM_ARGS];
            for (int i = 0; i < Linedef.NUM_ARGS; i++) this.Args[i] = new ArgumentInfo(i);

            // Read properties
            this.Sprite = cat.Sprite;
            this.SpriteFrame = new[] { new SpriteFrameInfo { Sprite = Sprite, SpriteLongName = Lump.MakeLongName(Sprite, true), } }; //mxd
            this.Color = cat.Color;
            this.Alpha = cat.Alpha; //mxd
            this.AlphaByte = (byte)(this.Alpha * 255); //mxd
            this.RenderStyle = cat.RenderStyle; //mxd
            this.Arrow = cat.Arrow != 0;
            this.Radius = cat.Radius;
            this.Height = cat.Height;
            this.Hangs = cat.Hangs != 0;
            this.Blocking = cat.Blocking;
            this.ErrorCheck = cat.ErrorCheck;
            this.FixedSize = cat.FixedSize;
            this.FixedRotation = cat.FixedRotation; //mxd
            this.AbsoluteZ = cat.AbsoluteZ;
            this.spritescale = new SizeF(cat.SpriteScale, cat.SpriteScale);
            this.flagsrename = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase); //mxd
            this.adduniversalfields = new List<string>();

            // Safety
            if (this.Hangs && this.AbsoluteZ) this.Hangs = false; //mxd

            // Apply settings from actor
            ModifyByDecorateActor(actor);

            //mxd. Create sprite frame
            this.SpriteFrame = new[] { new SpriteFrameInfo { Sprite = Sprite, SpriteLongName = Lump.MakeLongName(Sprite, true) } };

            //
            this.Optional = false; // [ZZ]

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Constructor
        internal ThingTypeInfo(int index, ThingTypeInfo other)
        {
            // Initialize
            this.Index = index;
            this.Category = other.Category;
            this.Title = other.Title;
            this.Actor = other.Actor;
            this.ClassName = other.ClassName; //mxd
            this.IsKnown = true;
            this.Args = new ArgumentInfo[Linedef.NUM_ARGS];
            for (int i = 0; i < Linedef.NUM_ARGS; i++)
                this.Args[i] = other.Args[i];

            // Copy properties
            this.Sprite = other.Sprite;
            this.SpriteFrame = new SpriteFrameInfo[other.SpriteFrame.Length]; //mxd
            other.SpriteFrame.CopyTo(this.SpriteFrame, 0); //mxd
            this.Color = other.Color;
            this.Alpha = other.Alpha; //mxd
            this.AlphaByte = other.AlphaByte; //mxd
            this.RenderStyle = other.RenderStyle; //mxd
            this.Bright = other.Bright; //mxd
            this.Arrow = other.Arrow;
            this.Radius = other.Radius;
            this.Height = other.Height;
            this.DistanceCheckSq = other.DistanceCheckSq; //mxd
            this.Hangs = other.Hangs;
            this.Blocking = other.Blocking;
            this.ErrorCheck = other.ErrorCheck;
            this.FixedSize = other.FixedSize;
            this.FixedRotation = other.FixedRotation; //mxd
            this.AbsoluteZ = other.AbsoluteZ;
            this.XYBillboard = other.XYBillboard; //mxd
            this.spritescale = new SizeF(other.spritescale.Width, other.spritescale.Height);
            this.flagsrename = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase); //mxd
            this.adduniversalfields = new List<string>(other.adduniversalfields);

            //mxd. Copy GZDoom rendering properties
            this.RenderMode = other.RenderMode;
            this.RollSprite = other.RollSprite;
            this.RollCenter = other.RollCenter;

            //
            this.dynamiclighttype = other.dynamiclighttype;

            //
            this.Optional = other.Optional;

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        internal ThingTypeInfo(ThingCategory cat, DehackedThing thing) : this(cat, thing.DoomEdNum, thing.Name)
        {
            Category = cat;
            Bright = thing.Bright;

            ModifyByDehackedThing(thing);
        }

        internal ThingTypeInfo(ThingCategory cat, ActorStructure actor, ThingTypeInfo other) : this(actor.DoomEdNum, other)
        {
            Category = cat;

            ModifyByDecorateActor(actor);
        }

        #endregion

        #region ================== Methods

        // This updates the properties from a decorate actor
        internal void ModifyByDecorateActor(ActorStructure actor) { ModifyByDecorateActor(actor, false); } //mxd
        internal void ModifyByDecorateActor(ActorStructure actor, bool replacetitle)
        {
            // Keep reference to actor
            this.Actor = actor;
            this.ClassName = actor.ClassName; //mxd

            // Set the title
            if (actor.HasPropertyWithValue("$title"))
                Title = actor.GetPropertyAllValues("$title");
            else if (actor.HasPropertyWithValue("tag"))
            {
                string tag = actor.GetPropertyAllValues("tag");
                if (!tag.StartsWith("\"$")) Title = tag; //mxd. Don't use LANGUAGE keywords.
            }

            if (string.IsNullOrEmpty(Title) || replacetitle) Title = actor.ClassName;

            //mxd. Color override?
            if (actor.HasPropertyWithValue("$color"))
            {
                int ci = actor.GetPropertyValueInt("$color", 0);
                Color = ci == 0 || ci > 19 ? 18 : ci;
            }

            //mxd. Custom argument titles?
            for (int i = 0; i < Args.Length; i++)
            {
                ArgumentInfo arg = actor.GetArgumentInfo(i);
                if (arg != null)
                    Args[i] = arg;
            }

            //mxd. Some SLADE compatibility
            if (actor.HasProperty("$angled")) this.Arrow = true;
            else if (actor.HasProperty("$notangled")) this.Arrow = false;

            //mxd. Marked as obsolete?
            if (actor.HasPropertyWithValue("$obsolete"))
            {
                ObsoleteMessage = actor.GetPropertyValueString("$obsolete", 0, true);
                IsObsolete = true;
                Color = 4; //red
            }

            // Remove doublequotes from title
            Title = ZDTextParser.StripQuotes(Title); //mxd

            // Set sprite
            StateStructure.FrameInfo info = actor.FindSuitableSprite(); //mxd
            if (!locksprite && info != null) //mxd. Added locksprite property
                Sprite = info.Sprite;
            else if (string.IsNullOrEmpty(Sprite))//mxd
                Sprite = DataManager.INTERNAL_PREFIX + "unknownthing";

            //mxd. Store dynamic light name
            LightName = info != null ? info.LightName : string.Empty;

            //mxd. Create sprite frame
            this.SpriteFrame = new[] { new SpriteFrameInfo { Sprite = Sprite, SpriteLongName = Lump.MakeLongName(Sprite, true) } };

            // Set sprite scale (mxd. Scale is translated to xscale and yscale in ActorStructure)
            if (actor.HasPropertyWithValue("xscale"))
                this.spritescale.Width = actor.GetPropertyValueFloat("xscale", 0);

            if (actor.HasPropertyWithValue("yscale"))
                this.spritescale.Height = actor.GetPropertyValueFloat("yscale", 0);

            // Size
            if (actor.HasPropertyWithValue("radius")) Radius = actor.GetPropertyValueFloat("radius", 0);
            if (actor.HasPropertyWithValue("height")) Height = actor.GetPropertyValueFloat("height", 0);
            if (actor.HasPropertyWithValue("renderradius")) RenderRadius = actor.GetPropertyValueFloat("renderradius", 0);
            if (RenderRadius == 0)
                RenderRadius = Radius;

            // DistanceCheck. The value is CVAR. Also we'll need squared value
            if (actor.HasPropertyWithValue("distancecheck"))
            {
                string cvarname = actor.GetPropertyValueString("distancecheck", 0);
                if (General.Map.Data.CVars.AllNames.Contains(cvarname))
                {
                    if (!General.Map.Data.CVars.Integers.ContainsKey(cvarname))
                    {
                        General.ErrorLogger.Add(ErrorType.Error, "Error in actor \"" + Title + "\":" + Index + ". DistanceCheck property references cvar \"" + cvarname + "\" which has to be of type int, but is not");
                        DistanceCheckSq = double.MaxValue;
                    }
                    else
                    {
                        DistanceCheckSq = Math.Pow(General.Map.Data.CVars.Integers[cvarname], 2);
                    }
                }
                else
                {
                    General.ErrorLogger.Add(ErrorType.Error, "Error in actor \"" + Title + "\":" + Index + ". DistanceCheck property references undefined cvar \"" + cvarname + "\"");
                }
            }

            //mxd. Renderstyle
            if (actor.HasPropertyWithValue("renderstyle") && !actor.HasProperty("$ignorerenderstyle"))
                RenderStyle = actor.GetPropertyValueString("renderstyle", 0, true).ToLower();

            //mxd. Alpha
            if (actor.HasPropertyWithValue("alpha"))
            {
                this.Alpha = General.Clamp(actor.GetPropertyValueFloat("alpha", 0), 0f, 1f);
                this.AlphaByte = (byte)(this.Alpha * 255);
            }
            else if (actor.HasProperty("defaultalpha"))
            {
                this.Alpha = General.Map.Config.BaseGame == GameType.HERETIC ? 0.4f : 0.6f;
                this.AlphaByte = (byte)(this.Alpha * 255);
            }

            //mxd. BRIGHT
            this.Bright = (info != null && info.Bright) || actor.GetFlagValue("bright", false);

            // Safety
            if (this.Radius < 4f || this.FixedSize) this.Radius = THING_FIXED_SIZE;
            if (this.spritescale.Width == 0.0f) this.spritescale.Width = 1.0f;
            if (this.spritescale.Height == 0.0f) this.spritescale.Height = 1.0f;

            // Options
            Hangs = actor.GetFlagValue("spawnceiling", Hangs);
            int blockvalue = (Blocking > 0) ? Blocking : 2;
            Blocking = actor.GetFlagValue("solid", Blocking != 0) ? blockvalue : 0;
            XYBillboard = actor.GetFlagValue("forcexybillboard", false); //mxd

            //mxd. GZDoom rendering flags
            if (actor.GetFlagValue("wallsprite", false)) RenderMode = ThingRenderMode.WALLSPRITE;
            if (actor.GetFlagValue("flatsprite", false))
            {
                // WALLSPRITE + FLATSPRITE = HORRIBLE GLITCHES in GZDoom
                if (RenderMode == ThingRenderMode.WALLSPRITE)
                    General.ErrorLogger.Add(ErrorType.Error, "Error in actor \"" + Title + "\":" + Index + ". WALLSPRITE and FLATSPRITE flags can not be combined");
                else
                    RenderMode = ThingRenderMode.FLATSPRITE;
            }
            //mxd. WALLSPRITE and FLATSPRITE support rolling without the ROLLSPRITE flag
            RollSprite = actor.GetFlagValue("rollsprite", RenderMode == ThingRenderMode.WALLSPRITE || RenderMode == ThingRenderMode.FLATSPRITE);
            if (RollSprite) RollCenter = actor.GetFlagValue("rollcenter", false);

            //mxd
            if (Blocking > THING_BLOCKING_NONE) ErrorCheck = THING_ERROR_INSIDE_STUCK;

            // [ZZ]
            dynamiclighttype = GZGeneral.GetGZLightTypeByClass(actor);
        }

        /// <summary>
        /// Modifies the thing type info by the given Dehacked thing.
        /// </summary>
        /// <param name="thing">The Dehacked thing to modify the thing type info by</param>
        internal void ModifyByDehackedThing(DehackedThing thing)
        {
            if (string.IsNullOrEmpty(thing.Sprite))
                Sprite = DataManager.INTERNAL_PREFIX + "unknownthing";
            else
                Sprite = thing.Sprite;

            Title = thing.Name;
            if (thing.Height != 0) Height = thing.Height;
            if (thing.Width != 0) Radius = thing.Width;
            Blocking = thing.Bits.Contains("solid") ? 1 : 0;
            Hangs = thing.Bits.Contains("spawnceiling");

            if (thing.Props.ContainsKey("$editor angled"))
                Arrow = thing.Angled == ThingAngled.YES;

            if (thing.Color >= 0 && thing.Color <= 19)
                Color = thing.Color;

            SpriteFrame = new[] { new SpriteFrameInfo { Sprite = Sprite, SpriteLongName = Lump.MakeLongName(Sprite, true) } };
        }

        /// <summary>
        /// Changes the sprite of the thing by Dehacked sprite replacements.
        /// </summary>
        /// <param name="texts">Dehacked sprite replacements</param>
        internal void ModifyBySpriteReplacement(Dictionary<string, string> texts)
        {
            // Cut the sprite into the 4 character base sprite and the animation, then rebuild 
            // the sprite with the sprite replacement if necessary...
            if (Sprite.Length >= 4)
            {
                string basesprite = Sprite.Substring(0, 4);
                string animation = Sprite.Substring(4);

                if (texts.ContainsKey(basesprite))
                {
                    Sprite = texts[basesprite] + animation;
                }
            }

            // ... then do the same with every sprite frame
            for (int i = 0; i < SpriteFrame.Length; i++)
            {
                SpriteFrameInfo sfi = SpriteFrame[i];

                if (sfi.Sprite.Length < 4)
                    continue;

                string basesprite = sfi.Sprite.Substring(0, 4);
                string animation = sfi.Sprite.Substring(4);

                if (texts.ContainsKey(basesprite))
                {
                    string newsprite = texts[basesprite] + animation;
                    SpriteFrame[i] = new SpriteFrameInfo { Sprite = newsprite, SpriteLongName = Lump.MakeLongName(newsprite, true) };
                }
            }
        }

        //mxd. This tries to find all possible sprite rotations. Returns true when voxel substitute exists
        internal bool SetupSpriteFrame(HashSet<string> allspritenames, HashSet<string> allvoxelnames)
        {
            // Empty, invalid or internal sprites don't have rotations
            // Info: we can have either partial 5-char sprite name from DECORATE parser,
            // or fully defined 6/8-char sprite name defined in Game configuration or by $Sprite property 
            if (string.IsNullOrEmpty(Sprite) || Sprite.StartsWith(DataManager.INTERNAL_PREFIX)
                || (Sprite.Length != 5 && Sprite.Length != 6 && Sprite.Length != 8)) return false;

            string sourcename = Sprite.Substring(0, 4);
            char sourceframe = Sprite[4];

            // First try voxels
            if (allvoxelnames.Count > 0)
            {
                // Find a voxel, which matches sourcename
                HashSet<string> voxelnames = new HashSet<string>();
                foreach (string s in allvoxelnames)
                {
                    if (s.StartsWith(sourcename)) voxelnames.Add(s);
                }

                // Find a voxel, which matches baseframe
                // Valid voxel can be either 4-char (POSS), 5-char (POSSA) or 6-char (POSSA0)
                string newsprite = string.Empty;

                // Check 6-char voxels...
                foreach (string v in voxelnames)
                {
                    if (v.Length == 6 && v.StartsWith(sourcename + sourceframe) && WADReader.IsValidSpriteName(v))
                    {
                        newsprite = v;
                        break;
                    }
                }

                // Check 5-char voxels...
                if (voxelnames.Contains(sourcename + sourceframe)) newsprite = sourcename + sourceframe;

                // Check 4-char voxels...
                if (voxelnames.Contains(sourcename)) newsprite = sourcename;

                // Voxel found?
                if (!string.IsNullOrEmpty(newsprite))
                {
                    // Assign new sprite
                    Sprite = newsprite;

                    // Recreate sprite frame
                    SpriteFrame = new[] { new SpriteFrameInfo { Sprite = Sprite, SpriteLongName = Lump.MakeLongName(Sprite, true) } };

                    // Substitute voxel found
                    return true;
                }
            }

            // Then try sprites
            // Find a sprite, which matches sourcename
            string sourcesprite = string.Empty;
            HashSet<string> spritenames = new HashSet<string>();
            foreach (string s in allspritenames)
            {
                if (s.StartsWith(sourcename))
                    spritenames.Add(s);
            }

            // Find a sprite, which matches baseframe
            foreach (string s in spritenames)
            {
                if (s[4] == sourceframe || (s.Length == 8 && s[6] == sourceframe))
                {
                    sourcesprite = s;
                    break;
                }
            }

            // Abort if no sprite was found
            if (string.IsNullOrEmpty(sourcesprite)) return false;

            // Get sprite angle
            string anglestr = sourcesprite.Substring(5, 1);
            int sourceangle;
            if (!int.TryParse(anglestr, NumberStyles.Integer, CultureInfo.InvariantCulture, out sourceangle))
            {
                General.ErrorLogger.Add(ErrorType.Error, "Error in actor \"" + Title + "\":" + Index + ". Unable to get sprite angle from sprite \"" + sourcesprite + "\"");
                return false;
            }

            if (sourceangle < 0 || sourceangle > 8)
            {
                General.ErrorLogger.Add(ErrorType.Error, "Error in actor \"" + Title + "\":" + Index + ", sprite \"" + sourcesprite + "\". Sprite angle must be in [0..8] range");
                return false;
            }

            // No rotations? Then spriteframe is already setup
            if (sourceangle == 0)
            {
                // Sprite name still incomplete?
                if (Sprite.Length < 6)
                {
                    Sprite = sourcesprite;

                    // Recreate sprite frame. Mirror the sprite if sourceframe matches the second frame block
                    SpriteFrame = new[] { new SpriteFrameInfo { Sprite = Sprite, SpriteLongName = Lump.MakeLongName(Sprite, true),
                                                                Mirror = Sprite.Length == 8 && Sprite[6] == sourceframe } };
                }

                return false;
            }

            // Gather rotations
            string[] frames = new string[8];
            bool[] mirror = new bool[8];
            int processedcount = 0;

            // Process gathered sprites
            foreach (string s in spritenames)
            {
                // Check first frame block
                char targetframe = s[4];
                if (targetframe == sourceframe)
                {
                    // Check angle
                    int targetangle;
                    anglestr = s.Substring(5, 1);
                    if (!int.TryParse(anglestr, NumberStyles.Integer, CultureInfo.InvariantCulture, out targetangle))
                    {
                        General.ErrorLogger.Add(ErrorType.Error, "Error in actor \"" + Title + "\":" + Index + ". Unable to get sprite angle from sprite \"" + s + "\"");
                        return false;
                    }

                    // Sanity checks
                    if (targetangle == 0)
                    {
                        General.ErrorLogger.Add(ErrorType.Warning, "Warning: actor \"" + Title + "\":" + Index + ", sprite \"" + sourcename + "\", frame " + targetframe + " has both rotated and non-rotated versions");
                        continue;
                    }

                    // More sanity checks
                    if (targetangle < 1 || targetangle > 8)
                    {
                        General.ErrorLogger.Add(ErrorType.Error, "Error in actor \"" + Title + "\":" + Index + ", sprite \"" + s + "\". Expected sprite angle in [1..8] range");
                        return false;
                    }

                    // Even more sanity checks
                    if (!string.IsNullOrEmpty(frames[targetangle - 1]))
                    {
                        General.ErrorLogger.Add(ErrorType.Warning, "Warning in actor \"" + Title + "\":" + Index
                            + ". Sprite \"" + sourcename + "\", frame " + targetframe + ", angle " + targetangle
                            + " is double-defined in sprites \"" + frames[targetangle - 1] + "\" and \"" + s + "\"");
                    }
                    else
                    {
                        // Add to collection
                        frames[targetangle - 1] = s;
                        processedcount++;
                    }
                }

                // Check second frame block?
                if (s.Length == 6) continue;

                targetframe = s[6];
                if (targetframe == sourceframe)
                {
                    // Check angle
                    int targetangle;
                    anglestr = s.Substring(7, 1);
                    if (!int.TryParse(anglestr, NumberStyles.Integer, CultureInfo.InvariantCulture, out targetangle))
                    {
                        General.ErrorLogger.Add(ErrorType.Error, "Error in actor \"" + Title + "\":" + Index + ". Unable to get sprite angle from sprite \"" + s + "\"");
                        return false;
                    }

                    // Sanity checks
                    if (targetangle == 0)
                    {
                        General.ErrorLogger.Add(ErrorType.Warning, "Warning: actor \"" + Title + "\":" + Index + ", sprite \"" + sourcename + "\", frame " + targetframe + " has both rotated and non-rotated versions");
                        continue;
                    }

                    // More sanity checks
                    if (targetangle < 1 || targetangle > 8)
                    {
                        General.ErrorLogger.Add(ErrorType.Error, "Error in actor \"" + Title + "\":" + Index + ", sprite \"" + s + "\". Expected sprite angle in [1..8] range");
                        return false;
                    }

                    // Even more sanity checks
                    if (!string.IsNullOrEmpty(frames[targetangle - 1]))
                    {
                        General.ErrorLogger.Add(ErrorType.Warning, "Warning in actor \"" + Title + "\":" + Index
                            + ". Sprite \"" + sourcename + "\", frame " + targetframe + ", angle " + targetangle
                            + " is double-defined in sprites \"" + frames[targetangle - 1] + "\" and \"" + s + "\"");
                    }
                    else
                    {
                        // Add to collections
                        frames[targetangle - 1] = s;
                        mirror[targetangle - 1] = true;
                        processedcount++;
                    }
                }

                // Gathered all sprites?
                if (processedcount == 8) break;
            }

            // Check collected data
            if (processedcount != 8)
            {
                // Check which angles are missing
                List<string> missingangles = new List<string>();
                for (int i = 0; i < frames.Length; i++)
                {
                    if (string.IsNullOrEmpty(frames[i]))
                        missingangles.Add((i + 1).ToString());
                }

                // Assemble angles to display
                string ma = string.Join(", ", missingangles.ToArray());
                if (missingangles.Count > 2)
                {
                    int pos = ma.LastIndexOf(",", StringComparison.Ordinal);
                    if (pos != -1) ma = ma.Remove(pos, 1).Insert(pos, " and");
                }

                General.ErrorLogger.Add(ErrorType.Error, "Error in actor \"" + Title + "\":" + Index + ". Sprite rotations " + ma + " for sprite " + sourcename + ", frame " + sourceframe + " are missing");
                return false;
            }

            // Create collection
            SpriteFrame = new SpriteFrameInfo[frames.Length];
            for (int i = 0; i < frames.Length; i++)
            {
                SpriteFrame[i] = new SpriteFrameInfo { Sprite = frames[i], SpriteLongName = Lump.MakeLongName(frames[i]), Mirror = mirror[i] };
            }

            // Update preview sprite
            Sprite = SpriteFrame[1].Sprite;

            // Done
            return false;
        }

        // This is used for sorting
        public int CompareTo(ThingTypeInfo other)
        {
            return string.Compare(this.Title, other.Title, true);
        }

        // String representation
        public override string ToString()
        {
            return Title + " (" + Index + ")";
        }

        public bool HasAddUniversalField(string fieldname)
        {
            return adduniversalfields != null && adduniversalfields.Contains(fieldname);
        }

        #endregion
    }
}
