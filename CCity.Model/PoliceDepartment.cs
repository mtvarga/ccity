using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class PoliceDepartment : Placeable, IFlammable
    {
        #region Constants

        private const int effectRadius = 10;

        private const byte PoliceDepartmentPotential = 1;

        #endregion

        #region Properties

        public override int PlacementCost => 100;

        public override int MaintenanceCost => 10;

        public override int NeededElectricity => 20;

        byte IFlammable.Potential => Owner?.FireDepartmentEffect > 0 ? (byte)0 : PoliceDepartmentPotential;

        bool IFlammable.Burning { get; set; }

        ushort IFlammable.Health { get; set; } = IFlammable.FlammableMaxHealth;

        #endregion

        #region Public methods

        public override List<Field> Effect(Func<Placeable, bool, Action<Field, int>, int, List<Field>> spreadingFunction, bool add)
        {
            if (EffectSpreaded == add) return new();
            EffectSpreaded = add;
            return spreadingFunction(this, add, (f, i) => f.ChangePoliceDepartmentEffect(i), effectRadius);
        }

        #endregion
    }
}
