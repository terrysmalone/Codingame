using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace BackTrackKing;

internal static class Logger
{

    private static bool DISABLE_LOGGING = false;

    private static bool DISABLE_TIMES = false;

    private static long _roundStartTime;
    private static long _lastTimedLog;

    private static List<TimeSpan> _roundTimes = new List<TimeSpan>();

    internal static void DisableLogging()
    {
        DISABLE_LOGGING = true;
    }

    internal static void EnableLogging()
    {
        DISABLE_LOGGING = false;
    }

    internal static void DisableTimes()
    {
        DISABLE_TIMES = true;
    }

    internal static void EnableTimes()
    {
        DISABLE_TIMES = false;
    }

    internal static void LogTime(string message)
    {
        if (DISABLE_TIMES)
        {
            return;
        }
        TimeSpan elapsedTime = Stopwatch.GetElapsedTime(_roundStartTime);
        TimeSpan elapsedSinceLastLog = Stopwatch.GetElapsedTime(_lastTimedLog);
        Console.Error.WriteLine($"{elapsedTime.TotalMilliseconds}ms({elapsedSinceLastLog.TotalMilliseconds}ms): {message}");
        _lastTimedLog = Stopwatch.GetTimestamp();
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

    internal static void EndRoundStopwatch()
    {
        if (DISABLE_TIMES)
        {
            return;
        }

        TimeSpan totalRoundTime = Stopwatch.GetElapsedTime(_roundStartTime);
        _roundTimes.Add(totalRoundTime);
        Console.Error.WriteLine($"Total round time: {totalRoundTime.TotalMilliseconds}ms");

        // Get an average of all round times
        TimeSpan averageRoundTime = new TimeSpan((long)_roundTimes.Average(t => t.Ticks));
        Console.Error.WriteLine($"Average round time: {averageRoundTime.TotalMilliseconds}ms");
    }



    internal static void Error(string message)
    {
        Console.Error.WriteLine("ERROR: " + message);
    }

    internal static void Message(string message)
    {
        if (DISABLE_LOGGING) 
        {
            return;
        }

        Console.Error.WriteLine(message);
    }

    internal static void DesirePaths(List<DesirePath> desirePaths)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine($"DESIRE PATHS");
        foreach (var desirePath in desirePaths)
        {
            Console.Error.WriteLine($"{desirePath.FullPath[0].X},{desirePath.FullPath[0].Y} -> {desirePath.FullPath[desirePath.FullPathCount-1].X},{desirePath.FullPath[desirePath.FullPathCount - 1].Y} - Path:{desirePath.RemainingPathCount}/{desirePath.FullPathCount}, Action:{desirePath.RemainingActionCount}/{desirePath.FullActionCount}, Me/Opponenet tracks:{desirePath.MyTracksOnPathCount}/{desirePath.OpponentTracksOnPathCount}");
        }
    }

    internal static void DesirePath(DesirePath desirePath)
    {
        Console.Error.WriteLine($"{desirePath.FullPath[0].X},{desirePath.FullPath[0].Y} -> {desirePath.FullPath[desirePath.FullPathCount - 1].X},{desirePath.FullPath[desirePath.FullPathCount - 1].Y} - Path:{desirePath.RemainingPathCount}/{desirePath.FullPathCount}, Action:{desirePath.RemainingActionCount}/{desirePath.FullActionCount}");
    }

    internal static void TypeMap(Map map)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                Console.Error.Write(ToSymbol(map.CellCosts[x, y]));
            }
            Console.Error.WriteLine();
        }
    }

    private static string ToSymbol(int cellCost)
    {
        switch (cellCost)
        {
                case 1:
                    return " ";
                case 2:
                    return "~";
                case 3:
                    return "^";
                default:
                    return " ";
        }
    }

    internal static void Regions(List<Region> regions)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine("REGIONS");

        foreach (var region in regions)
        {
            Console.Error.WriteLine($"{region.Id}: Connections: {string.Join(",", region.GetActiveConnections())}, Instability: {region.Instability}, Inked: {region.IsInked}, HasTown: {region.HasTown}, MyTracks: {region.GetMyTracks()}, EnemyTracks: {region.GetEnemyTracks()}");
        }
    }

    internal static void Connections(Dictionary<string, int> connections)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine("CONNECTIONS");

        foreach (var connection in connections)
        {
            Console.Error.WriteLine($"{connection.Key}: {connection.Value}");
        }
    }

    internal static void RegionScores(Dictionary<int, int> regionScores)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine("REGION SCORES");

        foreach (var regionScore in regionScores)
        {
            Console.Error.WriteLine($"{regionScore.Key}: {regionScore.Value}");
        }

    }

    internal static void ConnectionScoresMap(int[,] connectionScoresMap)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine("CONNECTION SCORES MAP");

        for (int y = 0; y < connectionScoresMap.GetLength(1); y++)
        {
            for (int x = 0; x < connectionScoresMap.GetLength(0); x++)
            {
                Console.Error.Write($"{connectionScoresMap[x, y]} ");
            }
            Console.Error.WriteLine();
        }
    }

    internal static void TrackCandidates(List<TrackCandidate> candidates)
    {
        if (DISABLE_LOGGING)
        {
            return;
        }

        Console.Error.WriteLine("TRACK CANDIDATES");

        foreach (var trackCandidate in candidates)
        {
            Console.Error.WriteLine($"Cell: {trackCandidate.CellPosition.X},{trackCandidate.CellPosition.Y} - IsWorthwhile: {trackCandidate.IsPathWorthwhile} - Region: {trackCandidate.RegionId}, Cost: {trackCandidate.ActionCost}, ShortestPathCount: {trackCandidate.ShortestRemainingCount()}, TownsOnPathCount: {trackCandidate.GetTownCount()}, SafeRegion: {trackCandidate.IsInSafeRegion}, Instability: {trackCandidate.InstabilityLevel}");
        }
    }
}