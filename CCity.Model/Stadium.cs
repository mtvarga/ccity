using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Stadium : IFlammable, IMultifield
    {
        #region Constructors

        public Stadium()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Properties

        double IFlammable.Pontential => throw new NotImplementedException();

        double IFlammable.Health => throw new NotImplementedException();

        bool IFlammable.IsOnFire { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        int IMultifield.Width => throw new NotImplementedException();

        int IMultifield.Height => throw new NotImplementedException();

        List<Filler> IMultifield.Occupies { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion
    }
}
