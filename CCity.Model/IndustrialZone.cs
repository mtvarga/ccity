﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class IndustrialZone : WorkplaceZone
    {
        #region Constants

        private const int effectRadius = 10;

        private const byte IndustrialZonePotential = 2;

        #endregion

        #region Properties

        public override int PlacementCost => 100;
        public override int MaintenanceCost => 0;
        
        public override byte Potential => Owner?.FireDepartmentEffect > 0 ? (byte)0 : IndustrialZonePotential ; 

        #endregion

        #region Public methods

        public override List<Field> Effect(Func<Placeable, bool, Action<Field, int>, int, List<Field>> spreadingFunction, bool add)
        {
            if (EffectSpreaded == add) return new();
            EffectSpreaded = add;
            return spreadingFunction(this, add, (f, i) => f.ChangeIndustrialEffect(i), effectRadius);
        }

        #endregion

    }
}
