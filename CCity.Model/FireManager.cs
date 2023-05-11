namespace CCity.Model;

internal class FireManager
{
    #region Constants

    private const ushort FireSpreadThreshold = IFlammable.FlammableMaxHealth / 2;

    #endregion

    #region Properties

    internal bool FirePresent => ActiveFires.Any();

    internal bool FireTrucksDeployed => DeployedFireTrucks.Any();
    
    private FieldManager FieldManager { get; }
    
    private HashSet<FireDepartment> FireDepartments { get; }

    private HashSet<Placeable> Flammables { get; }
    
    private HashSet<Fire> ActiveFires { get; }
    
    private HashSet<FireTruck> DeployedFireTrucks { get; }
    
    private Random Random { get; }
    
    #endregion
    
    #region Constructors

    internal FireManager(FieldManager fieldManager)
    {
        FieldManager = fieldManager;
        FireDepartments = new HashSet<FireDepartment>();
        Flammables = new HashSet<Placeable>();
        ActiveFires = new HashSet<Fire>();
        DeployedFireTrucks = new HashSet<FireTruck>();
        
        Random = new Random(DateTime.Now.Millisecond);
    }

    #endregion
    
    #region Internal Methods

    internal bool AddFireDepartment(FireDepartment fireDepartment) => FireDepartments.Add(fireDepartment);

    internal bool RemoveFireDepartment(FireDepartment fireDepartment) => FireDepartments.Remove(fireDepartment);
    
    internal bool AddFlammable(Placeable placeable)
    {
        if (placeable is not IFlammable)
            throw new Exception("Internal inconsistency: Attempted to track non-flammable placeable as flammable");

        return Flammables.Add(placeable);
    }

    internal bool RemoveFlammable(Placeable placeable)
    {
        if (placeable is not IFlammable { Burning: false })
            throw new Exception("Internal inconsistency: Attempted to remove tracking of flammable that is burning");
        
        return Flammables.Remove(placeable);
    }

    internal void AddFire(Fire fire) => ActiveFires.Add(fire);

    internal void RemoveFire(Fire fire) => ActiveFires.Remove(fire);
    
    internal Fire? Fire(Placeable placeable) => ActiveFires.FirstOrDefault(f => f.Location == placeable.Owner);

    internal IEnumerable<Field> FireTruckLocations() => DeployedFireTrucks.Select(ft => ft.Location);
    
    internal Field? IgniteRandomFlammable()
    {
        foreach (var placeable in Flammables)
        {
            if (Random.Next(0, 100) > ((IFlammable)placeable).Potential) 
                continue;

            Model.Fire.BreakOut(this, placeable);
            return placeable.Owner;
        }

        return null;
    }
    
    internal List<Field> UpdateFires()
    {
        var result = new List<Field>();
        
        foreach (var fire in ActiveFires)
        {
            switch (fire.AssignedFireTruck)
            {
                case null or { Active: true, Moving: true }:
                    // Damage the building if there is no assigned fire truck yet or it is still on its way
                    var oldHealth = fire.Flammable.Health;
            
                    fire.Damage();

                    if (oldHealth > FireSpreadThreshold && fire.Flammable.Health <= FireSpreadThreshold)
                        result.AddRange(SpreadFire(fire));

                    if (fire is { AssignedFireTruck: not null, Flammable.Health: <= 0 })
                        // Cancel the fire truck's deployment if the fire burned down the building before it could arrive
                        CancelFireTruck(fire.AssignedFireTruck);
                    
                    break;
                case { Active: false }:
                    fire.Neutralize();
                    break;
            }

            if (fire.Flammable.Health > 0)
                result.Add(fire.Location);
            else
                result.AddRange(FieldManager.Demolish(fire.Location.X, fire.Location.Y).Item2);
        }

        return result;
    }

    // This method requires the level data (fields, width, height) so that it can calculate the shortest roads
    internal FireTruck? DeployFireTruck(Fire fire)
    {
        // Find nearest fire truck
        var fireTruck = NearestAvailableFireTruck(fire.Location);

        if (fireTruck == null)
            // We cannot deploy any fire trucks -> the game will give an error message
            return null;
        
        // Find shortest road from the fire truck's location to the fire
        var path = Utilities.ShortestRoad(FieldManager.Fields, FieldManager.Width, FieldManager.Height, fireTruck.Location, fire.Location);
        
        // Calculate the return path to the station
        var returnPath = fireTruck.Moving ? Utilities.ShortestRoad(FieldManager.Fields, FieldManager.Width, FieldManager.Height, fireTruck.Station, fire.Location) : path;
        
        // Deploy the fire truck
        fireTruck.Deploy(path, returnPath);
        DeployedFireTrucks.Add(fireTruck);
        
        fire.AssignedFireTruck = fireTruck;

        return fireTruck;
    }
    
    internal List<Field> UpdateFireTrucks()
    {
        var result = new List<Field>();

        foreach (var fireTruck in DeployedFireTrucks)
        {
            result.Add(fireTruck.Location);
            fireTruck.Update();
            
            if (!fireTruck.Deployed)
                DeployedFireTrucks.Remove(fireTruck);
        }

        return result;
    }
    
    #endregion
    
    #region Private Methods
    
    private IEnumerable<Field> SpreadFire(Fire fire)
    {
        var flammableNeighbors = FieldManager.GetNeighbours(fire.Location.Placeable!).Where(p => p is IFlammable).ToList();

        foreach (var neighbor in flammableNeighbors)
            Model.Fire.BreakOut(this, neighbor);

        return flammableNeighbors.Select(p => p.Owner!);
    }
    
    private FireTruck? NearestAvailableFireTruck(Field f)
    {
        var availableFireTrucks = AvailableFireTrucks().ToList();
        var nearestFireTruck = availableFireTrucks.FirstOrDefault();
        var smallestDistance = Utilities.AbsoluteDistance(f, nearestFireTruck?.Location);
            
        foreach (var fireTruck in availableFireTrucks)
        {
            var currentDistance = Utilities.AbsoluteDistance(f, fireTruck.Location);

            if (currentDistance < smallestDistance)
                (nearestFireTruck, smallestDistance) = (fireTruck, currentDistance);
        }

        return nearestFireTruck;
    }

    private void CancelFireTruck(FireTruck fireTruck) => fireTruck.Cancel(fireTruck.DepartedFromStation
        ? null
        : Utilities.ShortestRoad(FieldManager.Fields, FieldManager.Width, FieldManager.Height, fireTruck.Station,
            fireTruck.Location));
    
    private IEnumerable<FireTruck> AvailableFireTrucks() => FireDepartments
        .Where(f => !f.FireTruck.Active)
        .Select(f => f.FireTruck);
    
    #endregion
}