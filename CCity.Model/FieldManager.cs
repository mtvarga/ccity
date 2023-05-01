using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using System.ComponentModel.Design.Serialization;

namespace CCity.Model
{
    public class FieldManager
    {
        #region Constants

        private const int MAX_EFFECT = 10;
        private const int EFFECT_RADIUS = 10;
        private const int HEIGHT = 30;
        private const int WIDTH = 45;
        private const int ROOTX = WIDTH / 2;
        private const int ROOTY = HEIGHT - 1;

        private const ushort FireSpreadThreshold = IFlammable.FlammableMaxHealth / 2;
        
        #endregion

        #region Fields

        public Field[,] Fields { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int CommercialZoneCount { get => _commercialZones.Count; }
        public int IndustrialZoneCount { get => _industrialZones.Count; }
        private Dictionary<Forest, int> _growingForests;
        
        private List<ResidentialZone> _residentialZones;
        private List<CommercialZone> _commercialZones;
        private List<IndustrialZone> _industrialZones;
        
        private HashSet<Placeable> Flammables { get; }
        
        private HashSet<Placeable> BurningBuildings { get; }

        private Spreader _publicitySpreader;
        private Spreader _electricitySpreader;

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
            _residentialZones = new();
            _commercialZones = new();
            _industrialZones = new();

            //starter public road
            Road starterRoad = new Road();
            PlaceOnField(Fields[ROOTX, ROOTY], starterRoad);
            starterRoad.MakeRoot(SpreadType.Publicity);

            _publicitySpreader = new Spreader(
                SpreadType.Publicity,
                (s, t) => s.CouldGivePublicityTo(t),
                (p) => GetNeighbours(p)
                );

            _electricitySpreader = new Spreader(
                SpreadType.Electricity,
                (s, t) => s.CouldGiveElectricityTo(t),
                (p) => GetNeighbours(p)
                );
            
            Flammables = new HashSet<Placeable>();
            BurningBuildings = new HashSet<Placeable>();
        }

        #endregion

        #region Public methods  

        public List<Field> Place(int x, int y, Placeable placeable)
        {
            if (!OnMap(x, y)) throw new Exception("PLACE-OUTOFFIELDBOUNDRIES");

            Field field = Fields[x, y];
            List<Field> modifiedFields = PlaceOnField(field, placeable);
            List<Field> modifiedFieldsBySpreading = RefreshSpread(placeable);
            return modifiedFields.Concat(modifiedFieldsBySpreading).ToList();
        }

        public List<Field> Upgrade(int x, int y)
        {
            throw new NotImplementedException();
        }

        public (Placeable, List<Field>) Demolish(int x, int y)
        {
            if (!OnMap(x, y)) throw new Exception("DEMOLISH-OUTOFFIELDBOUNDS");

            Field field = Fields[x, y];
            if (!field.HasPlaceable) throw new Exception("DEMOLISH-NOTEMPTYFIELD");
            Placeable placeable = field.Placeable!.Root;
            List<Field> modifiedFields = DemolishFromField(field);
            List<Field> modifiedFieldsBySpreading = RefreshSpread(placeable);
            UpdatePlaceableList(placeable, false);
            return (placeable, modifiedFields.Concat(modifiedFieldsBySpreading).ToList());
        }

        public List<Field> GrowForests()
        {
            throw new NotImplementedException();
        }

        public Field? IgniteRandomBuilding()
        {
            var random = new Random(DateTime.Now.Millisecond);
            
            foreach (var placeable in Flammables)
            {
                if (placeable is not IFlammable flammable) 
                    throw new Exception("Internal inconsistency: FieldManager is tracking a non-flammable Placeable as flammable");

                if (random.Next(0, 100) > flammable.Potential) 
                    continue;
                
                // For now, only 1 building will be ignited at once
                Ignite(placeable);
                return placeable.Owner;
            }

            return null;
        }

        public List<Field> DamageBurningBuildings()
        {
            var result = new List<Field>();
            
            foreach (var placeable in BurningBuildings)
            {
                if (placeable is not IFlammable { Burning: true } flammable)
                    throw new Exception("Internal inconsistency: FieldManager is tracking a non-flammable Placeable or a flammable that is not burning as a burning building");

                var oldHealth = flammable.Health;
                
                Damage(placeable);

                if (oldHealth > FireSpreadThreshold && flammable.Health < FireSpreadThreshold)
                    result.AddRange(SpreadFire(placeable));

                if (flammable.Health > 0)
                    result.Add(placeable.Owner!);
                else
                {
                    // TODO: Destroy the placeable that is burning
                    // For this, we must adjust how the Demolish() method works
                }
            }

            return result;
        }

        public List<ResidentialZone> ResidentialZones(bool showUnavailable) => _residentialZones.FindAll(zone => !zone.Full || showUnavailable);
        public List<CommercialZone> CommercialZones(bool showUnavailable) => _commercialZones.FindAll(zone => !zone.Full || showUnavailable);
        public List<IndustrialZone> IndustrialZones(bool showUnavailable) => _industrialZones.FindAll(zone => !zone.Full || showUnavailable);

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

