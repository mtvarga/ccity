using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class FireDepartment : Placeable
    {
        #region Constants

        private const int effectRadius = 6;

        #endregion


        #region Properties

        public override int PlacementCost => 1000;

        public override int MaintenanceCost => 200;

        public override int NeededElectricity => 20;

        public override Field? Owner
        {
            get => base.Owner;
            
            internal set
            {
                if (value != null)
                    FireTruck = new FireTruck(value);
                
                base.Owner = value;
            }
        }

        public FireTruck FireTruck { get; private set; } = null!;

        internal bool FireTruckDeployed => FireTruck.Active || FireTruck.Moving;

        #endregion
        
        #region Public methods

        public override List<Field> Effect(Func<Placeable, bool, Action<Field, int>, int, List<Field>> spreadingFunction, bool add)
        {
            if (EffectSpreaded == add) return new();
            EffectSpreaded = add;
            return spreadingFunction(this, add, (f, i) => f.ChangeFireDepartmentEffect(i), effectRadius);
        }

        #endregion
    }
}
