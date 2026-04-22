
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;
using System;

namespace CodeImp.DoomBuilder.BuilderModes
{
    public class ResultVertexOverlappingVertex : ErrorResult
    {

        private readonly Vertex vertex1;
        private readonly Vertex vertex2;

        public override int Buttons { get { return 1; } }
        public override string Button1Text { get { return "Merge Vertices"; } }

        // Constructor
        public ResultVertexOverlappingVertex(Vertex v1, Vertex v2)
        {
            // Initialize
            vertex1 = v1;
            vertex2 = v2;
            viewobjects.Add(v1);
            viewobjects.Add(v2);
            hidden = v1.IgnoredErrorChecks.Contains(this.GetType()) && v2.IgnoredErrorChecks.Contains(this.GetType()); //mxd
            description = "These vertices have the same position.";
        }

        // This sets if this result is displayed in ErrorCheckForm (mxd)
        internal override void Hide(bool hide)
        {
            hidden = hide;
            Type t = this.GetType();
            if (hide)
            {
                vertex1.IgnoredErrorChecks.Add(t);
                vertex2.IgnoredErrorChecks.Add(t);
            }
            else
            {
                if (vertex1.IgnoredErrorChecks.Contains(t)) vertex1.IgnoredErrorChecks.Remove(t);
                if (vertex2.IgnoredErrorChecks.Contains(t)) vertex2.IgnoredErrorChecks.Remove(t);
            }
        }

        // This must return the string that is displayed in the listbox
        public override string ToString()
        {
            return "Vertices " + vertex1.Index + " and " + vertex2.Index + " have the same position";
        }

        // Rendering
        public override void PlotSelection(IRenderer2D renderer)
        {
            renderer.PlotVertex(vertex1, ColorCollection.SELECTION);
        }

        // Fix by splitting the line
        public override bool Button1Click(bool batchMode)
        {
            if (!batchMode) General.Map.UndoRedo.CreateUndo("Merge vertices");
            vertex2.Join(vertex1);
            General.Map.Map.Update();
            return true;
        }
    }
}
