
using CodeImp.DoomBuilder.Map;
using System.Collections.Generic;

namespace CodeImp.DoomBuilder.BuilderModes
{
    internal class ThingData
    {

        // VisualMode
        private BaseVisualMode mode;

        // Thing for which this data is
        private Thing thing;

        // Sectors that must be updated when this thing is changed
        // The boolean value is the 'includeneighbours' of the UpdateSectorGeometry function which
        // indicates if the sidedefs of neighbouring sectors should also be rebuilt.
        private Dictionary<Sector, bool> updatesectors;

        public Thing Thing { get { return thing; } }
        public BaseVisualMode Mode { get { return mode; } }
        public Dictionary<Sector, bool> UpdateAlso { get { return updatesectors; } }

        // Constructor
        public ThingData(BaseVisualMode mode, Thing t)
        {
            // Initialize
            this.mode = mode;
            this.thing = t;
            this.updatesectors = new Dictionary<Sector, bool>(2);
        }

        // This adds a sector for updating
        public void AddUpdateSector(Sector s, bool includeneighbours)
        {
            updatesectors[s] = includeneighbours;
        }
    }
}
