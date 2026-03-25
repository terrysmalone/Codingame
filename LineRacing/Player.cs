using System;
using System.Drawing;

namespace LineRacing;

public class Player
{
    static void Main(string[] args)
    {
        const int WIDTH = 30;
        const int HEIGHT = 20;

        var game = new Game(WIDTH, HEIGHT);

        string currentDirection = "LEFT";

        string[] inputs;

        bool firstTurn = true;

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int numberOfPlayers = int.Parse(inputs[0]); // total number of players (2 to 4).
            int playerNumber = int.Parse(inputs[1]); // your player number (0 to 3).

            for (int i = 0; i < numberOfPlayers; i++)
            {
                inputs = Console.ReadLine().Split(' ');

                if (i == playerNumber)
                {
                    Point playerStartPosition = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
                    Point playerEndPosition = new Point(int.Parse(inputs[2]), int.Parse(inputs[3]));

                    if (!firstTurn)
                    {
                        game.UpdateMyPosition(playerStartPosition);
                    }

                    game.UpdateMyPosition(playerEndPosition);
                }
                else
                {
                    Point enemyStartPosition = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));
                    Point enemyEndPosition = new Point(int.Parse(inputs[2]), int.Parse(inputs[3]));

                    if (firstTurn)
                    {
                        Console.Error.WriteLine($"Enemy start position: {enemyStartPosition}");
                        game.UpdateEnemyPosition(enemyStartPosition);
                    }

                    if (enemyEndPosition.X != -1 && enemyEndPosition.Y != -1)
                    {
                        game.UpdateEnemyPosition(enemyEndPosition);
                    }
                }
            }
            string nextMove = game.GetNextMove();

            if (nextMove != string.Empty)
            {
                currentDirection = nextMove;
            }

            firstTurn = false;

            Console.WriteLine(currentDirection);
        }
    }
}

