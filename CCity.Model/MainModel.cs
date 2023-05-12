using System.Diagnostics;
using Microsoft.VisualBasic;

namespace CCity.Model
{
    public class MainModel
    {
        #region Constants

        private const double MinSatisfaction = 0.2;
        private const int MinPopulation = 15;

        #endregion
        #region Fields

        private FieldManager _fieldManager;
        private CitizenManager _citizenManager;
        private GlobalManager _globalManager;

        DateTime _date;
        DateTime _previousDate;

        #endregion

        #region Properties

        public string CityName { get; private set; }
        public string MayorName { get; private set; }
        public Field[,] Fields { get => _fieldManager.Fields; }
        public int Budget { get => _globalManager.Budget; }
        public Taxes Taxes { get => _globalManager.Taxes; }
        public DateTime Date { get => _date; }
        public Speed Speed { get; private set; }
        public double Satisfaction { get => _globalManager.TotalSatisfaction; }
        public int Population { get => _citizenManager.Population; }
        public LinkedList<ITransaction> Logbook { get => _globalManager.Logbook; }
        public int Width { get => _fieldManager.Width; }
        public int Height { get => _fieldManager.Height; }
        //for test
        public GameErrorType LastErrorType { private set; get; }
        #endregion

        #region Constructors

        public MainModel(bool notTestMode=true)
        {
            _fieldManager = new FieldManager(notTestMode);
            _citizenManager = new CitizenManager();
            _globalManager = new GlobalManager();

            Speed = Speed.Normal;

            _date = DateTime.Now;
        }

        #endregion

