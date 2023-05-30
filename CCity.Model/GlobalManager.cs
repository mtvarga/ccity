namespace CCity.Model
{
    public class GlobalManager
    {
        #region Constants
        private const double MaxResTax = 0.50;
        private const double MaxComTax = 0.25;
        private const double MaxIndTax = 0.25;
        
        private const double MinResTax = 0.01;
        private const double MinComTax = 0.01;
        private const double MinIndTax = 0.01;

        private const double ResTaxRatio = 0.8;
        private const double IndTaxRatio = 0.1;
        private const double ComTaxRatio = 0.1;
        
        private const int StartingBudget = 10000;

        private const double CitizenAverageRatio = 0.25;
        private const double GlobalRatio = 0.75;
        
        private const double MinSafetyRatio = 0.25;
        private const double MaxSafetyRatio = 0.5;
        private const double SafetyRatioParts = 10;
        private const double MaxSafetyRatioPopulation = 1000;
        
        private const double MinNegativeBudgetRatio = 0;
        private const double MaxNegativeBudgetRatio = 0.80;
        private const double NegativeBudgetRatioParts = 4;

        private const double HomeRatio = 0.5;
        private const double WorkplaceRatio = 0.25;
        private const double DistanceRatio = 0.25;

        private const double HomePollutionRatio = 0.5;
        private const double HomeElectricityRatio = (double)1 / 3;
        private const double HomeStadiumRatio = (double)1 / 6;

        private const double WorkplaceStadiumRatio = 1;
        
        private const double TaxRatio = 0.8;
        private const double IndustrialCommercialBalanceRatio = 0.2;

        private const double WorkplaceTaxElectricityFactor = 0.5;
        
        private const int MaxLogbookLength = 250;
        
        #endregion

        #region Fields

        private Taxes _taxes;

        #endregion

        #region Properties

        public double TotalSatisfaction => Population switch
        {
            > 0 => TotalSatisfactionForCitizenAverage(AverageCitizenFactors),
            _ => 0
        };
        
        public int Budget { get; private set; }

        public Taxes Taxes => _taxes;

        private double NegativeBudgetRatio => MinNegativeBudgetRatio + 
                                              NegativeBudgetYears * (MaxNegativeBudgetRatio - MinNegativeBudgetRatio) / 
                                              NegativeBudgetRatioParts;  

        private int NegativeBudgetYears { get; set; }

        private double AverageCitizenFactors { get; set; }

        private double SafetyRatio => SafetyRatioForPopulation(Population);

        private double GlobalFactors => (TaxRatio * TaxFactors + 
                                         IndustrialCommercialBalanceRatio * IndustrialCommercialBalance) 
                                        /
                                        (TaxRatio + IndustrialCommercialBalanceRatio);

        private double TaxFactors => 1 - (ResTaxRatio * (Taxes.ResidentialTax - MinResTax) / (MaxResTax - MinResTax) +
                                          ComTaxRatio * (Taxes.CommercialTax - MinComTax) / (MaxComTax - MinComTax) +
                                          IndTaxRatio * (Taxes.IndustrialTax - MinIndTax) / (MaxIndTax - MinIndTax)) /
                                         (ResTaxRatio + ComTaxRatio + IndTaxRatio);

        private double IndustrialCommercialBalance => (CommercialZoneCount + IndustrialZoneCount) switch
        {
            0 => 0,
            _ => 1 -
                 Math.Abs(CommercialZoneCount - IndustrialZoneCount) /
                 ((double)CommercialZoneCount + IndustrialZoneCount)
        };

        private int CommercialZoneCount { get; set; }
        
        private int IndustrialZoneCount { get; set; }
        
        private int Population { get; set; }
        
        public LinkedList<ITransaction> Logbook { get;}
        

        #endregion

        #region Constructors

        public GlobalManager()
        {
            Budget = StartingBudget;
            NegativeBudgetYears = 0;

            _taxes = new Taxes
            {
                ResidentialTax = 0.27,
                CommercialTax = 0.15,
                IndustrialTax = 0.05
            };

            Population = 0;
            CommercialZoneCount = 0;
            IndustrialZoneCount = 0;
            Logbook = new LinkedList<ITransaction>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Add a transaction to the logbook
        /// If the logbook is full, remove the last transaction
        /// </summary>
        /// <param name="transaction">The transaction to add</param>
        public void AddOnlyOneToLogbook(ITransaction transaction)
        {
            Logbook.AddFirst(transaction);
            if (Logbook.Count > MaxLogbookLength)
                Logbook.RemoveLast();
        }
        
        /// <summary>
        /// Add a list of tax transactions to the logbook
        /// Group them by tax type
        /// </summary>
        /// <param name="transactions">The list of tax transactions to add</param>
        /// <exception cref="Exception"></exception>
        public void AddTaxToLogbook(List<ITransaction> transactions)
        {
            uint residentialTaxes = 0;
            uint commercialTaxes = 0;
            uint industrialTaxes = 0;
            foreach (var tmpTransaction in transactions)
            {
                var transaction = (TaxTransaction)tmpTransaction;
                switch (transaction.TaxType)
                {
                    case TaxType.Residental:
                        residentialTaxes +=transaction.Amount;
                        break;
                    case TaxType.Commercial:
                        commercialTaxes += transaction.Amount;
                        break;
                    case TaxType.Industrial:
                        industrialTaxes += transaction.Amount;
                        break;
                    default:
                        throw new Exception("Wrong tax type");
                }
            }
            Logbook.AddFirst(new TaxTransaction{Add=true,Amount = residentialTaxes,TaxType = TaxType.Residental});
            if (Logbook.Count > MaxLogbookLength)
                Logbook.RemoveLast();
            Logbook.AddFirst(new TaxTransaction{Add=true,Amount = commercialTaxes,TaxType = TaxType.Commercial});
            if (Logbook.Count > MaxLogbookLength)
                Logbook.RemoveLast();
            Logbook.AddFirst(new TaxTransaction{Add=true,Amount = industrialTaxes,TaxType = TaxType.Industrial});
            if (Logbook.Count > MaxLogbookLength)
                Logbook.RemoveLast();
        }

        /// <summary>
        /// Add a list of maintenance transactions to the logbook
        /// Group them by placeable type
        /// </summary>
        /// <param name="transactions"></param>
        public void AddMaintenanceToLogbook(List<ITransaction> transactions)
        {
            uint roadMaintenance = 0;
            uint forestMaintenance = 0;
            uint poleMaintenance = 0;
            foreach (var tmpTransaction in transactions)
            {
                var transaction = (PlaceableTransaction)tmpTransaction;
                switch (transaction.Placeable)
                {
                    case Road:
                        roadMaintenance += transaction.Amount;
                        break;
                    case Forest:
                        forestMaintenance += transaction.Amount;
                        break;
                    case Pole:
                        poleMaintenance += transaction.Amount;
                        break;
                    default:
                        Logbook.AddFirst(new PlaceableTransaction{Add=false,Amount = transaction.Amount,Placeable = transaction.Placeable,TransactionType = PlaceableTransactionType.Maintenance});
                        if (Logbook.Count > MaxLogbookLength)
                            Logbook.RemoveLast();
                        break;
                }
            }
            Road road= new Road();
            Logbook.AddFirst(new PlaceableTransaction{Add=false,Amount = roadMaintenance,Placeable = road,TransactionType = PlaceableTransactionType.Maintenance});
            if (Logbook.Count > MaxLogbookLength)
                Logbook.RemoveLast();
            Forest forest = new Forest();
            Logbook.AddFirst(new PlaceableTransaction{Add=false,Amount = forestMaintenance,Placeable = forest,TransactionType = PlaceableTransactionType.Maintenance});
            if (Logbook.Count > MaxLogbookLength)
                Logbook.RemoveLast();
            Pole pole = new Pole();
            Logbook.AddFirst(new PlaceableTransaction{Add=false,Amount = poleMaintenance,Placeable = pole,TransactionType = PlaceableTransactionType.Maintenance});
            if (Logbook.Count > MaxLogbookLength)
                Logbook.RemoveLast();
        }
        
        /// <summary>
        /// Commit a transaction to the budget
        /// Check if the budget is negative
        /// </summary>
        /// <param name="transaction">The transaction to commit</param>
        /// <returns>The committed transaction</returns>
        public ITransaction CommitTransaction(ITransaction transaction)
        {
            int newBudget = 0;
            newBudget = transaction.Add ? Convert.ToInt32(Budget + transaction.Amount) : Convert.ToInt32(Budget - transaction.Amount);
            

            NegativeBudgetYears = newBudget switch
            {
                <= 0 when Budget > 0 => 1,
                > 0 when Budget <= 0 => 0,
                _ => NegativeBudgetYears
            };

            Budget = newBudget;
            return transaction;
        }

        /// <summary>
        /// Collect taxes from all citizens and workplaces
        /// Then commit the transactions to the budget
        /// Then add the transactions to the logbook
        /// </summary>
        /// <param name="residentialZones"> List of residential zones</param>
        /// <param name="workplaceZones">List of workplace zones</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void CollectTax(List<ResidentialZone> residentialZones, List<WorkplaceZone> workplaceZones)
        {
            var allTransactions = new List<ITransaction>();
            foreach (var residentialZone in residentialZones)
            {
                foreach (var citizen in residentialZone.Citizens)
                {
                    if (!citizen.Jobless)
                    {
                        allTransactions.Add(CommitTransaction(
                            Transactions.ResidentialTaxCollection(TaxType.Residental, _taxes.ResidentialTax)));
                    }
                }
            }

            foreach (var workplaceZone in workplaceZones)
            {
                double multiplier = 1;
                if (workplaceZone.IsElectrified == false) multiplier = WorkplaceTaxElectricityFactor;
                allTransactions.Add(CommitTransaction(workplaceZone switch
                {
                    IndustrialZone => Transactions.WorkplaceTaxCollection(TaxType.Industrial, _taxes.IndustrialTax * multiplier,
                        workplaceZone.Count),
                    CommercialZone => Transactions.WorkplaceTaxCollection(TaxType.Commercial, _taxes.CommercialTax * multiplier,
                        workplaceZone.Count),
                    _ => throw new ArgumentOutOfRangeException()
                }));
            }

            AddTaxToLogbook(allTransactions);
        }

        /// <summary>
        /// Change the tax of a given type by a given amount
        /// </summary>
        /// <param name="taxType"> The type of tax to change</param>
        /// <param name="amount"> The amount to change the tax by</param>
        /// <returns>True if the tax was changed, false if the tax would be out of bounds</returns>
        public bool ChangeTax(TaxType taxType, double amount)
        {
            double changedTax;

            switch (taxType)
            {
                case TaxType.Residental:
                    changedTax = Math.Round(_taxes.ResidentialTax + amount, 2);
                    
                    if (changedTax < MinResTax || changedTax > MaxResTax)
                        return false;
                    
                    _taxes.ResidentialTax = changedTax;
                    break;
                case TaxType.Commercial:
                    changedTax = Math.Round(_taxes.CommercialTax + amount, 2);
                    
                    if (changedTax < MinComTax || changedTax > MaxComTax)
                        return false;
                    
                    _taxes.CommercialTax = changedTax;
                    break;
                case TaxType.Industrial:
                    changedTax = Math.Round(_taxes.IndustrialTax + amount, 2);
                    
                    if (changedTax < MinIndTax || changedTax > MaxIndTax)
                        return false;
                    
                    _taxes.IndustrialTax = changedTax;
                    break;
                default:
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Update the satisfaction of all citizens
        /// </summary>
        /// <param name="zones"> List of all zones</param>
        /// <param name="commercialZoneCount"> Number of commercial zones</param>
        /// <param name="industrialZoneCount"> Number of industrial zones</param>
        public void UpdateSatisfaction(IEnumerable<Zone> zones, int commercialZoneCount, int industrialZoneCount)
        {
            CommercialZoneCount = commercialZoneCount;
            IndustrialZoneCount = industrialZoneCount;

            UpdateSatisfaction(zones.SelectMany(zone => zone.Citizens));
        }
        
        /// <summary>
        /// Update the satisfaction of all citizens
        /// </summary>
        /// <param name="citizens"> List of all citizens</param>
        public void UpdateSatisfaction(IEnumerable<Citizen> citizens)
        {
            if (Population <= 0)
                return;

            var citizenSatisfactionSum = AverageCitizenFactors * Population;

            foreach (var citizen in citizens)
            {
                citizenSatisfactionSum -= citizen.LastCalculatedSatisfaction;
                CalculateSatisfaction(citizen);
                citizenSatisfactionSum += citizen.LastCalculatedSatisfaction;
            }
            
            AverageCitizenFactors = citizenSatisfactionSum / Population;
        }
        
        /// <summary>
        /// Update the satisfaction of all citizens if they moved in or out
        /// </summary>
        /// <param name="movedIn"> True if the citizens moved in, false if they moved out</param>
        /// <param name="changes"> List of citizens that moved in or out</param>
        /// <param name="citizens"> List of all citizens</param>
        public void UpdateSatisfaction(bool movedIn, List<Citizen> changes, List<Citizen> citizens)
        {
            if (citizens.Count == 0)
            {
                Population = 0;
                return;
            }

            var citizenSatisfactionSum = AverageCitizenFactors * Population;
            var oldSafetyRatio = SafetyRatioForPopulation(Population);
            
            Population = citizens.Count;
            
            if (oldSafetyRatio < SafetyRatio || oldSafetyRatio > SafetyRatio)
            {
                citizenSatisfactionSum = 0;
                
                foreach (var citizen in citizens)
                {
                    CalculateSatisfaction(citizen);
                    citizenSatisfactionSum += citizen.LastCalculatedSatisfaction;
                }
            }
            else
            {
                foreach (var citizen in changes)
                {
                    if (movedIn)
                        CalculateSatisfaction(citizen);
                
                    citizenSatisfactionSum += (movedIn ? 1 : -1) * citizen.LastCalculatedSatisfaction;
                }
            }

            AverageCitizenFactors = citizenSatisfactionSum / Population;
        }
        
        /// <summary>
        /// Pass a year and check if the budget is negative
        /// </summary>
        public void PassYear()
        {
            if (Budget <= 0)
                NegativeBudgetYears++;
        }

        /// <summary>
        /// Calculate the total satisfaction of a citizen
        /// </summary>
        /// <param name="zone"> The zone the citizen is in</param>
        /// <returns></returns>
        public double CalculateSatisfaction(Zone zone) => zone.Empty switch
        {
            true => 0,
            _ => TotalSatisfactionForCitizenAverage(zone.Citizens.Sum(citizen => citizen.LastCalculatedSatisfaction) / zone.Count)
        };
        
        #endregion

        #region Private methods

        private void CalculateSatisfaction(Citizen citizen)
        {
            var homeFactors = citizen.Home switch
            {
                { Owner: { } homeField } =>       SafetyRatio * homeField.PoliceDepartmentEffect +
                                            (1 - SafetyRatio) * ((HomePollutionRatio * (1 - homeField.IndustrialEffect + homeField.ForestEffect) +
                                                                    HomeStadiumRatio * homeField.StadiumEffect + 
                                                                HomeElectricityRatio * (citizen.Home.IsElectrified ? 1 : 0))
                                                                   /
                                                                   (HomePollutionRatio + HomeStadiumRatio + HomeElectricityRatio)),
                _ => 0
            };

            var workplaceFactors = citizen.Workplace switch
            {
                { Owner: { } workplaceField } =>       SafetyRatio * workplaceField.PoliceDepartmentEffect + 
                                                 (1 - SafetyRatio) * (WorkplaceStadiumRatio * workplaceField.StadiumEffect)
                                                                      /
                                                                      WorkplaceStadiumRatio,
                _ => 0
            };

            citizen.LastCalculatedSatisfaction = (HomeRatio * homeFactors + 
                                                  WorkplaceRatio * workplaceFactors + 
                                                  DistanceRatio * citizen.HomeWorkplaceDistanceEffect)
                                                 /
                                                 (HomeRatio + WorkplaceRatio + DistanceRatio);
        }

        private double TotalSatisfactionForCitizenAverage(double citizenAverage) =>
            (1 - NegativeBudgetRatio) * (CitizenAverageRatio * citizenAverage +
                                         GlobalRatio * GlobalFactors)
                                        /
                                        (CitizenAverageRatio + GlobalRatio);

        private static double SafetyRatioForPopulation(int population) => MinSafetyRatio + 
                                                                          Math.Floor(Math.Min(population, MaxSafetyRatioPopulation) /
                                                                                     (MaxSafetyRatioPopulation / SafetyRatioParts)) * 
                                                                          ((MaxSafetyRatio - MinSafetyRatio) / SafetyRatioParts);

        #endregion

    }
}
