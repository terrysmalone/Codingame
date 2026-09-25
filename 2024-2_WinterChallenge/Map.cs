using System;
using System.Collections.Generic;
using System.Linq;

namespace WinterChallenge2024;

internal sealed class Map
{
    internal int Width { get; private set; }

    internal int Height { get; private set; }

    private bool[,] _walls;

    private bool[,] _sporerPoints;

    private bool[,] _isBlocked;

    private bool[,] _hasAnyProtein;

    private ProteinType[,] _proteinTypes;

    private bool[,] _hasHarvestedProtein;

    private bool[,] _opponentOrgans;

    private bool[,] _opponentOrganEdges;

    private int[,] _opponentOrganChildren;

    private bool[,] _opponentTentaclePath;

    internal Map(int width, int height)
    {
        Width = width;
        Height = height;

        _walls = new bool[width, height];

        ResetMaps();
    }

    internal void ResetMaps()
    {
        _sporerPoints = new bool[Width, Height];
        _isBlocked = new bool[Width, Height];
        _hasAnyProtein = new bool[Width, Height];
        _proteinTypes = new ProteinType[Width, Height];
        _hasHarvestedProtein = new bool[Width, Height];
        _opponentOrgans = new bool[Width, Height];
        _opponentOrganEdges = new bool[Width, Height];
        _opponentOrganChildren = new int[Width, Height];
        _opponentTentaclePath = new bool[Width, Height];
    }

    internal void UpdateIsBlocked(List<Organism> playerOrganisms, List<Organism> opponentOrganisms)
    {
        // Not walkable if player organ on that spot
        foreach (Organism organism in playerOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                _isBlocked[organ.Position.X, organ.Position.Y] = true;
            }
        }

        // Not walkable if opponent organ on that spot
        foreach (Organism organism in opponentOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                _isBlocked[organ.Position.X, organ.Position.Y] = true;
            }
        }

        // Not walkable if wall on that spot
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_walls[x, y])
                {
                    _isBlocked[x, y] = true;
                }
            }
        }
    }

    internal void UpdateHasProteins(List<Protein> proteins)
    {
        foreach (Protein protein in proteins)
        {
            _hasAnyProtein[protein.Position.X, protein.Position.Y] = true;

            _proteinTypes[protein.Position.X, protein.Position.Y] = protein.Type;

            if (protein.IsHarvested)
            {
                _hasHarvestedProtein[protein.Position.X, protein.Position.Y] = true;
            }
        }
    }

    internal void UpdateOpponentOrgans(List<Organism> opponentOrganisms)
    {
        foreach (Organism organism in opponentOrganisms)
        {
            foreach (Organ organ in organism.Organs)
            {
                _opponentOrgans[organ.Position.X, organ.Position.Y] = true;

                int childCount = GetChildCount(opponentOrganisms, organism.RootId, organ);
                _opponentOrganChildren[organ.Position.X, organ.Position.Y] = childCount;

                // We can't walk on an outward facing tentacle
                // So add these to the isBlocked list and not to the valid edges

                // North
                if (organ.Position.Y - 1 >= 0)
                {
                    _opponentOrganEdges[organ.Position.X, organ.Position.Y - 1] = true;

                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.N)
                    {
                        _opponentTentaclePath[organ.Position.X, organ.Position.Y - 1] = true;
                    }
                }

                // East
                if (organ.Position.X + 1 < Width)
                {
                    _opponentOrganEdges[organ.Position.X + 1, organ.Position.Y] = true;

                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.E)
                    {
                        _opponentTentaclePath[organ.Position.X + 1, organ.Position.Y] = true;
                    }
                }

                // South
                if (organ.Position.Y + 1 < Height)
                {
                    _opponentOrganEdges[organ.Position.X, organ.Position.Y + 1] = true;

                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.S)
                    {
                        _opponentTentaclePath[organ.Position.X, organ.Position.Y + 1] = true;
                    }
                }

                // WEST
                if (organ.Position.X - 1 >= 0)
                {
                    _opponentOrganEdges[organ.Position.X - 1, organ.Position.Y] = true;

                    if (organ.Type == OrganType.TENTACLE && organ.Direction == OrganDirection.W)
                    {
                        _opponentTentaclePath[organ.Position.X - 1, organ.Position.Y] = true;
                    }
                }
            }
        }
    }

    private static int GetChildCount(List<Organism> opponentOrganisms, int organismId, Organ organ)
    {
        int count = 0;
        if (opponentOrganisms.First(o => o.RootId == organismId).Organs.Any(o => o.ParentId == organ.Id))
        {
            List<Organ> children = opponentOrganisms.First(o => o.RootId == organismId).Organs.Where(o => o.ParentId == organ.Id).ToList();

            count += children.Count;

            foreach (Organ child in children)
            {
                count += GetChildCount(opponentOrganisms, organismId, child);
            }
        }

        return count;
    }

    internal void SetWalls(bool[,] walls)
    {
        _walls = walls;
    }

    internal bool IsBlocked(int x, int y)
    {
        return _isBlocked[x, y];
    }

    internal bool HasOpponentTentaclePath(int x, int y)
    {
        return _opponentTentaclePath[x, y];
    }

    internal int OpponentOrganChildren(int x, int y)
    {
        return _opponentOrganChildren[x, y];
    }

    internal void SetSporerPoints(int x, int y, bool isSporePoint)
    {
        _sporerPoints[x, y] = isSporePoint;
    }

    internal bool IsSporerPoint(int x, int y)
    {
        return _sporerPoints[x, y];
    }

    internal bool HasHarvestedProtein(int x, int y)
    {
        return _hasHarvestedProtein[x, y];
    }

    internal bool HasAnyProtein(int x, int y)
    {
        return _hasAnyProtein[x, y];
    }

    internal bool HasWall(int x, int y)
    {
        return _walls[x, y];
    }
}

