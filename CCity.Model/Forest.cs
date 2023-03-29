using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Forest: Placeable
    {
        #region Properties 

        public override int PlacementCost => throw new NotImplementedException();

        public override int MaintenanceCost => throw new NotImplementedException();

        public int GrowthMonthsLeft { get; private set; }

        #endregion

        #region Constructors

        public Forest()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
