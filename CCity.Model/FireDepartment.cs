using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class FireDepartment : Placeable
    {

        #region Properties

        public override int PlacementCost => 100;

        public override int MaintenanceCost => 10;

        public int AvailableFiretrucks { get; internal set; }

        #endregion

    }
}
