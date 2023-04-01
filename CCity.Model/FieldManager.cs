namespace CCity.Model
{
    public class FieldManager
    {
        #region Constants

        private const int MAX_EFFECT = 20;
        private const int EFFECT_RADIUS = 10;
        private const int HEIGHT = 50;
        private const int WIDTH = 50;

        #endregion

        #region Fields

        public Field[,] Fields;
        public int Width { get; private set; }
        public int Height { get; private set; }
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
            Field field = Fields[x,y];

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
            return 0 <= x && x < Width && 0 <= y && y < Height;
        }

        private bool CanPlace(int x, int y, Placeable placeable)
        {
            if (!OnMap(x, y))
            {
                return false;
            }
            Field field = Fields[x,y];
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
                            Field currentField = Fields[currentX,currentY];
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
                    Field effectedField = Fields[tupleX,tupleY];
                    effectFunction(effectedField, effect);
                    effectedFields.Add(effectedField);
                }
            }
            return effectedFields;
        }

        private bool CanDemolishRoad(Field field)
        {
            List<Field> neigbours = GetNeighbours(field);
            foreach (Field neigbour in neigbours)
            {
                if (!neigbour.Has(typeof(Road)) && !StayPublic(neigbour))
                {
                    return false;
                }
            }
            return true;
        }

        private void HandleRoadPlacement(Field field)
        {
            if (field.Placeable == null) return;
            Road placedRoad = (Road)field.Placeable;
            List<Field> roadNeigbours = GetTypeNeighbours(field, typeof(Road));

            foreach (Field neigbour in roadNeigbours)
            {
                if (neigbour.Placeable == null) continue;
                Road road = (Road)neigbour.Placeable;
                if (road.IsPublic)
                {
                    placedRoad.GetPublicityFrom = road;
                    road.GivesPublicityTo.Add(neigbour);
                    break;
                }
            }

            if (placedRoad.IsPublic)
            {
                SpreadRoadPublicity(field);
            }
        }

        private bool HandleRoadDemolition(Field field)
        {
            if (field.Placeable == null || !CanDemolishRoad(field))
            {
                return false;
            }

            Road demolishedRoad = (Road)field.Placeable;
            if (demolishedRoad.IsPublic)
            {
                demolishedRoad.GetPublicityFrom = null;
                List<Field> givesPublicityTo = demolishedRoad.GivesPublicityTo.ToList();
                foreach (Field roadField in givesPublicityTo)
                {
                    if (!ModifyRoad(roadField)) return false; ;
                }
                List<Field> neigbours = GetNeighbours(field);
            }
            return true;
        }

        private List<Field> GetTypeNeighbours(Field field,Type type)
        {
            int x = field.X;
            int y = field.Y;
            List<Field> neighbours = new List<Field>();

            if (OnMap(x-1, y) && Fields[x - 1,y].Has(type)) neighbours.Add(Fields[x - 1,y]);
            if (OnMap(x+1, y) && Fields[x + 1,y].Has(type)) neighbours.Add(Fields[x + 1,y]);
            if (OnMap(x, y-1) && Fields[x,y - 1].Has(type)) neighbours.Add(Fields[x,y - 1]);
            if (OnMap(x, y+1) && Fields[x,y + 1].Has(type)) neighbours.Add(Fields[x,y + 1]);

            return neighbours;
        }

        private List<Field> GetNeighbours(Field field)
        {
            int x = field.X;
            int y = field.Y;
            List<Field> neighbours = new List<Field>();

            if (OnMap(x - 1, y)) neighbours.Add(Fields[x - 1, y]);
            if (OnMap(x + 1, y)) neighbours.Add(Fields[x + 1, y]);
            if (OnMap(x, y - 1)) neighbours.Add(Fields[x, y - 1]);
            if (OnMap(x, y + 1)) neighbours.Add(Fields[x, y + 1]);

            return neighbours;
        }


        private void SpreadRoadPublicity(Field field)
        {
            if (field.Placeable == null) return;
            Road actualRoad = (Road)field.Placeable;
            List<Field> roadNeigbours = GetTypeNeighbours(field, typeof(Road));
            List<Field> neigbours = GetNeighbours(field);
            foreach (Field neigbour in neigbours)
            {
                neigbour.RefreshPublicity();
            }
            foreach (Field neigbour in roadNeigbours)
            {
                if (neigbour.Placeable == null) continue;
                Road road = (Road)neigbour.Placeable;
                if (!road.IsPublic)
                {
                    road.GetPublicityFrom = actualRoad;
                    actualRoad.GivesPublicityTo.Add(neigbour);
                    SpreadRoadPublicity(neigbour);
                };
            }
        }

        private bool ModifyRoad(Field actualField)
        {
            if (actualField.Placeable == null) return false;
            Road actualRoad = (Road)actualField.Placeable;
            actualRoad.GetPublicityFrom = null;
            List<Field> roadNeigbourFields = GetTypeNeighbours(actualField, typeof(Road));
            List<Field> neigbours = GetNeighbours(actualField);

            foreach (Field roadNeigbourField in roadNeigbourFields)
            {
                if (roadNeigbourField.Placeable == null) continue;
                Road neigbourRoad = (Road)roadNeigbourField.Placeable;
                if (neigbourRoad.IsPublic)
                {
                    actualRoad.GetPublicityFrom = neigbourRoad;
                    neigbourRoad.GivesPublicityTo.Add(actualField);
                    break;
                }
            }

            if (actualRoad.IsPublic)
            {
                SpreadRoadPublicity(actualField);
                return true;
            }
            else
            {
                if (!HandleRoadDemolition(actualField))
                {
                    SpreadRoadPublicity(actualField);
                    return false;
                }
                return true;
            }

        }

        private bool StayPublic(Field field)
        {
            int count = 0;
            if (field.Placeable == null || !field.Placeable.isPublic) return true;
            List<Field> neigbours = GetNeighbours(field);
            foreach (Field neigbbour in neigbours)
            {
                if (neigbbour.Has(typeof(Road)))
                {
                    ++count;
                }
            }
            if (count < 2) return false;
            else return true;
        }


        #endregion
    }
}
