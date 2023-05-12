using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class ResidentialZone : Zone
    {
        #region Constants

        private const byte ResidentialZonePotential = 1;
        
        #endregion

        #region Properties

        public override int PlacementCost => 100;
        public override int MaintenanceCost => 0;

        public override byte Potential => Owner?.FireDepartmentEffect > 0 ? (byte)0 : ResidentialZonePotential;

        #endregion
    }
}
