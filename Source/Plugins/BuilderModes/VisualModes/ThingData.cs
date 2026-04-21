#region === Copyright (c) 2010 Pascal van der Heiden ===

using CodeImp.DoomBuilder.Map;
using System.Collections.Generic;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
    internal class ThingData
    {
        #region ================== Variables

        // VisualMode

        // Thing for which this data is
        // Sectors that must be updated when this thing is changed
        // The boolean value is the 'includeneighbours' of the UpdateSectorGeometry function which
        // indicates if the sidedefs of neighbouring sectors should also be rebuilt.

        #endregion

        #region ================== Properties

        public Thing Thing { get; }
        public BaseVisualMode Mode { get; }
        public Dictionary<Sector, bool> UpdateAlso { get; }

        #endregion

        #region ================== Constructor / Destructor

        // Constructor
        public ThingData(BaseVisualMode mode, Thing t)
        {
            // Initialize
            this.Mode = mode;
            this.Thing = t;
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
