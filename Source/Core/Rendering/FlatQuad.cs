
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
using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.Rendering
{
    // FlatQuad
    internal class FlatQuad
    {
        #region ================== Variables

        // Vertices
        private int numvertices;

        #endregion

        #region ================== Properties

        public FlatVertex[] Vertices { get; private set; }
        public PrimitiveType Type { get; private set; }

        #endregion

        #region ================== Constructors

        // Constructor
        public FlatQuad(PrimitiveType type, float left, float top, float right, float bottom)
        {
            // Initialize
            Initialize(type);

            // Set coordinates
            switch (type)
            {
                case PrimitiveType.TriangleList:
                    SetTriangleListCoordinates(left, top, right, bottom, 0f, 0f, 1f, 1f);
                    break;
                case PrimitiveType.TriangleStrip:
                    SetTriangleStripCoordinates(left, top, right, bottom, 0f, 0f, 1f, 1f);
                    break;
            }

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Constructor
        public FlatQuad(PrimitiveType type, float left, float top, float right, float bottom, float twidth, float theight)
        {
            // Initialize
            Initialize(type);

            // Determine texture size dividers
            float twd = 1f / twidth;
            float thd = 1f / theight;

            // Set coordinates
            switch (type)
            {
                case PrimitiveType.TriangleList:
                    SetTriangleListCoordinates(left, top, right, bottom, twd, thd, 1f - twd, 1f - thd);
                    break;
                case PrimitiveType.TriangleStrip:
                    SetTriangleStripCoordinates(left, top, right, bottom, twd, thd, 1f - twd, 1f - thd);
                    break;
            }

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Constructor
        public FlatQuad(PrimitiveType type, RectangleF pos, float tl, float tt, float tr, float tb)
        {
            // Initialize
            Initialize(type);

            // Set coordinates
            switch (type)
            {
                case PrimitiveType.TriangleList:
                    SetTriangleListCoordinates(pos.Left, pos.Top, pos.Right, pos.Bottom, tl, tt, tr, tb);
                    break;
                case PrimitiveType.TriangleStrip:
                    SetTriangleStripCoordinates(pos.Left, pos.Top, pos.Right, pos.Bottom, tl, tt, tr, tb);
                    break;
            }

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        // Constructor
        public FlatQuad(PrimitiveType type, float left, float top, float right, float bottom, float tl, float tt, float tr, float tb)
        {
            // Initialize
            Initialize(type);

            // Set coordinates
            switch (type)
            {
                case PrimitiveType.TriangleList:
                    SetTriangleListCoordinates(left, top, right, bottom, tl, tt, tr, tb);
                    break;
                case PrimitiveType.TriangleStrip:
                    SetTriangleStripCoordinates(left, top, right, bottom, tl, tt, tr, tb);
                    break;
            }

            // We have no destructor
            GC.SuppressFinalize(this);
        }

        #endregion

        #region ================== Methods

        // This sets the color on all vertices
        public void SetColors(int color)
        {
            // Go for all vertices to set the color
            for (int i = 0; i < numvertices; i++)
                Vertices[i].c = color;
        }

        // This sets the color on all vertices
        public void SetColors(int clt, int crt, int clb, int crb)
        {
            // Determine polygon type
            switch (Type)
            {
                case PrimitiveType.TriangleList:
                    Vertices[0].c = clt;
                    Vertices[1].c = crt;
                    Vertices[2].c = clb;
                    Vertices[3].c = clb;
                    Vertices[4].c = crt;
                    Vertices[5].c = crb;
                    break;
                case PrimitiveType.TriangleStrip:
                    Vertices[0].c = clt;
                    Vertices[1].c = crt;
                    Vertices[2].c = clb;
                    Vertices[3].c = crb;
                    break;
            }
        }

        // This applies coordinates for TriangleList type
        private void SetTriangleListCoordinates(float vl, float vt, float vr, float vb,
                                                float tl, float tt, float tr, float tb)
        {
            // Setup coordinates
            Vertices[0].x = vl;
            Vertices[0].y = vt;
            Vertices[1].x = vr;
            Vertices[1].y = vt;
            Vertices[2].x = vl;
            Vertices[2].y = vb;
            Vertices[3].x = vl;
            Vertices[3].y = vb;
            Vertices[4].x = vr;
            Vertices[4].y = vt;
            Vertices[5].x = vr;
            Vertices[5].y = vb;

            // Set texture coordinates
            Vertices[0].u = tl;
            Vertices[0].v = tt;
            Vertices[1].u = tr;
            Vertices[1].v = tt;
            Vertices[2].u = tl;
            Vertices[2].v = tb;
            Vertices[3].u = tl;
            Vertices[3].v = tb;
            Vertices[4].u = tr;
            Vertices[4].v = tt;
            Vertices[5].u = tr;
            Vertices[5].v = tb;
        }

        // This applies coordinates for TriangleStrip type
        private void SetTriangleStripCoordinates(float vl, float vt, float vr, float vb,
                                                 float tl, float tt, float tr, float tb)
        {
            // Setup coordinates
            Vertices[0].x = vl;
            Vertices[0].y = vt;
            Vertices[1].x = vr;
            Vertices[1].y = vt;
            Vertices[2].x = vl;
            Vertices[2].y = vb;
            Vertices[3].x = vr;
            Vertices[3].y = vb;

            // Set texture coordinates
            Vertices[0].u = tl;
            Vertices[0].v = tt;
            Vertices[1].u = tr;
            Vertices[1].v = tt;
            Vertices[2].u = tl;
            Vertices[2].v = tb;
            Vertices[3].u = tr;
            Vertices[3].v = tb;
        }

        // This initializes vertices to default values
        private void Initialize(PrimitiveType type)
        {
            // Determine primitive type
            this.Type = type;

            // Determine number of vertices
            switch (type)
            {
                case PrimitiveType.TriangleList: numvertices = 6; break;
                case PrimitiveType.TriangleStrip: numvertices = 4; break;
                default: throw new NotSupportedException("Unsupported PrimitiveType");
            }

            // Make the array
            Vertices = new FlatVertex[numvertices];

            // Go for all vertices
            for (int i = 0; i < numvertices; i++)
            {
                // Initialize to defaults
                Vertices[i].c = -1;
            }
        }

        #endregion

        #region ================== Rendering

        // This renders the quad
        public void Render(RenderDevice device)
        {
            // Render the quad
            device.Draw(Type, 0, 2, Vertices);
        }

        #endregion
    }
}
