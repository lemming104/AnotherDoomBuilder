
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
using System;
using System.Drawing;
using System.Globalization;
using Configuration = CodeImp.DoomBuilder.IO.Configuration;

#endregion

namespace CodeImp.DoomBuilder.Rendering
{
    public sealed class ColorCollection
    {
        #region ================== Constants

        // Assist color creation
        private const float BRIGHT_MULTIPLIER = 1.0f;
        private const float BRIGHT_ADDITION = 0.4f;
        private const float DARK_MULTIPLIER = 0.9f;
        private const float DARK_ADDITION = -0.2f;

        // Palette size
        private const int NUM_COLORS = 53;
        public const int NUM_THING_COLORS = 20;
        public const int THING_COLORS_OFFSET = 20;

        // Colors!
        public const int BACKGROUND = 0;
        public const int VERTICES = 1;
        public const int LINEDEFS = 2;
        public const int MODELWIRECOLOR = 3; //mxd
        public const int INFOLINECOLOR = 4; //mxd
        public const int HIGHLIGHT = 5;
        public const int SELECTION = 6;
        public const int INDICATION = 7;
        public const int GRID = 8;
        public const int GRID64 = 9;
        public const int CROSSHAIR3D = 10;
        public const int HIGHLIGHT3D = 11;
        public const int SELECTION3D = 12;
        public const int SCRIPTBACKGROUND = 13;
        public const int LINENUMBERS = 14;
        public const int PLAINTEXT = 15;
        public const int COMMENTS = 16;
        public const int KEYWORDS = 17;
        public const int LITERALS = 18;
        public const int CONSTANTS = 19;
        public const int THINGCOLOR00 = 20;
        public const int THINGCOLOR01 = 21;
        public const int THINGCOLOR02 = 22;
        public const int THINGCOLOR03 = 23;
        public const int THINGCOLOR04 = 24;
        public const int THINGCOLOR05 = 25;
        public const int THINGCOLOR06 = 26;
        public const int THINGCOLOR07 = 27;
        public const int THINGCOLOR08 = 28;
        public const int THINGCOLOR09 = 29;
        public const int THINGCOLOR10 = 30;
        public const int THINGCOLOR11 = 31;
        public const int THINGCOLOR12 = 32;
        public const int THINGCOLOR13 = 33;
        public const int THINGCOLOR14 = 34;
        public const int THINGCOLOR15 = 35;
        public const int THINGCOLOR16 = 36;
        public const int THINGCOLOR17 = 37;
        public const int THINGCOLOR18 = 38;
        public const int THINGCOLOR19 = 39;
        public const int THREEDFLOORCOLOR = 40; //mxd
        public const int SCRIPTINDICATOR = 41; //mxd. Additional Script Editor colors
        public const int SCRIPTBRACEHIGHLIGHT = 42;
        public const int SCRIPTBADBRACEHIGHLIGHT = 43;
        public const int SCRIPTWHITESPACE = 44;
        public const int SCRIPTSELECTIONFORE = 45;
        public const int SCRIPTSELECTIONBACK = 46;
        public const int STRINGS = 47;
        public const int INCLUDES = 48;
        public const int SCRIPTFOLDFORE = 49;
        public const int SCRIPTFOLDBACK = 50;
        public const int PROPERTIES = 51;
        public const int GUIDELINECOLOR = 52; //mxd

        #endregion

        #region ================== Variables

        // Colors

        // Color-correction table
        private byte[] correctiontable;

        #endregion

        #region ================== Properties

        public PixelColor[] Colors { get; }
        public PixelColor[] BrightColors { get; }
        public PixelColor[] DarkColors { get; }

        public PixelColor Background { get { return Colors[BACKGROUND]; } internal set { Colors[BACKGROUND] = value; } }
        public PixelColor Vertices { get { return Colors[VERTICES]; } internal set { Colors[VERTICES] = value; } }
        public PixelColor Linedefs { get { return Colors[LINEDEFS]; } internal set { Colors[LINEDEFS] = value; } }
        public PixelColor Highlight { get { return Colors[HIGHLIGHT]; } internal set { Colors[HIGHLIGHT] = value; } }
        public PixelColor Selection { get { return Colors[SELECTION]; } internal set { Colors[SELECTION] = value; } }
        public PixelColor Indication { get { return Colors[INDICATION]; } internal set { Colors[INDICATION] = value; } }
        public PixelColor Grid { get { return Colors[GRID]; } internal set { Colors[GRID] = value; } }
        public PixelColor Grid64 { get { return Colors[GRID64]; } internal set { Colors[GRID64] = value; } }

