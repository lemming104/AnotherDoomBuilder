

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


using System;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.Config
{
    public class LinedefActionCategory : IComparable<LinedefActionCategory>
    {

        // Category properties
        private string name;
        private string title;

        // Actions
        private List<LinedefActionInfo> actions;

        // Disposing
        private bool isdisposed;

        public string Name { get { return name; } }
        public string Title { get { return title; } }
        public List<LinedefActionInfo> Actions { get { return actions; } }
        public bool IsDisposed { get { return isdisposed; } }

        // Constructor
        internal LinedefActionCategory(string name, string title)
        {
            // Initialize
            this.name = name;
            this.title = title;
            this.actions = new List<LinedefActionInfo>();

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Disposer
        internal void Dispose()
        {
            // Not already disposed?
            if (!isdisposed)
            {
                // Clean up
                actions = null;

                // Done
                isdisposed = true;
            }
        }

        // This adds an action to this category
        internal void Add(LinedefActionInfo a)
        {
            // Make it so.
            actions.Add(a);
        }

        // This compares against another action category
        public int CompareTo(LinedefActionCategory other)
        {
            return string.Compare(this.name, other.name);
        }

        // String representation
        public override string ToString()
        {
            return title;
        }
    }
}
