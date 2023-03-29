using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class PoliceDepartment : Placeable, IFlammable
    {
        #region Constructors

        public PoliceDepartment()
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

        #endregion
    }
}
