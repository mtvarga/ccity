﻿using System;
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
        private const int res_tax_norm = 1500;
        private const int com_tax_norm = 5000;
        private const int ind_tax_norm = 7500;

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
            double res_tax = 0;
            double work_tax = 0;
            foreach (var residentalZone in residentialZones)
            {
                res_tax = Math.Floor(res_tax_norm * _taxes.ResidentalTax * residentalZone.Current);
            }
            foreach (var workplaceZone in workplaceZones)
            {
                work_tax = Math.Floor(com_tax_norm * _taxes.CommercialTax * workplaceZone.Current);
            }
            Budget += (int)res_tax + (int)work_tax;
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
