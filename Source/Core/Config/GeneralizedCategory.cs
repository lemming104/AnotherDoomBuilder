using CodeImp.DoomBuilder.IO;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.Config
{
    /// <summary>
    /// Category of generalized type options.
    /// </summary>
    public class GeneralizedCategory
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // Category properties
        // Disposing

        #endregion

        #region ================== Properties

        public string Title { get; }
        public int Offset { get; }
        public int Length { get; }
        public List<GeneralizedOption> Options { get; private set; }
        public bool IsDisposed { get; private set; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal GeneralizedCategory(string structure, string name, Configuration cfg)
        {
            // Initialize
            this.Options = new List<GeneralizedOption>();

            // Read properties
            this.Title = cfg.ReadSetting(structure + "." + name + ".title", "");
            this.Offset = cfg.ReadSetting(structure + "." + name + ".offset", 0);
            this.Length = cfg.ReadSetting(structure + "." + name + ".length", 0);

            // Read the options
            IDictionary opts = cfg.ReadSetting(structure + "." + name, new Hashtable());
            foreach (DictionaryEntry de in opts)
            {
                // Is this an option and not just some value?
                IDictionary value = de.Value as IDictionary;
                if (value != null)
                {
                    // Add the option
                    this.Options.Add(new GeneralizedOption(structure, name, de.Key.ToString(), value));
                }
            }

            //mxd. Sort by bits step
            if (this.Options.Count > 1)
            {
                this.Options.Sort(delegate (GeneralizedOption o1, GeneralizedOption o2)
                {
                    if (o1.BitsStep > o2.BitsStep) return 1;
                    if (o1.BitsStep == o2.BitsStep)
                    {
                        if (o1 != o2) General.ErrorLogger.Add(ErrorType.Error, "\"" + o1.Name + "\" and \"" + o2.Name + "\" generalized categories have the same bit step (" + o1.BitsStep + ")!");
                        return 0;
                    }
                    return -1;
                });
            }

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Disposer
        internal void Dispose()
        {
            // Not already disposed?
            if (!IsDisposed)
            {
                // Clean up
                Options = null;

                // Done
                IsDisposed = true;
            }
        }

        #endregion

        #region ================== Methods

        // String representation
        public override string ToString()
        {
            return Title;
        }

        #endregion
    }
}
