namespace CCity.Model
{
    public class MainModel
    {
        #region Constants

        internal const int TicksPerSecond = 4;
        
        private const double MinSatisfaction = 0.2;
        private const int MinPopulation = 15;

        #endregion
        
        #region Fields

        private FieldManager _fieldManager;
        private CitizenManager _citizenManager;
        private GlobalManager _globalManager;

        DateTime _previousDate;

        #endregion

        #region Properties

        public string CityName { get; private set; }
        public string MayorName { get; private set; }
        public DateTime Date { get; private set; }
        public Speed Speed { get; private set; }
        public Field[,] Fields => _fieldManager.Fields;
        public int Budget => _globalManager.Budget;
        public Taxes Taxes => _globalManager.Taxes;
        public double Satisfaction => _globalManager.TotalSatisfaction;
        public int Population => _citizenManager.Population;
        public LinkedList<ITransaction> Logbook => _globalManager.Logbook;
        public int Width => _fieldManager.Width;
        public int Height => _fieldManager.Height;

        //for test
        public GameErrorType LastErrorType { get; private set; }

        #endregion

        #region Constructors

        public MainModel(bool testModeGenerateForest = false, bool testModeRandomIgniteOff = false)
        {
            _fieldManager = new FieldManager(testModeGenerateForest, testModeRandomIgniteOff);
            _citizenManager = new CitizenManager();
            _globalManager = new GlobalManager();

            Speed = Speed.Normal;
            Date = new DateTime(2023,01,01);

            CityName = "";
            MayorName = "";
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Places a placeable on the given coordinates of the Field matrix
        /// </summary>
        /// <param name="x">X coordinate of the Field matrix</param>
        /// <param name="y">Y coordinate of the Field matrix</param>
        /// <param name="placeable">The Placeable to place</param>
        public void Place(int x, int y, Placeable placeable)
        {
            try
            {
                List<Field>  effectedFields = _fieldManager.Place(x, y, placeable);
                List<Zone> zones = effectedFields.Where(e => e.Placeable is Zone).Select(e => (Zone)e.Placeable!).ToList();
                _globalManager.CommitTransaction(Transactions.Placement(placeable));
                _globalManager.AddOnlyOneToLogbook(Transactions.Placement(placeable));
                _globalManager.UpdateSatisfaction(zones, _fieldManager.CommercialZoneCount, _fieldManager.IndustrialZoneCount);
                BudgetChanged?.Invoke(this, EventArgs.Empty);
                SatisfactionChanged?.Invoke(this, EventArgs.Empty);

                FieldsUpdated?.Invoke(this, new FieldEventArgs(effectedFields));
            }
            catch (GameErrorException ex)
            {
                LastErrorType = ex.ErrorType;
                ErrorOccured?.Invoke(this, new ErrorEventArgs(ex.ErrorType));
            }
            catch(Exception)
            {
                LastErrorType = GameErrorType.Unhandled;
                ErrorOccured?.Invoke(this, new ErrorEventArgs(GameErrorType.Unhandled));
            }
        }

        /// <summary>
        /// Demolish the placeable from the given coordinates of the Field matrix
        /// </summary>
        /// <param name="x">X coordinate of the Field matrix</param>
        /// <param name="y">Y coordinate of the Field matrix</param>
        public void Demolish(int x, int y)
        {
            try
            {
                (Placeable placeable, List<Field>  effectedFields) = _fieldManager.Demolish(x, y);
                List<Zone> zones = effectedFields.Where(e => e.Placeable is Zone).Select(e => (Zone)e.Placeable!).ToList();
                _globalManager.CommitTransaction(Transactions.Takeback(placeable));
                _globalManager.AddOnlyOneToLogbook(Transactions.Takeback(placeable));
                _globalManager.UpdateSatisfaction(zones, _fieldManager.CommercialZoneCount, _fieldManager.IndustrialZoneCount);
                BudgetChanged?.Invoke(this, EventArgs.Empty);
                SatisfactionChanged?.Invoke(this, EventArgs.Empty);
                FieldsUpdated?.Invoke(this, new FieldEventArgs(effectedFields));
            }
            catch(GameErrorException ex)
            {
                LastErrorType = ex.ErrorType;
                ErrorOccured?.Invoke(this, new ErrorEventArgs(ex.ErrorType));
            }
            catch (Exception)
            {
                ErrorOccured?.Invoke(this, new ErrorEventArgs(GameErrorType.Unhandled));
            }
        }

        /// <summary>
        /// Upgrades the placeable on the given coordinates of the Field matrix
        /// </summary>
        /// <param name="x">X coordinate of the Field matrix</param>
        /// <param name="y">Y coordinate of the Field matrix</param>
        public void Upgrade(int x, int y)
        {
            try
            {
                (IUpgradeable upgradeable, int cost) = _fieldManager.Upgrade(x, y);
                _globalManager.CommitTransaction(Transactions.Upgrade((Placeable)upgradeable,cost));
                _globalManager.AddOnlyOneToLogbook(Transactions.Upgrade((Placeable)upgradeable,cost));
                BudgetChanged?.Invoke(this, EventArgs.Empty);
                FieldsUpdated?.Invoke(this, new FieldEventArgs(new List<Field>() { ((Placeable)upgradeable).Owner!}));
            }
            catch (GameErrorException ex)
            {
                ErrorOccured?.Invoke(this, new ErrorEventArgs(ex.ErrorType));
            }
        }

        public void DeployFireTruck(int x, int y)
        {
            try
            {
                _fieldManager.DeployFireTruck(x, y);
            }
            catch (GameErrorException ex)
            {
                LastErrorType = ex.ErrorType;
                ErrorOccured?.Invoke(this, new ErrorEventArgs(ex.ErrorType));
            }
            catch(Exception)
            {
                ErrorOccured?.Invoke(this, new ErrorEventArgs(GameErrorType.Unhandled));
            }
        }

        public void IgniteBuilding(int x, int y)
        {
            List<Field>? updatedFields = null;
            
            try
            {
                updatedFields = _fieldManager.IgniteBuilding(x, y);
                
                // A building was set on fire
                EngageFireEmergency();
            }
            catch(GameErrorException ex)
            {
                LastErrorType = ex.ErrorType;
                ErrorOccured?.Invoke(this, new ErrorEventArgs(ex.ErrorType));
            }
            catch (Exception)
            {
                ErrorOccured?.Invoke(this, new ErrorEventArgs(GameErrorType.Unhandled));
            }
            
            if (updatedFields != null)
                FieldsUpdated?.Invoke(this, new FieldEventArgs(updatedFields));
        } 


        /// <summary>
        /// Changes the tax by adding the amount (percentage) to the current tax value
        /// </summary>
        /// <param name="type">The type of the tax</param>
        /// <param name="amount">The amount to add</param>
        public void ChangeTax(TaxType type, double amount)
        {
            if(!_globalManager.ChangeTax(type, amount))
                return;
                
            TaxChanged?.Invoke(this, EventArgs.Empty);
            SatisfactionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Changes the speed of the game
        /// </summary>
        /// <param name="speed">New speed</param>
        public void ChangeSpeed(Speed speed)
        {
            if (_fieldManager.FirePresent)
                return;
            
            Speed = speed;
            SpeedChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Forwards the time based on the current speed
        /// </summary>
        public void TimerTick()
        {

            _previousDate = Date;
            Date = Speed switch
            {
                Speed.Slow => Date.AddMinutes(10),
                Speed.Normal => Date.AddHours(3),
                Speed.Fast => Date.AddHours(45),
                _ => Date
            };

            DateChanged?.Invoke(this, EventArgs.Empty);

            Tick();

            if(_previousDate.Month != Date.Month) MonthlyTick();

            if(_previousDate.Year != Date.Year) YearlyTick();
        }

        /// <summary>
        /// Resets the game state to default
        /// </summary>
        /// <param name="cityName">The name of the city</param>
        /// <param name="mayorName">The name of the major</param>
        public void StartNewGame(string cityName, string mayorName)
        {
            _fieldManager = new FieldManager();
            _citizenManager = new CitizenManager();
            _globalManager = new GlobalManager();
            CityName = cityName;
            MayorName = mayorName;

            Date = new DateTime(2023,01,01);
            Speed = Speed.Normal;

            NewGame?.Invoke(this, EventArgs.Empty);
        }

        /// <param name="road">The road to </param>
        /// <returns>A tuple with 4 elements indicating the presence (0) or absence (1) of a Road on the current side (top, right, bottom, left), and a list of these Road neighbours</returns>
        public ((byte t, byte r, byte b, byte l) indicators, List<Road> neighbours) GetFourRoadNeighbours(Road road) => _fieldManager.GetFourRoadNeighbours(road);

        public IEnumerable<Field> FireTruckLocations() => _fieldManager.FireTruckLocations();
        
        #endregion

        #region Private methods
        
        private void Tick()
        {
            if (Satisfaction<MinSatisfaction && Population>MinPopulation || Satisfaction>MinSatisfaction && Population==0)
            {
                GameOver?.Invoke(this, EventArgs.Empty);
                return;
            }
            List<Field>? updatedFields = null;
            List<Field>? wreckedFields = null;
            List<Field>? oldFireTruckLocations = null;
            
            if (_fieldManager.FireTrucksDeployed)
                oldFireTruckLocations = _fieldManager.UpdateFireTrucks();

            if (_fieldManager.FirePresent)
            {
                (updatedFields, wreckedFields) = _fieldManager.UpdateFires();

                if (wreckedFields.Any())
                {
                    var citizens = wreckedFields
                        .Where(f => f.Placeable is Zone)
                        .SelectMany(f => (f.Placeable as Zone)!.Citizens).ToHashSet().ToList();

                    // Move out citizens of the effected fields
                    updatedFields.AddRange(_citizenManager.DecreasePopulation(citizens));
                    _globalManager.UpdateSatisfaction(false, citizens, _citizenManager.Citizens);
                    
                    // Demolish the fields
                    foreach (var field in wreckedFields)
                        updatedFields.AddRange(_fieldManager.Demolish(field.X, field.Y).Item2);
                    
                    _globalManager.UpdateSatisfaction(Enumerable.Empty<Zone>(), _fieldManager.CommercialZoneCount, _fieldManager.IndustrialZoneCount);
                }
            }

            GameTicked?.Invoke(this, EventArgs.Empty);

            if (updatedFields != null)
                FieldsUpdated?.Invoke(this, new FieldEventArgs(updatedFields));

            if (oldFireTruckLocations != null)
                FireTruckMoved?.Invoke(this, new FieldEventArgs(oldFireTruckLocations));

            if (wreckedFields == null) 
                return;
            
            PopulationChanged?.Invoke(this, EventArgs.Empty);
            SatisfactionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void MonthlyTick()
        {
            var vacantHomes = _fieldManager.ResidentialZones(false);
            var vacantCommercialZones = _fieldManager.CommercialZones(false).Cast<WorkplaceZone>().ToList();
            var vacantIndustrialZones = _fieldManager.IndustrialZones(false).Cast<WorkplaceZone>().ToList();

            var movedOutCitizens = new List<Citizen>();
            var newCitizens = new List<Citizen>();
            
            movedOutCitizens = _citizenManager.DecreasePopulation();
            if (movedOutCitizens.Any())
                _globalManager.UpdateSatisfaction(false, movedOutCitizens, _citizenManager.Citizens);

            List<Field> fields = _fieldManager.UpdateModifiedZonesSpread();
           

            _globalManager.UpdateSatisfaction(
                fields.Where(e => e.Placeable is Zone).Select(e => (Zone)e.Placeable!).ToList(),
                _fieldManager.CommercialZoneCount, _fieldManager.IndustrialZoneCount);

            if (vacantHomes.Any() && (vacantCommercialZones.Any() || vacantIndustrialZones.Any()))
            {
                newCitizens = _citizenManager.IncreasePopulation(vacantHomes, vacantCommercialZones, vacantIndustrialZones,Satisfaction);
                if (newCitizens.Any())
                    _globalManager.UpdateSatisfaction(true, newCitizens, _citizenManager.Citizens);
            }

            fields = fields.Concat(_fieldManager.UpdateModifiedZonesSpread()).Concat(_fieldManager.GrowForests()).ToList();

            _globalManager.UpdateSatisfaction(
                fields.Where(e => e.Placeable is Zone).Select(e => (Zone)e.Placeable!).ToList(), 
                _fieldManager.CommercialZoneCount, _fieldManager.IndustrialZoneCount);

            foreach (Zone zone in _fieldManager.Zones(true)) fields.Add(zone.Owner!);
            
            var ignitedFields = _fieldManager.IgniteRandomFlammable();

            if (ignitedFields.Any())
            {
                // A building was set on fire
                EngageFireEmergency();
                fields.AddRange(ignitedFields);
            }
            
            FieldsUpdated?.Invoke(this, new FieldEventArgs(fields));
            PopulationChanged?.Invoke(this, EventArgs.Empty);
            SatisfactionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void YearlyTick()
        {
            var workplaceZones = _fieldManager.CommercialZones(true).Cast<WorkplaceZone>()
                .Concat(_fieldManager.IndustrialZones(true)).ToList();
            
            _globalManager.PassYear();
            _globalManager.CollectTax(_fieldManager.ResidentialZones(true), workplaceZones);
            var allTransactions = new List<ITransaction>();
            for (int i = 0; i < _fieldManager.Width; i++)
            for (int j = 0; j < _fieldManager.Height; j++)
            {
                if (_fieldManager.Fields[i, j].ActualPlaceable is { } placeable && placeable.MaintenanceCost>0)
                {
                    allTransactions.Add(
                    _globalManager.CommitTransaction(Transactions.Maintenance(placeable)));
                }
            }
            _globalManager.AddMaintenanceToLogbook(allTransactions);
            BudgetChanged?.Invoke(this, EventArgs.Empty);
            SatisfactionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void EngageFireEmergency()
        {
            Speed = Speed.Slow;
            SpeedChanged?.Invoke(this, EventArgs.Empty);
        }

        public double ZoneSatisfaction(Zone zone) => _globalManager.CalculateSatisfaction(zone);
        
        #endregion

        #region Events

        public event EventHandler<EventArgs>? GameTicked;
        public event EventHandler<FieldEventArgs>? FieldsUpdated;
        public event EventHandler<FieldEventArgs>? FireTruckMoved;
        public event EventHandler<ErrorEventArgs>? ErrorOccured;
        public event EventHandler<EventArgs>? PopulationChanged;
        public event EventHandler<EventArgs>? BudgetChanged;
        public event EventHandler<EventArgs>? SatisfactionChanged;
        public event EventHandler<EventArgs>? TaxChanged;
        public event EventHandler<EventArgs>? SpeedChanged;
        public event EventHandler<EventArgs>? DateChanged;
        public event EventHandler<EventArgs>? NewGame;
        public event EventHandler<EventArgs>? GameOver;

        #endregion
    }
}
