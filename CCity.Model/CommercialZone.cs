﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class CommercialZone : WorkplaceZone
    {
        #region  Constants

        private const byte CommercialZonePotential = 1;

        #endregion
        
        #region Properties

        public override int PlacementCost => 100;
        public override int MaintenanceCost => 10;
        
        public override byte Potential => CommercialZonePotential;

        #endregion

    }
}
