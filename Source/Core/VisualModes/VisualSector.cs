
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

using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;
using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.VisualModes
{
    public class VisualSector : IRenderResource, IDisposable
    {
        #region ================== Constants

        #endregion

        #region ================== Variables

        // Geometry
        private readonly Dictionary<Sidedef, List<VisualGeometry>> sidedefgeometry;
        private bool updategeo;

        // Original sector

        // Disposing

        #endregion

        #region ================== Properties

        internal List<VisualGeometry> FixedGeometry { get; }
        internal List<VisualGeometry> AllGeometry { get; }
        internal VertexBuffer GeometryBuffer { get; private set; }
        internal bool NeedsUpdateGeo
        {
            get { return updategeo; }
            set { updategeo |= value; }
        }

        public bool IsDisposed { get; private set; }
        public Sector Sector { get; }

        #endregion

        #region ================== Constructor / Disposer

        // Constructor
        public VisualSector(Sector s)
        {
            // Initialize
            this.Sector = s;
            AllGeometry = new List<VisualGeometry>();
            FixedGeometry = new List<VisualGeometry>();
            sidedefgeometry = new Dictionary<Sidedef, List<VisualGeometry>>();
            this.Sector.UpdateFogColor(); //mxd

            // Register as resource
            General.Map.Graphics.RegisterResource(this);
        }

        // Disposer
        public virtual void Dispose()
        {
            // Not already disposed?
            if (!IsDisposed)
            {
                // Clean up
                if (GeometryBuffer != null) GeometryBuffer.Dispose();
                GeometryBuffer = null;

                // Unregister resource
                General.Map.Graphics.UnregisterResource(this);

                // Done
                IsDisposed = true;
            }
        }

        #endregion

        #region ================== Methods

        // This is called before a device is reset
        // (when resized or display adapter was changed)
        public virtual void UnloadResource()
        {
            // Trash geometry buffer
            if (GeometryBuffer != null) GeometryBuffer.Dispose();
            GeometryBuffer = null;
            NeedsUpdateGeo = true;
        }

        // This is called resets when the device is reset
        // (when resized or display adapter was changed)
        public virtual void ReloadResource()
        {
            // Make new geometry
            //Update();
        }

        //mxd. Added to allow to properly update visual geometry from plugins
        public virtual void UpdateSectorData() { }
        public virtual void UpdateSectorGeometry(bool includeneighbours) { }

        // This updates the visual sector
        public void Update(RenderDevice graphics)
        {
            int numverts = 0;
            int v = 0;

            // Trash geometry buffer
            if (GeometryBuffer != null) GeometryBuffer.Dispose();
            GeometryBuffer = null;

            // Count the number of vertices there are
            foreach (VisualGeometry g in AllGeometry) if (g.Vertices != null) numverts += g.Vertices.Length;

            // Any vertics?
            if (numverts > 0)
            {
                // Make a new buffer
                GeometryBuffer = new VertexBuffer();
                graphics.SetBufferData(GeometryBuffer, numverts, VertexFormat.World);

                // Fill the buffer
                foreach (VisualGeometry g in AllGeometry)
                {
                    if ((g.Vertices != null) && (g.Vertices.Length > 0))
                    {
                        graphics.SetBufferSubdata(GeometryBuffer, v, g.Vertices);
                        g.VertexOffset = v;
                        v += g.Vertices.Length;
                    }
                }
            }

            this.Sector.UpdateFogColor(); //mxd

            // Done
            updategeo = false;
        }

        /// <summary>
        /// This adds geometry for this sector. If the geometry inherits from VisualSidedef then it
        /// will be added to the SidedefGeometry, otherwise it will be added as FixedGeometry.
        /// </summary>
        public void AddGeometry(VisualGeometry geo)
        {
            NeedsUpdateGeo = true;
            AllGeometry.Add(geo);
            if (geo.Sidedef != null)
            {
                if (!sidedefgeometry.ContainsKey(geo.Sidedef))
                    sidedefgeometry[geo.Sidedef] = new List<VisualGeometry>(3);
                sidedefgeometry[geo.Sidedef].Add(geo);
            }
            else
            {
                FixedGeometry.Add(geo);
            }
        }

        /// <summary>
        /// This removes all geometry.
        /// </summary>
        public void ClearGeometry()
        {
            AllGeometry.Clear();
            FixedGeometry.Clear();
            sidedefgeometry.Clear();
            NeedsUpdateGeo = true;
        }

        // This gets the geometry list for the specified sidedef
        public List<VisualGeometry> GetSidedefGeometry(Sidedef sd)
        {
            if (sidedefgeometry.ContainsKey(sd)) return sidedefgeometry[sd];
            return new List<VisualGeometry>();
        }

        #endregion
    }
}
