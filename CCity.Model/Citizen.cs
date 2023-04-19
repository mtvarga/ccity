namespace CCity.Model
{
    public class Citizen
    {
        #region Constants

        private const int CloseProximityRadius = 10;
        
        #endregion
        
        #region Properties

        public string Name { get; }
        
        public ResidentialZone Home { get; }
        
        public WorkplaceZone? Workplace { get; private set; }
        
        public double HomeWorkplaceDistanceEffect { get; private set; }
        
        public double LastCalculatedSatisfaction { get; internal set; }

        public bool Jobless => Workplace == null;
        
        #endregion

        #region Constructors

        public Citizen(ResidentialZone home, WorkplaceZone? workplace = null)
        {
            if (!home.AddCitizen(this))
                throw new Exception("Couldn't add Citizen to ResidentialZone.");

            Home = home;
            ChangeWorkplace(workplace);
        }

        #endregion

        #region Public methods
        
        public void ChangeWorkplace(WorkplaceZone? workplace)
        {
            if (Workplace != null && workplace == null)
                throw new Exception("Illegal operation: Attempted to set Citizen's workplace to null while they already have a workplace.");
            
            Workplace?.DropCitizen(this);

            if (!(Workplace?.AddCitizen(this) ?? true))
                throw new Exception("Couldn't change Citizen's workplace.");
            
            Workplace = workplace;
            CalculateHomeWorkplaceDistanceEffect();
        }

        public void MoveOut()
        {
            if (!Home.DropCitizen(this) || !(Workplace?.DropCitizen(this) ?? true))
                throw new Exception("Couldn't move out Citizen.");
        }
        
        #endregion

        #region Private methods

        private void CalculateHomeWorkplaceDistanceEffect() => HomeWorkplaceDistanceEffect = Workplace switch
        {
            null => 0,
            _ => Utilities.GetPointsInRadiusWeighted(Home.Owner!, CloseProximityRadius)
                .Where(tuple => tuple.X == Workplace.Owner!.X && tuple.Y == Workplace.Owner!.Y)
                .Select(tuple => tuple.Weight).FirstOrDefault()
        };

        #endregion

    }
}
