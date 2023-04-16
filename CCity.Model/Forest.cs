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

        public override int PlacementCost => 100;

        public override int MaintenanceCost => 10;

        public int GrowthMonthsLeft { get; private set; }

        #endregion

    }
}
