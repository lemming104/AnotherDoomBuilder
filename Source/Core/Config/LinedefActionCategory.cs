
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
using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.Config
{
    public class LinedefActionCategory : IComparable<LinedefActionCategory>
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // Category properties
        // Actions

        // Disposing

        #endregion

        #region ================== Properties

        public string Name { get; }
        public string Title { get; }
        public List<LinedefActionInfo> Actions { get; private set; }
        public bool IsDisposed { get; private set; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        internal LinedefActionCategory(string name, string title)
        {
            // Initialize
            this.Name = name;
            this.Title = title;
            this.Actions = new List<LinedefActionInfo>();

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
                Actions = null;

                // Done
                IsDisposed = true;
            }
        }

        #endregion

        #region ================== Methods

        // This adds an action to this category
        internal void Add(LinedefActionInfo a)
        {
            // Make it so.
            Actions.Add(a);
        }

        // This compares against another action category
        public int CompareTo(LinedefActionCategory other)
        {
            return string.Compare(this.Name, other.Name);
        }

        // String representation
        public override string ToString()
        {
            return Title;
        }

        #endregion
    }
}
