using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class PowerPlant : Placeable, IFlammable, IMultifield
    {
        #region Constuctors

        public PowerPlant()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Properties

        public override int PlacementCost => throw new NotImplementedException();

        public override int MaintenanceCost => throw new NotImplementedException();


        double IFlammable.Pontential => throw new NotImplementedException();

        double IFlammable.Health => throw new NotImplementedException();

        bool IFlammable.IsOnFire { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        int IMultifield.Width => throw new NotImplementedException();

        int IMultifield.Height => throw new NotImplementedException();

        List<Filler> IMultifield.Occupies { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion
    }
}
