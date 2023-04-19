namespace CCity.Model
{
    public class CitizenManager
    {
        #region Constants

        private const double CitizenMoveOutThreshold = 0.25;

        #endregion

        #region Properties

        public List<Citizen> Citizens { get; }
        
        public int Population => Citizens.Count;

        //private List<Citizen> JoblessCitizens { get; }
        
        private bool NextWorkplaceIsCommercial { get; set; }
        
        #endregion

        #region Constructors

        public CitizenManager()
        {
            Citizens = new List<Citizen>();
            // JoblessCitizens = new List<Citizen>();
            
            NextWorkplaceIsCommercial = true;
        }

        #endregion

        #region Public methods

        public List<Citizen> IncreasePopulation(List<ResidentialZone> vacantHomes, List<WorkplaceZone> vacantCommercialZones, List<WorkplaceZone> vacantIndustrialZones)
        {
            var result = new List<Citizen>();
            WorkplaceZone? nextWorkplace = null;

            // For now, the method will take all the vacant homes and put some citizens in them
            // It might not fill up a home entirely -- there might still be empty slots left in a home or a workplace after this algorithm is done
            // TODO: Improve algorithm? - Select some homes randomly and fill those up
            // TODO: Implement some sort of updating of these vacant homes in FieldManager
            foreach (var home in vacantHomes)
            {
                var freeSlots = home.Capacity - home.Count;
                var newCitizenCount = freeSlots > 2 
                    ? new Random(DateTime.Now.Millisecond).Next(3, Math.Min(6, freeSlots)) 
                    : freeSlots;

                for (var i = 0; i < newCitizenCount; i++)
                {
                    nextWorkplace = NextWorkplace(home, vacantCommercialZones, vacantIndustrialZones);
                    
                    if (nextWorkplace == null)
                        break;

                    var citizen = new Citizen(home, nextWorkplace);
                    
                    Citizens.Add(citizen);
                    result.Add(citizen);
                    
                    if (nextWorkplace.Full)
                        (NextWorkplaceIsCommercial ? vacantCommercialZones : vacantIndustrialZones).Remove(nextWorkplace);
                    
                    NextWorkplaceIsCommercial = !NextWorkplaceIsCommercial;
                }
                
                if (nextWorkplace == null)
                    break;
            }

            return result;
        }

        public List<Citizen> DecreasePopulation()
        {
            var result = new List<Citizen>();

            foreach (var citizen in Citizens)
            {
                if (citizen.LastCalculatedSatisfaction < CitizenMoveOutThreshold)
                {
                    citizen.MoveOut();
                    
                    result.Add(citizen);
                    Citizens.Remove(citizen);
                }
            }

            return result;
        }

        public List<Citizen> OptimizeWorkplaces(List<WorkplaceZone> vacantCommercialZones, List<WorkplaceZone> vacantIndustrialZones)
        {
            var result = new List<Citizen>();
            
            /*foreach (var citizen in JoblessCitizens)
            {
                var nextWorkplace = NextWorkplace(citizen.Home, vacantCommercialZones, vacantIndustrialZones);
                
                if (nextWorkplace == null)
                    break;
                
                citizen.ChangeWorkplace(nextWorkplace);
                result.Add(citizen);
                
                if (nextWorkplace.Full)
                    (NextWorkplaceIsCommercial ? vacantCommercialZones : vacantIndustrialZones).Remove(nextWorkplace);

                NextWorkplaceIsCommercial = !NextWorkplaceIsCommercial;
            }*/

            return result;
        }

        #endregion

        #region Private methods

        private WorkplaceZone? NextWorkplace(Placeable p, List<WorkplaceZone> vacantCommercialZones, List<WorkplaceZone> vacantIndustrialZones)
        { 
            var result = NearestWorkplace(p, NextWorkplaceIsCommercial ? vacantCommercialZones : vacantIndustrialZones);

            if (result == null)
            {
                result = NearestWorkplace(p, !NextWorkplaceIsCommercial ? vacantCommercialZones : vacantIndustrialZones);
                NextWorkplaceIsCommercial = !NextWorkplaceIsCommercial;
            }

            return result;
        }
        
        private static WorkplaceZone? NearestWorkplace(Placeable p, List<WorkplaceZone> vacantWorkplaces)
        {
            var nearestWorkplace = vacantWorkplaces.FirstOrDefault();
            var smallestDistance = Utilities.AbsoluteDistance(p, nearestWorkplace);
            
            foreach (var workplace in vacantWorkplaces)
            {
                var currentDistance = Utilities.AbsoluteDistance(p, workplace);

                if (currentDistance < smallestDistance)
                    (nearestWorkplace, smallestDistance) = (workplace, currentDistance);
            }

            return nearestWorkplace;
        }

        #endregion
    }
}