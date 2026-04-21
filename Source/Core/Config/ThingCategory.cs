
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

using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.ZDoom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

#endregion

namespace CodeImp.DoomBuilder.Config
{
    public class ThingCategory
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // Things

        // Category properties

        // Thing properties for inheritance

        // Disposing

        //mxd. Validity
        private bool isinvalid;

        // [ZZ]

        #endregion

        #region ================== Properties

        public string Name { get; }
        public string Title { get; }
        public string Sprite { get; }
        public bool Sorted { get; }
        public List<ThingCategory> Children { get; } //mxd
        public int Color { get; }
        public float Alpha { get; } //mxd
        public string RenderStyle { get; } //mxd
        public int Arrow { get; }
        public float Radius { get; }
        public float Height { get; }
        public int Hangs { get; }
        public int Blocking { get; }
        public int ErrorCheck { get; }
        public bool FixedSize { get; }
        public bool FixedRotation { get; } //mxd
        public bool IsDisposed { get; private set; }
        public bool IsValid { get { return !isinvalid; } } //mxd
        public bool AbsoluteZ { get; }
        public float SpriteScale { get; }
        public List<ThingTypeInfo> Things { get; private set; }
        public bool Optional { get; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal ThingCategory(ThingCategory parent, string name, string title, DecorateCategoryInfo catinfo)
        {
            // Initialize
            this.Name = name;
            this.Title = title;
            this.Things = new List<ThingTypeInfo>();
            this.Children = new List<ThingCategory>();

            //mxd. Copy properties from the parent
            if (parent != null)
            {
                this.Sprite = parent.Sprite;
                this.Sorted = parent.Sorted;
                this.Color = parent.Color;
                this.Alpha = parent.Alpha;
                this.RenderStyle = parent.RenderStyle;
                this.Arrow = parent.Arrow;
                this.Radius = parent.Radius;
                this.Height = parent.Height;
                this.Hangs = parent.Hangs;
                this.Blocking = parent.Blocking;
                this.ErrorCheck = parent.ErrorCheck;
                this.FixedSize = parent.FixedSize;
                this.FixedRotation = parent.FixedRotation;
                this.AbsoluteZ = parent.AbsoluteZ;
                this.SpriteScale = parent.SpriteScale;
                this.Optional = parent.Optional;
            }
            // Set default properties
            else
            {
                this.Sprite = "";
                this.Sorted = true;
                this.Color = 18;
                this.Alpha = 1f; //mxd
                this.RenderStyle = "normal"; //mxd
                this.Arrow = 1;
                this.Radius = 10;
                this.Height = 20;
                this.Hangs = 0;
                this.Blocking = 0;
                this.ErrorCheck = 1;
                this.FixedSize = false;
                this.FixedRotation = false; //mxd
                this.AbsoluteZ = false;
                this.SpriteScale = 1.0f;
                this.Optional = false;
            }

            //mxd. Apply DecorateCategoryInfo overrides...
            if (catinfo != null && catinfo.Properties.Count > 0)
            {
                this.Sprite = catinfo.GetPropertyValueString("$sprite", 0, this.Sprite);
                this.Sorted = catinfo.GetPropertyValueInt("$sort", 0, this.Sorted ? 1 : 0) != 0;
                this.Color = catinfo.GetPropertyValueInt("$color", 0, this.Color);
                this.Arrow = catinfo.GetPropertyValueInt("$arrow", 0, this.Arrow);
                this.ErrorCheck = catinfo.GetPropertyValueInt("$error", 0, this.ErrorCheck);
                this.FixedSize = catinfo.GetPropertyValueBool("$fixedsize", 0, this.FixedSize);
                this.FixedRotation = catinfo.GetPropertyValueBool("$fixedrotation", 0, this.FixedRotation);
                this.AbsoluteZ = catinfo.GetPropertyValueBool("$absolutez", 0, this.AbsoluteZ);
            }

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Constructor
        internal ThingCategory(Configuration cfg, ThingCategory parent, string name, IDictionary<string, EnumList> enums)
        {
            // Initialize
            this.Name = name;
            this.Things = new List<ThingTypeInfo>();
            this.Children = new List<ThingCategory>();

            // Read properties
            this.Title = cfg.ReadSetting("thingtypes." + name + ".title", name);

            //mxd. If current block has no settings, it should be a grouping block, not a thing category.
            if (this.Title == name)
            {
                string[] props = new[] { "sprite", "sort", "color", "alpha", "renderstyle", "arrow", "width",
                    "height", "hangs", "blocking", "error", "fixedsize", "fixedrotation", "absolutez", "spritescale" };

                isinvalid = true;
                foreach (string prop in props)
                {
                    if (cfg.SettingExists("thingtypes." + name + "." + prop))
                    {
                        isinvalid = false;
                        break;
                    }
                }

                if (isinvalid) return;
            }

            if (parent != null) //mxd
            {
                this.Sprite = cfg.ReadSetting("thingtypes." + name + ".sprite", parent.Sprite);
                this.Sorted = cfg.ReadSetting("thingtypes." + name + ".sort", parent.Sorted ? 1 : 0) != 0;
                this.Color = cfg.ReadSetting("thingtypes." + name + ".color", parent.Color);
                this.Alpha = cfg.ReadSetting("thingtypes." + name + ".alpha", parent.Alpha);
                this.RenderStyle = cfg.ReadSetting("thingtypes." + name + ".renderstyle", parent.RenderStyle).ToLower();
                this.Arrow = cfg.ReadSetting("thingtypes." + name + ".arrow", parent.Arrow);
                this.Radius = cfg.ReadSetting("thingtypes." + name + ".width", parent.Radius);
                this.Height = cfg.ReadSetting("thingtypes." + name + ".height", parent.Height);
                this.Hangs = cfg.ReadSetting("thingtypes." + name + ".hangs", parent.Hangs);
                this.Blocking = cfg.ReadSetting("thingtypes." + name + ".blocking", parent.Blocking);
                this.ErrorCheck = cfg.ReadSetting("thingtypes." + name + ".error", parent.ErrorCheck);
                this.FixedSize = cfg.ReadSetting("thingtypes." + name + ".fixedsize", parent.FixedSize);
                this.FixedRotation = cfg.ReadSetting("thingtypes." + name + ".fixedrotation", parent.FixedRotation);
                this.AbsoluteZ = cfg.ReadSetting("thingtypes." + name + ".absolutez", parent.AbsoluteZ);
                this.SpriteScale = cfg.ReadSetting("thingtypes." + name + ".spritescale", parent.SpriteScale);
                this.Optional = cfg.ReadSetting("thingtypes." + name + ".optional", parent.Optional);
            }
            else
            {
                this.Sprite = cfg.ReadSetting("thingtypes." + name + ".sprite", "");
                this.Sorted = cfg.ReadSetting("thingtypes." + name + ".sort", 0) != 0;
                this.Color = cfg.ReadSetting("thingtypes." + name + ".color", 0);
                this.Alpha = General.Clamp(cfg.ReadSetting("thingtypes." + name + ".alpha", 1f), 0f, 1f);
                this.RenderStyle = cfg.ReadSetting("thingtypes." + name + ".renderstyle", "normal").ToLower();
                this.Arrow = cfg.ReadSetting("thingtypes." + name + ".arrow", 0);
                this.Radius = cfg.ReadSetting("thingtypes." + name + ".width", 10);
                this.Height = cfg.ReadSetting("thingtypes." + name + ".height", 20);
                this.Hangs = cfg.ReadSetting("thingtypes." + name + ".hangs", 0);
                this.Blocking = cfg.ReadSetting("thingtypes." + name + ".blocking", 0);
                this.ErrorCheck = cfg.ReadSetting("thingtypes." + name + ".error", 1);
                this.FixedSize = cfg.ReadSetting("thingtypes." + name + ".fixedsize", false);
                this.FixedRotation = cfg.ReadSetting("thingtypes." + name + ".fixedrotation", false); //mxd
                this.AbsoluteZ = cfg.ReadSetting("thingtypes." + name + ".absolutez", false);
                this.SpriteScale = cfg.ReadSetting("thingtypes." + name + ".spritescale", 1.0f);
                this.Optional = cfg.ReadSetting("thingtypes." + name + ".optional", false);
            }

            // Safety
            if (this.Radius < 4f) this.Radius = 8f;

            // Go for all items in category
            IDictionary dic = cfg.ReadSetting("thingtypes." + name, new Hashtable());
            Dictionary<string, ThingCategory> cats = new Dictionary<string, ThingCategory>(StringComparer.Ordinal); //mxd
            foreach (DictionaryEntry de in dic)
            {
                // Check if the item key is numeric
                int index;
                if (int.TryParse(de.Key.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                {
                    // Check if the item value is a structure
                    if (de.Value is IDictionary)
                    {
                        // Create this thing
                        Things.Add(new ThingTypeInfo(this, index, cfg, enums));
                    }
                    // Check if the item value is a string
                    else if (de.Value is string)
                    {
                        // Interpret this as the title
                        Things.Add(new ThingTypeInfo(this, index, de.Value.ToString()));
                    }
                }
                //mxd. This should be a child category 
                else if (de.Value is IDictionary)
                {
                    ThingCategory child = new ThingCategory(cfg, this, name + "." + de.Key, enums);
                    if (child.IsValid && child.Things.Count > 0)
                    {
                        if (cats.ContainsKey(child.Title.ToLowerInvariant()))
                            General.ErrorLogger.Add(ErrorType.Warning, "Thing Category \"" + child.Title + "\" is double defined in " + this.Title);
                        cats[child.Title.ToLowerInvariant()] = child;
                    }
                }
            }

            //mxd. Add to main collection
            foreach (ThingCategory tc in cats.Values) Children.Add(tc);

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
                Things = null;

                //mxd. Dispose children (oh so cruel!!11)
                foreach (ThingCategory tc in Children) tc.Dispose();

                // Done
                IsDisposed = true;
            }
        }

        #endregion

        #region ================== Methods

        // This sorts the category, if preferred
        internal void SortIfNeeded()
        {
            if (Sorted) Things.Sort();

            //mxd. Sort children as well
            foreach (ThingCategory tc in Children) tc.SortIfNeeded();
        }

        // This adds a thing to the category
        internal void AddThing(ThingTypeInfo t)
        {
            // Add
            Things.Add(t);
        }

        //mxd. This removes a thing from the category
        internal void RemoveThing(ThingTypeInfo t)
        {
            // Remove
            if (Things.Contains(t)) Things.Remove(t);
        }

        // String representation
        public override string ToString()
        {
            return Title;
        }

        #endregion
    }
}

