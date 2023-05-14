using Priority_Queue;

namespace CCity.Model
{
    public class Field
    {
        #region Constants
        internal const int MAX_POLICE_DEPARTMENT_EFFECT = 10;
        internal const int MAX_STADIUM_EFFECT = 10;
        internal const int MAX_FIRE_DEPARTMENT_EFFECT = 10;
        internal const int MAX_FOREST_EFFECT = 20;
        internal const int MAX_INDUSTRIAL_EFFECT = 30;
        #endregion

        #region Fields

        public Placeable? Placeable { get => _placeable == null ? null : _placeable.Root; }
        public Placeable? ActualPlaceable { get => _placeable; }

        private Placeable? _placeable;
        private int _policeDepartmentEffect;
        private int _stadiumEffect;
        private int _fireDepartmentEffect;
        private int _forestEffect;
        private int _industrialEffect;

        public int X { get; }
        public int Y { get; }
        public bool HasPlaceable { get => Placeable != null; }
        public double PoliceDepartmentEffect => Math.Min(_policeDepartmentEffect, MAX_POLICE_DEPARTMENT_EFFECT) / (double)MAX_POLICE_DEPARTMENT_EFFECT;
        public double StadiumEffect => Math.Min(_stadiumEffect, MAX_STADIUM_EFFECT) / (double)MAX_STADIUM_EFFECT;
        public double FireDepartmentEffect => Math.Min(_fireDepartmentEffect, MAX_FIRE_DEPARTMENT_EFFECT) / (double)MAX_FIRE_DEPARTMENT_EFFECT;
        public double ForestEffect => Math.Min(_forestEffect, MAX_FOREST_EFFECT) / (double)MAX_FOREST_EFFECT;
        public double IndustrialEffect => Math.Max(Math.Min(_industrialEffect, MAX_INDUSTRIAL_EFFECT) / (double)MAX_INDUSTRIAL_EFFECT,0);

        #endregion

        #region Constructors

        public Field(int x, int y)
        {
            _placeable = null;
            X = x;
            Y = y;
            _policeDepartmentEffect = 0;
            _stadiumEffect = 0;
            _fireDepartmentEffect = 0;
            _forestEffect = 0;
            _industrialEffect = 0;
        }

        #endregion

        #region Public methods}

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

        public void ChangeIndustrialEffect(int n)
        {
            _industrialEffect += n;
        }

        /*public bool Has(Type type)
        {
            if (Placeable == null) return false;
            return Placeable.GetType() == type;
        }*/

        public void RefreshPublicity()
        {
            if (_placeable != null)
            {
                //Placeable.IsPublic = true;
            }
        }

        internal bool Place(Placeable placeable)
        {
            if (_placeable != null) return false;
            _placeable = placeable;
            placeable.PlaceAt(this);
            return true;
        }

        internal bool Demolish()
        {
            if (_placeable == null) return false;
            _placeable = null;
            return true;
        }

        #endregion

    }
}
