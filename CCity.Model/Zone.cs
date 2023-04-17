using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public abstract class Zone : Placeable, IFlammable, IUpgradeable
    {
        #region Properties
        public int Capacity { get; private set; }
        public int Current { get; private set; }

        public List<Citizen> Citizens { get; private set; }

        double IFlammable.Pontential => throw new NotImplementedException();

        double IFlammable.Health => throw new NotImplementedException();

        bool IFlammable.IsOnFire { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        Level IUpgradeable.Level { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        int IUpgradeable.NextUpgradeCost => throw new NotImplementedException();

        bool IUpgradeable.CanUpgrade => throw new NotImplementedException();

        public bool IsFull => Current >= Capacity;

        public bool HasCitizen => Current > 0;

        #endregion

        #region Constructors

        internal Zone()
        {
            Current = 0;
            Capacity = 5;

            Citizens = new List<Citizen>();
        }
        
        #endregion
        
        #region Public methods

        public void AddCitizen(Citizen citizen)
        {
            Citizens.Add(citizen);
            ++Current;
        }

        public void DropCitizen(Citizen citizen)
        {
            Citizens.Remove(citizen);
            --Current;
        }

        #endregion
    }
}
