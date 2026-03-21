using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace _2026_1_WinterChallenge_SnakeByte;

// https://www.codingame.com/ide/challenge/winter-challenge-2026-exotec
internal class Player
{
    static void Main(string[] args)
    {        
        string[] inputs;
        int myId = int.Parse(Console.ReadLine());
        int width = int.Parse(Console.ReadLine());
        int height = int.Parse(Console.ReadLine());


        bool[,] platforms = new bool[height, width];
        for (int y = 0; y < height; y++)
        {            
            string row = Console.ReadLine();

            char[] splitRow = row.ToCharArray();
            for (int x = 0; x < width; x++)
            {
                platforms[y, x] = splitRow[x] == '#';
            }
        }

        Game game = new Game(width, height, platforms);

        int snakebotsPerPlayer = int.Parse(Console.ReadLine());
        for (int i = 0; i < snakebotsPerPlayer; i++)
        {
            int mySnakebotId = int.Parse(Console.ReadLine());
            game.AddMySnake(new SnakeBot(mySnakebotId));

        }
        for (int i = 0; i < snakebotsPerPlayer; i++)
        {
            int oppSnakebotId = int.Parse(Console.ReadLine());
            game.AddOpponentSnake(new SnakeBot(oppSnakebotId));
        }

        while (true)
        { 
            game.MarkAllSnakesForRemoval();

            game.RemoveAllPowerSources();
            int powerSourceCount = int.Parse(Console.ReadLine());
            Logger.StartRoundStopwatch();
            Logger.LogTime("START OF TURN");

            for (int i = 0; i < powerSourceCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);

                game.AddPowerSource(x, y);
            }

            int snakebotCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < snakebotCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int snakebotId = int.Parse(inputs[0]);
                string body = inputs[1];

                SnakeBot snakeBot = game.GetSnake(snakebotId);

                if (snakeBot != null)
                {
                    snakeBot.Remove = false;
                    snakeBot.Body.Clear();
                    string[] segments = body.Split(':');
                    foreach (string segment in segments)
                    {
                        string[] coordinates = segment.Split(',');
                        int x = int.Parse(coordinates[0]);
                        int y = int.Parse(coordinates[1]);
                        snakeBot.Body.Add(new Point(x, y));
                    }
                }
            }

            Logger.LogTime("PARSED ALL INPUT");

            game.RemoveMarkedSnakes();

            List<string> actions = game.GetActions();

            Logger.LogTime("END OF TURN");
            Console.WriteLine(string.Join(";", actions));
        }
    }
}
