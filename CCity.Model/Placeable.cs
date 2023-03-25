using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public abstract class Placeable
    {
        #region Properties

        public abstract int PlacementCost { get; }
        public abstract int MaintenanceCost { get; }
        public int ElectrifiedNeighbours { get; internal set; }
        public bool HasElectricity { get; }

        #endregion
    }
}
