using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model.Test
{
    internal static class TestUtilities
    {
        public static void DragPlacePlaceables(MainModel model, Placeable placeableTypeInstance, (int x, int y) point1, (int x, int y) point2)
        {
            int fromX = Math.Min(point1.x, point2.x);
            int toX = Math.Max(point1.x, point2.x);
            int fromY = Math.Min(point1.y, point2.y);
            int toY = Math.Max(point1.y, point2.y);
            for (int x = fromX; x <= toX; x++)
                for (int y = fromY; y <= toY; y++)
                {
                    model.Place(x, y, GetPlaceableFromInstance(placeableTypeInstance));
                }
        }

        public static void PlaceSinglePlaceables(MainModel model, Placeable placeableTypeInstance, List<(int, int)> points)
        {
            foreach ((int x, int y) point in points)
            {
                model.Place(point.x, point.y, GetPlaceableFromInstance(placeableTypeInstance));
            }
        }

        private static Placeable GetPlaceableFromInstance(Placeable placeableTypeInstance) => placeableTypeInstance switch
        {
            Road _ => new Road(),
            PowerPlant _ => new PowerPlant(),
            Stadium _ => new Stadium(),
            FireDepartment _ => new FireDepartment(),
            Pole _ => new Pole(),
            _ => throw new ArgumentException()
        };

    }
}
