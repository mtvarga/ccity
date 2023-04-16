namespace CCity.Model
{
    public class GlobalManager
    {
        #region Constants 
        
        private const int ResTaxNorm = 1500;
        private const int ComTaxNorm = 5000;
        private const int IndTaxNorm = 7500;

        private const double MaxResTax = 0.5;
        private const double MaxComTax = 0.25;
        private const double MaxIndTax = 0.25;
        
        private const double MinResTax = 0.15;
        private const double MinComTax = 0.1;
        private const double MinIndTax = 0.5;
        
        private const int StartingBudget = 10000;

        private const double CitizenAverageRatio = (double)2 / 3;
        private const double GlobalRatio = (double)1 / 3;
        
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

        private const double HomePollutionRatio = 0.75;
        private const double HomeStadiumRatio = 0.25;
        
        private const double WorkplaceStadiumRatio = 1;
        
        private const double TaxRatio = (double)2 / 3;
        private const double IndustrialCommercialBalanceRatio = (double)1 / 3;
        
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

        private double TaxFactors => 1 -
                                     (Taxes.ResidentalTax - MinResTax) / 3 * (MaxResTax - MinResTax) +
                                     (Taxes.CommercialTax - MinComTax) / 3 * (MaxComTax - MinComTax) +
                                     (Taxes.IndustrialTax - MinIndTax) / 3 * (MaxIndTax - MinIndTax);

        private double IndustrialCommercialBalance => 1 - 
                                                      Math.Abs(CommercialZoneCount - IndustrialZoneCount) / 
                                                      CommercialZoneCount + IndustrialZoneCount;

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
                Budget += Convert.ToInt32(Math.Round(ResTaxNorm * _taxes.ResidentalTax * residentialZone.Current));
            }
            
            foreach (var workplaceZone in workplaceZones)
            {
                Budget += workplaceZone switch
                {
                    IndustrialZone => Convert.ToInt32(Math.Round(IndTaxNorm * _taxes.IndustrialTax * workplaceZone.Current)),
                    CommercialZone => Convert.ToInt32(Math.Round(ComTaxNorm * _taxes.CommercialTax * workplaceZone.Current)),
                    _ => 0,
                };
            }
        }
        
        public bool ChangeTax(TaxType taxType, double amount)
        {
            double changedTax;

            switch (taxType)
            {
                case TaxType.Residental:
                    changedTax = _taxes.ResidentalTax + amount;
                    
                    if (changedTax <= MinResTax || changedTax >= MaxResTax)
                        return false;
                    
                    _taxes.ResidentalTax = changedTax;
                    break;
                case TaxType.Commercial:
                    changedTax = _taxes.CommercialTax + amount;
                    
                    if (changedTax <= MinComTax || changedTax >= MaxComTax)
                        return false;
                    
                    _taxes.CommercialTax = changedTax;
                    break;
                case TaxType.Industrial:
                    changedTax = _taxes.IndustrialTax + amount;
                    
                    if (changedTax <= MinIndTax || changedTax >= MaxIndTax)
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

            if (Population <= 0)
                return;
            
            UpdateSatisfaction(zones.SelectMany(zone => zone.Citizens));
        }
        
        public void UpdateSatisfaction(IEnumerable<Citizen> citizens)
        {
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
            var homeFactors = SafetyRatio * citizen.Home.Owner.PoliceDepartmentEffect + 
                              (1 - SafetyRatio) * ((HomePollutionRatio * (1 - citizen.Home.Owner.IndustrialEffect) + 
                                                    HomeStadiumRatio * citizen.Home.Owner.StadiumEffect) 
                                                   / 
                                                   (HomePollutionRatio + HomeStadiumRatio));

            var workplaceFactors = SafetyRatio * citizen.Workplace.Owner.PoliceDepartmentEffect + 
                                   (1 - SafetyRatio) * ((WorkplaceStadiumRatio * citizen.Workplace.Owner.StadiumEffect)
                                                        /
                                                        (WorkplaceStadiumRatio));

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
