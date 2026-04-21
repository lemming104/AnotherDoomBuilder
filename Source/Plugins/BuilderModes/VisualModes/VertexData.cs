using CodeImp.DoomBuilder.Map;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.BuilderModes
{
    internal class VertexData
    {
        #region ================== Variables

        // VisualMode

        // Vertex for which this data is
        // Sectors that must be updated when this vertex is changed
        // The boolean value is the 'includeneighbours' of the UpdateSectorGeometry function which
        // indicates if the sidedefs of neighbouring sectors should also be rebuilt.

        #endregion

        #region ================== Properties

        public Vertex Vertex { get; }
        public BaseVisualMode Mode { get; }
        public Dictionary<Sector, bool> UpdateAlso { get; }

        #endregion

        #region ================== Constructor / Destructor

        // Constructor
        public VertexData(BaseVisualMode mode, Vertex v)
        {
            // Initialize
            this.Mode = mode;
            this.Vertex = v;
            this.UpdateAlso = new Dictionary<Sector, bool>(2);
        }

        #endregion

        #region ================== Public Methods

        // This adds a sector for updating
        public void AddUpdateSector(Sector s, bool includeneighbours)
        {
            UpdateAlso[s] = includeneighbours;
        }

        #endregion
    }
}
