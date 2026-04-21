#region === Copyright (c) 2010 Pascal van der Heiden ===

using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.GZBuilder.Data;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;
using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
    internal class SectorData
    {
        #region ================== Variables

        // VisualMode

        // Sector for which this data is

        // Levels have been updated?

        // This prevents recursion
        private bool isupdating;

        // All planes in the sector that cast or are affected by light

        // Effects
        private readonly EffectGlowingFlat glowingflateffect; //mxd

        internal GlowingFlatData CeilingGlow; //mxd
        internal GlowingFlatData FloorGlow; //mxd
        internal Plane FloorGlowPlane; //mxd
        internal Plane CeilingGlowPlane; //mxd

        // [ZZ] Doom64 lighting system
        internal PixelColor ColorCeiling;
        internal PixelColor ColorFloor;
        internal PixelColor ColorWallBottom;
        internal PixelColor ColorWallTop;
        internal PixelColor ColorSprites;

        // Sectors that must be updated when this sector is changed
        // The boolean value is the 'includeneighbours' of the UpdateSectorGeometry function which
        // indicates if the sidedefs of neighbouring sectors should also be rebuilt.

        // Original floor and ceiling levels
        private readonly SectorLevel floorbase; // mxd. Sector floor level, unaffected by glow / light properties transfer
        private readonly SectorLevel ceilingbase; // mxd. Sector ceiling level, unaffected by glow / light properties transfer 

        // This helps keeping track of changes
        // otherwise we update ceiling/floor too much
        private bool floorchanged;
        private bool ceilingchanged;

        //mxd. Absolute lights are not affected by brightness transfers...
        private bool lightfloorabsolute;
        private bool lightceilingabsolute;
        private int lightfloor;
        private int lightceiling;

        #endregion

        #region ================== Properties

        public Sector Sector { get; }
        public bool Updated { get; private set; }
        public bool FloorChanged { get { return floorchanged; } set { floorchanged |= value; } }
        public bool CeilingChanged { get { return ceilingchanged; } set { ceilingchanged |= value; } }
        public List<SectorLevel> LightLevels { get; }
        public List<Effect3DFloor> ExtraFloors { get; }
        public List<SectorEffect> Effects { get; } //mxd
        public SectorLevel Floor { get; }
        public SectorLevel Ceiling { get; }
        public BaseVisualMode Mode { get; }
        public Dictionary<Sector, bool> UpdateAlso { get; }

        #endregion

        #region ================== Constructor / Destructor

        // Constructor
        public SectorData(BaseVisualMode mode, Sector s)
        {
            // Initialize
            this.Mode = mode;
            this.Sector = s;
            this.Updated = false;
            this.floorchanged = false;
            this.ceilingchanged = false;
            this.LightLevels = new List<SectorLevel>(2);
            this.ExtraFloors = new List<Effect3DFloor>(1);
            this.Effects = new List<SectorEffect>(1);
            this.UpdateAlso = new Dictionary<Sector, bool>(2);
            this.Floor = new SectorLevel(Sector, SectorLevelType.Floor);
            this.floorbase = new SectorLevel(Sector, SectorLevelType.Floor); //mxd
            this.Ceiling = new SectorLevel(Sector, SectorLevelType.Ceiling);
            this.ceilingbase = new SectorLevel(Sector, SectorLevelType.Ceiling); //mxd
            this.glowingflateffect = new EffectGlowingFlat(this); //mxd

            // Add ceiling and floor
            LightLevels.Add(Floor);
            LightLevels.Add(Ceiling);

            BasicSetup();
        }

        #endregion

        #region ================== Public Methods

        // 3D Floor effect
        public void AddEffect3DFloor(Linedef sourcelinedef)
        {
            Effect3DFloor e = new Effect3DFloor(this, sourcelinedef);
            ExtraFloors.Add(e);
            Effects.Add(e);

            //mxd. Extrafloor neighbours should be updated when extrafloor is changed
            foreach (Sidedef sd in this.Sector.Sidedefs)
            {
                if (sd.Other != null && sd.Other.Sector != null)
                    AddUpdateSector(sd.Other.Sector, false);
            }
        }

        // Brightness level effect
        public void AddEffectBrightnessLevel(Linedef sourcelinedef)
        {
            EffectBrightnessLevel e = new EffectBrightnessLevel(this, sourcelinedef);
            Effects.Add(e);
        }

        //mxd. Transfer Floor Brightness effect
        public void AddEffectTransferFloorBrightness(Linedef sourcelinedef)
        {
            EffectTransferFloorBrightness e = new EffectTransferFloorBrightness(this, sourcelinedef);
            Effects.Add(e);
        }

        //mxd. Transfer Floor Brightness effect
        public void AddEffectTransferCeilingBrightness(Linedef sourcelinedef)
        {
            EffectTransferCeilingBrightness e = new EffectTransferCeilingBrightness(this, sourcelinedef);
            Effects.Add(e);
        }

        // Line slope effect
        public void AddEffectLineSlope(Linedef sourcelinedef)
        {
            EffectLineSlope e = new EffectLineSlope(this, sourcelinedef);
            Effects.Add(e);
        }

        //mxd. Plane copy slope effect
        public void AddEffectPlaneClopySlope(Linedef sourcelinedef, bool front)
        {
            EffectPlaneCopySlope e = new EffectPlaneCopySlope(this, sourcelinedef, front);
            Effects.Add(e);
        }

        // Copy slope effect
        public void AddEffectCopySlope(Thing sourcething)
        {
            EffectCopySlope e = new EffectCopySlope(this, sourcething);
            Effects.Add(e);
        }

        // Thing line slope effect
        public void AddEffectThingLineSlope(Thing sourcething, Sidedef sourcesidedef)
        {
            EffectThingLineSlope e = new EffectThingLineSlope(this, sourcething, sourcesidedef);
            Effects.Add(e);
        }

        // Thing slope effect
        public void AddEffectThingSlope(Thing sourcething)
        {
            EffectThingSlope e = new EffectThingSlope(this, sourcething);
            Effects.Add(e);
        }

        // Thing vertex slope effect
        public void AddEffectThingVertexSlope(List<Thing> sourcethings, bool slopefloor)
        {
            EffectThingVertexSlope e = new EffectThingVertexSlope(this, sourcethings, slopefloor);
            Effects.Add(e);
        }

        //mxd. Add UDMF vertex offset effect
        public void AddEffectVertexOffset()
        {
            EffectUDMFVertexOffset e = new EffectUDMFVertexOffset(this);
            Effects.Add(e);
        }

        // This adds a sector for updating
        public void AddUpdateSector(Sector s, bool includeneighbours)
        {
            UpdateAlso[s] = includeneighbours;
        }

        // This adds a sector level
        public void AddSectorLevel(SectorLevel level)
        {
            // Note: Inserting before the end so that the ceiling stays
            // at the end and the floor at the beginning
            LightLevels.Insert(LightLevels.Count - 1, level);
        }

        // This resets this sector data and all sectors that require updating after me
        /*public void Reset()
		{
			if(isupdating) return;
			isupdating = true;

			// This is set to false so that this sector is rebuilt the next time it is needed!
			updated = false;

			// The visual sector associated is now outdated
			if(mode.VisualSectorExists(sector))
			{
				BaseVisualSector vs = (BaseVisualSector)mode.GetVisualSector(sector);
				vs.UpdateSectorGeometry(false);
			}
			
			// Also reset the sectors that depend on this sector
			foreach(KeyValuePair<Sector, bool> s in updatesectors)
			{
				SectorData sd = mode.GetSectorData(s.Key);
				sd.Reset();
			}

			isupdating = false;
		}*/

        //mxd. This marks this sector data and all sector datas that require updating as not updated
        public void Reset(bool resetneighbours)
        {
            if (isupdating) return;
            isupdating = true;

            // This is set to false so that this sector is rebuilt the next time it is needed!
            Updated = false;

            // The visual sector associated is now outdated
            if (Mode.VisualSectorExists(Sector))
            {
                BaseVisualSector vs = (BaseVisualSector)Mode.GetVisualSector(Sector);
                vs.Changed = true;
            }

            // Reset the sectors that depend on this sector
            if (resetneighbours)
            {
                foreach (KeyValuePair<Sector, bool> s in UpdateAlso)
                {
                    SectorData sd = Mode.GetSectorDataEx(s.Key);
                    if (sd != null) sd.Reset(s.Value);
                }
            }

            isupdating = false;
        }

        // This sets up the basic floor and ceiling, as they would be in normal Doom circumstances
        private void BasicSetup()
        {
            //mxd
            if (Sector.FloorSlope.GetLengthSq() > 0 && !double.IsNaN(Sector.FloorSlopeOffset / Sector.FloorSlope.z))
            {
                // Sloped plane
                Floor.plane = new Plane(Sector.FloorSlope, Sector.FloorSlopeOffset);
            }
            else
            {
                // Normal (flat) floor plane
                Floor.plane = new Plane(new Vector3D(0, 0, 1), -Sector.FloorHeight);
            }

            if (Sector.CeilSlope.GetLengthSq() > 0 && !double.IsNaN(Sector.CeilSlopeOffset / Sector.CeilSlope.z))
            {
                // Sloped plane
                Ceiling.plane = new Plane(Sector.CeilSlope, Sector.CeilSlopeOffset);
            }
            else
            {
                // Normal (flat) ceiling plane
                Ceiling.plane = new Plane(new Vector3D(0, 0, -1), Sector.CeilHeight);
            }

            // Fetch ZDoom fields
            int color = Sector.Fields.GetValue("lightcolor", -1);
            lightfloor = Sector.Fields.GetValue("lightfloor", 0);
            lightfloorabsolute = Sector.Fields.GetValue("lightfloorabsolute", false);
            lightceiling = Sector.Fields.GetValue("lightceiling", 0);
            lightceilingabsolute = Sector.Fields.GetValue("lightceilingabsolute", false);
            if (!lightfloorabsolute) lightfloor = Sector.Brightness + lightfloor;
            if (!lightceilingabsolute) lightceiling = Sector.Brightness + lightceiling;

            // Determine colors & light levels
            // [ZZ] Doom64 lighting
            //
            // ceiling/floor
            ColorCeiling = PixelColor.FromInt(Sector.Fields.GetValue("color_ceiling", PixelColor.INT_WHITE));
            ColorFloor = PixelColor.FromInt(Sector.Fields.GetValue("color_floor", PixelColor.INT_WHITE));
            ColorSprites = PixelColor.FromInt(Sector.Fields.GetValue("color_sprites", PixelColor.INT_WHITE));
            ColorWallTop = PixelColor.FromInt(Sector.Fields.GetValue("color_walltop", PixelColor.INT_WHITE));
            ColorWallBottom = PixelColor.FromInt(Sector.Fields.GetValue("color_wallbottom", PixelColor.INT_WHITE));

            PixelColor floorbrightness = PixelColor.FromInt(Mode.CalculateBrightness(lightfloor));
            PixelColor ceilingbrightness = PixelColor.FromInt(Mode.CalculateBrightness(lightceiling));
            PixelColor lightcolor = PixelColor.FromInt(color);
            PixelColor floorcolor = PixelColor.Modulate(ColorFloor, PixelColor.Modulate(lightcolor, floorbrightness));
            PixelColor ceilingcolor = PixelColor.Modulate(ColorCeiling, PixelColor.Modulate(lightcolor, ceilingbrightness));
            Floor.color = floorcolor.WithAlpha(255).ToInt();
            Floor.brightnessbelow = Sector.Brightness;
            Floor.colorbelow = lightcolor.WithAlpha(255);
            Floor.d64color = ColorFloor;
            Ceiling.color = ceilingcolor.WithAlpha(255).ToInt();
            Ceiling.brightnessbelow = Sector.Brightness;
            Ceiling.colorbelow = lightcolor.WithAlpha(255);
            Ceiling.d64color = ColorCeiling;

            //mxd. Store a copy of initial settings
            Floor.CopyProperties(floorbase);
            Ceiling.CopyProperties(ceilingbase);

            //mxd. We need sector brightness here, unaffected by custom ceiling brightness...
            ceilingbase.brightnessbelow = Sector.Brightness;
            ceilingbase.color = PixelColor.FromInt(Mode.CalculateBrightness(Sector.Brightness)).WithAlpha(255).ToInt();

            //mxd
            glowingflateffect.Update();
        }

        //mxd
        public void UpdateForced()
        {
            Updated = false;
            Update();
        }

        // When no geometry has been changed and no effects have been added or removed,
        // you can call this again to update existing effects. The effects will update
        // the existing SectorLevels to match with any changes.
        public void Update()
        {
            if (isupdating || Updated) return;
            isupdating = true;

            // Set floor/ceiling to their original setup
            BasicSetup();

            // Update all effects
            foreach (SectorEffect e in Effects) e.Update();

            //mxd. Do complicated light level shenanigans only when there are extrafloors
            if (LightLevels.Count > 2)
            {
                // Sort the levels
                SectorLevelComparer comparer = new SectorLevelComparer(Sector);
                LightLevels.Sort(0, LightLevels.Count, comparer);

                // Now that we know the levels in this sector (and in the right order)
                // we can determine the lighting in between and on the levels.
                SectorLevel stored = ceilingbase;

                //mxd. Special cases...
                if (LightLevels[LightLevels.Count - 1].disablelighting)
                {
                    LightLevels[LightLevels.Count - 1].colorbelow = stored.colorbelow;
                    LightLevels[LightLevels.Count - 1].brightnessbelow = stored.brightnessbelow;
                    LightLevels[LightLevels.Count - 1].color = GetLevelColor(stored, LightLevels[LightLevels.Count - 1]);
                }

                //mxd. Cast light properties from top to bottom
                for (int i = LightLevels.Count - 2; i >= 0; i--)
                {
                    SectorLevel l = LightLevels[i];
                    SectorLevel pl = LightLevels[i + 1];

                    // Glow levels don't cast light
                    if (pl.type == SectorLevelType.Glow && LightLevels.Count > i + 2) pl = LightLevels[i + 2];

                    if (l.lighttype == LightLevelType.TYPE1)
                    {
                        stored = pl;
                    }
                    // Use stored light params when "disablelighting" flag is set
                    else if (l.disablelighting)
                    {
                        l.colorbelow = stored.colorbelow;
                        l.brightnessbelow = stored.brightnessbelow;
                        l.color = GetLevelColor(stored, l);
                    }
                    else if (l.restrictlighting)
                    {
                        if (!pl.restrictlighting && pl != Ceiling) stored = pl;
                        l.color = GetLevelColor(stored, l);

                        // This is the bottom side of extrafloor with "restrict lighting" flag. Make it cast stored light props. 
                        if (l.type == SectorLevelType.Ceiling)
                        {
                            // Special case: 2 intersecting extrafloors with "restrictlighting" flag...
                            if (pl.restrictlighting && pl.type == SectorLevelType.Floor && pl.sector.Index != l.sector.Index)
                            {
                                // Use light and color settings from previous layer
                                l.colorbelow = pl.colorbelow;
                                l.brightnessbelow = pl.brightnessbelow;
                                l.color = GetLevelColor(pl, l);

                                // Also colorize previous layer using next higher level color 
                                if (i + 2 < LightLevels.Count) pl.color = GetLevelColor(LightLevels[i + 2], pl);
                            }
                            else
                            {
                                l.colorbelow = stored.colorbelow;
                                l.brightnessbelow = stored.brightnessbelow;
                            }
                        }
                    }
                    // Bottom TYPE1 border requires special handling...
                    else if (l.lighttype == LightLevelType.TYPE1_BOTTOM)
                    {
                        // Use brightness and color from previous light level when it's between TYPE1 and TYPE1_BOTTOM levels
                        if (pl.type == SectorLevelType.Light && pl.lighttype != LightLevelType.TYPE1)
                        {
                            l.brightnessbelow = pl.brightnessbelow;
                            l.colorbelow = pl.colorbelow;
                        }
                        // Use brightness and color from the light level above TYPE1 level
                        else if (stored.type == SectorLevelType.Light)
                        {
                            l.brightnessbelow = stored.brightnessbelow;
                            l.colorbelow = stored.colorbelow;
                        }
                        // Otherwise light values from the real ceiling are used 
                    }
                    else if (l.lighttype == LightLevelType.UNKNOWN)
                    {
                        // Use stored light level when previous one has "disablelighting" flag
                        // or is the lower boundary of an extrafloor with "restrictlighting" flag
                        SectorLevel src = pl.disablelighting || (pl.restrictlighting && pl.type == SectorLevelType.Ceiling) ? stored : pl;

                        // Don't change real ceiling light when previous level has "disablelighting" flag
                        // Don't change anything when light properties were reset before hitting floor (otherwise floor UDMF brightness will be lost)
                        if ((src == ceilingbase && l == Ceiling)
                            || (src == Ceiling && l == Floor && src.LightPropertiesMatch(ceilingbase)))
                            continue;

                        // Transfer color and brightness if previous level has them
                        if (src.colorbelow.a > 0 && src.brightnessbelow != -1)
                        {
                            // Only surface brightness is retained when a glowing flat is used as extrafloor texture
                            if (!l.affectedbyglow) l.color = GetLevelColor(src, l);

                            // Transfer brightnessbelow and colorbelow if current level is not extrafloor top
                            if (!(l.extrafloor && l.type == SectorLevelType.Floor))
                            {
                                l.brightnessbelow = src.brightnessbelow;
                                l.colorbelow = src.colorbelow;
                            }
                        }

                        // Store bottom extrafloor level if it doesn't have "restrictlighting" or "restrictlighting" flags set
                        if (l.extrafloor && l.type == SectorLevelType.Ceiling && !l.restrictlighting && !l.disablelighting) stored = l;
                    }

                    // Reset lighting?
                    if (l.resetlighting) stored = ceilingbase;
                }
            }

            //mxd. Apply ceiling glow effect?
            if (CeilingGlow != null && CeilingGlow.Fullbright)
            {
                Ceiling.color = PixelColor.INT_WHITE;
            }

            //mxd. Apply floor glow effect?
            if (FloorGlow != null)
            {
                // Update floor color
                if (FloorGlow.Fullbright) Floor.color = PixelColor.INT_WHITE;

                // Update brightness
                Floor.brightnessbelow = FloorGlow.Fullbright ? 255 : Math.Max(128, Floor.brightnessbelow);

                if (Floor.colorbelow.ToInt() == 0)
                {
                    byte bb = (byte)Floor.brightnessbelow;
                    Floor.colorbelow = new PixelColor(255, bb, bb, bb);
                }
            }

            //mxd
            Floor.affectedbyglow = FloorGlow != null;
            Ceiling.affectedbyglow = CeilingGlow != null;

            floorchanged = false;
            ceilingchanged = false;
            Updated = true;
            isupdating = false;
        }

        // This returns the level above the given point
        public SectorLevel GetLevelAbove(Vector3D pos)
        {
            SectorLevel found = null;
            double dist = double.MaxValue;

            foreach (SectorLevel l in LightLevels)
            {
                double d = l.plane.GetZ(pos) - pos.z;
                if ((d > 0.0f) && (d < dist))
                {
                    dist = d;
                    found = l;
                }
            }

            return found;
        }

        //mxd. This returns the level above the given point or the level given point is located on
        public SectorLevel GetLevelAboveOrAt(Vector3D pos)
        {
            SectorLevel found = null;
            double dist = double.MaxValue;

            foreach (SectorLevel l in LightLevels)
            {
                double d = l.plane.GetZ(pos) - pos.z;
                if ((d >= 0.0f) && (d < dist))
                {
                    dist = d;
                    found = l;
                }
            }

            return found;
        }

        // This returns the level above the given point
        public SectorLevel GetCeilingAbove(Vector3D pos)
        {
            SectorLevel found = null;
            double dist = double.MaxValue;

            foreach (SectorLevel l in LightLevels)
            {
                if (l.type == SectorLevelType.Ceiling)
                {
                    double d = l.plane.GetZ(pos) - pos.z;
                    if ((d > 0.0f) && (d < dist))
                    {
                        dist = d;
                        found = l;
                    }
                }
            }

            return found;
        }

        // This returns the level below the given point
        public SectorLevel GetLevelBelow(Vector3D pos)
        {
            SectorLevel found = null;
            double dist = double.MaxValue;

            foreach (SectorLevel l in LightLevels)
            {
                double d = pos.z - l.plane.GetZ(pos);
                if ((d > 0.0f) && (d < dist))
                {
                    dist = d;
                    found = l;
                }
            }

            return found;
        }

        // This returns the floor below the given point
        public SectorLevel GetFloorBelow(Vector3D pos)
        {
            SectorLevel found = null;
            double dist = double.MaxValue;

            foreach (SectorLevel l in LightLevels)
            {
                if (l.type == SectorLevelType.Floor)
                {
                    double d = pos.z - l.plane.GetZ(pos);
                    if ((d > 0.0f) && (d < dist))
                    {
                        dist = d;
                        found = l;
                    }
                }
            }

            return found;
        }

        //mxd
        private int GetLevelColor(SectorLevel src, SectorLevel target)
        {
            PixelColor brightness;
            if (lightfloorabsolute && target == Floor)
                brightness = PixelColor.FromInt(Mode.CalculateBrightness(lightfloor));
            else if (lightceilingabsolute && target == Ceiling)
                brightness = PixelColor.FromInt(Mode.CalculateBrightness(lightceiling));
            else
                brightness = PixelColor.FromInt(Mode.CalculateBrightness(src.brightnessbelow));

            PixelColor color = PixelColor.Modulate(target.d64color, PixelColor.Modulate(src.colorbelow, brightness));
            return color.WithAlpha(255).ToInt();
        }

        #endregion
    }
}
