
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

using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
    internal class FindReplaceObject
    {
        #region ================== Variables

        #endregion

        #region ================== Properties

        public object Object { get; set; }
        public Sector Sector { get { return (Sector)Object; } }
        public Linedef Linedef { get { return (Linedef)Object; } }
        public Sidedef Sidedef { get { return (Sidedef)Object; } }
        public Thing Thing { get { return (Thing)Object; } }
        public Vertex Vertex { get { return (Vertex)Object; } }
        public string Title { get; set; }

        #endregion

        #region ================== Constructor / Destructor

        // Constructor
        public FindReplaceObject(object obj, string title)
        {
            // Initialize
            this.Object = obj;
            this.Title = title;
        }

        #endregion

        #region ================== Methods

        // String representation
        public override string ToString()
        {
            return Title;
        }

        // This adds the vertices of the object used for view area calculation
        public void AddViewPoints(IList<Vector2D> points)
        {
            if (Object is Vertex)
            {
                points.Add(((Vertex)Object).Position);
            }
            else if (Object is Linedef)
            {
                points.Add(((Linedef)Object).Start.Position);
                points.Add(((Linedef)Object).End.Position);
            }
            else if (Object is Sidedef)
            {
                points.Add(((Sidedef)Object).Line.Start.Position);
                points.Add(((Sidedef)Object).Line.End.Position);
            }
            else if (Object is Sector)
            {
                Sector s = (Sector)Object;
                foreach (Sidedef sd in s.Sidedefs)
                {
                    points.Add(sd.Line.Start.Position);
                    points.Add(sd.Line.End.Position);
                }
            }
            else if (Object is Thing)
            {
                Thing t = (Thing)Object;
                Vector2D p = t.Position;
                points.Add(p);
                points.Add(p + new Vector2D(t.Size * 2.0f, t.Size * 2.0f));
                points.Add(p + new Vector2D(t.Size * 2.0f, -t.Size * 2.0f));
                points.Add(p + new Vector2D(-t.Size * 2.0f, t.Size * 2.0f));
                points.Add(p + new Vector2D(-t.Size * 2.0f, -t.Size * 2.0f));
            }
        }

        #endregion
    }
}
