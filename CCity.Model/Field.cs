using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Field
    {
        #region Properties

        public Placeable? Placeable { get; internal set; }
        public int LastCalculatedSatisfaction { get; internal set; }
        public double ForestEffect { get; internal set; }
        public double FireDepartmentEffect { get; internal set; }
        public double PoliceDepartmentEffect { get; internal set; }

        #endregion

        #region Constructors

        public Field()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public methods

        public int CalculateSatisfaction()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
