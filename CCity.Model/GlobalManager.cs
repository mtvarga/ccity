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

        private const double MinSafetyRatio = 0.25;
        private const double MaxSafetyRatio = 0.5;
        private const double SafetyRatioParts = 10;
        private const double MaxSafetyRatioPopulation = 1000;
        
        private const double MinNegativeBudgetRatio = 0;
        private const double MaxNegativeBudgetRatio = 0.25;
        private const double NegativeBudgetRatioParts = 4;

        #endregion

        #region Fields

        private Taxes _taxes;

        private double _safetyRatio;
        private double _negativeBudgetRatio;

        private double _citizenAverageSatisfactionFactors;
        private double _normalSatisfactionFactors;

        private int _commercialZoneCount;
        private int _industrialZoneCount;
        private int _population;

        #endregion

        #region Properties

        public double TotalSatisfaction { get; private set; }

        public int Budget { get; private set; }

        public Taxes Taxes => _taxes;

        private double GlobalSatisfactionFactors => (double)2 / 3 * TaxFactors + (double)1 / 3 * IndustrialCommercialBalance;
        
        private double TaxFactors =>
            1.3 - (double) 2 / 3 * (Taxes.ResidentalTax + 2 * Taxes.CommercialTax + 2 * Taxes.IndustrialTax);

        private double IndustrialCommercialBalance => 1 - (Math.Abs(_commercialZoneCount - _industrialZoneCount) / _commercialZoneCount + _industrialZoneCount);
        
        #endregion

        #region Constructors

        public GlobalManager()
        {
            TotalSatisfaction = 0;

            _negativeBudgetRatio = MinNegativeBudgetRatio;
            _safetyRatio = MinSafetyRatio;

            Budget = StartingBudget;
            
            _taxes = new Taxes
            {
                ResidentalTax = 0.27,
                CommercialTax = 0.15,
                IndustrialTax = 0.05
            };

            _population = 0;
            _commercialZoneCount = 0;
            _industrialZoneCount = 0;
        }

        #endregion

        #region Public methods

        public void Pay(int price)
        {
            var newBudget = Budget - price;
            
            switch (newBudget)
            {
                case <= 0 when Budget > 0:
                    IncreaseNegativeBudgetRatio();
                    break;
                case > 0 when Budget <= 0:
                    ResetNegativeBudgetRatio();
                    break;
            }

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
            var changedTax=0.0;
            switch (taxType)
            {
                case TaxType.Residental:
                    changedTax=_taxes.ResidentalTax+ amount;
                    if(changedTax <= MinResTax || changedTax >= MaxResTax)
                        return false;
                    _taxes.ResidentalTax = changedTax;
                    break;
                case TaxType.Commercial:
                    changedTax=_taxes.CommercialTax+ amount;
                    if (changedTax <= MinComTax || changedTax >= MaxComTax)
                        return false;
                    _taxes.CommercialTax = changedTax;
                    break;
                case TaxType.Industrial:
                    changedTax=_taxes.IndustrialTax+ amount;
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
            _commercialZoneCount = commercialZoneCount;
            _industrialZoneCount = industrialZoneCount;

            if (_population <= 0)
                return;
            
            UpdateSatisfaction(zones.SelectMany(zone => zone.Citizens));
        }
        
        public void UpdateSatisfaction(IEnumerable<Citizen> citizens)
        {
            var citizenSatisfactionSum = _citizenAverageSatisfactionFactors * _population;

            foreach (var citizen in citizens)
            {
                citizenSatisfactionSum -= citizen.LastCalculatedSatisfaction;
                CalculateSatisfaction(citizen);
                citizenSatisfactionSum += citizen.LastCalculatedSatisfaction;
            }
            
            _citizenAverageSatisfactionFactors = citizenSatisfactionSum / _population;
            RecalculateTotalSatisfaction();
        }
        
        public void UpdateSatisfaction(bool movedIn, List<Citizen> changes, List<Citizen> citizens)
        {
            if (citizens.Count == 0)
            {
                _population = 0;
                TotalSatisfaction = 0;
                return;
            }

            var citizenSatisfactionSum = 
                _citizenAverageSatisfactionFactors * _population;
            var newSafetyRatio = 
                Math.Floor(Math.Min(citizens.Count, MaxSafetyRatioPopulation) / (MaxSafetyRatioPopulation / SafetyRatioParts)) *
                ((MaxSafetyRatio - MinSafetyRatio) / SafetyRatioParts);
            
            _population = citizens.Count;
            
            if (newSafetyRatio < _safetyRatio || newSafetyRatio > _safetyRatio)
            {
                _safetyRatio = newSafetyRatio;
                
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

            _citizenAverageSatisfactionFactors = citizenSatisfactionSum / _population;
            RecalculateTotalSatisfaction();
        }

        public void PassYear()
        {
            if (Budget <= 0)
                IncreaseNegativeBudgetRatio();
        }
        
        #endregion

        #region Private methods

        private void RecalculateTotalSatisfaction()
        {
            _normalSatisfactionFactors = (double)2 / 3 * _citizenAverageSatisfactionFactors + (double)1 / 3 * GlobalSatisfactionFactors;
            TotalSatisfaction = _negativeBudgetRatio * _normalSatisfactionFactors;
        }

        private void CalculateSatisfaction(Citizen citizen)
        {
            var homeFactors = 
                _safetyRatio       * citizen.Home.Owner.PoliceDepartmentEffect + 
                (1 - _safetyRatio) * (0.75 * (1 - citizen.Home.Owner.IndustrialEffect) + 
                                                0.25 * citizen.Home.Owner.StadiumEffect);

            var workplaceFactors = 
                _safetyRatio       * citizen.Workplace.Owner.PoliceDepartmentEffect +
                (1 - _safetyRatio) * citizen.Workplace.Owner.StadiumEffect;

            citizen.LastCalculatedSatisfaction = 0.5 * homeFactors + 0.25 * workplaceFactors + 0.25 * citizen.HomeWorkplaceDistanceEffect;
        }
        
        private void IncreaseNegativeBudgetRatio()
        {
            _negativeBudgetRatio += MaxNegativeBudgetRatio / NegativeBudgetRatioParts;
            
            RecalculateTotalSatisfaction();
        }

        private void ResetNegativeBudgetRatio()
        {
            _negativeBudgetRatio = 0;
            
            RecalculateTotalSatisfaction();
        }

        #endregion

    }
}
