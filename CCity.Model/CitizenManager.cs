
using System.ComponentModel;

namespace CCity.Model
{
    public class CitizenManager
    {
        #region Constants

        private const double CitizenMoveOutThreshold = 0.25;
        private const int CloseProximityRadius = 10;
        private const double DesireToMoveInThreshold = 0.5;
        private const double DistanceEffectThreshold = 0.2;
        private const double MaxCitizenMoveInRate = 0.1;

        #endregion

        #region Properties

        public List<Citizen> Citizens { get; set; }
        
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

         public List<Citizen> IncreasePopulation(List<ResidentialZone> vacantHomes, List<WorkplaceZone> vacantCommercialZones, List<WorkplaceZone> vacantIndustrialZones,double satisfaction)
        {
            var result = new List<Citizen>();
            WorkplaceZone? nextWorkplace = null;
            var allFreeHomeSlots = vacantHomes.Sum(home => home.Capacity);
            var allNewCitizens = 0;
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
                allNewCitizens += newCitizenCount;
                var asd=allFreeHomeSlots* MaxCitizenMoveInRate;
                if (allNewCitizens > Math.Max(4, allFreeHomeSlots * MaxCitizenMoveInRate)) 
                {
                    break;
                }
                for (var i = 0; i < newCitizenCount; i++)
                {
                    nextWorkplace = NextWorkplace(home, vacantCommercialZones, vacantIndustrialZones);
                    double desireToMoveIn = CalculateDesireToMoveIn(home,nextWorkplace,satisfaction);
                    home.DesireToMoveIn = desireToMoveIn;
                    if (nextWorkplace.Full)
                    {
                        (nextWorkplace is IndustrialZone ? vacantIndustrialZones : vacantCommercialZones).Remove(
                            nextWorkplace);
                        nextWorkplace = NextWorkplace(home, vacantCommercialZones, vacantIndustrialZones);
                    }
                    if (nextWorkplace == null)
                        break;
                    if(Population>15 && desireToMoveIn < DesireToMoveInThreshold)
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

        private WorkplaceZone? NextWorkplace(ResidentialZone home, List<WorkplaceZone> vacantCommercialZones, List<WorkplaceZone> vacantIndustrialZones)
        {
            WorkplaceZone? result = null;
            var nearestCommercialZone = NearestWorkplace(home, vacantCommercialZones);
            var nearestIndustrialZone = NearestWorkplace(home, vacantIndustrialZones);
            var commercialZoneDistanceEffect = CalculateHomeWorkplaceDistanceEffect(home, nearestCommercialZone);
            var indusrialZoneDistanceEffect = CalculateHomeWorkplaceDistanceEffect(home, nearestIndustrialZone);
            if (commercialZoneDistanceEffect==indusrialZoneDistanceEffect || Math.Abs(commercialZoneDistanceEffect-indusrialZoneDistanceEffect)<DistanceEffectThreshold)
            {
                if (NextWorkplaceIsCommercial)
                {
                    result = nearestCommercialZone;
                }
                else
                {
                    result = nearestIndustrialZone;
                }
                NextWorkplaceIsCommercial = !NextWorkplaceIsCommercial;
            }
            else
            {
                if (commercialZoneDistanceEffect > indusrialZoneDistanceEffect)
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
            var industrialEffect = home.Owner.IndustrialEffect;
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