namespace CCity.Model;

internal class Fire
{
    internal Field Location { get; }

    internal IFlammable Flammable => (IFlammable)Location.Placeable!;

    internal FireTruck? AssignedFireTruck { get; set; }
    
    private IFlammable Building => (IFlammable)Location.Placeable!;
    
    private FireManager Manager { get; }

    private Fire(FireManager manager, Field location)
    {
        if (location.Placeable is not IFlammable { Burning: true })
            throw new Exception("Internal inconsistency: Attempted to assign a field as the destination of a fire emergency whose placeable is not a burning building");

        Manager = manager;
        Location = location;
    }

    internal void Damage()
    {
        Building.Health -= 1;

        if (Building.Health <= 0)
            Neutralize();
    }

    internal void Neutralize()
    {
        Flammable.Burning = false;
        Manager.RemoveFire(this);
    }

    internal static Fire? BreakOut(FireManager manager, Placeable placeable)
    {
        if (placeable is not IFlammable { Burning: false, Health: > 0 } flammable)
            return null;
            
        flammable.Burning = true;
        flammable.Health = IFlammable.FlammableMaxHealth; // Reset the building's health upon ignition

        var result = new Fire(manager, placeable.Owner!);
        manager.AddFire(result);

        return result;
    }
}