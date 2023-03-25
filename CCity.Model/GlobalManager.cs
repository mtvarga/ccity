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
            throw new NotImplementedException();
        }
        public void UpdateSatisfaction(List<Field> filelds)
        {
            throw new NotImplementedException();
        }
        public void UpdateSatisfaction(Dictionary<WorkplaceZone, Citizen> changes)
        {
            throw new NotImplementedException();
        }
        public void UpdateSatisfaction(bool movedIn, List<Citizen> changes)
        {
            throw new NotImplementedException();
        }


        #endregion

        #region Private methods



        #endregion
    }
}
