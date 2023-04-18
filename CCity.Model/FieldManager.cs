using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Diagnostics;

namespace CCity.Model
{
    public class FieldManager
    {
        #region Constants

        private const int MAX_EFFECT = 10;
        private const int EFFECT_RADIUS = 10;
        private const int HEIGHT = 10;
        private const int WIDTH = 20;
        private const int ROOTX = WIDTH / 2;
        private const int ROOTY = HEIGHT - 1;

        #endregion

        #region Fields

        public Field[,] Fields { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int CommercialZoneCount { get => _commercialZones.Count; }
        public int IndustrialZoneCount { get => _industrialZones.Count; }
        private Dictionary<Forest, int> _growingForests;
        private List<Field> _burningBuildings;
        private List<ResidentialZone> _residentialZones;
        private List<CommercialZone> _commercialZones;
        private List<IndustrialZone> _industrialZones;
        private List<Road> _roads;

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
            _commercialZones = new();
            _industrialZones = new();
            _roads = new();

            //starter public road
            Road starterRoad = new Road();
            starterRoad.GetsPublicityFrom = starterRoad;
            Fields[ROOTX, ROOTY].Place(starterRoad);
        }

        #endregion

        #region Public methods  

        public List<Field> Place(int x, int y, Placeable placeable)
        {
            if (!OnMap(x, y)) throw new Exception("PLACE-OUTOFFIELDBOUNDRIES");

            Field field = Fields[x, y];
            List<Field> effectedFields = PlaceOnField(field, placeable);

            switch (placeable)
            {
                case Road _: 
                     effectedFields = effectedFields.Concat(HandleRoadPlacement(field)).ToList(); break;
                default:
                    List<Field>? effectedFieldsBySpreading = RefreshPublicity(placeable);
                    if (effectedFieldsBySpreading != null) effectedFields = effectedFields.Concat(effectedFieldsBySpreading).ToList();
                    break;
            }
            UpdatePlaceableList(placeable, true);
            return effectedFields;
        }

        public List<Field> Upgrade(int x, int y)
        {
            throw new NotImplementedException();
        }

