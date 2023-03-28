using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Field
    {
        #region Fields

        public Placeable Placeable { get; internal set; }
        
        //to int
        public double LastCalculatedSatisfaction { get; internal set; }

        private int _policeDepartmentEffect;
        private int _stadiumEffect;
        private int _fireDepartmentEffect;
        private int _forestEffect;

        public double PoliceDepartmentEffect { get; internal set; }
        public double StadiumEffect { get; internal set; }
        public double FireDepartmentEffect { get; internal set; }
        public double ForestEffect { get; internal set; }

        #endregion

        #region Constructors

        public Field()
        {
            Placeable = null;
            LastCalculatedSatisfaction = 0;
            PoliceDepartmentEffect = 0;
            StadiumEffect = 0;
            FireDepartmentEffect = 0;
            ForestEffect = 0;
        }

        #endregion

        #region Public methods

        public int CalculateSatisfaction()
        {
            if (Placeable == null || !(Placeable is Zone))
            {
                LastCalculatedSatisfaction = 0;
                return 0;
            }

            Zone zone = Placeable as Zone;
            double tempSatisfaction = 0;
            foreach(Citizen citizen in zone.Citizens)
            {
                tempSatisfaction += citizen.Satisfaction;
            }

            LastCalculatedSatisfaction = tempSatisfaction;
            return 0;
        }

        #endregion

    }
}
