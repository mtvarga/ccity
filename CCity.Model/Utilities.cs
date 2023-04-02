namespace CCity.Model
{
    public static class Utilities
    {
        public static IEnumerable<(int X, int Y)> GetPointsInRadius(int x, int y, int r)
        {
            var size = r * 2 + 1;
            var startX = x - r;
            var startY = y - r;

            var result = new List<(int X, int Y)>();
            
            for (var i = 0; i < size; i++)
            {
                for (var j = 0; j < size; j++)
                {
                    var currentX = startX + i;
                    var currentY = startY + j;
                    
                    var distance = Convert.ToInt32(Math.Round(Math.Sqrt(Math.Pow(currentX - x, 2) + Math.Pow(currentY - y, 2))));

                    if (distance <= r)
                        result.Add((currentX, currentY));
                }
            }
            
            return result;
        } 
        
        public static IEnumerable<(int X, int Y, double Weight)> GetPointsInRadiusWeighted(int x, int y, int r) => 
            from coordinate in GetPointsInRadius(x, y, r) 
            select (
                coordinate.X, 
                coordinate.Y, 
                Math.Sin(Math.Round(Math.Sqrt(Math.Pow(coordinate.X - x, 2) + Math.Pow(coordinate.Y - y, 2))) / r)
            );
    }
}