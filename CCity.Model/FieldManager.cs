using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class FieldManager
    {
        #region Constants

        

        #endregion

        #region Fields

        public Field[][] Fields;
        private Dictionary<Forest, int> _growingForests;
        private List<Field> _burningBuildings;

        #endregion

        #region Public methods


        public List<Field> Place(int x, int y, Placeable placeable)
        {
            throw new NotImplementedException();
        }

        public List<Field> Upgrade(int x, int y)
        {
            throw new NotImplementedException();
        }

        public bool Demolish(int x, int y)
        {
            throw new NotImplementedException();
        }

        public List<Field> GrowForests()
        {
            throw new NotImplementedException();
        }

        public Field RandomIncinerate()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private methods

        private List<Tuple<int, int, double>> GetCoordinatesInRadius(int x, int y, int r)
        {
            throw new NotImplementedException();
        }

        private bool OnMap(int x, int y)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
