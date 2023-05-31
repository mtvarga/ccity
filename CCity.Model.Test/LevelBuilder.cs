namespace CCity.Model.Test;

public class LevelBuilder
{
    private static readonly LevelBuilder Instance = new();

    private MainModel Model { get; set; } = null!;
    
    internal LevelBuilder Drag<T>((int x, int y) p1, (int x, int y) p2) where T: Placeable, new()
    {
        var fromX = Math.Min(p1.x, p2.x);
        var toX = Math.Max(p1.x, p2.x);
        var fromY = Math.Min(p1.y, p2.y);
        var toY = Math.Max(p1.y, p2.y);
                
        for (var x = fromX; x <= toX; x++)
        for (var y = fromY; y <= toY; y++)
            Model.Place(x, y, new T());

        return this;
    }

    internal LevelBuilder Place<T>(params (int x, int y)[] pts) where T: Placeable, new()
    {
        foreach (var (x, y) in pts)
            Model.Place(x, y, new T());

        return this;
    }

    internal LevelBuilder Place<T>(int x, int y) where T : Placeable, new() => Place<T>((x, y));

    internal LevelBuilder Place(int x, int y, Placeable placeable)
    {
        Model.Place(x, y, placeable);
        return this;
    }

    internal LevelBuilder Demolish(params (int x, int y)[] pts)
    {
        foreach (var (x, y) in pts)
            Model.Demolish(x, y);

        return this;
    }

    internal LevelBuilder Demolish(int x, int y) => Demolish((x, y));
    
    internal LevelBuilder DemolishIf(Predicate<Field> predicate, params (int x, int y)[] pts)
    {
        foreach (var (x, y) in pts)
            if (predicate(Model.Fields[x, y]))
                Model.Demolish(x, y);

        return this;
    }

    internal static LevelBuilder For(MainModel model)
    {
        Instance.Model = model;
        return Instance;
    }
}