        private List<Field> PlaceOnField(Field field, Placeable placeable)
        {
            if (!CanPlace(field, placeable))
            {
                throw new Exception("PLACE-ALREADYUSEDFIELD");
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

        private void UpdatePlaceableList(Placeable placeable, bool add)
        {
            switch (placeable)
            {
                case ResidentialZone residentialZone: if (add) _residentialZones.Add(residentialZone); else _residentialZones.RemoveAll(e => e == residentialZone); break;
                case CommercialZone commercialZone: if (add) _commercialZones.Add(commercialZone); else _commercialZones.RemoveAll(e => e == commercialZone); break;
                case IndustrialZone industrialZone: if (add) _industrialZones.Add(industrialZone); else _industrialZones.RemoveAll(e => e == industrialZone); break;
                default: break;
            }

            if (placeable is IFlammable flammable)
            {
                if (add)
                    Flammables.Add(placeable);
                else if (flammable.Burning)
                    throw new Exception("Internal inconsistency: Attempted to remove remove tracking of flammable that is currently burning");
                else
                    Flammables.Remove(placeable);
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

            //At this point, both electricity and publicity spreaded
            //Now we can check modified placeables, and switch them on/off based on the two props mentioned
            //
            //(we cant do it in the previous loop,
            //because modifiedFields could have expanded due to switching on PowerPlant(s))
            foreach (Field f in modifiedFields)
            {
                if(f.Placeable != null)
                {
                    //TODO - electricity required for moving in
                    UpdatePlaceableList(f.Placeable, f.Placeable.IsPublic);

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
                    modifiedFields = modifiedFields.Concat(f.Placeable.Effect(SpreadPlaceableEffect, f.Placeable.IsPublic && f.Placeable.IsElectrified)).ToList();
                }
            }
            return modifiedFields;
        }

        private List<Field> SpreadPlaceableEffect(Placeable placeable, bool add, Action<Field, int> effectFunction, int radius = EFFECT_RADIUS)
        {
            List<Field> effectedFields = new();
            Field field = placeable.Owner!;
            List<(int, int, double)> coordinates = (Utilities.GetPointsInRadiusWeighted(field, radius).ToList());
            foreach ((int X, int Y, double weight) coord in coordinates)
            {
                if (OnMap(coord.X, coord.Y))
                {
                    int effect = (int)Math.Round(coord.weight * MAX_EFFECT);
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
            if (field == Fields[ROOTX, ROOTY]) throw new Exception("DEMOLISH-MAINROAD");
            if (!field.HasPlaceable) throw new Exception("DEMOLISH-NOTEMPTYFIELD");
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
                        throw new Exception("DEMOLISH-FIELDPUBLICITY");
                    }
                    Place(field.X, field.Y, road);
                    break;
            }
            return true;
        }

        private List<Field> DemolishFromField(Field field)
        {
            if (!CanDemolish(field)) throw new Exception("DEMOLISH - FIELDHASCIZIZEN");
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
            List<Field> modifiedFieldsBySpreading = placeable.Effect(SpreadPlaceableEffect, false);
            field.Demolish();
            effectedFields.Add(field);
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

        #region Fire Related

        private void Ignite(Placeable placeable)
        {
            if (placeable is not IFlammable flammable)
                throw new Exception("Internal inconsistency: Attempted to ignite a non-flammable Placeable");
            
            flammable.Burning = true;
            flammable.Health = 1; // Reset the building's health upon ignition
            
            BurningBuildings.Add(placeable);
        }

        private void PutOut(Placeable placeable)
        {
            if (placeable is not IFlammable flammable)
                throw new Exception("Internal inconsistency: Attempted to put out fire on a non-flammable Placeable");
            
            flammable.Burning = false;

            BurningBuildings.Remove(placeable);
        }

        private void Damage(Placeable placeable)
        {
            if (placeable is not IFlammable { Burning: true } flammable)
                throw new Exception("Internal inconsistency: Attempted to take fire damage on a non-flammable Placeable or on a flammable that is not burning");

            // In one tick, the building takes 0.25% damage
            // This way:
            //  - the building takes 1% damage in 1 second
            //  - the building is completely destroyed in 100 seconds
            flammable.Health -= 1;

            if (flammable.Health <= 0)
                BurningBuildings.Remove(placeable);
        }

        private List<Field> SpreadFire(Placeable placeable)
        {
            if (placeable is not IFlammable { Burning: true, Health: < FireSpreadThreshold })
                throw new Exception("Internal inconsistency: Attempted to spread fire from a non-flammable Placeable or on a flammable that isn't burning or its health is not low enough in order for the fire to spread");

            var flammableNeighbors = GetNeighbours(placeable).Where(p => p is IFlammable).ToList();

            foreach (var neighbor in flammableNeighbors)
                Ignite(neighbor);

            return flammableNeighbors.Select(p => p.Owner!).ToList();
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

        private List<Placeable> GetNeighbours(Placeable placeable)
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

#endregion

#endregion
    }
}
