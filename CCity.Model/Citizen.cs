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

        public Citizen(ResidentialZone home, WorkplaceZone? workplace)
        {
            Home = home;
            Home.AddCitizen(this);
            Workplace = workplace;
            Workplace?.AddCitizen(this);
            
            CalculateHomeWorkplaceDistanceEffect();
        }

        #endregion

        #region Public methods
        
        public void SwapWorkplace(WorkplaceZone workplace)
        {
            Workplace?.DropCitizen(this);
            Workplace = workplace;
            Workplace.AddCitizen(this);

            CalculateHomeWorkplaceDistanceEffect();
        }

        public void MoveOut()
        {
            Home.DropCitizen(this);
            Workplace?.DropCitizen(this);
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
