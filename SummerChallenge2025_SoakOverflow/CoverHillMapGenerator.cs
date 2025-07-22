using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SummerChallenge2025_SoakOverflow;

public static class CoverHillMapGenerator
{
    public static int[,] CreateMap(int[,] cover)
    {
        int width = cover.GetLength(0);
        int height = cover.GetLength(1);
        int[,] scoreGrid = new int[width, height];

        Queue<(int x, int y, int score)> queue = new Queue<(int, int, int)>();

        // Initialize queue with cover positions and assign base scores
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cover[x, y] == 1) // medium cover
                {
                    scoreGrid[x, y] = 50;
                    queue.Enqueue((x, y, 50));
                }
                else if (cover[x, y] == 2) // high cover
                {
                    scoreGrid[x, y] = 100;
                    queue.Enqueue((x, y, 100));
                }
            }
        }

        // Directions: up, down, left, right
        int[] xChange = { 0, 0, -1, 1 };
        int[] yChange = { -1, 1, 0, 0 };

        int decay = 1;

        while (queue.Count > 0)
        {
            var (x, y, score) = queue.Dequeue();

            for (int direction = 0; direction < 4; direction++)
            {
                int xPos = x + xChange[direction];
                int yPos = y + yChange[direction];

                if (xPos >= 0 && xPos < width && yPos >= 0 && yPos < height)
                {
                    int newScore = score - decay;
                    if (newScore > scoreGrid[xPos, yPos])
                    {
                        scoreGrid[xPos, yPos] = newScore;
                        queue.Enqueue((xPos, yPos, newScore));
                    }
                }
            }
        }

        // We don't want to attempt to move onto the cover so set all actual cover to 0
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cover[x, y] > 0)
                {
                    scoreGrid[x, y] = 0;
                }
            }
        }

        return scoreGrid;
    }
}
