namespace _2026_1_WinterChallenge_SnakeByte;

internal static class Logger    
{
    private static char _platformChar = '#';
    private static char _emptySpaceChar = '.';
    private static char _powerUpChar = '*';

    private static char _mySnakeHeadChar = '@';
    private static char _mySnakeBodyChar = 'o';

    private static char _opponentSnakeHeadChar = 'X';
    private static char _opponentSnakeBodyChar = 'x';

    internal static void Snakes(string debugMessage, List<SnakeBot> snakeBots)
    {
        Console.Error.WriteLine(debugMessage);
        Console.Error.WriteLine("-----------");
        foreach (var snakeBot in snakeBots)
        {
            Snake(snakeBot);
        }
    }

    internal static void Snake(SnakeBot snakeBot)
    {
        Console.Error.WriteLine($"SnakeBot {snakeBot.Id}: {string.Join(";", snakeBot.Body.Select(p => $"{p.X},{p.Y}"))}");
    }

    internal static void Platforms(bool[,] platforms)
    {
        Console.Error.WriteLine("Platforms:");
        for (int y = 0; y < platforms.GetLength(0); y++)
        {
            for (int x = 0; x < platforms.GetLength(1); x++)
            {
                if (platforms[y, x])
                {
                    Console.Error.Write(_platformChar);
                }
                else                
                {
                    Console.Error.Write(" ");
                }
            }
            Console.Error.WriteLine();
        }
    }

    internal static void EntireGame(bool[,] platforms, List<SnakeBot> mySnakeBots, List<SnakeBot> opponentSnakeBots, List<System.Drawing.Point> powerSources)
    {
        Console.Error.WriteLine("Platforms:");
        for (int y = 0; y < platforms.GetLength(0); y++)
        {
            for (int x = 0; x < platforms.GetLength(1); x++)
            {
                if (platforms[y, x])
                {
                    Console.Error.Write(_platformChar);
                }
                else
                {
                    bool charPlaced = false;
                    // check if a snake is in this position
                    foreach (var bot in mySnakeBots)
                    {
                        if (bot.Body.Any(p => p.X == x && p.Y == y))
                        {
                            if (bot.Body[0].X == x && bot.Body[0].Y == y)
                            {                                
                                Console.Error.Write(_mySnakeHeadChar);
                                charPlaced = true;
                                break;
                            }
                            else
                            {
                                Console.Error.Write(_mySnakeBodyChar);
                                charPlaced = true;
                                break;
                            }
                        }
                    }

                    if (charPlaced)
                    {
                        continue;
                    }

                    foreach (var bot in opponentSnakeBots)
                    {
                        if (bot.Body.Any(p => p.X == x && p.Y == y))
                        {
                            if (bot.Body[0].X == x && bot.Body[0].Y == y)
                            {
                                Console.Error.Write(_opponentSnakeHeadChar);
                                charPlaced = true;
                                break;
                            }
                            else
                            {
                                Console.Error.Write(_opponentSnakeBodyChar);
                                charPlaced = true;
                                break;
                            }
                        }
                    }

                    if (charPlaced)
                    {
                        continue;
                    }

                    foreach(var powerUp in powerSources)
                    {
                        if (powerUp.X == x && powerUp.Y == y)
                        {
                            Console.Error.Write(_powerUpChar);
                            charPlaced = true;
                            break;
                        }
                    }

                    if (charPlaced)
                    {
                        continue;
                    }



                    Console.Error.Write(_emptySpaceChar);
                }
            }
            Console.Error.WriteLine();
        }
    }
}