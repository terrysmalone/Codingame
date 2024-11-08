using System;
using System.Collections.Generic;
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

            List<Point> filledPositions = new List<Point>();

            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                int numberOfPlayers = int.Parse(inputs[0]); // total number of players (2 to 4).
                int playerNumber = int.Parse(inputs[1]); // your player number (0 to 3).

                Point[] enemyStartPositions = new Point[numberOfPlayers - 1];
                Point[] enemyEndPositions = new Point[numberOfPlayers - 1];

                int enemyIndex = 0;

                for (int i = 0; i < numberOfPlayers; i++)
                {
                    inputs = Console.ReadLine().Split(' ');

                    if (i == playerNumber)
                    {
                        playerStartPosition = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
                        playerEndPosition = new Point(int.Parse(inputs[2]), int.Parse(inputs[3]));

                        if (!filledPositions.Contains(playerStartPosition))
                        {
                            filledPositions.Add(playerStartPosition);
                        }

                        if (!filledPositions.Contains(playerEndPosition))
                        {
                            filledPositions.Add(playerEndPosition);
                        }
                    }
                    else
                    {
                        enemyStartPositions[enemyIndex] = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
                        enemyEndPositions[enemyIndex] = new Point(int.Parse(inputs[2]), int.Parse(inputs[3]));

                        if (!filledPositions.Contains(enemyStartPositions[enemyIndex]))
                        {
                            filledPositions.Add(enemyStartPositions[enemyIndex]);
                        }

                        if (!filledPositions.Contains(enemyEndPositions[enemyIndex]))
                        {
                            filledPositions.Add(enemyEndPositions[enemyIndex]);
                        }

                        enemyIndex++;
                    }
                }

                DisplayLightCyclePosition(playerStartPosition, playerEndPosition);
                DisplayLightCyclePositions(enemyStartPositions, enemyEndPositions);

                string nextMove = GetNextMove(playerEndPosition, currentDirection, filledPositions);

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

        private static string GetNextMove(Point position, string currentDirection, List<Point> filledPositions)
        {
            // Check left
            Point leftPos = new Point(position.X-1, position.Y);

            bool isViable = !(leftPos.X <= -1);

            if (isViable && !filledPositions.Contains(leftPos))
            {
                return "LEFT";
            }

            // Check right
            Point rightPos = new Point(position.X+1, position.Y);

            isViable = !(rightPos.X >= WIDTH);

            if (isViable && !filledPositions.Contains(rightPos))
            {
                return "RIGHT";
            }

            // Check down
            Point downPos = new Point(position.X, position.Y+1);

            isViable = !(downPos.Y <= -1);

            if (isViable && !filledPositions.Contains(downPos))
            {
                return "DOWN";
            }

            // Check up
            Point upPos = new Point(position.X, position.Y-1);

            isViable = !(upPos.Y <= HEIGHT);

            if (isViable && !filledPositions.Contains(upPos))
            {
                return "UP";
            }

            return "LEFT";
        }

        private static void DisplayLightCyclePosition(Point position0, Point position1)
        {
            Console.Error.WriteLine($"Player position: ({position0.X},{position0.Y}) - ({position1.X},{position1.Y}) ");
        }

        private static void DisplayLightCyclePositions(Point[] positions0, Point[] positions1)
        {
            for (int i = 0; i < positions0.Length; i++)
            {
                Console.Error.WriteLine($"Enemy position: ({positions0[i].X},{positions0[i].Y}) - ({positions1[i].X},{positions1[i].Y}) ");
            }

        }
    }
}
