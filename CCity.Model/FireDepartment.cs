using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class FireDepartment: Placeable
    {
        #region Properties

        public override int PlacementCost => throw new NotImplementedException();

        public override int MaintenanceCost => throw new NotImplementedException();

        public int AvailableFiretrucks { get; internal set; }

        #endregion

        #region Constructors

        public FireDepartment()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
