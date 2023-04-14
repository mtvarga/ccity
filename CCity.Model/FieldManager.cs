using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace CCity.Model
{
    public class FieldManager
    {
        #region Constants

        private const int MAX_EFFECT = 10;
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
        private List<ResidentialZone> _residentialZones;
        private List<WorkplaceZone> _workplaceZones;

        #endregion

        #region Constructors

        public FieldManager()
        {
            Width = WIDTH;
            Height = HEIGHT;
            Fields = new Field[Width, Height];

            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Fields[i, j] = new Field(i, j);

            //todo: Place forests

            //lists
            _growingForests = new();
            _burningBuildings = new();
            _residentialZones = new();
            _workplaceZones = new();
        }

        #endregion

        #region Public methods

        //Pole, ResidentalZone, CommercialZone
        public List<Field> Place(int x, int y, Placeable placeable)
        {
            List<Field> effectedFields = new();
            if (!CanPlace(x, y, placeable))
            {
                // event
                return effectedFields; //empty
            }
            Field field = Fields[x, y];
            if (!PlaceOnField(field, placeable))
            {
                throw new Exception();
            }

            if (placeable is ResidentialZone)
            {
                _residentialZones.Add((ResidentialZone)placeable);
            }
            else if (placeable is CommercialZone)
            {
                _workplaceZones.Add((WorkplaceZone)placeable);
            }

            effectedFields.Add(field);
            return effectedFields; //empty
        }

        public List<Field> Place(int x, int y, Road road)
        {
            List<Field> effectedFields = new();
            if (!CanPlace(x, y, road))
            {
                // event
                return effectedFields; //empty
            }
            Field field = Fields[x, y];
            if (!PlaceOnField(field, road))
            {
                throw new Exception();
            };
            effectedFields = HandleRoadPlacement(field);
            /*TO DO: HandleRoadPlacement returns List<Field>
            with Roads affected and Placeables become public*/
            return effectedFields;
        }

        public List<Field> Place(int x, int y, PowerPlant powerPlant)
        {
            List<Field> effectedFields = new();
            if (!CanPlace(x, y, powerPlant))
            {
                // event
                return effectedFields; //empty
            }
            Field field = Fields[x, y];
            if (!PlaceOnField(field, powerPlant))
            {
                throw new Exception();
            };
            /*TO DO: Electricity,
             fill effectedFields */
            return effectedFields;
        }

        public List<Field> Place(int x, int y, Forest forest)
        {
            List<Field> effectedFields = new();
            if (!CanPlace(x, y, forest))
            {
                // event
                return effectedFields; //empty
            }
            Field field = Fields[x, y];
            if (!PlaceOnField(field, forest))
            {
                throw new Exception();
            };
            /*TO DO: Forest effect,
             fill effectedFields */
            return effectedFields;
        }

        #region Placeables with effect

        public List<Field> Place(int x, int y, FireDepartment fireDepartment)
        {
            return PlacePlaceableWithEffect(x, y, fireDepartment, (f, i) => f.ChangeFireDepartmentEffect(i));
        }

        public List<Field> Place(int x, int y, PoliceDepartment policeDepartment)
        {
            return PlacePlaceableWithEffect(x, y, policeDepartment, (f, i) => f.ChangePoliceDepartmentEffect(i));
        }

        public List<Field> Place(int x, int y, Stadium stadium)
        {
            return PlacePlaceableWithEffect(x, y, stadium, (f, i) => f.ChangeStadiumEffect(i));
        }

        public List<Field> Place(int x, int y, IndustrialZone industrialZone)
        {
            return PlacePlaceableWithEffect(x, y, industrialZone, (f, i) => f.ChangeIndustrialEffect(i));
        }

        #endregion

        public List<Field> Upgrade(int x, int y)
        {
            throw new NotImplementedException();
        }

        public List<Field>? Demolish(int x, int y)
        {
            if (!CanDemolish(x, y))
            {
                return null;
            }
            Field field = Fields[x, y];
            Placeable placeable = field.Placeable;
            List<Field>? effectedFields = new();

            if (placeable is Road) effectedFields = DemolishRoad(field);
            else if (placeable is FireDepartment) effectedFields = DemolishPlaceableWithEffect(field, (f, i) => f.ChangeFireDepartmentEffect(i));
            else if (placeable is PoliceDepartment) effectedFields = DemolishPlaceableWithEffect(field, (f, i) => f.ChangePoliceDepartmentEffect(i));
            else if (placeable is Stadium) effectedFields = DemolishPlaceableWithEffect(field, (f, i) => f.ChangeStadiumEffect(i));
            else if (placeable is IndustrialZone) effectedFields = DemolishPlaceableWithEffect(field, (f, i) => f.ChangeIndustrialEffect(i));
            else
            {
                field.Demolish();
                effectedFields.Add(field);
            }
            return effectedFields;
        }

        public List<Field> GrowForests()
        {
            throw new NotImplementedException();
        }

        public Field RandomIncinerate()
        {
            throw new NotImplementedException();
        }

        public List<ResidentialZone> ResidentialZones(bool showUnavailable) => _residentialZones.FindAll(zone => !zone.IsFull || showUnavailable);
        public List<WorkplaceZone> WorkplaceZones(bool showUnavailable) => _workplaceZones.FindAll(zone => !zone.IsFull || showUnavailable);

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

        private bool OnMap(Tuple<int, int> coordinates)
        {
            return 0 <= coordinates.Item1 && coordinates.Item1 < Width && 0 <= coordinates.Item2 && coordinates.Item2 < Height;
        }

        private bool CanPlace(int x, int y, Placeable placeable)
        {
            if (!OnMap(x, y))
            {
                return false;
            }
            Field field = Fields[x, y];
            if (field.HasPlaceable)
            {
                return false;
            }
            if (placeable is IMultifield)
            {
                int width = ((IMultifield)placeable).Width;
                int height = ((IMultifield)placeable).Height;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int currentX = field.X + i;
                        int currentY = field.Y - j;
                        if (!OnMap(currentX, currentY))
                        {
                            Field currentField = Fields[currentX, currentY];
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

        private bool CanDemolish(int x, int y)
        {
            if (!OnMap(x, y)) return false;
            Field field = Fields[x, y];
            if (!field.HasPlaceable)
            {
                return false;
            }

            Placeable placeable = field.Placeable;
            //if (placeable is Road) return CanDemolishRoad(field);
            if (placeable is FireDepartment fireDepartment) return fireDepartment.AvailableFiretrucks == 1; //every firetruck is available
            else if (placeable is Zone zone) return zone.HasCitizen;
            return true;
        }

        private List<Field> PlacePlaceableWithEffect(int x, int y, Placeable placeable, Action<Field, int> effectFunction, int radius = EFFECT_RADIUS, int maxEffect = MAX_EFFECT)
        {
            List<Field> effectedFields = new();
            if (!CanPlace(x, y, placeable))
            {
                // event
                return effectedFields; //empty
            }
            Field field = Fields[x, y];
            if (!(placeable is FireDepartment || placeable is PoliceDepartment || placeable is Stadium || placeable is IndustrialZone))
            {
                throw new ArgumentException("Illegal argument.");
            }
            List<Tuple<int, int, double>> coordinates = GetCoordinatesInRadius(field.X, field.Y, radius);
            foreach (Tuple<int, int, double> coord in coordinates)
            {
                int tupleX = coord.Item1; int tupleY = coord.Item2; double percentage = coord.Item3;
                if (OnMap(tupleX, tupleY))
                {
                    int effect = (int)Math.Round(percentage * maxEffect);
                    Field effectedField = Fields[tupleX, tupleY];
                    effectFunction(effectedField, effect);
                    effectedFields.Add(effectedField);
                }
            }
            if (!PlaceOnField(field, placeable))
            {
                throw new Exception();
            };
            if (placeable is IndustrialZone)
            {
                _workplaceZones.Add((WorkplaceZone)placeable);
            }
            return effectedFields;
        }

        private bool PlaceOnField(Field field, Placeable placeable)
        {
            int x = field.X;
            int y = field.Y;
            if (!CanPlace(x, y, placeable))
            {
                return false;
            }

            if (placeable is IMultifield multifield)
            {
                int width = multifield.Width;
                int height = multifield.Height;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        int currentX = field.X + i;
                        int currentY = field.Y - j;
                        if (currentX != field.X || currentY != field.Y)
                        {
                            Field currentField = Fields[currentX, currentY];
                            Filler filler = new Filler(multifield);
                            if (!currentField.Place(filler))
                            {
                                return false;
                            }
                            multifield.Occupies.Add(filler);
                        }
                    }
                }
            }
            return field.Place(placeable);
        }

        private List<Field> DemolishFromField(Field field)
        {
            int x = field.X;
            int y = field.Y;
            List<Field> effectedFields = new();
            if (!CanDemolish(x, y))
            {
                return null;
            }

            Placeable placeable = field.Placeable;

            if (placeable is IMultifield || placeable is Filler)
            {
                IMultifield multifield = (IMultifield)GetRoot(placeable);
                foreach (Filler filler in multifield.Occupies)
                {
                    Field fillerField = filler.Owner;
                    if (!fillerField.Demolish())
                    {
                        throw new Exception();
                    }
                    effectedFields.Add(fillerField);
                }
                Field multifieldField = ((Placeable)multifield).Owner;
                multifieldField.Demolish();
                effectedFields.Add(multifieldField);
            }
            else
            {
                field.Demolish();
                effectedFields.Add(field);
            }
            return effectedFields;

        }

        private Placeable GetRoot(Placeable placeable)
        {
            if (placeable is Filler) return (Placeable)(((Filler)placeable).Main);
            else return placeable;
        }

        private List<Field>? DemolishRoad(Field field)
        {
            if(field.Placeable == null) return null;
            Road road = (Road)field.Placeable;
            List<Field> effectedFields = new();
            HashSet<Placeable> privatedPlaceables = new();
            List<Road> gavePublicityTo = road.GivesPublicityTo.ToList();
            //TO DO demolish road
            foreach (Road giftedRoad in gavePublicityTo)
            {
                ModifyRoad(giftedRoad, privatedPlaceables, effectedFields);
            }
            
            if(privatedPlaceables.Count > 0 && !privatedPlaceables.All(e => WouldStayPublic(e)))
            {
                Place(field.X, field.Y, road);
                return null;
            }

            return effectedFields;

        }

        private List<Field>? DemolishPlaceableWithEffect(Field field, Action<Field, int> effectFunction, int radius = EFFECT_RADIUS, int maxEffect = MAX_EFFECT)
        {
            List<Field> effectedFields = new();
            Placeable placeable = GetRoot(field.Placeable);
            field = placeable.Owner;
            if (!(placeable is FireDepartment || placeable is PoliceDepartment || placeable is Stadium || placeable is IndustrialZone))
            {
                throw new ArgumentException("Illegal argument.");
            }
            List<Tuple<int, int, double>> coordinates = GetCoordinatesInRadius(field.X, field.Y, radius);
            foreach (Tuple<int, int, double> coord in coordinates)
            {
                int tupleX = coord.Item1; int tupleY = coord.Item2; double percentage = coord.Item3;
                if (OnMap(tupleX, tupleY))
                {
                    int effect = (int)Math.Round(percentage * maxEffect);
                    Field effectedField = Fields[tupleX, tupleY];
                    effectFunction(effectedField, (-1) * effect);
                    effectedFields.Add(effectedField);
                }
            }
            List<Field> demolishedFields = DemolishFromField(field);
            demolishedFields.ForEach(field => effectedFields.Add(field));
            if (placeable is IndustrialZone)
            {
                _workplaceZones.Remove((WorkplaceZone)placeable);
            }
            return effectedFields;
        }

        private List<Field> HandleRoadPlacement(Field field)
        {
            List<Field> modifiedFields = new() { field };
            if (field.Placeable == null) return modifiedFields;
            Road placedRoad = (Road)field.Placeable;
            List<Placeable> roadNeigbours = GetTypeNeighbours(placedRoad, typeof(Road));
            foreach (Road neigbourRoad in roadNeigbours)
            {
                if (neigbourRoad.IsPublic)
                {
                    placedRoad.GetPublicityFrom = neigbourRoad;
                    neigbourRoad.GivesPublicityTo.Add(placedRoad);
                    break;
                }
            }

            if (placedRoad.IsPublic)
            {
                List<Field> modifiedFieldsByRecursion = new();
                SpreadRoadPublicity(placedRoad, modifiedFieldsByRecursion);
                modifiedFields = modifiedFields.Concat(modifiedFieldsByRecursion).ToList();
            }
            return modifiedFields;
        }

        private bool RefreshPublicity(Placeable placeable)
        {
            List<Placeable> neighbours = GetTypeNeighbours(placeable, typeof(Road));
            if (placeable is Road || placeable.IsPublic) return false;
            foreach (Road neighbour in neighbours)
            {
                if (neighbour.IsPublic)
                {
                    placeable.IsPublic = true;
                    return true;
                }
            }
            return false;
        }

        private List<Placeable> GetTypeNeighbours(Placeable placeable, Type type, bool pessimist = false)
        {
            List<Placeable> placeables = GetNeighbours(placeable);
            placeables = placeables.FindAll(p => (p.GetType() == type && !pessimist) || (p.GetType() != type && pessimist));
            return placeables;
        }

        private List<Placeable> GetNeighbours(Placeable placeable)
        {
            List<Placeable> placeables = new();
            Placeable mainPlaceable = GetRoot(placeable);
            Field field = mainPlaceable.Owner;
            if (field == null) return placeables;
            int x = field.X;
            int y = field.Y;
            int width = 1; 
            int height = 1;
            if (mainPlaceable is IMultifield multifield)
            {
                width = multifield.Width;
                height = multifield.Height;
            }
            IterateThroughSide(x - 1, y, false, height, placeables); //left side
            IterateThroughSide(x, y + 1, true, width, placeables); //bottom
            IterateThroughSide(x, y - Height, true, width, placeables); //top
            IterateThroughSide(x + Width, y, false, height, placeables); //right side
            return placeables;
        }

        private void IterateThroughSide(int startX, int startY, bool xIterates, int iterationNumber, List<Placeable> placeables)
        {
            int currentX;
            int currentY;
            for (int i = 0; i < iterationNumber; i++)
            {
                currentX = xIterates ? startX + i : startX;
                currentY = xIterates ? startY: startY - i;
                if (OnMap(currentX, currentY))
                {
                    Field neighbour = Fields[currentX, currentY];
                    if (neighbour.HasPlaceable) placeables.Add(neighbour.Placeable);
                }
            }
        }


        private void SpreadRoadPublicity(Road road, List<Field> modifiedFields)
        {
            List<Placeable> roadNeighbours = GetTypeNeighbours(road, typeof(Road));
            List<Placeable> placeableNeighbours = GetTypeNeighbours(road, typeof(Road), false);
            foreach (Placeable neigbour in placeableNeighbours)
            {
                if (RefreshPublicity(neigbour)) modifiedFields.Add(neigbour.Owner);
            }
            foreach (Road neighbourRoad in roadNeighbours)
            {
                if (!neighbourRoad.IsPublic)
                {
                    neighbourRoad.GetPublicityFrom = road;
                    road.GivesPublicityTo.Add(neighbourRoad);
                    modifiedFields.Add(neighbourRoad.Owner);
                    SpreadRoadPublicity(neighbourRoad, modifiedFields);
                };
            }
        }

        private void ModifyRoad(Road actualRoad, HashSet<Placeable> privatedPlaceables, List<Field> effectedFields)
        {
            List<Placeable> roadNeighbours = GetTypeNeighbours(actualRoad, typeof(Road));
            List<Placeable> notRoadNeighbours = GetTypeNeighbours(actualRoad, typeof(Road), false);

            actualRoad.GetPublicityFrom = null;
            foreach (Road roadNeighbour in roadNeighbours)
            {
                if (roadNeighbour.IsPublic && roadNeighbour.GetPublicityFrom != actualRoad.GetPublicityFrom)
                {
                    actualRoad.GetPublicityFrom = roadNeighbour;
                    roadNeighbour.GivesPublicityTo.Add(actualRoad);
                    break;
                }
            }

            if (actualRoad.IsPublic)
            {
                actualRoad.GetPublicityFrom = null;
                foreach (Road giftedRoad in actualRoad.GivesPublicityTo)
                {
                    ModifyRoad(giftedRoad, privatedPlaceables, effectedFields);
                }
            }
            else
            {
                effectedFields.Add(actualRoad.Owner);
                foreach (Placeable notRoadNeighbour in notRoadNeighbours)
                {
                    if(WouldStayPublic(notRoadNeighbour))
                    {
                        privatedPlaceables.Remove(notRoadNeighbour);
                    }
                    else
                    {
                        privatedPlaceables.Add(notRoadNeighbour);
                    }
                }
            }
        }


        private bool WouldStayPublic(Placeable placeable)
        {
            List<Placeable> roadNeighbours = GetTypeNeighbours(placeable,typeof(Road));
            foreach (Road roadNeighbour in roadNeighbours)
            {
                if (roadNeighbour.IsPublic)
                {
                    return true;
                }
            }
            return false;
        }


        #endregion
    }
}
