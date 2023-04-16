namespace CCity.Model
{
    public class MainModel
    {
        #region Fields

        private FieldManager _fieldManager;
        private CitizenManager _citizenManager;
        private GlobalManager _globalManager;

        #endregion

        #region Properties

        public string CityName { get; private set; }
        public string MayorName { get; private set; }
        public Field[,] Fields { get => _fieldManager.Fields; }
        public int Budget { get => _globalManager.Budget; }
        public Taxes Taxes { get => _globalManager.Taxes; }
        public int Date { get; }
        public Speed Speed { get; }
        public double Satisfaction { get => _globalManager.TotalSatisfaction; }
        public int Population { get => _citizenManager.Population; }
        public int Width { get => _fieldManager.Width; }
        public int Height { get => _fieldManager.Height; }


        #endregion

        #region Constructors

        public MainModel()
        {
            _fieldManager = new FieldManager();
            _citizenManager = new CitizenManager();
            _globalManager = new GlobalManager();
        }

        #endregion

        #region Public methods

        public void Place(int x, int y, Placeable placeable)
        {
            List<Field>? effectedFields = _fieldManager.Place(x, y, placeable);
            if(effectedFields != null)
            {
                List<Zone> zones = effectedFields.Where(e => e.Placeable is Zone).Select(e => (Zone)e.Placeable!).ToList();
                _globalManager.Pay(placeable.PlacementCost);
                _globalManager.UpdateSatisfaction(zones, _fieldManager.CommercialZoneCount, _fieldManager.IndustrialZoneCount);
                BudgetChanged?.Invoke(this, EventArgs.Empty);
                SatisfactionChanged?.Invoke(this, EventArgs.Empty);
            }
            FieldsUpdated?.Invoke(this, new FieldEventArgs(effectedFields));
        }

        public void Demolish(int x, int y)
        {
            (Placeable placeable, List<Field>? effectedFields) = _fieldManager.Demolish(x, y);
            if(effectedFields != null)
            {
                List<Zone> zones = effectedFields.Where(e => e.Placeable is Zone).Select(e => (Zone)e.Placeable!).ToList();
                _globalManager.Pay(-(placeable.PlacementCost/2));
                _globalManager.UpdateSatisfaction(zones, _fieldManager.CommercialZoneCount, _fieldManager.IndustrialZoneCount);
                BudgetChanged?.Invoke(this, EventArgs.Empty);
                SatisfactionChanged?.Invoke(this, EventArgs.Empty);
            }
            FieldsUpdated?.Invoke(this, new FieldEventArgs(effectedFields));
        }

        public void Upgrade(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void SendFiretruck(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void ChangeTax(TaxType type, double amount)
        {
            _globalManager.ChangeTax(type, amount);
        }

        public void ChangeSpeed(int n)
        {
            throw new NotImplementedException();
        }

        public void TimerTick()
        {
            throw new NotImplementedException();
        }

        public void NewGame(string cityName, string mayorName)
        {
            _fieldManager = new FieldManager();
            _citizenManager = new CitizenManager();
            _globalManager = new GlobalManager();
            CityName = cityName;
            MayorName = mayorName;
        }

        #endregion

        #region Private methods

        private void ChangeDate()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Events

        public event EventHandler<EventArgs>? GameTicked;
        public event EventHandler<FieldEventArgs>? FieldsUpdated;
        public event EventHandler<EventArgs>? PopulationChanged;
        public event EventHandler<EventArgs>? BudgetChanged;
        public event EventHandler<EventArgs>? SatisfactionChanged;
        public event EventHandler<EventArgs>? TaxChanged;
        public event EventHandler<EventArgs>? GameOver;

        #endregion
    }
}
