using System.Diagnostics;

namespace CCity.Model
{
    public abstract class Zone : Placeable, IFlammable, IUpgradeable
    {
        #region Fields

        private Level _level;
        private const int upgradeCost = 100;

        #endregion
        #region Constants
        
        private const int BasicUpgradeCost= 100;
        private const int BeginnerCapacity = 10;
        private const int IntermediateCapacity = 30;
        private const int AdvancedCapacity = 100;

        #endregion
       
        #region Properties

        public override int NeededElectricity => Count;

        public int Count => Citizens.Count;
        
        public int Capacity => _level switch
        {
            Level.Beginner => BeginnerCapacity,
            Level.Intermediate => IntermediateCapacity,
            Level.Advanced => AdvancedCapacity,
            _ => throw new System.NotImplementedException()
        };

        public List<Citizen> Citizens { get; }

        public abstract byte Potential { get; }
    
        bool IFlammable.Burning { get; set; }

        Level IUpgradeable.Level { get => _level; set => _level = value; }
        
        int IUpgradeable.NextUpgradeCost =>_level!=Level.Advanced? ((int)_level+1)*BasicUpgradeCost: 0;

        bool IUpgradeable.CanUpgrade => _level!=Level.Advanced;
        
        ushort IFlammable.Health { get; set; } = IFlammable.FlammableMaxHealth;

        public bool Full => Count == Capacity;

        public bool Empty => Count == 0;

        public bool BelowHalfPopulation => Count * 2 < Capacity;
        public double DesireToMoveIn { get; set; }
        public double DistanceEffect { get; set; }

        #endregion

        #region Constructors

        internal Zone()
        {
            Citizens = new List<Citizen>();
            _level = Level.Beginner;
        }
        
        #endregion
        
        #region Public methods

        public bool AddCitizen(Citizen citizen)
        {
            if (Count + 1 > Capacity) 
                return false;
            
            Citizens.Add(citizen);
            return true;
        }

        public bool DropCitizen(Citizen citizen) => Citizens.Remove(citizen);

        public double Satisfaction()
        {
            if (Count == 0) return 0;
            double sum = Citizens.Sum(e => e.LastCalculatedSatisfaction);
            return sum / Count;
        }
        
        public void Upgrade()
        {
            if (_level == Level.Advanced) return;
            _level++;
        }
        #endregion
    }
}
