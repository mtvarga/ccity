using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class CommercialZone : WorkplaceZone
    {
        #region Properties

        public override int PlacementCost => throw new NotImplementedException();
        public override int MaintenanceCost => throw new NotImplementedException();

        #endregion

        #region Constructors 

        public CommercialZone()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
