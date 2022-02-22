using System;
using System.Drawing;

namespace LineRacing
{
    public class Player
    {
        const int WIDTH = 30;
        const int HEIGHT = 20;

        static void Main(string[] args)
        {
            string currentDirection = "LEFT";
            string[] inputs;

            Point playerStartPosition = new Point(-1, -1);
            Point playerEndPosition = new Point(-1, -1);

            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                int numberOfPlayers = int.Parse(inputs[0]); // total number of players (2 to 4).
                int playerNumber = int.Parse(inputs[1]); // your player number (0 to 3).

                Point[] enemyStartPositions = new Point[numberOfPlayers - 1];
                Point[] enemyEndPositions = new Point[numberOfPlayers - 1];

                var enemyIndex = 0;

                for (int i = 0; i < numberOfPlayers; i++)
                {
                    inputs = Console.ReadLine().Split(' ');

                    if (i == playerNumber)
                    {
                        playerStartPosition = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
                        playerEndPosition = new Point(int.Parse(inputs[2]), int.Parse(inputs[3]));
                    }
                    else
                    {
                        enemyStartPositions[enemyIndex] = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
                        enemyEndPositions[enemyIndex] = new Point(int.Parse(inputs[2]), int.Parse(inputs[3]));

                        enemyIndex++;
                    }
                }

                DisplayLightCyclePosition(playerStartPosition, playerEndPosition);
                DisplayLightCyclePositions(enemyStartPositions, enemyEndPositions);

                var nextMove = GetNextMove(playerEndPosition, currentDirection);

                if (nextMove != string.Empty)
                {
                    currentDirection = nextMove;
                }

                Console.WriteLine(currentDirection);

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                // Console.WriteLine("LEFT"); // A single line with UP, DOWN, LEFT or RIGHT

            }
        }

        private static string GetNextMove(Point position, string currentDirection)
        {
            // Very basic - just avoid walls
            if (position.X <= 0)
            {
                if (position.Y <= 0)
                {
                    return "DOWN";
                }
                else if (position.Y >= HEIGHT - 1)
                {
                    return "UP";
                }
                else
                {
                    return "UP";
                }
            }
            else if (position.X >= WIDTH - 1)
            {
                if (position.Y <= 0)
                {
                    return "DOWN";
                }
                else if (position.Y >= HEIGHT - 1)
                {
                    return "UP";
                }
                else
                {
                    return "DOWN";
                }
            }

            if (position.Y <= 0)
            {
                if (position.X <= 0)
                {
                    return "RIGHT";
                }
                else if (position.X >= WIDTH - 1)
                {
                    return "LEFT";
                }
                else
                {
                    return "LEFT";
                }

            }
            else if (position.Y >= HEIGHT - 1)
            {
                if (position.X <= 0)
                {
                    return "RIGHT";
                }
                else if (position.X >= WIDTH - 1)
                {
                    return "LEFT";
                }
                else
                {
                    return "RIGHT";
                }
            }

            return string.Empty;
        }

        private static void DisplayLightCyclePosition(Point position0, Point position1)
        {
            Console.Error.WriteLine($"Player position: ({position0.X},{position0.Y}) - ({position1.X},{position1.Y}) ");
        }

        private static void DisplayLightCyclePositions(Point[] positions0, Point[] positions1)
        {
            for (var i = 0; i < positions0.Length; i++)
            {
                Console.Error.WriteLine($"Enemy position: ({positions0[i].X},{positions0[i].Y}) - ({positions1[i].X},{positions1[i].Y}) ");
            }

        }
    }
}
