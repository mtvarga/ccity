using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public abstract class Zone : Placeable, IFlammable, IUpgradeable
    {
        #region Fields

        public int Capacity;
        public int Current;

        #endregion

        #region Properties

        public List<Citizen> Citizens { get; private set; }

        double IFlammable.Pontential => throw new NotImplementedException();

        double IFlammable.Health => throw new NotImplementedException();

        bool IFlammable.IsOnFire { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        Level IUpgradeable.Level { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        int IUpgradeable.NextUpgradeCost => throw new NotImplementedException();

        bool IUpgradeable.CanUpgrade => throw new NotImplementedException();

        #endregion

        #region Public methods

        public void AddCitizen(Citizen citizen)
        {
            throw new NotImplementedException();
        }

        public void DropCitizen(Citizen citizen)
        {
            throw new NotImplementedException();
        }

        public override int CalculateSatisfaction()
        {
            return (int)Citizens.Sum(citizen => citizen.Satisfaction);
        }

        #endregion
    }
}
