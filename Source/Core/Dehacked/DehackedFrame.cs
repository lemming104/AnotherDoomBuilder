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

using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.Dehacked
{
    public class DehackedFrame
    {
        #region ================== Variables

        #endregion

        #region ================== Properties

        public int Number { get; internal set; }
        public int SpriteNumber { get; internal set; }
        public long SpriteSubNumber { get; internal set; }
        public Dictionary<string, string> Props { get; }
        public string Sprite { get; internal set; }
        public bool Bright { get; private set; }

        #endregion

        #region ================== Constructor

        internal DehackedFrame(int number)
        {
            this.Number = number;
            Sprite = string.Empty;
            Props = new Dictionary<string, string>();
        }

        internal DehackedFrame(int number, Dictionary<string, string> props) : this(number)
        {
            foreach (string key in props.Keys)
                this.Props[key.ToLowerInvariant()] = props[key];
        }

        #endregion

        #region ================== Methods

        /// <summary>
        /// Processes the frame, setting it up so it can be used by things
        /// </summary>
        /// <param name="definedsprites">All available Dehacked sprites</param>
        /// <param name="baseframe">The base Dehacked frame</param>
        internal void Process(Dictionary<int, string> definedsprites, DehackedFrame baseframe)
        {
            // Copy all missing properties of the base frame
            if (baseframe != null)
            {
                foreach (string key in baseframe.Props.Keys)
                    if (!Props.ContainsKey(key))
                        Props[key] = baseframe.Props[key];
            }

            foreach (KeyValuePair<string, string> kvp in Props)
            {
                string prop = kvp.Key.ToLowerInvariant();
                string value = kvp.Value;

                switch (prop)
                {
                    case "sprite number":
                        SpriteNumber = int.Parse(value);
                        if (definedsprites.ContainsKey(SpriteNumber))
                            Sprite = definedsprites[SpriteNumber];
                        else
                            General.ErrorLogger.Add(ErrorType.Error, "Dehacked frame " + Number + " is referencing sprite " + SpriteNumber + " that is not defined.");
                        break;
                    case "sprite subnumber":
                        SpriteSubNumber = long.Parse(value);
                        if (SpriteSubNumber >= 32768)
                        {
                            SpriteSubNumber -= 32768;
                            Bright = true;
                        }
                        break;
                }
            }
        }

        #endregion
    }
}
