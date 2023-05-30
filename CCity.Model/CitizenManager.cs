namespace CCity.Model
{
    public class CitizenManager
    {
        #region Constants

        private const double CitizenMoveOutThreshold = 0.3;
        private const int CloseProximityRadius = 10;
        private const double DesireToMoveInThreshold = 0.4;
        private const double DistanceEffectThreshold = 0.2;
        private const double MaxCitizenMoveInRate = 0.2;
        private const int MinPopulation = 15;

        #endregion

        #region Properties

        public List<Citizen> Citizens { get; set; }
        
        public int Population => Citizens.Count;
        
        private bool NextWorkplaceIsCommercial { get; set; }
        
        #endregion

        #region Constructors

        public CitizenManager()
        {
            Citizens = new List<Citizen>();
            NextWorkplaceIsCommercial = true;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Increases population by moving in citizens to vacant homes.
        /// Checks if there are any vacant homes and workplaces.
        /// Checks the desire of citizens to move in.
        /// </summary>
        /// <param name="vacantHomes"> List of vacant homes. </param>
        /// <param name="vacantCommercialZones"> List of vacant commercial zones. </param>
        /// <param name="vacantIndustrialZones"> List of vacant industrial zones. </param>
        /// <param name="satisfaction"> Satisfaction of citizens. </param>
        /// <returns> List of moved in citizens. </returns>
         public List<Citizen> IncreasePopulation(List<ResidentialZone> vacantHomes, List<WorkplaceZone> vacantCommercialZones, List<WorkplaceZone> vacantIndustrialZones,double satisfaction)
        {
            var result = new List<Citizen>();
            WorkplaceZone? nextWorkplace = null;
            var allFreeHomeSlots = vacantHomes.Sum(home => home.Capacity);
            var allNewCitizens = 0;
            foreach (var home in vacantHomes)
            {
                var freeSlots = home.Capacity - home.Count;
                var newCitizenCount = freeSlots > 2 
                    ? new Random(DateTime.Now.Millisecond).Next(3, Math.Min(6, freeSlots)) 
                    : freeSlots;
                allNewCitizens += newCitizenCount;
                if (allNewCitizens > Math.Max(4, allFreeHomeSlots * MaxCitizenMoveInRate)) 
                {
                    break;
                }
                for (var i = 0; i < newCitizenCount; i++)
                {
                    nextWorkplace = NextWorkplace(home, vacantCommercialZones, vacantIndustrialZones);
                    double desireToMoveIn = CalculateDesireToMoveIn(home,nextWorkplace,satisfaction);
                    home.DesireToMoveIn = desireToMoveIn;
                    if (nextWorkplace == null)
                        break;
                    if (nextWorkplace.Full)
                    {
                        (nextWorkplace is IndustrialZone ? vacantIndustrialZones : vacantCommercialZones).Remove(
                            nextWorkplace);
                        nextWorkplace = NextWorkplace(home, vacantCommercialZones, vacantIndustrialZones);
                    }
                  
                    if(Population>MinPopulation && desireToMoveIn < DesireToMoveInThreshold)
                        break;
                    var citizen = new Citizen(home, nextWorkplace);
                    
                    Citizens.Add(citizen);
                    result.Add(citizen);
                }
                
                if (nextWorkplace == null)
                    break;
            }

            return result;
        }

        /// <summary>
        /// Decreases population by moving out citizens.
        /// Checks the satisfaction of citizens.
        /// </summary>
        /// <returns> List of moved out citizens. </returns>
        public List<Citizen> DecreasePopulation()
        {
            var result = new List<Citizen>();

            foreach (var citizen in Citizens)
            {
                if (citizen.LastCalculatedSatisfaction < CitizenMoveOutThreshold)
                {
                    citizen.MoveOut();
                    
                    result.Add(citizen);
                }
            }
            foreach (var citizen in result)
                Citizens.Remove(citizen);
            return result;
        }

        /// <summary>
        /// Decreases population by moving out citizens.
        /// </summary>
        /// <param name="citizens"> List of citizens to move out. </param>
        /// <returns> List of moved out citizens. </returns>
        public List<Field> DecreasePopulation(List<Citizen> citizens)
        {
            var result = new List<Field>();

            foreach (var citizen in citizens)
            {
                if (Citizens.Remove(citizen))
                {
                    result.Add(citizen.Home.Owner!);
                    result.Add(citizen.Workplace?.Owner!);
                }
                
                citizen.MoveOut();
            }

            return result;
        }

        #endregion

        #region Private methods

        private WorkplaceZone? NextWorkplace(ResidentialZone home, List<WorkplaceZone> vacantCommercialZones, List<WorkplaceZone> vacantIndustrialZones)
        {
            WorkplaceZone? result = null;
            var nearestCommercialZone = NearestWorkplace(home, vacantCommercialZones);
            var nearestIndustrialZone = NearestWorkplace(home, vacantIndustrialZones);
            var commercialZoneDistanceEffect = CalculateHomeWorkplaceDistanceEffect(home, nearestCommercialZone);
            var industrialZoneDistanceEffect = CalculateHomeWorkplaceDistanceEffect(home, nearestIndustrialZone);
            if (Math.Abs(commercialZoneDistanceEffect - industrialZoneDistanceEffect) == 0 || Math.Abs(commercialZoneDistanceEffect-industrialZoneDistanceEffect)<DistanceEffectThreshold)
            {
                result = NextWorkplaceIsCommercial ? nearestCommercialZone : nearestIndustrialZone;
                NextWorkplaceIsCommercial = !NextWorkplaceIsCommercial;
            }
            else
            {
                if (commercialZoneDistanceEffect > industrialZoneDistanceEffect)
                {
                    result = nearestCommercialZone;
                    if (NextWorkplaceIsCommercial)
                    {
                        NextWorkplaceIsCommercial = !NextWorkplaceIsCommercial;
                    }
                }
                else
                {
                    result = nearestIndustrialZone;
                    if (!NextWorkplaceIsCommercial)
                    {
                        NextWorkplaceIsCommercial = !NextWorkplaceIsCommercial;
                    }
                }
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
        private double CalculateDesireToMoveIn(ResidentialZone home,WorkplaceZone? workplace,double satisfaction)
        {
            var distanceEffect = CalculateHomeWorkplaceDistanceEffect(home,workplace);
            home.DistanceEffect = distanceEffect;
            var totalSatisfaction = satisfaction;
            var industrialEffect = home.Owner!.IndustrialEffect;
            var forestEffect = home.Owner.ForestEffect;
            var result=totalSatisfaction/3+forestEffect/6+(1-industrialEffect)/6+distanceEffect/3;
            return result;
        }
        
        private static double CalculateHomeWorkplaceDistanceEffect(Placeable home, WorkplaceZone? workplace) => workplace switch
        {
            null => 0,
            _ => Utilities.GetPointsInRadiusWeighted(home.Owner!, CloseProximityRadius)
                .Where(tuple => tuple.X == workplace.Owner!.X && tuple.Y == workplace.Owner!.Y)
                .Select(tuple => tuple.Weight).FirstOrDefault()
        };
        #endregion
    }
}