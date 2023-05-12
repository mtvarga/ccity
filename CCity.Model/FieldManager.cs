using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using System.ComponentModel.Design.Serialization;
using System.ComponentModel.Design;
using System.Reflection.Metadata.Ecma335;

namespace CCity.Model
{
    public class FieldManager
    {
        #region Constants

        private const int MAX_EFFECT = 10;
        private const int EFFECT_RADIUS = 10;
        private const int FOREST_EFFECT_RADIUS = 3;
        private const int HEIGHT = 30;
        private const int WIDTH = 45;
        private const int ROOTX = WIDTH / 2;
        private const int ROOTY = HEIGHT - 1;

        #endregion

        #region Fields

        public Field[,] Fields { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int CommercialZoneCount { get => _commercialZones.Count; }
        public int IndustrialZoneCount { get => _industrialZones.Count; }
        
        private HashSet<ResidentialZone> _residentialZones;
        private HashSet<CommercialZone> _commercialZones;
        private HashSet<IndustrialZone> _industrialZones;

        private HashSet<Forest> _growingForests;
        
        public bool FirePresent => FireManager.FirePresent;

        public bool FireTrucksDeployed => FireManager.FireTrucksDeployed;

        private FireManager FireManager { get; }
        
        private Spreader _publicitySpreader;
        private Spreader _electricitySpreader;

        #endregion

        #region Constructors

        public FieldManager(bool notTestMode= true)
        {
            Width = WIDTH;
            Height = HEIGHT;
            Fields = new Field[Width, Height];

            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Fields[i, j] = new Field(i, j);

            //lists
            _growingForests = new();
            _residentialZones = new();
            _commercialZones = new();
            _industrialZones = new();
            _growingForests = new();

            FireManager = new FireManager(this);
            
            //starter public road
            Road starterRoad = new Road();
            PlaceOnField(Fields[ROOTX, ROOTY], starterRoad);
            starterRoad.MakeRoot(SpreadType.Publicity);

            _publicitySpreader = new Spreader(
                SpreadType.Publicity,
                false,
                (s, t) => s.CouldGivePublicityTo(t),
                (p) => GetNeighbours(p)
                );

            _electricitySpreader = new Spreader(
                SpreadType.Electricity,
                true,
                (s, t) => s.CouldGiveElectricityTo(t),
                (p) => GetNeighbours(p)
                );

            if(notTestMode) GenerateRandomForests();
        }

        #endregion

        #region Public methods  

        public List<Field> Place(int x, int y, Placeable placeable)
        {
            if (!OnMap(x, y)) throw new GameErrorException(GameErrorType.PlaceOutOfFieldBoundries);

            Field field = Fields[x, y];
            List<Field> modifiedFields = PlaceDemolishManager(field, placeable,true);
            List<Field> modifiedFieldsBySpreading;
            modifiedFieldsBySpreading = RefreshSpread(placeable);
            return modifiedFields.Concat(modifiedFieldsBySpreading).ToList();
        }

        public (IUpgradeable, int) Upgrade(int x, int y)
        {
            if (!OnMap(x, y)) throw new GameErrorException(GameErrorType.UpgradeOutOfFieldBoundries);
            Field field = Fields[x, y];
            if (field.Placeable is not IUpgradeable) throw new GameErrorException(GameErrorType.UpgradeNotUpgradeable);
            IUpgradeable upgradeable = (IUpgradeable)(field.Placeable);
            int upgradeCost = upgradeable.NextUpgradeCost;
            upgradeable.Upgrade();
            return (upgradeable, upgradeCost);
        }

        public (Placeable, List<Field>) Demolish(int x, int y)
        {
            if (!OnMap(x, y)) throw new GameErrorException(GameErrorType.DemolishOutOfFieldBoundries);

            Field field = Fields[x, y];
            if (!field.HasPlaceable) throw new GameErrorException(GameErrorType.DemolishEmptyField);
            Placeable placeable = field.Placeable!.Root;
            List<Field> modifiedFields = PlaceDemolishManager(field,placeable,false);
            List<Field> modifiedFieldsBySpreading;
            modifiedFieldsBySpreading = RefreshSpread(placeable);
            UpdatePlaceableList(placeable, false);
            return (placeable, modifiedFields.Concat(modifiedFieldsBySpreading).ToList());
        }

        public List<Field> GrowForests()
        {
            List<Field> effectedFields = new();
            foreach  (Forest  forest in _growingForests.ToList())
            {
                if(forest.CanGrow)
                {
                    effectedFields.Add(forest.Owner!);
                    if (forest.WillAge)
                    {
                        List<Field> industrialZonesAround = GetPlaceableInRadius(forest.Owner!, EFFECT_RADIUS, p => p is IndustrialZone);
                        foreach (Field industrialZone in industrialZonesAround)
                        {
                            effectedFields = effectedFields.Concat(industrialZone.Placeable!.Effect(SpreadRadiusEffect, false)).ToList();
                        }
                        effectedFields.Concat(forest.Effect(SpreadForestEffect, false).ToList());
                        forest.Grow();
                        effectedFields.Concat(forest.Effect(SpreadForestEffect, true).ToList());
                        foreach (Field industrialZone in industrialZonesAround)
                        {
                            effectedFields = effectedFields.Concat(industrialZone.Placeable!.Effect(SpreadRadiusEffect, true)).ToList();
                        }
                    }
                    else
                    {
                        forest.Grow();
                    }
                    
                }
                else
                {
                    _growingForests.Remove(forest);
                }
            }
            return effectedFields;
        }

        public List<Field> UpdateModifiedZonesSpread()
        {
            _electricitySpreader.RefreshRoots();
            return _electricitySpreader.GetAndClearModifiedFields();
        }

        public List<Field> IgniteBuilding(int x, int y)
        {
            if (!OnMap(x, y))
                throw new Exception("IGNITE_BUILDING-OUT_OF_FIELD_BOUNDS");

            if (Fields[x, y].Placeable is null or not IFlammable { Burning: false })
                throw new Exception("IGNITE_BUILDING-BAD_FIELD");

            var result = new List<Field>();
            
            var fireLocation = Fire.BreakOut(FireManager, Fields[x, y].Placeable!)!.Location;
            result.Add(fireLocation);
            
            if (fireLocation.Placeable is IMultifield multifield)
                result.AddRange(multifield.Occupies.Select(f => f.Owner!));
            
            return result;
        }

        public List<Field> IgniteRandomFlammable() => FireManager.IgniteRandomFlammable();

        public (List<Field> Updated, List<Field> Wrecked) UpdateFires() => FireManager.UpdateFires();

        public void DeployFireTruck(int x, int y)
        {
            if (!FirePresent)
                throw new GameErrorException(GameErrorType.DeployFireTruckNoFire);
            
            if (!OnMap(x, y)) 
                throw new GameErrorException(GameErrorType.DeployFireTruckOutOfFieldBounds);

            var placeable = Fields[x, y].Placeable;
            Fire? fire;
            
            if (placeable == null || (fire = FireManager.Fire(placeable)) == null)
                throw new GameErrorException(GameErrorType.DeployFireTruckBadBuilding);

            if (fire.AssignedFireTruck != null)
                throw new GameErrorException(GameErrorType.DeployFireTruckAlreadyAssigned);

            if (FireManager.DeployFireTruck(fire) == null)
                throw new GameErrorException(GameErrorType.DeployFireTruckNoneAvaiable);
        }

        // NOTE: This method returns the old locations (aka. the location of the fire trucks in the previous tick) of all the fire trucks
        public List<Field> UpdateFireTrucks()
        {
            if (!FireTrucksDeployed)
                throw new Exception("Internal inconsistency: Attempted to update fire truck locations when there are no fire trucks deployed");

            return FireManager.UpdateFireTrucks();
        }

        public List<ResidentialZone> ResidentialZones(bool showUnavailable) => _residentialZones.Where(zone => !zone.Full && zone.IsElectrified || showUnavailable).ToList();
        public List<CommercialZone> CommercialZones(bool showUnavailable) => _commercialZones.Where(zone => !zone.Full && zone.IsElectrified || showUnavailable).ToList();
        public List<IndustrialZone> IndustrialZones(bool showUnavailable) => _industrialZones.Where(zone => !zone.Full && zone.IsElectrified || showUnavailable).ToList();
        public IEnumerable<Field> FireTruckLocations() => FireManager.FireTruckLocations();
        
        #endregion

        #region Private methods

        #region Place related

        private bool CanPlace(Field field, Placeable placeable)
        {
            if (field.HasPlaceable)
            {
                return false;
            }
            if (placeable is IMultifield multifield)
            {
                List<(int X, int Y)> fillerCoordinates = GetMultifieldFillerCoordinates(field, multifield);
                return fillerCoordinates.All(coord => OnMap(coord.X, coord.Y) && !Fields[coord.X, coord.Y].HasPlaceable);
            }
            return true;
        }

        private List<Field> PlaceDemolishManager(Field field, Placeable placeable,bool place)
        {
            List<Field> effectedFields = new();
            List<Field> forestsInRadius = GetPlaceableInRadius(field, FOREST_EFFECT_RADIUS, p => p is Forest);
            foreach (Field forest in forestsInRadius)
            {
                effectedFields = effectedFields.Concat(forest.Placeable!.Effect(SpreadForestEffect, false)).ToList();
            }
            List<Field> industrialZonesAround = new();
            if (placeable is Forest) industrialZonesAround = GetPlaceableInRadius(field, EFFECT_RADIUS, p => p is IndustrialZone);
            foreach (Field industrialZone in industrialZonesAround)
            {
                effectedFields = effectedFields.Concat(industrialZone.Placeable!.Effect(SpreadRadiusEffect, false)).ToList();
            }
            try
            {
                if (place) effectedFields = effectedFields.Concat(PlaceOnField(field, placeable)).ToList();
                else effectedFields = effectedFields.Concat(DemolishFromField(field)).ToList();
            }
            finally
            {
                foreach (Field industrialZone in industrialZonesAround)
                {
                    effectedFields = effectedFields.Concat(industrialZone.Placeable!.Effect(SpreadRadiusEffect, true)).ToList();
                }
                foreach (Field forest in forestsInRadius)
                {
                    effectedFields = effectedFields.Concat(forest.Placeable!.Effect(SpreadForestEffect, true)).ToList();
                }
            }
            return effectedFields; 
        }

        private List<Field> PlaceOnField(Field field, Placeable placeable)
        {
            if (!CanPlace(field, placeable))
            {
                throw new GameErrorException(GameErrorType.PlaceAlreadyUsedField);
            }
            List<Field> effectedFields = new() {field };
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
            return effectedFields;
        }

        private void UpdatePlaceableList(Placeable placeable, bool add)
        {
            switch (placeable)
            {
                case ResidentialZone residentialZone: if (add) _residentialZones.Add(residentialZone); else _residentialZones.Remove(residentialZone); break;
                case CommercialZone commercialZone: if (add) _commercialZones.Add(commercialZone); else _commercialZones.Remove(commercialZone); break;
                case IndustrialZone industrialZone: if (add) _industrialZones.Add(industrialZone); else _industrialZones.Remove(industrialZone); break;
                case FireDepartment fireDepartment: if (add) FireManager.AddFireDepartment(fireDepartment); else FireManager.RemoveFireDepartment(fireDepartment); break;
                case Forest forest: if (add) _growingForests.Add(forest); else _growingForests.Remove(forest); break;
                default: break;
            }

            if (placeable is IFlammable flammable)
            {
                if (add)
                    FireManager.AddFlammable(placeable);
                else if (flammable.Burning)
                    throw new Exception("Internal inconsistency: Attempted to remove remove tracking of flammable that is currently burning");
                else
                    FireManager.RemoveFlammable(placeable);
            }
        }

        //you can rename it, i was not creative sorry
        //Method called in Place and Demolish (see references)
        private List<Field> RefreshSpread(Placeable placeable)
        {
            if (placeable == null) return new();

            _publicitySpreader.Refresh(placeable);
            List<Field> modifiedFields = _publicitySpreader.GetAndClearModifiedFields();

            _electricitySpreader.Refresh(placeable);
            modifiedFields = modifiedFields.Concat(_electricitySpreader.GetAndClearModifiedFields()).ToList();
     
            foreach (Field f in modifiedFields.ToList())
            {
                if (f.Placeable != null && f.Placeable.IsPublic && f.Placeable is PowerPlant powerPlant)
                {
                    //Switching on PowerPlant if it has become public
                    powerPlant.MakeRoot(SpreadType.Electricity);
                    _electricitySpreader.Refresh(powerPlant);
                    modifiedFields = modifiedFields.Concat(_electricitySpreader.GetAndClearModifiedFields()).ToList();
                }
            }

            modifiedFields.Add(placeable.Owner!);

            //At this point, both electricity and publicity spreaded
            //Now we can check modified placeables, and switch them on/off based on the two props mentioned
            //
            //(we cant do it in the previous loop,
            //because modifiedFields could have expanded due to switching on PowerPlant(s))
            foreach (Field f in modifiedFields.ToList())
            {
                if(f.Placeable != null)
                {
                    //TODO - electricity required for moving in
                    UpdatePlaceableList(f.Placeable, f.Placeable.ListingCondition);

                    //SWITCHING ON/OFF FIREDEPARTMENT COMES HERE (based on electricity and publicity)
                    //Suggestion: use f.Placeable.IsPublic && placeable.IsElectrified bool
                    //and try switching on when true, switching off otherwise
                    //(logic explained below)

                    //--------------------------------------------------------------------------

                    //Spreading the effect - IT HAS TO BE THE LAST METHOD IN THIS SECTION,
                    //BECAUSE OF - for example - THE APPLYMENT OF FIREDEPARTMENT EFFECT
                    //
                    //The second parameter of effect is:
                    //- true: try to spread the effect of the placecable
                    //- false: try to revoke the effect of the placeable
                    //("try", because if it is already spreaded/revoked (stored in Placeable), skips)
                    //
                    //so it's true if the Placeable is public and electrified
                    modifiedFields = modifiedFields.Concat(SpreadPlaceableEffectRouter(f.Placeable)).ToList();
                }
            }
            return modifiedFields;
        }

        private List<Field> SpreadPlaceableEffectRouter(Placeable placeable)
        {
            return placeable switch
            {
                Forest forest => forest.Effect(SpreadForestEffect, placeable.EffectSpreadingCondition),
                _ => placeable.Effect(SpreadRadiusEffect, placeable.EffectSpreadingCondition)
            };
        }

        private List<Field> SpreadRadiusEffect(Placeable placeable, bool add, Action<Field, int> effectFunction, int radius = EFFECT_RADIUS)
        {
            List<Field> effectedFields = new();
            Field field = placeable.Owner!;
            List<(int, int, double)> coordinates = (Utilities.GetPointsInRadiusWeighted(field, radius).ToList());
            foreach ((int X, int Y, double weight) coord in coordinates)
            {
                if (OnMap(coord.X, coord.Y))
                {
                    int effect = (int)Math.Round(coord.weight * MAX_EFFECT);
                    Field effectedField = Fields[coord.X, coord.Y];
                    if (placeable is IndustrialZone)
                    {
                        List<Field> forestsBetween = GetPlaceablesBetween(field, effectedField, p => p is Forest);
                        foreach (Field forest in forestsBetween)
                        {
                            Forest actualForest = (Forest)forest.Placeable!;
                            effect -= (int)Math.Round(MAX_EFFECT * actualForest.EffectRate);
                        }
                    }
                    if (!add) effect *= -1;
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
            if (field == Fields[ROOTX, ROOTY]) throw new GameErrorException(GameErrorType.DemolishMainRoad);
            if (!field.HasPlaceable) throw new GameErrorException(GameErrorType.DemolishEmptyField);
            Placeable placeable = field.Placeable!;
            switch (placeable)
            {
                //case FireDepartment fireDepartment: return fireDepartment.AvailableFiretrucks == 1; //every firetruck is available
                case Zone zone: return zone.Empty;
                case Road road:
                    field.Demolish();
                    _publicitySpreader.Refresh(road);
                    if (_publicitySpreader.GetAndClearModifiedFields().Find(e => e.Placeable is not Road && e.Placeable is not null && !e.Placeable!.IsPublic) != null)
                    {
                        Place(field.X, field.Y, road);
                        throw new GameErrorException(GameErrorType.DemolishFieldPublicity);
                    }
                    Place(field.X, field.Y, road);
                    break;
                case FireDepartment fireDepartment:
                    return !fireDepartment.FireTruckDeployed;
            }
            return true;
        }

        private List<Field> DemolishFromField(Field field)
        {
            if (!CanDemolish(field)) throw new GameErrorException(GameErrorType.DemolishFieldHasCitizen);
            List<Field> effectedFields = new();
            Placeable placeable = field.Placeable!.Root;
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
            //TEMP SOLUTION
            //TO DO - consistent SpreadPlaceableEffect
            field.Demolish();
            effectedFields.Add(field);
            List<Field> modifiedFieldsBySpreading = SpreadPlaceableEffectRouter(placeable);
            return effectedFields.Concat(modifiedFieldsBySpreading).Concat(GetNeighbours(placeable).Select(e => e.Owner!)).ToList();
        }

        #endregion

        #region Road related

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

        #endregion

        #region Helpers

        private bool OnMap(int x, int y)
        {
            return 0 <= x && x < Width && 0 <= y && y < Height;
        }

        private List<Placeable> GetNeighboursBySide(Placeable placeable, int side)
        {
            List<Placeable> placeables = new();
            Placeable mainPlaceable = placeable.Root;
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
            switch (side)
            {
                case 0: IterateThroughSide(x, y - height, true, width, placeables); break; //top
                case 1: IterateThroughSide(x - 1, y, false, height, placeables); break; //left side
                case 2: IterateThroughSide(x, y + 1, true, width, placeables); break; //bottom
                case 3: IterateThroughSide(x + width, y, false, height, placeables); break; //right side
            }
            return placeables;
        }

        internal List<Placeable> GetNeighbours(Placeable placeable)
        {
            List<Placeable> result = GetNeighboursBySide(placeable, 0)
                .Concat(GetNeighboursBySide(placeable, 1))
                .Concat(GetNeighboursBySide(placeable, 2))
                .Concat(GetNeighboursBySide(placeable, 3)).ToList();
            return result;
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

        private List<Field> GetPlaceablesBetween(Field s, Field t, Func<Placeable, bool> cond)
        {
            List<(int, int)> getPointsBetween = Utilities.GetPointsBetween(s, t);
            getPointsBetween = getPointsBetween.Where(e => cond(Fields[e.Item1, e.Item2].Placeable!)).ToList();
            List<Field> placeables = new List<Field>();
            foreach ((int X,int Y) coord in getPointsBetween)
            {
                placeables.Add(Fields[coord.X, coord.Y]);
            }
            return placeables;
        }
        private List<Field> GetPlaceableInRadius(Field field, int radius, Func<Placeable, bool> cond)
        {
            List<Field> placeables = new List<Field>();
            List<(int, int)> cordinates = (Utilities.GetPointsInRadius(field, radius)).ToList();
            foreach ((int X, int Y) coord in cordinates)
            {
                if (!OnMap(coord.X, coord.Y) || field == Fields[coord.X, coord.Y]) continue;
                Field fieldInRadius = Fields[coord.X, coord.Y];
                if (cond(fieldInRadius.Placeable!)) placeables.Add(fieldInRadius);
            }
            return placeables;
        }

        #endregion

        #region Forest related

        private void GenerateRandomForests()
        {
            Random rand = new Random();
            int forestCount = rand.Next(3, 5);
            int i = 0;
            while (i < forestCount)
            {
                int randX = rand.Next(0, Width);
                int randY = rand.Next(0, Height);
                if(!Fields[randX, randY].HasPlaceable)
                {
                    Place(randX, randY, new Forest(true));
                    Field field = Fields[randX, randY];
                    int forestSize = rand.Next(2,3);
                    int density = rand.Next(4, 10);
                    GenerateForestAround(field,forestSize,density);
                    i++;
                }

            }
        }

        private void GenerateForestAround(Field field,int forestSize,int density)
        {
            Random rand = new Random();
            List<(int, int)> cordinates = Utilities.GetPointsInRadius(field, forestSize).ToList();
            foreach ((int x,int y) cord  in cordinates)
            {
                if (rand.Next(0, 10) < density)
                {
                    if (OnMap(cord.x,cord.y) && !Fields[cord.x,cord.y].HasPlaceable)
                    {
                        Place(cord.x, cord.y, new Forest(true));
                    }
                }
            }
        }

        private List<Field> SpreadForestEffect(Placeable placeable,bool add,Action<Field,int> effectFunction,int radius)
        {
            Forest forest = (Forest)placeable;
            List<Field> effectedFields = new();
            Field field = forest.Owner!;
            List<(int,int)> cordinates = (Utilities.GetPointsInRadius(field,radius)).ToList();
            foreach ((int X,int Y) coord in cordinates)
            {
                if(OnMap(coord.X,coord.Y))
                {
                    Field effectedField = Fields[coord.X,coord.Y];
                    if(GetPlaceablesBetween(field,effectedField,p => p is not null && p is not Road && p is not Pole).Count==0)
                    {
                        int effect = (int)Math.Round(MAX_EFFECT * forest.EffectRate);
                        if (!add) effect *= -1;
                        effectFunction(effectedField,effect);
                        effectedFields.Add(effectedField);
                    }
                    
                }
            }
            return effectedFields;
        }

        #endregion

        #endregion
    }
}
