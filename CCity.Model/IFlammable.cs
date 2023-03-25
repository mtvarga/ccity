using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public interface IFlammable
    {
        #region Properties

        public double Pontential { get; }
        public double Health { get; }
        public bool IsOnFire { get; internal set; }

        #endregion

        #region Pulbic methods

        public void Incinerate()
        {
            throw new NotImplementedException();
        }

        public void TakeDamage()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
