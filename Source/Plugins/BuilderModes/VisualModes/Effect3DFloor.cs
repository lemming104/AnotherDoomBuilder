#region === Copyright (c) 2010 Pascal van der Heiden ===

using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
    internal class Effect3DFloor : SectorEffect
    {
        // Linedef that is used to create this effect
        // The sector can be found by linedef.Front.Sector

        // Floor and ceiling planes
        public PixelColor ColorFloor { get; private set; }
        public PixelColor ColorCeiling { get; private set; }

        // Alpha transparency

        // Vavoom type?

        //mxd. Render backsides?

        //mxd. Dirty hack to emulate GZDoom behaviour?

        //mxd. Render using Additive pass?

        //mxd. Sidedef should be clipped by floor/ceiling?

        //mxd. Ignore Bottom Height?

        // Properties
        public int Alpha { get; private set; }
        public SectorLevel Floor { get; private set; }
        public SectorLevel Ceiling { get; private set; }
        public Linedef Linedef { get; }
        public bool VavoomType { get; private set; }
        public bool RenderInside { get; private set; } //mxd
        public bool RenderAdditive { get; private set; } //mxd
        public bool IgnoreBottomHeight { get; private set; } //mxd
        public bool Sloped3dFloor { get; private set; } //mxd
        public bool ClipSidedefs { get; private set; } //mxd

        //mxd. 3D-Floor Flags
        [Flags]
        public enum Flags
        {
            None = 0,
            DisableLighting = 1,
            RestrictLighting = 2,
            Fog = 4,
            IgnoreBottomHeight = 8,
            UseUpperTexture = 16,
            UseLowerTexture = 32,
            RenderAdditive = 64,
            Fade = 512,
            ResetLighting = 1024,
        }

        //mxd. 3D-Floor Types
        [Flags]
        public enum FloorTypes
        {
            VavoomStyle = 0,
            Solid = 1,
            Swimmable = 2,
            NonSolid = 3,
            RenderInside = 4,
            HiTagIsLineID = 8,
            InvertVisibilityRules = 16,
            InvertShootabilityRules = 32
        }

        // Constructor
        public Effect3DFloor(SectorData data, Linedef sourcelinedef) : base(data)
        {
            Linedef = sourcelinedef;

            // New effect added: This sector needs an update!
            if (data.Mode.VisualSectorExists(data.Sector))
            {
                BaseVisualSector vs = (BaseVisualSector)data.Mode.GetVisualSector(data.Sector);
                vs.UpdateSectorGeometry(true);
            }
        }

        // This makes sure we are updated with the source linedef information
        public override void Update()
        {
            SectorData sd = data.Mode.GetSectorData(Linedef.Front.Sector);
            if (!sd.Updated) sd.Update();
            sd.AddUpdateSector(data.Sector, true);

            if (Floor == null)
            {
                Floor = new SectorLevel(sd.Floor);
                data.AddSectorLevel(Floor);
            }

            if (Ceiling == null)
            {
                Ceiling = new SectorLevel(sd.Ceiling);
                data.AddSectorLevel(Ceiling);
            }

            // For non-vavoom types, we must switch the level types
            if (Linedef.Args[1] != (int)FloorTypes.VavoomStyle)
            {
                //mxd. check for Swimmable/RenderInside/RenderAdditive flags
                RenderAdditive = (Linedef.Args[2] & (int)Flags.RenderAdditive) == (int)Flags.RenderAdditive;
                RenderInside = (((Linedef.Args[1] & (int)FloorTypes.Swimmable) == (int)FloorTypes.Swimmable) && (Linedef.Args[1] & (int)FloorTypes.NonSolid) != (int)FloorTypes.NonSolid)
                              || ((Linedef.Args[1] & (int)FloorTypes.RenderInside) == (int)FloorTypes.RenderInside);
                IgnoreBottomHeight = (Linedef.Args[2] & (int)Flags.IgnoreBottomHeight) == (int)Flags.IgnoreBottomHeight;

                VavoomType = false;
                Alpha = General.Clamp(Linedef.Args[3], 0, 255);
                sd.Ceiling.CopyProperties(Floor);
                sd.Floor.CopyProperties(Ceiling);
                Floor.type = SectorLevelType.Floor;
                Floor.plane = sd.Ceiling.plane.GetInverted();
                Ceiling.type = SectorLevelType.Ceiling;
                Ceiling.plane = IgnoreBottomHeight ? sd.Ceiling.plane : sd.Floor.plane.GetInverted(); //mxd. Use upper plane when "ignorebottomheight" flag is set

                //mxd
                ClipSidedefs = !RenderInside && !RenderAdditive && Alpha > 254 && !IgnoreBottomHeight;

                // A 3D floor's color is always that of the sector it is placed in
                // (unless it's affected by glow) - mxd
                if (sd.CeilingGlow == null || !sd.CeilingGlow.Fullbright) Floor.color = 0;
            }
            else
            {
                VavoomType = true;
                RenderAdditive = false; //mxd
                ClipSidedefs = true; //mxd
                Floor.type = SectorLevelType.Ceiling;
                Floor.plane = sd.Ceiling.plane;
                Ceiling.type = SectorLevelType.Floor;
                Ceiling.plane = sd.Floor.plane;
                Alpha = 255;

                // A 3D floor's color is always that of the sector it is placed in
                // (unless it's affected by glow) - mxd
                if (sd.FloorGlow == null || !sd.FloorGlow.Fullbright) Ceiling.color = 0;
            }

            //mxd
            Floor.extrafloor = true;
            Ceiling.extrafloor = true;
            Floor.splitsides = !ClipSidedefs;
            Ceiling.splitsides = !ClipSidedefs && !IgnoreBottomHeight; // if "ignorebottomheight" flag is set, both ceiling and floor will be at the same level and sidedef clipping with floor level will fail resulting in incorrect light props transfer in some cases

            //mxd. Check slopes, cause GZDoom can't handle sloped translucent 3d floors...
            Sloped3dFloor = (Alpha < 255 || RenderAdditive) &&
                             (Angle2D.RadToDeg(Ceiling.plane.Normal.GetAngleZ()) != 270 ||
                              Angle2D.RadToDeg(Floor.plane.Normal.GetAngleZ()) != 90);

            // As GZDoom doesn't support translucent 3D floors make is fully opaque, except when the alpha is 0 (invisible)
            if (Sloped3dFloor && Alpha > 0)
                Alpha = 255;

            // Apply alpha
            Floor.alpha = Alpha;
            Ceiling.alpha = Alpha;

            // Do not adjust light? (works only for non-vavoom types)
            if (!VavoomType)
            {
                bool disablelighting = (Linedef.Args[2] & (int)Flags.DisableLighting) == (int)Flags.DisableLighting; //mxd
                bool restrictlighting = (Linedef.Args[2] & (int)Flags.RestrictLighting) == (int)Flags.RestrictLighting; //mxd
                Floor.resetlighting = (Linedef.Args[2] & (int)Flags.ResetLighting) == (int)Flags.ResetLighting; //mxd

                if (disablelighting || restrictlighting)
                {
                    Floor.restrictlighting = restrictlighting; //mxd
                    Floor.disablelighting = disablelighting; //mxd

                    if (disablelighting) //mxd
                    {
                        Floor.color = 0;
                        Floor.brightnessbelow = -1;
                        Floor.colorbelow = PixelColor.FromInt(0);
                    }

                    Ceiling.disablelighting = disablelighting; //mxd
                    Ceiling.restrictlighting = restrictlighting; //mxd

                    Ceiling.color = 0;
                    Ceiling.brightnessbelow = -1;
                    Ceiling.colorbelow = PixelColor.FromInt(0);
                }
            }

            if (VavoomType)
            {
                ColorFloor = sd.ColorCeiling;
                ColorCeiling = sd.ColorFloor;

            }
            else
            {
                ColorFloor = sd.ColorFloor;
                ColorCeiling = sd.ColorCeiling;
            }
        }
    }
}
