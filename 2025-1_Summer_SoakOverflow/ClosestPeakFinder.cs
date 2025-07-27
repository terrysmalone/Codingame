using System.Drawing;

namespace SummerChallenge2025_SoakOverflow;

// Returns the closest peak in the map
public static class ClosestPeakFinder
{
    public static (Point, double) FindClosestPeak(Point position, double[,] damageMap)
    {
        double highestValue = damageMap.Cast<double>().Max();

        Queue<Point> queue = new Queue<Point>();
        HashSet<Point> visited = new HashSet<Point>();
        queue.Enqueue(position);
        visited.Add(position);

        while (queue.Count > 0)
        {
            Point current = queue.Dequeue();
            if (current.X < 0 || current.Y < 0 || current.X >= damageMap.GetLength(0) || current.Y >= damageMap.GetLength(1))
            {
                continue; 
            }

            double value = damageMap[current.X, current.Y];

            if (value == highestValue)
            {
                return (current, value);
            }
            // Add neighbors to the queue
            foreach (Point neighbor in GetNeighbors(current))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        Console.Error.WriteLine("ERROR: No peak found in the damage map.");
        return (new Point(-1, -1), -1);

    }

    private static IEnumerable<Point> GetNeighbors(Point current)
    {
        yield return new Point(current.X - 1, current.Y); // Left
        yield return new Point(current.X + 1, current.Y); // Right
        yield return new Point(current.X, current.Y - 1); // Up
        yield return new Point(current.X, current.Y + 1); // Down
        yield return new Point(current.X - 1, current.Y - 1); // Top-left
        yield return new Point(current.X + 1, current.Y - 1); // Top-right
        yield return new Point(current.X - 1, current.Y + 1); // Bottom-left
        yield return new Point(current.X + 1, current.Y + 1); // Bottom-right
    }
}
