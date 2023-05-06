using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Road : Placeable
    {

        #region Properties

        public override int PlacementCost => 100;

        public override int MaintenanceCost => 10;

        public override int NeededElectricity => 0;

        public override bool CouldGivePublicityTo(Placeable _) => true;

        #endregion

        #region Public methods

        public override bool CouldGiveElectricityTo(Placeable placeable)
        {
            return placeable switch
            {
                Zone => true,
                FireDepartment => true,
                PoliceDepartment => true,
                Stadium => true,
                Pole => true,
                _ => false
            };
        }

        public override void MakeRoot(SpreadType spreadType)
        {
            if (spreadType != SpreadType.Publicity) return;
            base.MakeRoot(spreadType);
            MaxSpreadValue[spreadType] = () => 1;
        }

        #endregion
    }
}
