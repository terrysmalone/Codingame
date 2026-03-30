using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LineRacing;

internal static class Logger
{
    internal static void CandidateMoves(List<CandidateMove> candidateMoves)
    {
        foreach (var candidateMove in candidateMoves)
        {
            Console.Error.WriteLine($"Candidate move ({candidateMove.Move.X},{candidateMove.Move.Y})");

            if (candidateMove.Beam != null && candidateMove.Beam.Count > 0)
            {
                Console.Error.WriteLine($"Candidate move:  - Path: {string.Join(";", candidateMove.Beam.Select(p => $"({p.X},{p.Y})"))}");
            }

            Console.Error.WriteLine($"My space: {candidateMove.MySpace} - Enemy space: {candidateMove.EnemySpace}");
        }
    }

    internal static void LightCyclePosition(Point position0, Point position1)
    {
        Console.Error.WriteLine($"Player position: ({position0.X},{position0.Y}) - ({position1.X},{position1.Y}) ");
    }

    internal static void LightCyclePositions(Point[] positions0, Point[] positions1)
    {
        for (int i = 0; i < positions0.Length; i++)
        {
            Console.Error.WriteLine($"Enemy position: ({positions0[i].X},{positions0[i].Y}) - ({positions1[i].X},{positions1[i].Y}) ");
        }

    }
}
