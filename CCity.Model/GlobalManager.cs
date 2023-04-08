using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class GlobalManager
    {

        #region Fields

        public int GlobalSatisfactionScore;
        public int Budget;
        public Taxes Taxes { get; }
        private Taxes _taxes;

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

        public bool ChangeTax(TaxType taxtype, int amount)
        {
            switch (taxtype)
            {
                case TaxType.Residental:
                    _taxes.ResidentalTax = amount;
                    break;
                case TaxType.Commercial:
                    _taxes.CommercialTax = amount;
                    break;
                case TaxType.Industrial:
                    _taxes.IndustrialTax = amount;
                    break;
                default:
                    return false;
            }
            return true;
        }
        public void UpdateSatisfaction(List<Field> fields)
        {
            // TODO: Update this based on issue #27
            foreach (Field field in fields)
            {
                GlobalSatisfactionScore -= field.LastCalculatedSatisfaction;
                field.CalculateSatisfaction();
                GlobalSatisfactionScore += field.LastCalculatedSatisfaction;
            }
        }
        
        public void UpdateSatisfaction(List<Citizen> changes)
        {
            foreach (var citizen in changes)
            {
                GlobalSatisfactionScore -= citizen.LastCalculatedSatisfaction;
                CalculateSatisfaction(citizen);
                GlobalSatisfactionScore += citizen.LastCalculatedSatisfaction;
            }
        }
        
        public void UpdateSatisfaction(bool movedIn, List<Citizen> changes)
        {
            throw new NotImplementedException();
        }


        #endregion

        #region Private methods

        private static void CalculateSatisfaction(Citizen citizen)
        {
            
        }

        #endregion

    }
}
