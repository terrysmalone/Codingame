using System;

namespace WinterChallenge2024;

internal static class Logger
{
    internal static void Line(string message)
    {
        Console.Error.WriteLine(message);
    }
}
