using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class FireDepartment : Placeable, IMultifield
    {
        #region Constants

        private const int effectRadius = 10;

        private const int FireDeparmentInitialFireTruckCount = 1;

        #endregion

        #region Fields

        List<Filler> _occupies;

        #endregion

        #region Properties

        public override int PlacementCost => 100;

        public override int MaintenanceCost => 10;

        public override int NeededElectricity => 20;

        public int AvailableFireTrucks { get; internal set; } = FireDeparmentInitialFireTruckCount;

        int IMultifield.Width => 2;

        int IMultifield.Height => 1;

        List<Filler> IMultifield.Occupies { get => _occupies; set => _occupies = value; }

        #endregion

        #region Constructor

        public FireDepartment()
        {
            _occupies = new();
        }

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
