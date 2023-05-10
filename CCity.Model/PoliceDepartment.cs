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

        #endregion

        #region Properties

        public override int PlacementCost => 100;

        public override int MaintenanceCost => 10;

        public override int NeededElectricity => 20;

        double IFlammable.Pontential => throw new NotImplementedException();

        double IFlammable.Health => throw new NotImplementedException();

        bool IFlammable.IsOnFire { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
