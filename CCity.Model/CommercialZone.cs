using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class CommercialZone : WorkplaceZone
    {
        #region  Constants

        private const float CommercialZonePotential = 0.01f;

        #endregion
        
        #region Properties

        public override int PlacementCost => 500;
        public override int MaintenanceCost => 0;
        
        public override float Potential => Owner?.FireDepartmentEffect > 0.5 || Empty ? 0 : CommercialZonePotential;

        #endregion

    }
}
