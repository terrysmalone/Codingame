using System;
using System.Diagnostics;

namespace Connect4;

internal static class Logger
{
    private static long _roundStartTime;
    private static long _lastTimedLog;

    internal static void Board(int[,] board)
    {
        for (int row = 0; row < board.GetLength(0); row++)
        {
            string rowText = "|";
            for (int column = 0; column < board.GetLength(1); column++)
            {
                if (board[row, column] == 0)
                {
                    rowText += "  |";
                }
                else if (board[row, column] == 1)
                {
                    rowText += " 1|";
                }
                else
                {
                    rowText += "-1|";
                }                
            }
            Console.Error.WriteLine(rowText);
        }
    }

    internal static void StartRoundStopwatch()
    {   
        _roundStartTime = Stopwatch.GetTimestamp();
        _lastTimedLog = Stopwatch.GetTimestamp();
    }

    internal static void LogTime(string message)
    {
        TimeSpan elapsedTime = Stopwatch.GetElapsedTime(_roundStartTime);
        TimeSpan elapsedSinceLastLog = Stopwatch.GetElapsedTime(_lastTimedLog);
        Console.Error.WriteLine($"{elapsedTime.TotalMilliseconds}({elapsedSinceLastLog.TotalMilliseconds}): {message}");
        _lastTimedLog = Stopwatch.GetTimestamp();
    }
}