        //mxd
        public PixelColor ModelWireframe { get { return Colors[MODELWIRECOLOR]; } internal set { Colors[MODELWIRECOLOR] = value; } }
        public PixelColor InfoLine { get { return Colors[INFOLINECOLOR]; } internal set { Colors[INFOLINECOLOR] = value; } }
        public PixelColor Guideline { get { return Colors[GUIDELINECOLOR]; } internal set { Colors[GUIDELINECOLOR] = value; } }
        public PixelColor ThreeDFloor { get { return Colors[THREEDFLOORCOLOR]; } internal set { Colors[THREEDFLOORCOLOR] = value; } }

        public PixelColor Crosshair3D { get { return Colors[CROSSHAIR3D]; } internal set { Colors[CROSSHAIR3D] = value; } }
        public PixelColor Highlight3D { get { return Colors[HIGHLIGHT3D]; } internal set { Colors[HIGHLIGHT3D] = value; } }
        public PixelColor Selection3D { get { return Colors[SELECTION3D]; } internal set { Colors[SELECTION3D] = value; } }

        public PixelColor ScriptBackground { get { return Colors[SCRIPTBACKGROUND]; } internal set { Colors[SCRIPTBACKGROUND] = value; } }
        public PixelColor ScriptIndicator { get { return Colors[SCRIPTINDICATOR]; } internal set { Colors[SCRIPTINDICATOR] = value; } } //mxd
        public PixelColor ScriptBraceHighlight { get { return Colors[SCRIPTBRACEHIGHLIGHT]; } internal set { Colors[SCRIPTBRACEHIGHLIGHT] = value; } } //mxd
        public PixelColor ScriptBadBraceHighlight { get { return Colors[SCRIPTBADBRACEHIGHLIGHT]; } internal set { Colors[SCRIPTBADBRACEHIGHLIGHT] = value; } } //mxd
        public PixelColor ScriptWhitespace { get { return Colors[SCRIPTWHITESPACE]; } internal set { Colors[SCRIPTWHITESPACE] = value; } } //mxd
        public PixelColor ScriptSelectionForeColor { get { return Colors[SCRIPTSELECTIONFORE]; } internal set { Colors[SCRIPTSELECTIONFORE] = value; } } //mxd
        public PixelColor ScriptSelectionBackColor { get { return Colors[SCRIPTSELECTIONBACK]; } internal set { Colors[SCRIPTSELECTIONBACK] = value; } } //mxd
        public PixelColor LineNumbers { get { return Colors[LINENUMBERS]; } internal set { Colors[LINENUMBERS] = value; } }
        public PixelColor PlainText { get { return Colors[PLAINTEXT]; } internal set { Colors[PLAINTEXT] = value; } }
        public PixelColor Comments { get { return Colors[COMMENTS]; } internal set { Colors[COMMENTS] = value; } }
        public PixelColor Keywords { get { return Colors[KEYWORDS]; } internal set { Colors[KEYWORDS] = value; } }
        public PixelColor Properties { get { return Colors[PROPERTIES]; } internal set { Colors[PROPERTIES] = value; } }
        public PixelColor Literals { get { return Colors[LITERALS]; } internal set { Colors[LITERALS] = value; } }
        public PixelColor Constants { get { return Colors[CONSTANTS]; } internal set { Colors[CONSTANTS] = value; } }
        public PixelColor Strings { get { return Colors[STRINGS]; } internal set { Colors[STRINGS] = value; } } //mxd
        public PixelColor Includes { get { return Colors[INCLUDES]; } internal set { Colors[INCLUDES] = value; } } //mxd
        public PixelColor ScriptFoldForeColor { get { return Colors[SCRIPTFOLDFORE]; } internal set { Colors[SCRIPTFOLDFORE] = value; } } //mxd
        public PixelColor ScriptFoldBackColor { get { return Colors[SCRIPTFOLDBACK]; } internal set { Colors[SCRIPTFOLDBACK] = value; } } //mxd

