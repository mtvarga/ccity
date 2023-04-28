﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Stadium : Placeable, IFlammable, IMultifield
    {
        #region Constants

        private const int effectRadius = 10;

        #endregion

        #region Fields

        List<Filler> _occupies;

        #endregion

        #region Properties

        public override int PlacementCost => 100;

        public override int MaintenanceCost => 10;

        public override int NeededElectricity => 200;

        double IFlammable.Pontential => throw new NotImplementedException();

        double IFlammable.Health => throw new NotImplementedException();

        bool IFlammable.IsOnFire { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        int IMultifield.Width => 2;

        int IMultifield.Height => 2;

        List<Filler> IMultifield.Occupies { get => _occupies; set => _occupies=value; }

        #endregion

        #region Constructor

        public Stadium()
        {
            _occupies = new();
        }

        #endregion

        #region Public methods

        public override List<Field> Effect(Func<Placeable, bool, Action<Field, int>, int, List<Field>> spreadingFunction, bool add)
        {
            if (EffectSpreaded == add) return new();
            EffectSpreaded = add;
            return spreadingFunction(this, add, (f, i) => f.ChangeStadiumEffect(i), effectRadius);
        }

        #endregion
    }
}
