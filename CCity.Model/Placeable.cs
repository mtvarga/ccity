using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public abstract class Placeable
    {
        #region Properties

        public virtual Field? Owner { get; internal set; }
        public abstract int PlacementCost { get; }
        public abstract int MaintenanceCost { get; }
        public virtual int NeededElectricity => 0;
        public bool EffectSpreaded { get; internal set; }
        public bool IsDemolished { get => Owner != null && Owner.Placeable == null; }
        public Placeable Root => this is Filler filler ? (Placeable)filler.Main : this;
        public virtual bool EffectSpreadingCondition => IsPublic && IsElectrified && !IsDemolished;
        public virtual bool ListingCondition => IsPublic && !IsDemolished;

        //SPREADING

        //a Placeable is the root of an S spread, it GetsSpreadFrom[S].root = itself
        public Dictionary<SpreadType, (Placeable? direct, Placeable? root)> GetsSpreadFrom { get; set; }
        
        //if Placeable is root, then MaxSpread is the value of its capacity (PowerPlant capacity for example)
        //the needed value, otherwise
        public Dictionary<SpreadType, Func<int>> MaxSpreadValue { get; set; }

        //if Placeable is root, then CurrentSpreadValue is the value of how many values are used out of its capacity
        //the current value out of needed, otherwise
        public Dictionary<SpreadType, int> CurrentSpreadValue { get; set; }

        public virtual bool IsPublic => GetsSpreadFrom[SpreadType.Publicity].root != null;
        public bool IsElectrified => CurrentSpreadValue[SpreadType.Electricity] == MaxSpreadValue[SpreadType.Electricity]() && GetsSpreadFrom[SpreadType.Electricity].root != null || GetsSpreadFrom[SpreadType.Electricity].root == this;
        public bool IsPartlyElectrified => CurrentSpreadValue[SpreadType.Electricity] > 0 && !IsElectrified;

        #endregion

        public Placeable()
        {
            EffectSpreaded = false;

            GetsSpreadFrom = new();
            CurrentSpreadValue = new();
            MaxSpreadValue = new();

            InitSpread(SpreadType.Publicity);
            InitSpread(SpreadType.Electricity);
            MaxSpreadValue[SpreadType.Electricity] = () => NeededElectricity;
        }

        public virtual void MakeRoot(SpreadType spreadType)
        {
            GetsSpreadFrom[spreadType] = (null, this);
        }

        private void InitSpread(SpreadType spreadType)
        {
            GetsSpreadFrom.Add(spreadType, (null, null));
            CurrentSpreadValue.Add(spreadType, 0);
            MaxSpreadValue.Add(spreadType, () => 0);
        }

        #region Public methods

        public void PlaceAt(Field field)
        {
            Owner = field;
        }

        public virtual List<Field> Effect(Func<Placeable, bool, Action<Field, int>, int, List<Field>> f, bool b) => new();

        public virtual bool CouldGivePublicityTo(Placeable _) => false;
        public virtual bool CouldGiveElectricityTo(Placeable placeable)
        {
            return placeable switch
            {
                Zone => true,
                FireDepartment => true,
                PoliceDepartment => true,
                Stadium => true,
                Pole => true,
                Road => true,
                _ => false
            };
        }

        #endregion
    }
}
