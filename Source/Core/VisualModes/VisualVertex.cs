using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;

namespace CodeImp.DoomBuilder.VisualModes
{
    public class VisualVertexPair
    {
        public VisualVertex[] Vertices { get { return new[] { FloorVertex, CeilingVertex }; } }
        public VisualVertex FloorVertex { get; }
        public VisualVertex CeilingVertex { get; }
        public bool Changed { set { FloorVertex.Changed = value; CeilingVertex.Changed = value; } }

        public VisualVertexPair(VisualVertex floorvert, VisualVertex ceilvert)
        {
            if (floorvert.CeilingVertex == ceilvert.CeilingVertex)
                throw new Exception("VisualVertexPair: both verts have the same alignment! We cannot tolerate this!");

            this.FloorVertex = floorvert;
            this.CeilingVertex = ceilvert;
        }

        public void Update()
        {
            if (FloorVertex.Changed) FloorVertex.Update();
            if (CeilingVertex.Changed) CeilingVertex.Update();
        }

        public void Deselect()
        {
            FloorVertex.Selected = false;
            CeilingVertex.Selected = false;
        }
    }

    public abstract class VisualVertex : IVisualPickable
    {
        //Constants
        public const float DEFAULT_SIZE = 6.0f;

        //Variables
        protected readonly Vertex vertex;
        private Matrix position;
        protected bool selected;
        protected bool changed;
        protected readonly bool ceilingVertex;
        protected bool haveOffset;

        //Properties
        internal Matrix Position { get { return position; } }
        public Vertex Vertex { get { return vertex; } }
        public bool Selected { get { return selected; } set { selected = value; } }
        public bool Changed { get { return changed; } set { changed |= value; } }
        public bool CeilingVertex { get { return ceilingVertex; } }
        public bool HaveHeightOffset { get { return haveOffset; } }

        protected VisualVertex(Vertex v, bool ceilingVertex)
        {
            vertex = v;
            position = Matrix.Identity;
            this.ceilingVertex = ceilingVertex;
        }

        public void SetPosition(Vector3D pos)
        {
            position = Matrix.Translation((float)pos.x, (float)pos.y, (float)pos.z);
        }

        public virtual void Update() { }

        /// <summary>
        /// This is called when the thing must be tested for line intersection. This should reject
        /// as fast as possible to rule out all geometry that certainly does not touch the line.
        /// </summary>
        public virtual bool PickFastReject(Vector3D from, Vector3D to, Vector3D dir)
        {
            return false;
        }

        /// <summary>
        /// This is called when the thing must be tested for line intersection. This should perform
        /// accurate hit detection and set u_ray to the position on the ray where this hits the geometry.
        /// </summary>
        public virtual bool PickAccurate(Vector3D from, Vector3D to, Vector3D dir, ref double u_ray)
        {
            return false;
        }
    }
}