        #endregion

        #region ================== Constructor / Disposer

        // Constructor for settings from configuration
        internal ColorCollection(Configuration cfg)
        {
            // Initialize
            Colors = new PixelColor[NUM_COLORS];
            BrightColors = new PixelColor[NUM_COLORS];
            DarkColors = new PixelColor[NUM_COLORS];

            // Read all colors from config
            for (int i = 0; i < NUM_COLORS; i++)
            {
                // Read color
                Colors[i] = PixelColor.FromInt(cfg.ReadSetting("colors.color" + i.ToString(CultureInfo.InvariantCulture), 0));
            }

            //mxd. Set new colors (previously these were defined in GZBuilder.default.cfg)
            if (Colors[BACKGROUND].ToInt() == 0) Colors[BACKGROUND] = PixelColor.FromInt(-16777216);
            if (Colors[VERTICES].ToInt() == 0) Colors[VERTICES] = PixelColor.FromInt(-11425537);
            if (Colors[LINEDEFS].ToInt() == 0) Colors[LINEDEFS] = PixelColor.FromInt(-1);
            if (Colors[MODELWIRECOLOR].ToInt() == 0) Colors[MODELWIRECOLOR] = PixelColor.FromInt(-4259937);
            if (Colors[INFOLINECOLOR].ToInt() == 0) Colors[INFOLINECOLOR] = PixelColor.FromInt(-3750145);
            if (Colors[HIGHLIGHT].ToInt() == 0) Colors[HIGHLIGHT] = PixelColor.FromInt(-21504);
            if (Colors[SELECTION].ToInt() == 0) Colors[SELECTION] = PixelColor.FromInt(-49152);
            if (Colors[INDICATION].ToInt() == 0) Colors[INDICATION] = PixelColor.FromInt(-128);
            if (Colors[GRID].ToInt() == 0) Colors[GRID] = PixelColor.FromInt(-12171706);
            if (Colors[GRID64].ToInt() == 0) Colors[GRID64] = PixelColor.FromInt(-13018769);
            if (Colors[CROSSHAIR3D].ToInt() == 0) Colors[CROSSHAIR3D] = PixelColor.FromInt(-16711681); // Unused!
            if (Colors[HIGHLIGHT3D].ToInt() == 0) Colors[HIGHLIGHT3D] = PixelColor.FromInt(-24576);
            if (Colors[SELECTION3D].ToInt() == 0) Colors[SELECTION3D] = PixelColor.FromInt(-49152);
            if (Colors[SCRIPTBACKGROUND].ToInt() == 0) Colors[SCRIPTBACKGROUND] = PixelColor.FromInt(-1);
            if (Colors[LINENUMBERS].ToInt() == 0) Colors[LINENUMBERS] = PixelColor.FromInt(-13921873);
            if (Colors[PLAINTEXT].ToInt() == 0) Colors[PLAINTEXT] = PixelColor.FromInt(-16777216);
            if (Colors[COMMENTS].ToInt() == 0) Colors[COMMENTS] = PixelColor.FromInt(-16744448);
            if (Colors[KEYWORDS].ToInt() == 0) Colors[KEYWORDS] = PixelColor.FromInt(-16741493);
            if (Colors[LITERALS].ToInt() == 0) Colors[LITERALS] = PixelColor.FromInt(-16776999);
            if (Colors[CONSTANTS].ToInt() == 0) Colors[CONSTANTS] = PixelColor.FromInt(-8372160);
            if (Colors[GUIDELINECOLOR].ToInt() == 0) Colors[GUIDELINECOLOR] = PixelColor.FromInt(-256);

            // Set new thing colors
            if (Colors[THINGCOLOR00].ToInt() == 0) Colors[THINGCOLOR00] = PixelColor.FromColor(Color.DimGray);
            if (Colors[THINGCOLOR01].ToInt() == 0) Colors[THINGCOLOR01] = PixelColor.FromColor(Color.RoyalBlue);
            if (Colors[THINGCOLOR02].ToInt() == 0) Colors[THINGCOLOR02] = PixelColor.FromColor(Color.ForestGreen);
            if (Colors[THINGCOLOR03].ToInt() == 0) Colors[THINGCOLOR03] = PixelColor.FromColor(Color.LightSeaGreen);
            if (Colors[THINGCOLOR04].ToInt() == 0) Colors[THINGCOLOR04] = PixelColor.FromColor(Color.Firebrick);
            if (Colors[THINGCOLOR05].ToInt() == 0) Colors[THINGCOLOR05] = PixelColor.FromColor(Color.DarkViolet);
            if (Colors[THINGCOLOR06].ToInt() == 0) Colors[THINGCOLOR06] = PixelColor.FromColor(Color.DarkGoldenrod);
            if (Colors[THINGCOLOR07].ToInt() == 0) Colors[THINGCOLOR07] = PixelColor.FromColor(Color.Silver);
            if (Colors[THINGCOLOR08].ToInt() == 0) Colors[THINGCOLOR08] = PixelColor.FromColor(Color.Gray);
            if (Colors[THINGCOLOR09].ToInt() == 0) Colors[THINGCOLOR09] = PixelColor.FromColor(Color.DeepSkyBlue);
            if (Colors[THINGCOLOR10].ToInt() == 0) Colors[THINGCOLOR10] = PixelColor.FromColor(Color.LimeGreen);
            if (Colors[THINGCOLOR11].ToInt() == 0) Colors[THINGCOLOR11] = PixelColor.FromColor(Color.PaleTurquoise);
            if (Colors[THINGCOLOR12].ToInt() == 0) Colors[THINGCOLOR12] = PixelColor.FromColor(Color.Tomato);
            if (Colors[THINGCOLOR13].ToInt() == 0) Colors[THINGCOLOR13] = PixelColor.FromColor(Color.Violet);
            if (Colors[THINGCOLOR14].ToInt() == 0) Colors[THINGCOLOR14] = PixelColor.FromColor(Color.Yellow);
            if (Colors[THINGCOLOR15].ToInt() == 0) Colors[THINGCOLOR15] = PixelColor.FromColor(Color.WhiteSmoke);
            if (Colors[THINGCOLOR16].ToInt() == 0) Colors[THINGCOLOR16] = PixelColor.FromColor(Color.LightPink);
            if (Colors[THINGCOLOR17].ToInt() == 0) Colors[THINGCOLOR17] = PixelColor.FromColor(Color.DarkOrange);
            if (Colors[THINGCOLOR18].ToInt() == 0) Colors[THINGCOLOR18] = PixelColor.FromColor(Color.DarkKhaki);
            if (Colors[THINGCOLOR19].ToInt() == 0) Colors[THINGCOLOR19] = PixelColor.FromColor(Color.Goldenrod);

            //mxd. Set the rest of new colors (previously these were also defined in GZBuilder.default.cfg)
            if (Colors[THREEDFLOORCOLOR].ToInt() == 0) Colors[THREEDFLOORCOLOR] = PixelColor.FromInt(-65536);
            if (Colors[SCRIPTINDICATOR].ToInt() == 0) Colors[SCRIPTINDICATOR] = PixelColor.FromInt(-16711936);
            if (Colors[SCRIPTBRACEHIGHLIGHT].ToInt() == 0) Colors[SCRIPTBRACEHIGHLIGHT] = PixelColor.FromInt(-16711681);
            if (Colors[SCRIPTBADBRACEHIGHLIGHT].ToInt() == 0) Colors[SCRIPTBADBRACEHIGHLIGHT] = PixelColor.FromInt(-65536);
            if (Colors[SCRIPTWHITESPACE].ToInt() == 0) Colors[SCRIPTWHITESPACE] = PixelColor.FromInt(-8355712);
            if (Colors[SCRIPTSELECTIONFORE].ToInt() == 0) Colors[SCRIPTSELECTIONFORE] = PixelColor.FromInt(-1);
            if (Colors[SCRIPTSELECTIONBACK].ToInt() == 0) Colors[SCRIPTSELECTIONBACK] = PixelColor.FromInt(-13395457);
            if (Colors[STRINGS].ToInt() == 0) Colors[STRINGS] = PixelColor.FromInt(-8388608);
            if (Colors[INCLUDES].ToInt() == 0) Colors[INCLUDES] = PixelColor.FromInt(-9868951);
            if (Colors[SCRIPTFOLDFORE].ToInt() == 0) Colors[SCRIPTFOLDFORE] = PixelColor.FromColor(SystemColors.ControlDark);
            if (Colors[SCRIPTFOLDBACK].ToInt() == 0) Colors[SCRIPTFOLDBACK] = PixelColor.FromColor(SystemColors.ControlLightLight);
            if (Colors[PROPERTIES].ToInt() == 0) Colors[PROPERTIES] = PixelColor.FromInt(-16752191);

            // Create assist colors
            CreateAssistColors();

            // Create color correction table
            CreateCorrectionTable();

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        #endregion

        #region ================== Methods

        // This generates a color-correction table
        internal void CreateCorrectionTable()
        {
            // Determine amounts
            float gamma = (General.Settings.ImageBrightness + 10) * 0.1f;
            float bright = General.Settings.ImageBrightness * 5f;

            // Make table
            correctiontable = new byte[256];

            // Fill table
            for (int i = 0; i < 256; i++)
            {
                byte b;
                float a = (i * gamma) + bright;
                if (a < 0f) b = 0; else if (a > 255f) b = 255; else b = (byte)a;
                correctiontable[i] = b;
            }
        }

        // This applies color-correction over a block of pixel data
        internal unsafe void ApplyColorCorrection(PixelColor* pixels, int numpixels)
        {
            for (PixelColor* cp = pixels + numpixels - 1; cp >= pixels; cp--)
            {
                cp->r = correctiontable[cp->r];
                cp->g = correctiontable[cp->g];
                cp->b = correctiontable[cp->b];
            }
        }

        // This quantizes an image to a PLAYPAL lump, putting indices into an array of integers.
        internal unsafe void QuantizeColorsToPlaypal(PixelColor* inPixels, int[] indices, Playpal playpal)
        {
            System.Threading.Tasks.Parallel.For(0, indices.Length, (i) => indices[i] = playpal.FindClosestColor(inPixels[i]));
        }

        // This clamps a value between 0 and 1
        private static float Saturate(float v)
        {
            if (v < 0f) return 0f; else if (v > 1f) return 1f; else return v;
        }

        // This creates assist colors
        internal void CreateAssistColors()
        {
            // Go for all colors
            for (int i = 0; i < NUM_COLORS; i++)
            {
                // Create assist colors
                BrightColors[i] = CreateBrightVariant(Colors[i]);
                DarkColors[i] = CreateDarkVariant(Colors[i]);
            }
        }

        // This creates a brighter color
        public PixelColor CreateBrightVariant(PixelColor pc)
        {
            Color4 o = pc.ToColorValue();
            Color4 c = new Color4(0f, 0f, 0f, 1f);

            // Create brighter color
            c.Red = Saturate((o.Red * BRIGHT_MULTIPLIER) + BRIGHT_ADDITION);
            c.Green = Saturate((o.Green * BRIGHT_MULTIPLIER) + BRIGHT_ADDITION);
            c.Blue = Saturate((o.Blue * BRIGHT_MULTIPLIER) + BRIGHT_ADDITION);
            return PixelColor.FromInt(c.ToArgb());
        }

        // This creates a darker color
        public PixelColor CreateDarkVariant(PixelColor pc)
        {
            Color4 o = pc.ToColorValue();
            Color4 c = new Color4(0f, 0f, 0f, 1f);

            // Create darker color
            c.Red = Saturate((o.Red * DARK_MULTIPLIER) + DARK_ADDITION);
            c.Green = Saturate((o.Green * DARK_MULTIPLIER) + DARK_ADDITION);
            c.Blue = Saturate((o.Blue * DARK_MULTIPLIER) + DARK_ADDITION);
            return PixelColor.FromInt(c.ToArgb());
        }

        // This saves colors to configuration
        internal void SaveColors(Configuration cfg)
        {
            // Write all colors to config
            for (int i = 0; i < NUM_COLORS; i++)
            {
                // Write color
                cfg.WriteSetting("colors.color" + i.ToString(CultureInfo.InvariantCulture), Colors[i].ToInt());
            }
        }

        #endregion
    }
}
