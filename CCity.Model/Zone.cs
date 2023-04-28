namespace CCity.Model
{
    public abstract class Zone : Placeable, IFlammable, IUpgradeable
    {
        #region Fields

        private Level _level;
        private const int upgradeCost = 100;

        #endregion
        #region Constants

        private const int CapacityConstant = 10;

        #endregion

        #region Properties

        public override int NeededElectricity => Count;

        public int Count => Citizens.Count;

        public int Capacity => CapacityConstant;

        public List<Citizen> Citizens { get; }

        double IFlammable.Pontential => throw new NotImplementedException();

        double IFlammable.Health => throw new NotImplementedException();

        bool IFlammable.IsOnFire { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        Level IUpgradeable.Level { get => _level; set => _level = value; }

        int IUpgradeable.NextUpgradeCost => ((int)_level + 1) * upgradeCost;

        bool IUpgradeable.CanUpgrade => _level != Level.Advanced;

        public bool Full => Count == Capacity;

        public bool Empty => Count == 0;

        public bool BelowHalfPopulation => Count * 2 < Capacity;

        #endregion

        #region Constructors

        internal Zone()
        {
            Citizens = new List<Citizen>();
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

        #endregion
    }
}
