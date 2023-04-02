namespace CCity.Model
{
    public static class Utilities
    {
        public static IEnumerable<(int X, int Y)> GetPointsInRadius(Field f, int r)
        {
            var size = r * 2 + 1;
            var startX = f.X - r;
            var startY = f.Y - r;

            var result = new List<(int X, int Y)>();

            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    var currentX = startX + i;
                    var currentY = startY + j;

                    var distance =
                        Convert.ToInt32(Math.Round(SquareDistance(f.X, f.Y, currentX, currentY)));

                    if (distance <= r)
                        result.Add((currentX, currentY));
                }
            }

            return result;
        }

        public static IEnumerable<(int X, int Y, double Weight)> GetPointsInRadiusWeighted(Field f, int r) => 
            from point in GetPointsInRadius(f, r) 
            select (
                point.X, 
                point.Y, 
                Math.Sin(Math.Round(SquareDistance(f.X, f.Y, point.X, point.Y)) / r)
            );

        
        public static double SquareDistance(Placeable p1, Placeable p2) => SquareDistance(p1.Field, p2.Field);
        
        public static double SquareDistance(Field f1, Field f2) => SquareDistance(f1.X, f1.Y, f2.X, f2.Y);

        public static double SquareDistance(int x1, int y1, int x2, int y2) =>
            Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));

        public static int AbsoluteDistance(Placeable p1, Placeable p2) => AbsoluteDistance(p1.Field, p2.Field);

        public static int AbsoluteDistance(Field f1, Field f2) => AbsoluteDistance(f1.X, f1.X, f2.X, f2.Y);

        public static int AbsoluteDistance(int x1, int y1, int x2, int y2) => Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
    }
}