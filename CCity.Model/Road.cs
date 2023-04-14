using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Road : Placeable
    {

        #region Fileds

        public Road? GetPublicityFrom; 
        public List<Road> GivesPublicityTo;

        #endregion

        #region Properties

        public override int PlacementCost => throw new NotImplementedException();

        public override int MaintenanceCost => throw new NotImplementedException();

        public bool IsPublic { get { return GetPublicityFrom != null; } }

        #endregion

        #region Constructors

        public Road()
        {
            GetPublicityFrom = null;
            GivesPublicityTo = new List<Field>();
        }

        #endregion
    }
}
