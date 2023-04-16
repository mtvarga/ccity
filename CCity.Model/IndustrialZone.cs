using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class IndustrialZone : WorkplaceZone
    {

        #region Properties

        public override int PlacementCost => 100;
        public override int MaintenanceCost => 10;

        #endregion

    }
}