        #region Public methods

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
                ErrorOccured.Invoke(this, new ErrorEventArgs(ex.ErrorType));
            }
        }

        public void DeployFireTruck(int x, int y)
        {
            try
            {
                _fieldManager.DeployFireTruck(x, y);
                FireTruckMoved?.Invoke(this, new FieldEventArgs(new List<Field>()));
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
                updatedFields = new List<Field> { _fieldManager.IgniteBuilding(x, y) };
                
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

        public void ChangeTax(TaxType type, double amount)
        {
            if(!_globalManager.ChangeTax(type, amount))
                return;
                
            TaxChanged?.Invoke(this, EventArgs.Empty);
            SatisfactionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ChangeSpeed(Speed speed)
        {
            if (_fieldManager.FireEmergencyPresent)
                return;
            
            Speed = speed;
            SpeedChanged?.Invoke(this, EventArgs.Empty);
        }

        public void TimerTick()
        {

            _previousDate = _date;
            _date = Speed switch
            {
                Speed.Slow => _date.AddMinutes(10),
                Speed.Normal => _date.AddHours(3),
                Speed.Fast => _date.AddHours(45),
                _ => _date
            };

            DateChanged?.Invoke(this, EventArgs.Empty);

            Tick();

            if(_previousDate.Month != _date.Month) MonthlyTick();

            if(_previousDate.Year != _date.Year) YearlyTick();
        }

        public void StartNewGame(string cityName, string mayorName)
        {
            _fieldManager = new FieldManager();
            _citizenManager = new CitizenManager();
            _globalManager = new GlobalManager();
            CityName = cityName;
            MayorName = mayorName;

            _date=DateTime.Now;
            Speed = Speed.Normal;

            NewGame?.Invoke(this, EventArgs.Empty);
        }

        public (int[], List<Road>) GetFourRoadNeighbours(Road road) => _fieldManager.GetFourRoadNeighbours(road);

        public List<Field> FireTruckLocations() => _fieldManager.FireTruckLocations();
        
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
            List<Field>? oldFireTruckLocations = null;
            
            if (_fieldManager.FireEmergencyPresent)
            {
                if (_fieldManager.FireTrucksDeployed)
                    oldFireTruckLocations = _fieldManager.UpdateFireTrucks();

                updatedFields = _fieldManager.UpdateBurningBuildings();
            }

            GameTicked?.Invoke(this, EventArgs.Empty);
            
            if (updatedFields != null)
                FieldsUpdated?.Invoke(this, new FieldEventArgs(updatedFields));

            if (oldFireTruckLocations != null)
                FireTruckMoved?.Invoke(this, new FieldEventArgs(oldFireTruckLocations));
        }

        private void MonthlyTick()
        {
            var vacantHomes = _fieldManager.ResidentialZones(false);
            var vacantCommercialZones = _fieldManager.CommercialZones(false).Cast<WorkplaceZone>().ToList();
            var vacantIndustrialZones = _fieldManager.IndustrialZones(false).Cast<WorkplaceZone>().ToList();

            var movedOutCitizens = new List<Citizen>();
            var newCitizens = new List<Citizen>();
            
            /*if (vacantCommercialZones.Any() || vacantIndustrialZones.Any())
            {
                workplaceOptimizedCitizens = _citizenManager.OptimizeWorkplaces(vacantCommercialZones, vacantIndustrialZones);

                if (workplaceOptimizedCitizens.Any())
                    _globalManager.UpdateSatisfaction(workplaceOptimizedCitizens);

                if (vacantHomes.Any() && (vacantCommercialZones.Any() || vacantIndustrialZones.Any()))
                {
                    newCitizens = _citizenManager.IncreasePopulation(vacantHomes, vacantCommercialZones, vacantIndustrialZones);
                    
                    if (newCitizens.Any())
                        _globalManager.UpdateSatisfaction(true, newCitizens, _citizenManager.Citizens);
                }
            }*/
            movedOutCitizens = _citizenManager.DecreasePopulation();
            if (movedOutCitizens.Any())
                _globalManager.UpdateSatisfaction(false, movedOutCitizens, _citizenManager.Citizens);

            List<Field> fields = _fieldManager.UpdateModifiedZonesSpread();
            List<Zone> zones = fields.Where(e => e.Placeable is Zone).Select(e => (Zone)e.Placeable!).ToList();
            _globalManager.UpdateSatisfaction(zones, _fieldManager.CommercialZoneCount, _fieldManager.IndustrialZoneCount);

            if (vacantHomes.Any() && (vacantCommercialZones.Any() || vacantIndustrialZones.Any()))
            {
                newCitizens = _citizenManager.IncreasePopulation(vacantHomes, vacantCommercialZones, vacantIndustrialZones,Satisfaction);

                if (newCitizens.Any())
                    _globalManager.UpdateSatisfaction(true, newCitizens, _citizenManager.Citizens);
            }

            fields = fields.Concat(_fieldManager.UpdateModifiedZonesSpread()).ToList();
            fields = fields.Concat(_fieldManager.GrowForests()).ToList();
            zones = fields.Where(e => e.Placeable is Zone).Select(e => (Zone)e.Placeable!).ToList();
            _globalManager.UpdateSatisfaction(zones, _fieldManager.CommercialZoneCount, _fieldManager.IndustrialZoneCount);

            // TODO: Optimize this - add only affected zones to the list
            foreach (Zone zone in _fieldManager.ResidentialZones(true)) fields.Add(zone.Owner!);
            foreach (Zone zone in _fieldManager.CommercialZones(true)) fields.Add(zone.Owner!);
            foreach (Zone zone in _fieldManager.IndustrialZones(true)) fields.Add(zone.Owner!);
            
            var field = _fieldManager.IgniteRandomBuilding();

            if (field != null)
            {
                // A building was set on fire
                EngageFireEmergency();
                fields.Add(field);
            }
            
            FieldsUpdated?.Invoke(this, new FieldEventArgs(fields));
            PopulationChanged?.Invoke(this, EventArgs.Empty);
            SatisfactionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void YearlyTick()
        {
            var workplaceZones = _fieldManager.CommercialZones(true).Cast<WorkplaceZone>()
                .Concat(_fieldManager.IndustrialZones(true)).ToList();
            
            _globalManager.CollectTax(_fieldManager.ResidentialZones(true), workplaceZones);
            var allTransactions = new List<ITransaction>();
            for (int i = 0; i < _fieldManager.Width; i++)
            for (int j = 0; j < _fieldManager.Height; j++)
            {
                if (_fieldManager.Fields[i, j].ActualPlaceable is { } placeable && placeable.MaintenanceCost>0)
                {
                    allTransactions.Add(
                    _globalManager.CommitTransaction(Transactions.Maintance(placeable)));
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
