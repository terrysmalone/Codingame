/**************************************************************
  This file was generated by FileConcatenator.
  It combined all classes in the project to work in Codingame.
  This hasn't been put in a namespace to allow for class 
  name duplicates.
***************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;

    public class Player
    {
        const int WIDTH = 30;
        const int HEIGHT = 20;

        static void Main(string[] args)
        {
            string currentDirection = "LEFT";
            string[] inputs;

            var playerStartPosition = new Point(-1, -1);
            var playerEndPosition = new Point(-1, -1);

            var filledPositions = new List<Point>();

            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                var numberOfPlayers = int.Parse(inputs[0]); // total number of players (2 to 4).
                var playerNumber = int.Parse(inputs[1]); // your player number (0 to 3).

                Point[] enemyStartPositions = new Point[numberOfPlayers - 1];
                Point[] enemyEndPositions = new Point[numberOfPlayers - 1];

                var enemyIndex = 0;

                for (var i = 0; i < numberOfPlayers; i++)
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

                var nextMove = GetNextMove(playerEndPosition, currentDirection, filledPositions);

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
            var leftPos = new Point(position.X-1, position.Y);

            var isViable = !(leftPos.X <= 0);

            if (isViable && !filledPositions.Contains(leftPos))
            {
                return "LEFT";
            }

            // Check right
            var rightPos = new Point(position.X+1, position.Y);

            isViable = !(rightPos.X >= WIDTH - 1);

            if (isViable && !filledPositions.Contains(rightPos))
            {
                return "RIGHT";
            }

            // Check down
            var downPos = new Point(position.X, position.Y+1);

            isViable = !(downPos.Y <= 0);

            if (isViable && !filledPositions.Contains(downPos))
            {
                return "DOWN";
            }

            // Check up
            var upPos = new Point(position.X, position.Y-1);

            isViable = !(upPos.Y <= HEIGHT - 1);

            if (isViable && !filledPositions.Contains(upPos))
            {
                return "Up";
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
