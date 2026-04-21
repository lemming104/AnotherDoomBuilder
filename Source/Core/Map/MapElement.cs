
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
using System;
using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.Map
{
    public enum MapElementType
    {
        UNKNOWN,
        VERTEX,
        SIDEDEF,
        LINEDEF,
        SECTOR,
        THING
    }

    public abstract class MapElement : IDisposable
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // List index
        protected int listindex;

        // Univeral fields

        // Marking
        protected bool marked;

        // Disposing
        protected bool isdisposed;

        // Error Ignoring (mxd)

        //mxd. Hashing
        private static int hashcounter;
        private readonly int hashcode;

        //mxd. Element type
        protected MapElementType elementtype;

        #endregion

        #region ================== Properties

        public int Index { get { return listindex; } internal set { listindex = value; } }
        public UniFields Fields { get; private set; }
        public bool Marked { get { return marked; } set { marked = value; } }
        public bool IsDisposed { get { return isdisposed; } }
        public List<Type> IgnoredErrorChecks { get; } //mxd
        public MapElementType ElementType { get { return elementtype; } } //mxd

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        protected MapElement()
        {
            // Initialize
            Fields = new UniFields(this);
            IgnoredErrorChecks = new List<Type>(); //mxd
            hashcode = hashcounter++; //mxd
        }

        // Disposer
        public virtual void Dispose()
        {
            if (!isdisposed)
            {
                // Clean up
                Fields.Owner = null;
                Fields = null;

                // Done
                isdisposed = true;
            }
        }

        #endregion

        #region ================== Methods

        // Serialize / deserialize
        internal void ReadWrite(IReadWriteStream s)
        {
            int c = Fields.Count;
            s.rwInt(ref c);

            if (s.IsWriting)
            {
                foreach (KeyValuePair<string, UniValue> f in Fields)
                {
                    s.wString(f.Key);
                    f.Value.ReadWrite(s);
                }
            }
            else
            {
                Fields = new UniFields(this, c);
                for (int i = 0; i < c; i++)
                {
                    string t; s.rString(out t);
                    UniValue v = new UniValue(); v.ReadWrite(s);
                    Fields.Add(t, v);
                }
            }
        }

        // This copies properties to any other element
        public void CopyPropertiesTo(MapElement element)
        {
            //element.fields = new UniFields(this, this.fields);
            element.Fields = new UniFields(element, this.Fields); //mxd
        }

        // This must implement the call to the undo system to record the change of properties
        protected abstract void BeforePropsChange();

        // This is called before the custom fields change
        internal void BeforeFieldsChange()
        {
            BeforePropsChange();
        }

        //mxd. This greatly speeds up Dictionary lookups
        public override int GetHashCode()
        {
            return hashcode;
        }

        #endregion
    }
}
