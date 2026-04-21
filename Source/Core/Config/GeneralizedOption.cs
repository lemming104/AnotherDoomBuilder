
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

#endregion

namespace CodeImp.DoomBuilder.Config
{
    /// <summary>
    /// Option in generalized types.
    /// </summary>
    public class GeneralizedOption
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // Properties
        public int BitsStep { get; } // mxd. Each subsequent value is incremented  by this number

        #endregion

        #region ================== Properties

        public string Name { get; }
        public List<GeneralizedBit> Bits { get; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal GeneralizedOption(string structure, string cat, string name, IDictionary bitslist)
        {
            string fullpath;

            // Determine path
            if (cat.Length > 0) fullpath = structure + "." + cat;
            else fullpath = structure;

            // Initialize
            this.Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
            this.Bits = new List<GeneralizedBit>();

            // Go for all bits
            foreach (DictionaryEntry de in bitslist)
            {
                // Check if the item key is numeric
                int index;
                string key = de.Key.ToString();
                if (int.TryParse(key, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out index))
                {
                    // Add to list
                    this.Bits.Add(new GeneralizedBit(index, de.Value.ToString()));
                }
                else
                {
                    if (key == "name")
                        this.Name = de.Value.ToString();
                    else
                        General.ErrorLogger.Add(ErrorType.Warning, "Structure \"" + fullpath + "." + name + "\" contains invalid entries. The keys must be numeric.");
                }
            }

            // Sort the list
            Bits.Sort();

            //mxd. Determine and check increment steps
            // biwa. Setting this to be in debug build only. There are valid scenarios where this isn't a problem, specifically MBF21,
            // where the "alternate damage mode" for sectors is made up of 3 bits.

            if (Bits.Count > 1)
            {
                // Use the second bit as the structure's step
                BitsStep = Bits[1].Index;

#if DEBUG
                // Check the rest of the values
                for (int i = 1; i < Bits.Count; i++)
                {
                    if (Bits[i].Index - Bits[i - 1].Index != BitsStep)
                        General.ErrorLogger.Add(ErrorType.Warning, "Structure \"" + fullpath + "." + name + "\" contains options with mixed increments (option \"" + Bits[i].Title + "\" increment (" + (Bits[i - 1].Index - Bits[i].Index) + ") doesn't match the structure increment (" + BitsStep + ")).");
                }
#endif
            }

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        #endregion

        #region ================== Methods

        // This presents the item as string
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
