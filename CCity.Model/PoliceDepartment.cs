using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class PoliceDepartment : IFlammable
    {
        #region Constructors

        public PoliceDepartment()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Properties

        double IFlammable.Pontential => throw new NotImplementedException();

        double IFlammable.Health => throw new NotImplementedException();

        bool IFlammable.IsOnFire { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion
    }
}
