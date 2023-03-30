using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class FieldManager
    {
        #region Constants

        

        #endregion

        #region Fields

        public Field[][] Fields;
        private Dictionary<Forest, int> _growingForests;
        private List<Field> _burningBuildings;

        #endregion

        #region Public methods


        public List<Field> Place(int x, int y, Placeable placeable)
        {
            throw new NotImplementedException();
        }

        public List<Field> Upgrade(int x, int y)
        {
            throw new NotImplementedException();
        }

        public bool Demolish(int x, int y)
        {
            throw new NotImplementedException();
        }

        public List<Field> GrowForests()
        {
            throw new NotImplementedException();
        }

        public Field RandomIncinerate()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private methods

        private List<Tuple<int, int, double>> GetCoordinatesInRadius(int x, int y, int r)
        {
            throw new NotImplementedException();
        }

        private bool OnMap(int x,int y)
        {
            throw new NotImplementedException();
        }

        private bool CanDemolishRoad(Field field)
        {
            throw new NotImplementedException();
        }

        private void HandleRoadPlacement(Field field)
        {
            Road placedRoad = (Road) field.Placeable;
            List<Field> neigbours = GetRoadNeighbours(field.X,field.Y);

            foreach (Field neigbour in neigbours)
            {
                Road road = (Road) neigbour.Placeable;
                if (road.IsPublic)
                {
                    placedRoad.GetPublicityFrom = road;
                    road.GivesPublicityTo.Add(placedRoad);
                    neigbours.Remove(neigbour);
                    break;
                }
            }

            if(placedRoad.IsPublic)
            {
                SpreadPublicity(placedRoad,neigbours);
            }

        }

        private void HandleRoadDemolition(Field field)
        {
            Road demolishedRoad = (Road) field.Placeable;
            if(demolishedRoad.IsPublic)
            {
                foreach(Road road in demolishedRoad.GivesPublicityTo)
                {
                    ModifyRoad(road);
                }

            }
        }

        private List<Field> GetRoadNeighbours(int x,int y)
        {
            List<Field> neighbours = new List<Field>();

            if (OnMap(x,y) && Fields[x-1][y].Placeable is Road ) neighbours.Add(Fields[x-1][ y]);
            if (OnMap(x,y) && Fields[x + 1][y].Placeable is Road) neighbours.Add(Fields[x + 1][y]);
            if (OnMap(x,y) && Fields[x][y - 1].Placeable is Road) neighbours.Add(Fields[x][y - 1]);
            if (OnMap(x,y) && Fields[x][y + 1].Placeable is Road) neighbours.Add(Fields[x][y +1 ]);

            return neighbours;
        }

        private void SpreadPublicity(Road actualRoad, List<Field> neigbours)
        {
            if(neigbours.Count == 0) return;
            foreach (Field neigbour in neigbours)
            {
                Road road = (Road)neigbour.Placeable;
                if (!road.IsPublic)
                {
                    road.GetPublicityFrom = actualRoad;
                    actualRoad.GivesPublicityTo.Add(road);
                    SpreadPublicity(road,GetRoadNeighbours(neigbour.X,neigbour.Y));
                };
            }
        }

        private void ModifyRoad(Road road)
        {

        }

        #endregion
    }
}
