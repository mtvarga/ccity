using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Road : Placeable
    {

        #region Fields

        public Road? GetPublicityFrom; 
        public List<Road> GivesPublicityTo;

        #endregion

        #region Properties

        public override int PlacementCost => 100;

        public override int MaintenanceCost => 10;

        public new bool IsPublic { get { return GetPublicityFrom != null; } }

        #endregion

        #region Constructors

        public Road()
        {
            GetPublicityFrom = null;
            GivesPublicityTo = new List<Road>();
        }

        #endregion
    }
}
