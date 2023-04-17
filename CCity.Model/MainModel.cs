namespace CCity.Model
{
    public class MainModel
    {
        #region Fields

        private FieldManager _fieldManager;
        private CitizenManager _citizenManager;
        private GlobalManager _globalManager;

        private int _counter;
        private int _monthCounter;

        #endregion

        #region Properties

        public string CityName { get; private set; }
        public string MayorName { get; private set; }
        public Field[,] Fields { get => _fieldManager.Fields; }
        public int Budget { get => _globalManager.Budget; }
        public Taxes Taxes { get => _globalManager.Taxes; }
        public int Date { get; }
        public Speed Speed { get; private set; }
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

            _counter = 0;
            _monthCounter = 0;
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
            if(!_globalManager.ChangeTax(type, amount))
                return;
                
            TaxChanged?.Invoke(this, EventArgs.Empty);
            SatisfactionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeSpeed(Speed speed)
        {
            Speed = speed;
            SpeedChanged?.Invoke(this, EventArgs.Empty);
        }

        public void TimerTick()
        {
            _counter = Speed switch
            {
                Speed.Slow => _counter + 1,
                Speed.Normal => _counter + 16,
                Speed.Fast => _counter + 256,
                _ => _counter
            };

            Tick();

            if (_counter / (double)4096 >= 1)
            {
                _monthCounter++;
                _counter -= 4096;
                
                MonthlyTick();
            }
            
            if (_monthCounter == 12)
            {
                _monthCounter = 0;
                
                YearlyTick();
            }
        }

        public void StartNewGame(string cityName, string mayorName)
        {
            _fieldManager = new FieldManager();
            _citizenManager = new CitizenManager();
            _globalManager = new GlobalManager();
            CityName = cityName;
            MayorName = mayorName;
            NewGame?.Invoke(this, EventArgs.Empty);
        }

        public (int[], List<Road>) GetFourRoadNeighbours(Road road) => _fieldManager.GetFourRoadNeighbours(road);

        #endregion

        #region Private methods

        private void ChangeDate()
        {
            throw new NotImplementedException();
        }
        
        private void Tick() 
        {
            GameTicked?.Invoke(this, EventArgs.Empty);
        }

        private void MonthlyTick()
        {
            var vacantHomes = _fieldManager.ResidentialZones(false);
            var vacantCommercialZones = _fieldManager.CommercialZones(false).Cast<WorkplaceZone>().ToList();
            var vacantIndustrialZones = _fieldManager.IndustrialZones(false).Cast<WorkplaceZone>().ToList();
            if (vacantHomes.Any() && (vacantCommercialZones.Any() || vacantIndustrialZones.Any()))
            {
                var newCitizens = _citizenManager.IncreasePopulation(vacantHomes, vacantCommercialZones,vacantIndustrialZones);
                
                _globalManager.UpdateSatisfaction(true, newCitizens, _citizenManager.Citizens);
                
                PopulationChanged?.Invoke(this, EventArgs.Empty);
                SatisfactionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void YearlyTick()
        {
            var workplaceZones = _fieldManager.CommercialZones(true).Cast<WorkplaceZone>()
                .Concat(_fieldManager.IndustrialZones(true)).ToList();
            
            _globalManager.CollectTax(_fieldManager.ResidentialZones(true), workplaceZones);
            
            for (int i = 0; i < _fieldManager.Width; i++)
            for (int j = 0; j < _fieldManager.Height; j++)
            {
                if (_fieldManager.Fields[i, j].Placeable is { } placeable)
                {
                    _globalManager.Pay(placeable.MaintenanceCost);
                }
            }
            
            BudgetChanged?.Invoke(this, EventArgs.Empty);
            SatisfactionChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Events

        public event EventHandler<EventArgs>? GameTicked;
        public event EventHandler<FieldEventArgs>? FieldsUpdated;
        public event EventHandler<EventArgs>? PopulationChanged;
        public event EventHandler<EventArgs>? BudgetChanged;
        public event EventHandler<EventArgs>? SatisfactionChanged;
        public event EventHandler<EventArgs>? TaxChanged;
        public event EventHandler<EventArgs>? SpeedChanged;
        public event EventHandler<EventArgs>? NewGame;
        public event EventHandler<EventArgs>? GameOver;

        #endregion
    }
}
