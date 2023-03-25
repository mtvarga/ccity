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

        public override int PlacementCost => throw new NotImplementedException();
        public override int MaintenanceCost => throw new NotImplementedException();

        #endregion

        #region Constructors 

        public IndustrialZone()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
