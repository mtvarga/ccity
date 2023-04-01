using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public abstract class Placeable
    {
        #region Properties

        public Field? Owner { get; internal set; }
        public abstract int PlacementCost { get; }
        public abstract int MaintenanceCost { get; }
        public int ElectrifiedNeighbours { get; internal set; }
        public bool HasElectricity { get; }
        public bool isPublic { get; set; }

        #endregion

        #region Public methods

        public virtual int CalculateSatisfaction()
        {
            return 0;
        }

        public void PlaceAt(Field field)
        {
            Owner = field;
        }

        #endregion
    }
}