        public (Placeable, List<Field>?) Demolish(int x, int y)
        {
            if (!OnMap(x, y)) return (null!, null);

            Field field = Fields[x, y];
            if (!field.HasPlaceable) return (null!, null);
            Placeable placeable = field.Placeable!;
            List<Field>? effectedFields = new();

            switch (placeable)
            {
                case Road _:
                    List<Field>? effectedFieldsByRoadDemolition = DemolishRoad(field);
                    if (effectedFieldsByRoadDemolition == null) return (null!, null);
                    else effectedFields = effectedFields.Concat(effectedFieldsByRoadDemolition).ToList();
                    break;
                default:
                    effectedFields = DemolishFromField(field);
                    if (effectedFields == null) return (null!, null);
                    List<Field>? effectedFieldsBySpreading = SpreadPlaceableEffectConditional(placeable, false);
                    if (effectedFieldsBySpreading != null) effectedFields = effectedFields.Concat(effectedFieldsBySpreading).ToList();
                    break;
            }
            UpdatePlaceableList(placeable, false);
            return (placeable, effectedFields);
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
        public List<CommercialZone> CommercialZones(bool showUnavailable) => _commercialZones.FindAll(zone => !zone.IsFull || showUnavailable);
        public List<IndustrialZone> IndustrialZones(bool showUnavailable) => _industrialZones.FindAll(zone => !zone.IsFull || showUnavailable);

        #endregion

        #region Private methods

        #region Place related

        private bool CanPlace(Field field, Placeable placeable)
        {
            if (field.HasPlaceable)
            {
                throw new Exception("PLACE-ALREADYUSEDFIELD");
            }
            if (placeable is IMultifield multifield)
            {
                List<(int X, int Y)> fillerCoordinates = GetMultifieldFillerCoordinates(field, multifield);
                return fillerCoordinates.All(coord => OnMap(coord.X, coord.Y) && !Fields[coord.X, coord.Y].HasPlaceable);
            }
            return true;
        }

        private List<Field> PlaceOnField(Field field, Placeable placeable)
        {
            if (!CanPlace(field, placeable))
            {
                throw new Exception("PLACE-OUTOFFIELDBOUNDRIES");
            }
            List<Field> effectedFields = new();
            if (placeable is IMultifield multifield)
            {
                List<(int, int)> fillerCoordinates = GetMultifieldFillerCoordinates(field, multifield);
                foreach((int X, int Y) coord in fillerCoordinates)
                {
                    Field currentField = Fields[coord.X, coord.Y];
                    Filler filler = new Filler(multifield);
                    multifield.Occupies.Add(filler);
                    currentField.Place(filler);
                    effectedFields.Add(currentField);                    
                }
            }
            field.Place(placeable);
            effectedFields.Add(field);
            return effectedFields;
        }

        private List<Field> SpreadPlaceableEffectConditional(Placeable placeable, bool add)
        {
            switch (placeable)
            {
                case FireDepartment _: return SpreadPlaceableEffect(placeable, add, (f, i) => f.ChangeFireDepartmentEffect(i));
                case PoliceDepartment _: return SpreadPlaceableEffect(placeable, add, (f, i) => f.ChangePoliceDepartmentEffect(i));
                case Stadium _: return SpreadPlaceableEffect(placeable, add, (f, i) => f.ChangeStadiumEffect(i));
                case IndustrialZone _: return SpreadPlaceableEffect(placeable, add, (f, i) => f.ChangeIndustrialEffect(i));
                default: return new List<Field>() { placeable.Owner! };
            }
        }

        private void UpdatePlaceableList(Placeable placeable, bool add)
        {
            switch (placeable)
            {
                case ResidentialZone residentialZone: if (add) _residentialZones.Add(residentialZone); else _residentialZones.RemoveAll(e => e == residentialZone); break;
                case CommercialZone commercialZone: if (add) _commercialZones.Add(commercialZone); else _commercialZones.RemoveAll(e => e == commercialZone); break;
                case IndustrialZone industrialZone: if (add) _industrialZones.Add(industrialZone); else _industrialZones.RemoveAll(e => e == industrialZone); break;
                case Road road: if (add) _roads.Add(road); else _roads.RemoveAll(e => e == road); break;
                default: break;
            }
        }

        private List<Field> SpreadPlaceableEffect(Placeable placeable, bool add, Action<Field, int> effectFunction, int radius = EFFECT_RADIUS, int maxEffect = MAX_EFFECT)
        {
            List<Field> effectedFields = new();
            Field field = placeable.Owner!;
            List<(int, int, double)> coordinates = (Utilities.GetPointsInRadiusWeighted(field, radius).ToList());
            foreach ((int X, int Y, double weight) coord in coordinates)
            {
                if (OnMap(coord.X, coord.Y))
                {
                    int effect = (int)Math.Round(coord.weight * maxEffect);
                    if (!add) effect *= -1;
                    Field effectedField = Fields[coord.X, coord.Y];
                    effectFunction(effectedField, effect);
                    effectedFields.Add(effectedField);
                }
            }
            return effectedFields;
        }

        #endregion

        #region Demolish related

        private bool CanDemolish(Field field)
        {
            if (field == Fields[ROOTX, ROOTY]) return false;
            if (!field.HasPlaceable) return false;
            Placeable placeable = field.Placeable!;
            switch (placeable)
            {
                //case FireDepartment fireDepartment: return fireDepartment.AvailableFiretrucks == 1; //every firetruck is available
                case Zone zone: return !zone.HasCitizen;
            }
            return true;
        }

        private List<Field>? DemolishFromField(Field field)
        {
            if (!CanDemolish(field)) return null;
            List<Field> effectedFields = new();
            Placeable placeable = GetRoot(field.Placeable!);
            field = placeable.Owner!;

            if (placeable is IMultifield multifield)
            {
                foreach (Filler filler in multifield.Occupies)
                {
                    Field fillerField = filler.Owner!;
                    fillerField.Demolish();
                    effectedFields.Add(fillerField);
                }
            }
            field.Demolish();
            effectedFields.Add(field);
            return effectedFields;
        }

        #endregion

        #region Road related

        private List<Field> HandleRoadPlacement(Field field)
        {
            List<Field> modifiedFields = new();
            if (field.Placeable == null) return modifiedFields;
            Road placedRoad = (Road)field.Placeable;
            List<Placeable> roadNeigbours = GetRoadNeighbours(placedRoad);
            foreach (Road neigbourRoad in roadNeigbours)
            {
                if (neigbourRoad.IsPublic)
                {
                    placedRoad.GetsPublicityFrom = neigbourRoad;
                    neigbourRoad.GivesPublicityTo.Add(placedRoad);
                    break;
                }
            }

            if (placedRoad.IsPublic)
            {
                SpreadRoadPublicity(placedRoad, ref modifiedFields);
            }
            return modifiedFields;
        }

        private List<Field>? RefreshPublicity(Placeable placeable)
        {
            List<Placeable> neighbours = GetRoadNeighbours(placeable);
            if (placeable is Road || placeable.IsPublic) return null;
            List<Field> effectedFields = new();
            foreach (Road neighbour in neighbours)
            {
                if (neighbour.IsPublic)
                {
                    placeable.IsPublic = true;
                    effectedFields = effectedFields.Concat(SpreadPlaceableEffectConditional(placeable, true)).ToList();
                    effectedFields.Add(placeable.Owner!);
                    return effectedFields;
                }
            }
            return null;
        }

        private void SpreadRoadPublicity(Road road,ref List<Field> effectedFields)
        {
            List<Placeable> roadNeighbours = GetRoadNeighbours(road);
            List<Placeable> placeableNeighbours = GetRoadNeighbours(road, true);
            foreach (Placeable neigbour in placeableNeighbours)
            {
                List<Field>? effectedFieldsBySpreading = RefreshPublicity(neigbour);
                if (effectedFieldsBySpreading != null)
                {
                    effectedFields = effectedFields.Concat(effectedFieldsBySpreading).ToList();
                }
            }
            foreach (Road neighbourRoad in roadNeighbours)
            {
                if (!neighbourRoad.IsPublic)
                {
                    neighbourRoad.GetsPublicityFrom = road;
                    road.GivesPublicityTo.Add(neighbourRoad);
                    effectedFields.Add(neighbourRoad.Owner!);
                    SpreadRoadPublicity(neighbourRoad,ref effectedFields);
                };
            }
        }

        private List<Field>? DemolishRoad(Field field)
        {
            //#if TEMP_ROAD_DEMOLISH
            if (!CanDemolish(field)) return null;
            if (field.Placeable is not Road) return null;
            Road road = (Road)field.Placeable;
            List<Field>? effectedFields = new();
            if (effectedFields == null) return null;
            effectedFields = DemolishFromField(field);
            List<Placeable> privatedPlaceables = new();
            if (road.IsPublic)
            {
                foreach(Road tempRoad in _roads)
                {
                    tempRoad.GetsPublicityFrom = null;
                    tempRoad.GivesPublicityTo = new List<Road>();
                    List<Placeable> notRoadNeighbours = GetRoadNeighbours(tempRoad, true);
                    foreach (Placeable placeable in notRoadNeighbours)
                    {
                        privatedPlaceables.Add(placeable);
                    }
                }
                SpreadRoadPublicity((Road)Fields[ROOTX, ROOTY].Placeable!,ref effectedFields!);
                if (privatedPlaceables.Count > 0 && !privatedPlaceables.All(e => WouldStayPublic(e)))
                {
                    Place(field.X, field.Y, road);
                    return null;
                }

            }
            return effectedFields.Concat(_roads.Select(e => e.Owner)).ToList();
            //#else
            /*if (field.Placeable is not Road) return null;
            Road road = (Road)field.Placeable;
            List<Field>? effectedFields = new();
            HashSet<Placeable> privatedPlaceables = new();
            List<Road> gavePublicityTo = road.GivesPublicityTo.ToList();
            effectedFields = DemolishFromField(field);
            if (effectedFields == null) return null;
            foreach (Road giftedRoad in gavePublicityTo)
            {
                ModifyRoad(giftedRoad, privatedPlaceables, effectedFields);
            }
            if (privatedPlaceables.Count > 0 && !privatedPlaceables.All(e => WouldStayPublic(e)))
            {
                Place(field.X, field.Y, road);
                return null;
            }
            return effectedFields;*/
            //#endif
        }

        public (int[], List<Road>) GetFourRoadNeighbours(Road road)
        {
            int[] indicators = new int[4];
            List<Road> neighbours = new();
            (int fieldX, int fieldY) = (road.Owner!.X, road.Owner!.Y);
            List<(int, int)> coordinates = new() { (fieldX, fieldY - 1), (fieldX + 1, fieldY), (fieldX, fieldY + 1), (fieldX - 1, fieldY)};
            for(int i = 0; i < 4; i++)
            {
                (int x, int y) = coordinates[i];
                if (OnMap(x, y) && Fields[x, y].Placeable is Road actualRoad)
                {
                    indicators[i] = 1;
                    neighbours.Add(actualRoad);
                }
                else indicators[i] = 0;
            }
            return (indicators, neighbours);
        }

        private void ModifyRoad(Road actualRoad, HashSet<Placeable> privatedPlaceables, List<Field> effectedFields)
        {
            if (!actualRoad.IsPublic) return;
            List<Placeable> roadNeighbours = GetRoadNeighbours(actualRoad);
            List<Placeable> notRoadNeighbours = GetRoadNeighbours(actualRoad, true);

            bool found = false;
            foreach (Road roadNeighbour in roadNeighbours)
            {
                if (roadNeighbour.IsPublic && roadNeighbour.GetsPublicityFrom != actualRoad)
                {
                    actualRoad.GetsPublicityFrom = roadNeighbour;
                    roadNeighbour.GivesPublicityTo.Add(actualRoad);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                actualRoad.GetsPublicityFrom = null;
                List<Road> gavePublicityTo = actualRoad.GivesPublicityTo.ToList();
                foreach (Road giftedRoad in gavePublicityTo)
                {
                    ModifyRoad(giftedRoad, privatedPlaceables, effectedFields);
                }
            }
            if (actualRoad.IsPublic)
            {   
                SpreadRoadPublicity(actualRoad,ref effectedFields);
            }
            else
            {
                effectedFields.Add(actualRoad.Owner!);
                /*foreach (Placeable notRoadNeighbour in notRoadNeighbours)
                {
                    if(WouldStayPublic(notRoadNeighbour))
                    {
                        privatedPlaceables.Remove(notRoadNeighbour);
                    }
                    else
                    {
                        privatedPlaceables.Add(notRoadNeighbour);
                    }
                }*/
            }
        }

        private bool WouldStayPublic(Placeable placeable)
        {
            List<Placeable> roadNeighbours = GetRoadNeighbours(placeable);
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

#region Helpers

        private bool OnMap(int x, int y)
        {
            return 0 <= x && x < Width && 0 <= y && y < Height;
        }

        private Placeable GetRoot(Placeable placeable)
        {
            if (placeable is Filler) return (Placeable)(((Filler)placeable).Main);
            else return placeable;
        }

        private List<Placeable> GetRoadNeighbours(Placeable placeable, bool pessimist = false)
        {
            List<Placeable> placeables = GetNeighbours(placeable);
            placeables = placeables.FindAll(p => (p is Road && !pessimist) || (p is not Road && pessimist)).Select(e => GetRoot(e)).ToList();
            return placeables;
        }

        private List<Placeable> GetNeighbours(Placeable placeable)
        {
            List<Placeable> placeables = new();
            Placeable mainPlaceable = GetRoot(placeable);
            Field field = mainPlaceable.Owner!;
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
            IterateThroughSide(x, y - height, true, width, placeables); //top
            IterateThroughSide(x + width, y, false, height, placeables); //right side
            return placeables;
        }

        private void IterateThroughSide(int startX, int startY, bool xIterates, int iterationNumber, List<Placeable> placeables)
        {
            int currentX = startX;
            int currentY = startY;
            for (int i = 0; i < iterationNumber; i++)
            {
                currentX = xIterates ? currentX + i : currentX;
                currentY = xIterates ? currentY : currentY - i;
                if (OnMap(currentX, currentY))
                {
                    Field neighbour = Fields[currentX, currentY];
                    if (neighbour.HasPlaceable) placeables.Add(neighbour.Placeable!);
                }
            }
        }

        private List<(int, int)> GetMultifieldFillerCoordinates(Field field, IMultifield multifield)
        {
            List<(int, int)> coordinates = new List<(int, int)>();
            int width = multifield.Width;
            int height = multifield.Height;
            int currentX;
            int currentY;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    currentX = field.X + i;
                    currentY = field.Y - j;
                    if (currentX != field.X || currentY != field.Y)
                    {
                        coordinates.Add((field.X + i, field.Y - j));
                    }
                }
            }
            return coordinates;
        }

#endregion

#endregion
    }
}
