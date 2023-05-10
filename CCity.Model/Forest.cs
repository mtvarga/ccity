using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Forest : Placeable
    {
        const int effectRadius = 3;

        public int MaxAge => 10;

        #region Properties 

        public override int PlacementCost => 100;

        public override int MaintenanceCost => CanGrow ? 10 : 0;

        public override bool EffectSpreadingCondition => !IsDemolished;

        public override bool ListingCondition => !IsDemolished && CanGrow;

        public int GrowthMonts { get; private set; }

        public bool WillAge => GrowthMonts % 12 == 11;

        public double EffectRate => Math.Max((double)Age/MaxAge,(double)1/MaxAge); 

        public bool CanGrow => Age < MaxAge;

        public int Age => GrowthMonts / 12;

        public override bool IsPublic => true;

        #endregion

        #region Constructor

        public Forest(bool starter=false)
        {
            if (!starter) GrowthMonts = 0;
            else GrowthMonts = MaxAge * 12;
        }

        #endregion

        #region public methods

        public override List<Field> Effect(Func<Placeable, bool, Action<Field,int>,int,List<Field> > spreadingFunction, bool add)
        {
            if (EffectSpreaded == add) return new();
            EffectSpreaded = add;
            return spreadingFunction(this, add, (f, i) => f.ChangeForestEffect(i), effectRadius);
        }

        public void Grow()
        {
            if(CanGrow)
            {
                ++GrowthMonts;
            }
        }

        #endregion
    }

}
