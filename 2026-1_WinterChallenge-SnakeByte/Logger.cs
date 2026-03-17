using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace _2026_1_WinterChallenge_SnakeByte;

internal static class Logger    
{
    private static bool DISABLE_LOGGING = true;
    private static bool DISABLE_TIMES = true;

    private static long _roundStartTime;
    private static long _lastTimedLog;

    private static char _platformChar = '#';
    private static char _emptySpaceChar = '.';
    private static char _powerUpChar = '*';

    private static char _mySnakeHeadChar = '@';
    private static char _mySnakeBodyChar = 'o';

    private static char _opponentSnakeHeadChar = 'X';
    private static char _opponentSnakeBodyChar = 'x';

    internal static void Message(string message)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine(message);
    }
    internal static void Snakes(string debugMessage, List<SnakeBot> snakeBots)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine(debugMessage);
        Console.Error.WriteLine("-----------");
        foreach (var snakeBot in snakeBots)
        {
            Snake(snakeBot);
        }
    }

    internal static void Snake(SnakeBot snakeBot)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine($"SnakeBot {snakeBot.Id}: {string.Join(";", snakeBot.Body.Select(p => $"{p.X},{p.Y}"))}");
    }

    internal static void Platforms(bool[,] platforms)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

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
        if (DISABLE_LOGGING)
        {
            return;
        }

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

    internal static void Plans(string message, List<Plan> plans)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine(message);

        foreach (var plan in plans)
        {
            Console.Error.WriteLine($"Plan Type: {plan.PlanType}, Score: {plan.Score}, Turns to Fruition: {plan.TurnsToFruition}, Moves: {string.Join(";", plan.Moves.Select(p => $"{p.X},{p.Y}"))}");
        }
    }

    internal static void DisableLogging() 
    {
        DISABLE_LOGGING = true;
    }

    internal static void EnableLogging()
    {
        DISABLE_LOGGING = false;
    }

    internal static void StartRoundStopwatch()
    {
        if (DISABLE_TIMES)
        {
            return;
        }
        _roundStartTime = Stopwatch.GetTimestamp();
        _lastTimedLog = Stopwatch.GetTimestamp();
    }

    internal static void LogTime(string message)
    {
        if (DISABLE_TIMES)
        {
            return;
        }
        TimeSpan elapsedTime = Stopwatch.GetElapsedTime(_roundStartTime);
        TimeSpan elapsedSinceLastLog = Stopwatch.GetElapsedTime(_lastTimedLog);
        Console.Error.WriteLine($"{elapsedTime.TotalMilliseconds}({elapsedSinceLastLog.TotalMilliseconds}): {message}");
        _lastTimedLog = Stopwatch.GetTimestamp();
    }

    internal static void PlanCombinations(Dictionary<List<Plan>, int> planCombinations, int showTop)
    {
        if (DISABLE_LOGGING || planCombinations == null) 
        {
            return;
        }

        Console.Error.WriteLine("Plan Combinations:");
        for (int i = 0; i < Math.Min(showTop, planCombinations.Count); i++)
        {
            var kvp = planCombinations.ElementAt(i);
            var plans = kvp.Key;
            var score = kvp.Value;
            Console.Error.WriteLine($"Score: {score}");
            foreach (var plan in plans)
            {
                Console.Error.WriteLine($"  Plan Type: {plan.PlanType}, Score: {plan.Score}, Turns to Fruition: {plan.TurnsToFruition}, Moves: {string.Join(";", plan.Moves.Select(p => $"{p.X},{p.Y}"))}");
            }
        }
    }
}