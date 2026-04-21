#region ================== Copyright (c) 2021 Boris Iwanski

/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.Dehacked
{
    public enum ThingAngled
    {
        UNCHANGED,
        YES,
        NO
    }

    public class DehackedThing
    {
        #region ================== Variables

        private int doomednum;
        private int initialframe;
        private int height;
        private int width;
        private int color;

        #endregion

        #region ================== Properties

        public int Number { get; }
        public Dictionary<string, string> Props { get; }
        public int DoomEdNum { get { return doomednum; } internal set { doomednum = value; } }
        public bool HasDoomEdNum { get; internal set; }
        public string Name { get; internal set; }
        public int InitialFrame { get { return initialframe; } internal set { initialframe = value; } }
        public string Sprite { get; internal set; }
        public int Height { get { return height; } internal set { height = value; } }
        public int Width { get { return width; } internal set { width = value; } }
        public List<string> Bits { get; }
        public string Category { get; private set; }
        public int Color { get { return color; } }
        public bool Bright { get; private set; }
        public ThingAngled Angled { get; private set; }

        #endregion

        #region ================== Constructor

        internal DehackedThing(int number, string name)
        {
            this.Number = number;
            this.Name = name;
            color = -1;
            Sprite = null;
            Angled = ThingAngled.UNCHANGED;
            HasDoomEdNum = false;

            Props = new Dictionary<string, string>();
            Bits = new List<string>();
        }

        internal DehackedThing(int number, string name, Dictionary<string, string> props) : this(number, name)
        {
            foreach (string key in props.Keys)
            {
                this.Props[key.ToLowerInvariant()] = props[key];

                if (key.ToLowerInvariant() == "id #")
                    HasDoomEdNum = true;
            }
        }

        #endregion

        /// <summary>
        /// Processes the thing, setting it up according to the properties defined in the Dehacked patch.
        /// </summary>
        /// <param name="frames">Dehacked frames the thing could use</param>
        /// <param name="bitmnemonics">Bit mnemonics</param>
        /// <param name="basething">The base thing to copy properties from</param>
        /// <param name="availablesprites">All sprites available in the resources</param>
        internal void Process(Dictionary<int, DehackedFrame> frames, Dictionary<long, string> bitmnemonics, DehackedThing basething, HashSet<string> availablesprites)
        {
            // Copy all missing properties from the base thing
            if (basething != null)
            {
                doomednum = basething.DoomEdNum;
                HasDoomEdNum = basething.HasDoomEdNum;

                foreach (string key in basething.Props.Keys)
                    if (!Props.ContainsKey(key))
                        Props[key] = basething.Props[key];
            }

            foreach (KeyValuePair<string, string> kvp in Props)
            {
                string prop = kvp.Key.ToLowerInvariant();
                string value = kvp.Value;

                switch (prop)
                {
                    case "id #":
                        int.TryParse(value, out doomednum);

                        // Things with DoomEdNum -1 can not be placed in a map, so treat them as having no DoomEdNum at all
                        HasDoomEdNum = (doomednum == -1) ? false : true;
                        break;
                    case "initial frame":
                        if (Sprite == null && int.TryParse(value, out initialframe))
                        {
                            if (frames.ContainsKey(initialframe))
                            {
                                // It doesn't seem to matter which rotation we select, UDB will automagically
                                // find the correct sprites later. We just try to find a sprite that's available
                                // in the loaded resources, either xxxxA0 (i.e. without rotations) or xxxxA1 (i.e. with rotations)
                                if (!string.IsNullOrEmpty(frames[initialframe].Sprite))
                                {
                                    string spritename = frames[initialframe].Sprite + Convert.ToChar(frames[initialframe].SpriteSubNumber + 'A');
                                    if (availablesprites.Contains(spritename + "0"))
                                        Sprite = spritename + "0";
                                    else
                                        Sprite = spritename + "1";
                                }
                                else
                                    Sprite = null;

                                Bright = frames[initialframe].Bright;
                            }
                            else
                            {
                                General.ErrorLogger.Add(ErrorType.Error, "Dehacked thing " + Number + " is referencing initial frame " + initialframe + " that is not defined.");
                            }
                        }
                        break;
                    case "width":
                        if (int.TryParse(value, out width))
                        {
                            // Value is in 16.16 fixed point, so shift it
                            width >>= 16;
                        }
                        break;
                    case "height":
                        if (int.TryParse(value, out height))
                        {
                            // Value is in 16.16 fixed point, so shift it
                            height >>= 16;
                        }
                        break;
                    case "bits":
                        long allbits;
                        // Try to parse the value as an number, if that works it's an old-school bit set and not mnemonics
                        if (long.TryParse(value, out allbits))
                        {
                            // Go through all given mnemonics and translate the bits to them
                            foreach (long mask in bitmnemonics.Keys)
                                if ((mask & allbits) == mask)
                                    Bits.Add(bitmnemonics[mask]);
                        }
                        else
                        {
                            // The bits are mnemonics, so split them and turn them into a list
                            foreach (string mnemonic in value.Split('+'))
                                Bits.Add(mnemonic.Trim().ToLowerInvariant());
                        }
                        break;
                    case "$editor category":
                        Category = value;
                        break;
                    case "$editor color id":
                        if (!int.TryParse(value, out color))
                            color = 18; // Default light brown
                        break;
                    case "$editor sprite":
                        Sprite = value;
                        break;
                    case "$editor angled":
                        if (value.ToLowerInvariant() == "true")
                            Angled = ThingAngled.YES;
                        else
                            Angled = ThingAngled.NO;
                        break;
                }
            }
        }
    }
}
