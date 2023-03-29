using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Field
    {
        #region
        private const int MAX_EFFECT = 10;
        #endregion
        #region Fields

        public Placeable Placeable { get; internal set; }
        
        //to int
        public int LastCalculatedSatisfaction { get; private set; }

        private int _policeDepartmentEffect;
        private int _stadiumEffect;
        private int _fireDepartmentEffect;
        private int _forestEffect;

        public double PoliceDepartmentEffect { get => Math.Min(_policeDepartmentEffect, MAX_EFFECT) / MAX_EFFECT; }
        public double StadiumEffect { get; }
        public double FireDepartmentEffect { get; }
        public double ForestEffect { get; }

        #endregion

        #region Constructors

        public Field()
        {
            Placeable = null;
            LastCalculatedSatisfaction = 0;
            _policeDepartmentEffect = 0;
            StadiumEffect = 0;
            FireDepartmentEffect = 0;
            ForestEffect = 0;
        }

        #endregion

        #region Public methods

        public int CalculateSatisfaction()
        {
            if (Placeable == null)
            {
                return 0;
            }
            LastCalculatedSatisfaction = Placeable.CalculateSatisfaction();
            return LastCalculatedSatisfaction;
        }

        public void ChangePoliceDepartmentEffect(int n)
        {
            throw new NotImplementedException();
        }

        public void ChangeStadiumEffect(int n)
        {
            throw new NotImplementedException();
        }

        public void ChangeFireDepartmentEffect(int n)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
