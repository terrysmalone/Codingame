using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace _2026_1_WinterChallenge_SnakeByte;

internal class MovementHelper
{
    internal List<Point> SimulateSnakeMovement(List<Point> currentBody, Point currentHead, Point newHead, HashSet<Point> powerUpPoints)
    {
        List<Point> newBody = currentBody.Select(p => new Point(p.X, p.Y)).ToList();
        newBody.Insert(0, newHead);

        if (powerUpPoints.Contains(newHead))
        {
            // Don't remove the tail if we are eating a power source
            return newBody;
        }

        newBody.RemoveAt(newBody.Count - 1);

        return newBody;
    }

    internal List<Point> ApplyGravity(List<Point> snakeBody, HashSet<Point> platformPoints)
    {
        int count = 0;
        bool canMoveDown = true;
        while (canMoveDown)
        {
            foreach (var bodyPart in snakeBody)
            {
                Point bodyCheckPoint = new Point(bodyPart.X, bodyPart.Y + 1);

                if (platformPoints.Contains(bodyCheckPoint))
                {
                    canMoveDown = false;
                    break;
                }
            }

            if (canMoveDown)
            {
                // Move the snake down by one
                for (int i = 0; i < snakeBody.Count; ++i)
                {
                    snakeBody[i] = new Point(snakeBody[i].X, snakeBody[i].Y + 1);
                }
            }

            count++;
            if (count > 20)
            {
                Console.Error.WriteLine($"ERROR: Gravity count exceeded max of 20");
                Console.Error.WriteLine($"ERROR: Snake body: {string.Join(";", snakeBody.Select(p => $"({p.X},{p.Y})"))}");
                break;
            }
        }

        return snakeBody;
    }
}


