namespace CCity.Model
{
    public class GlobalManager
    {
        #region Constants 
        
        private const int ResTaxNorm = 150;
        private const int ComTaxNorm = 500;
        private const int IndTaxNorm = 750;

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
        private const double MaxNegativeBudgetRatio = 0.25;
        private const double NegativeBudgetRatioParts = 4;

        private const double HomeRatio = 0.5;
        private const double WorkplaceRatio = 0.25;
        private const double DistanceRatio = 0.25;

        private const double HomePollutionRatio = 0.5;
        private const double HomeStadiumRatio = 0.5;
        
        private const double WorkplaceStadiumRatio = 1;
        
        private const double TaxRatio = 0.8;
        private const double IndustrialCommercialBalanceRatio = 0.2;
        
        #endregion

        #region Fields

        private Taxes _taxes;

        #endregion

        #region Properties

        public double TotalSatisfaction => Population > 0 ? (1 - NegativeBudgetRatio) * PositiveBudgetFactors : 0;

        public int Budget { get; private set; }

        public Taxes Taxes => _taxes;

        private double NegativeBudgetRatio => MinNegativeBudgetRatio + 
                                              NegativeBudgetYears * (MaxNegativeBudgetRatio - MinNegativeBudgetRatio) / 
                                              NegativeBudgetRatioParts;  

        private int NegativeBudgetYears { get; set; }

        private double PositiveBudgetFactors => (CitizenAverageRatio * AverageCitizenFactors +
                                                 GlobalRatio * GlobalFactors)
                                                /
                                                (CitizenAverageRatio + GlobalRatio);

        private double AverageCitizenFactors { get; set; }

        private double SafetyRatio => SafetyRatioForPopulation(Population);

        private double GlobalFactors => (TaxRatio * TaxFactors + 
                                         IndustrialCommercialBalanceRatio * IndustrialCommercialBalance) 
                                        /
                                        (TaxRatio + IndustrialCommercialBalanceRatio);

        private double TaxFactors => 1 - (ResTaxRatio * (Taxes.ResidentalTax - MinResTax) / (MaxResTax - MinResTax) +
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

        #endregion

        #region Constructors

        public GlobalManager()
        {
            Budget = StartingBudget;
            NegativeBudgetYears = 0;

            _taxes = new Taxes
            {
                ResidentalTax = 0.27,
                CommercialTax = 0.15,
                IndustrialTax = 0.05
            };

            Population = 0;
            CommercialZoneCount = 0;
            IndustrialZoneCount = 0;
        }

        #endregion

        #region Public methods

        public void Pay(int price)
        {
            var newBudget = Budget - price;

            NegativeBudgetYears = newBudget switch
            {
                <= 0 when Budget > 0 => 1,
                > 0 when Budget <= 0 => 0,
                _ => NegativeBudgetYears
            };

            Budget = newBudget;
        }
        
        public void CollectTax(List<ResidentialZone> residentialZones, List<WorkplaceZone> workplaceZones)
        {
            foreach (var residentialZone in residentialZones)
            {
                foreach (var citizen in residentialZone.Citizens)
                {
                    if (!citizen.Jobless)
                        Pay(-Convert.ToInt32(Math.Round(ResTaxNorm * _taxes.ResidentalTax)));
                }
            }
            
            foreach (var workplaceZone in workplaceZones)
                Pay(-(workplaceZone switch
                {
                    IndustrialZone => Convert.ToInt32(Math.Round(IndTaxNorm * _taxes.IndustrialTax * workplaceZone.Count)),
                    CommercialZone => Convert.ToInt32(Math.Round(ComTaxNorm * _taxes.CommercialTax * workplaceZone.Count)),
                    _ => 0,
                }));
        }
        
        public bool ChangeTax(TaxType taxType, double amount)
        {
            double changedTax;

            switch (taxType)
            {
                case TaxType.Residental:
                    changedTax = Math.Round(_taxes.ResidentalTax + amount, 2);
                    
                    if (changedTax < MinResTax || changedTax > MaxResTax)
                        return false;
                    
                    _taxes.ResidentalTax = changedTax;
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
        
        public void UpdateSatisfaction(IEnumerable<Zone> zones, int commercialZoneCount, int industrialZoneCount)
        {
            CommercialZoneCount = commercialZoneCount;
            IndustrialZoneCount = industrialZoneCount;

            UpdateSatisfaction(zones.SelectMany(zone => zone.Citizens));
        }
        
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

        public void PassYear()
        {
            if (Budget <= 0)
                NegativeBudgetYears++;
        }
        
        #endregion

        #region Private methods

        private void CalculateSatisfaction(Citizen citizen)
        {
            var homeFactors = citizen.Home switch
            {
                { Owner: { } homeField } =>       SafetyRatio * homeField.PoliceDepartmentEffect +
                                            (1 - SafetyRatio) * ((HomePollutionRatio * (1 - homeField.IndustrialEffect) +
                                                                    HomeStadiumRatio * homeField.StadiumEffect)
                                                                   /
                                                                   (HomePollutionRatio + HomeStadiumRatio)),
                _ => 0
            };

            var workplaceFactors = citizen.Workplace switch
            {
                { Owner: { } workplaceField } =>       SafetyRatio * workplaceField.PoliceDepartmentEffect + 
                                                 (1 - SafetyRatio) * ((WorkplaceStadiumRatio * workplaceField.StadiumEffect)
                                                                      /
                                                                      (WorkplaceStadiumRatio)),
                _ => 0
            };

            citizen.LastCalculatedSatisfaction = (HomeRatio * homeFactors + 
                                                  WorkplaceRatio * workplaceFactors + 
                                                  DistanceRatio * citizen.HomeWorkplaceDistanceEffect)
                                                 /
                                                 (HomeRatio + WorkplaceRatio + DistanceRatio);
        }

        private static double SafetyRatioForPopulation(int population) => MinSafetyRatio + 
                                                                          Math.Floor(Math.Min(population, MaxSafetyRatioPopulation) /
                                                                              (MaxSafetyRatioPopulation / SafetyRatioParts)) * 
                                                                          ((MaxSafetyRatio - MinSafetyRatio) / SafetyRatioParts);

        #endregion

    }
}
