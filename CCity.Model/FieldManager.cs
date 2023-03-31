namespace CCity.Model
{
    public class FieldManager
    {
        #region Constants

        private const int MAX_EFFECT = 20;
        private const int EFFECT_RADIUS = 10;

        #endregion

        #region Fields

        public Field[][] Fields;
        private Dictionary<Forest, int> _growingForests;
        private List<Field> _burningBuildings;

        #endregion

        #region Public methods


        public List<Field> Place(int x, int y, Placeable placeable)
        {
            List<Field> effectedFields = new();
            if (!CanPlace(x, y, placeable))
            {
                // event
                return effectedFields; //empty
            }
            Field field = Fields[x][y];

            /*
             * 
             */

            return effectedFields; //empty
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
            int size = r * 2 + 1;
            int startX = x - r;
            int startY = y - r;
            List<Tuple<int, int, double>> list = new List<Tuple<int, int, double>>();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int currentX = startX + i;
                    int currentY = startY + j;
                    double distance = Math.Sqrt(Math.Pow(Math.Abs(currentX - x), 2) + Math.Pow(Math.Abs(currentY - y), 2));
                    int roundedDistance = (int)Math.Round(distance);
                    if (roundedDistance <= r)
                    {
                        double percentage = Math.Sin((double)roundedDistance / (double)r);
                        list.Add(new Tuple<int, int, double>(currentX, currentY, percentage));
                    }
                }
            }
            return list;
        }

        private bool OnMap(int x, int y)
        {
            throw new NotImplementedException();
        }

        private bool CanPlace(int x, int y, Placeable placeable)
        {
            if (!OnMap(x, y))
            {
                return false;
            }
            Field field = Fields[x][y];
            if (field.HasPlaceable)
            {
                return false;
            }
            if (field is IMultifield)
            {
                int width = ((IMultifield)field).Width;
                int height = ((IMultifield)field).Height;
                List<Field> neededFields = new();
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int currentX = field.X + i;
                        int currentY = field.Y + j;
                        if (!OnMap(currentX, currentY))
                        {
                            Field currentField = Fields[currentX][currentY];
                            if (currentField.HasPlaceable)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private List<Field> PlacePlaceableWithEffect(Field field, Placeable placeable, Action<Field, int> effectFunction)
        {
            if (!(placeable is FireDepartment || placeable is PoliceDepartment || placeable is Stadium))
            {
                throw new ArgumentException("Illegal argument.");
            }

            List<Field> effectedFields = new();
            List<Tuple<int, int, double>> coordinates = GetCoordinatesInRadius(field.X, field.Y, EFFECT_RADIUS);
            foreach (Tuple<int, int, double> coord in coordinates)
            {
                int tupleX = coord.Item1; int tupleY = coord.Item2; double percentage = coord.Item3;
                if (OnMap(tupleX, tupleY))
                {
                    int effect = (int)Math.Round(percentage * MAX_EFFECT);
                    Field effectedField = Fields[tupleX][tupleY];
                    effectFunction(effectedField, effect);
                    effectedFields.Add(effectedField);
                }
            }
            return effectedFields;
        }

        private bool CanDemolishRoad(Field field)
        {
            throw new NotImplementedException();
        }

        private void HandleRoadPlacement(Field field)
        {
            Road placedRoad = (Road)field.Placeable;
            List<Field> neigbours = GetRoadNeighbours(field.X, field.Y);

            foreach (Field neigbour in neigbours)
            {
                Road road = (Road)neigbour.Placeable;
                if (road.IsPublic)
                {
                    placedRoad.GetPublicityFrom = road;
                    road.GivesPublicityTo.Add(placedRoad);
                    neigbours.Remove(neigbour);
                    break;
                }
            }

            if (placedRoad.IsPublic)
            {
                SpreadPublicity(placedRoad, neigbours);
            }

        }

        private void HandleRoadDemolition(Field field)
        {
            Road demolishedRoad = (Road)field.Placeable;
            if (demolishedRoad.IsPublic)
            {
                foreach (Road road in demolishedRoad.GivesPublicityTo)
                {
                    ModifyRoad(road);
                }

            }
        }

        private List<Field> GetRoadNeighbours(int x, int y)
        {
            List<Field> neighbours = new List<Field>();

            if (OnMap(x, y) && Fields[x - 1][y].Placeable is Road) neighbours.Add(Fields[x - 1][y]);
            if (OnMap(x, y) && Fields[x + 1][y].Placeable is Road) neighbours.Add(Fields[x + 1][y]);
            if (OnMap(x, y) && Fields[x][y - 1].Placeable is Road) neighbours.Add(Fields[x][y - 1]);
            if (OnMap(x, y) && Fields[x][y + 1].Placeable is Road) neighbours.Add(Fields[x][y + 1]);

            return neighbours;
        }

        private void SpreadPublicity(Road actualRoad, List<Field> neigbours)
        {
            if (neigbours.Count == 0) return;
            foreach (Field neigbour in neigbours)
            {
                Road road = (Road)neigbour.Placeable;
                if (!road.IsPublic)
                {
                    road.GetPublicityFrom = actualRoad;
                    actualRoad.GivesPublicityTo.Add(road);
                    SpreadPublicity(road, GetRoadNeighbours(neigbour.X, neigbour.Y));
                };
            }
        }

        private void ModifyRoad(Road road)
        {

        }


        #endregion
    }
}
