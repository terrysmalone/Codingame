using System;
using System.Collections.Generic;
using System.Drawing;

namespace WinterChallenge2024;

internal sealed class ActionFinder
{
    private readonly AStar _aStar;

    private List<Protein> _proteinsToCheck = new List<Protein>();

    public ActionFinder(Game game)
    {
        _aStar = new AStar(game);
    }

    internal List<Tuple<int, ProteinType, List<Point>>> GetShortestPathsToProteins(Organism organism, List<Protein> proteins, int maxDistance)
    {
        List<Tuple<int, ProteinType, List<Point>>> paths = new List<Tuple<int, ProteinType, List<Point>>>();

        _proteinsToCheck = new List<Protein>();

        foreach (Protein protein in proteins)
        {
            if (!protein.IsHarvested)
            {
                _proteinsToCheck.Add(protein.Clone());
            }
        }

        if (_proteinsToCheck.Count == 0)
        {
            return paths;
        }

        // Search at max distance of 2
        paths.AddRange(GetShortestPathsToProteins(organism, 2));

        // Search at max distance of 3, being willing to walk over other proteins
        // Search at max distance of 3, being not willing to walk over other proteins

        // Search at max distance of 4, being willing to walk over other proteins
        // Search at max distance of 4, being not willing to walk over other proteins

        // Search at max distance of 5, being willing to walk over other proteins
        // Search at max distance of 5, being not willing to walk over other proteins

        return paths;   
    }

    private IEnumerable<Tuple<int, ProteinType, List<Point>>> GetShortestPathsToProteins(Organism organism, int maxDistance)
    {
        List<Tuple<int, ProteinType, List<Point>>> paths = new List<Tuple<int, ProteinType, List<Point>>>();

        foreach (Protein protein in _proteinsToCheck)
        {
            foreach (Organ organ in organism.Organs)
            {
                int manhattanDistance = MapChecker.CalculateManhattanDistance(organ.Position, protein.Position);
                
                if (manhattanDistance > maxDistance)
                {
                    continue;
                }

                List<Point> path = _aStar.GetShortestPath(organ.Position, protein.Position, maxDistance);
                
                if (path.Count > 0)
                {
                    paths.Add(new Tuple<int, ProteinType, List<Point>>(organ.Id, protein.Type, path));
                }
            }
        }
        return paths;
    }
}
