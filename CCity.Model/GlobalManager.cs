using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace CCity.Model
{
    public class GlobalManager
    {

        #region Fields

        public int GlobalSatisfactionScore;
        public int Budget;
        public Taxes Taxes { get; }
        private Taxes _taxes;
        private const int ResTaxNorm = 1500;
        private const int ComTaxNorm = 5000;
        private const int IndTaxNorm = 7500;

        #endregion

        #region Properties



        #endregion

        #region Constructors

        public GlobalManager()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public methods

        public bool ChangeTax(TaxType taxtype, double amount)
        {
            switch (taxtype)
            {
                case TaxType.Residental:
                    _taxes.ResidentalTax += amount;
                    break;
                case TaxType.Commercial:
                    _taxes.CommercialTax += amount;
                    break;
                case TaxType.Industrial:
                    _taxes.IndustrialTax += amount;
                    break;
                default:
                    return false;
            }
            return true;
        }
        public void UpdateSatisfaction(List<Zone> zones)
        {
            // TODO: Update this based on issue #27
            foreach (var zone in zones)
            {
                foreach (var citizen in zone.Citizens)
                {
                    GlobalSatisfactionScore -= citizen.LastCalculatedSatisfaction;
                    CalculateSatisfaction(citizen);
                    GlobalSatisfactionScore += citizen.LastCalculatedSatisfaction;
                }
            }
        }
        
        public void UpdateSatisfaction(List<Citizen> citizens)
        {
            foreach (var citizen in citizens)
            {
                GlobalSatisfactionScore -= citizen.LastCalculatedSatisfaction;
                CalculateSatisfaction(citizen);
                GlobalSatisfactionScore += citizen.LastCalculatedSatisfaction;
            }
        }
        
        public void UpdateSatisfaction(bool movedIn, List<Citizen> citizens)
        {
            foreach (var citizen in citizens)
            {
                if (movedIn)
                    CalculateSatisfaction(citizen);
                
                GlobalSatisfactionScore += (movedIn ? 1 : -1) * citizen.LastCalculatedSatisfaction;
            }
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
        public void PayMonthlyMaintenance(List<Placeable> facilities)
        {
            foreach (var facility in facilities)
            {
                Budget -= facility.MaintenanceCost;
            }
        }
        
        public void PayYearlyMaintenance(List<Placeable> facilities)
        {
            foreach (var facility in facilities)
            {
                Budget -= facility.MaintenanceCost;
            }
        }
        #endregion

        #region Private methods

        private static void CalculateSatisfaction(Citizen citizen)
        {
        }

        #endregion

    }
}
