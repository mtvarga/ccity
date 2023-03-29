using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Field
    {
        #region Constants
        private const int MAX_POLICE_DEPARTMENT_EFFECT = 10;
        private const int MAX_STADIUM_EFFECT = 10;
        private const int MAX_FIRE_DEPARTMENT_EFFECT = 10;
        private const int MAX_FOREST_EFFECT = 10;
        #endregion

        #region Fields

        public Placeable? Placeable { get; internal set; }
        
        //to int
        public int LastCalculatedSatisfaction { get; private set; }

        private int _policeDepartmentEffect;
        private int _stadiumEffect;
        private int _fireDepartmentEffect;
        private int _forestEffect;

        public int X { get; }
        public int Y { get; }
        public bool HasPlaceable { get => Placeable != null; }
        public double PoliceDepartmentEffect { get => Math.Min(_policeDepartmentEffect, MAX_POLICE_DEPARTMENT_EFFECT) / MAX_POLICE_DEPARTMENT_EFFECT; }
        public double StadiumEffect { get => Math.Min(_stadiumEffect, MAX_STADIUM_EFFECT) / MAX_STADIUM_EFFECT; }
        public double FireDepartmentEffect { get => Math.Min(_fireDepartmentEffect, MAX_FIRE_DEPARTMENT_EFFECT) / MAX_FIRE_DEPARTMENT_EFFECT; }
        public double ForestEffect { get => Math.Min(_forestEffect, MAX_FOREST_EFFECT) / MAX_FOREST_EFFECT; }

        #endregion

        #region Constructors

        public Field(int x, int y)
        {
            Placeable = null;
            X = x;
            Y = y;
            LastCalculatedSatisfaction = 0;
            _policeDepartmentEffect = 0;
            _stadiumEffect = 0;
            _fireDepartmentEffect = 0;
            _forestEffect = 0;
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
            _policeDepartmentEffect += n;
        }

        public void ChangeStadiumEffect(int n)
        {
            _stadiumEffect += n;
        }

        public void ChangeFireDepartmentEffect(int n)
        {
            _fireDepartmentEffect += n;
        }

        public void ChangeForestEffect(int n)
        {
            _forestEffect += n;
        }

        #endregion

    }
}
