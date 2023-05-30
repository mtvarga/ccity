namespace CCity.Model
{
    public class Citizen
    {
        #region Constants

        private const int CloseProximityRadius = 10;
        
        #endregion
        
        #region Properties
        
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
        
        /// <summary>
        /// Changes Citizen's workplace.
        /// Drops Citizen from their current workplace and adds them to the new one.
        /// </summary>
        /// <param name="workplace">The new workplace.</param>
        /// <exception cref="Exception"></exception>
        public void ChangeWorkplace(WorkplaceZone? workplace)
        {
            if (Workplace != null && workplace == null)
                throw new Exception("Illegal operation: Attempted to set Citizen's workplace to null while they already have a workplace.");
            
            Workplace?.DropCitizen(this);
            Workplace = workplace;
            
            if (!(Workplace?.AddCitizen(this) ?? true))
                throw new Exception("Couldn't change Citizen's workplace.");

            CalculateHomeWorkplaceDistanceEffect();
        }

        /// <summary>
        /// Moves Citizen out of the city.
        /// Drops Citizen from their Home and Workplace.
        /// </summary>
        /// <exception cref="Exception"></exception>
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
