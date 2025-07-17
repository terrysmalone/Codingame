
namespace SummerChallenge2025_SoakOverflow;

internal static class Display
{
    internal static void CoverMap(double[,] coverMap)
    {
        Console.Error.WriteLine("Cover Map:");
        for (int y = 0; y < coverMap.GetLength(1); y++)
        {
            for (int x = 0; x < coverMap.GetLength(0); x++)
            {
                Console.Error.Write($"{coverMap[x, y]:F2} ");
            }
            Console.Error.WriteLine();
        }
    }

    internal static void SplashMap(int[,] splashMap)
    {
        Console.Error.WriteLine("Splash Map:");
        for (int y = 0; y < splashMap.GetLength(1); y++)
        {
            for (int x = 0; x < splashMap.GetLength(0); x++)
            {
                Console.Error.Write($"{splashMap[x, y]} ");
            }
            Console.Error.WriteLine();
        }
    }
}