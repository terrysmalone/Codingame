using System;
using System.Collections.Generic;
using System.Drawing;

namespace _2026_1_WinterChallenge_SnakeByte;

internal sealed class SnakeState : IEquatable<SnakeState>
{ 
    internal Point Position { get; private set; }
    internal List<Point> Body { get; private set; }

    private readonly int _hashCode;

    internal SnakeState(Point position, List<Point> body)
    {
        Position = position;
        Body = body;
        _hashCode = CalculateHashCode();
    }

    private int CalculateHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(Position);
        foreach (var bodyPart in Body)
        {
            hash.Add(bodyPart);
        }

        return hash.ToHashCode();
    }

    public override int GetHashCode() => _hashCode;

    public override bool Equals(object? obj)
    {
        return Equals(obj as SnakeState);
    }

    public bool Equals(SnakeState? other)
    {
        if (other == null)
        {
            return false;
        }

        if (Position != other.Position)
        {
            return false;
        }

        if (Body.Count != other.Body.Count)
        {
            return false;
        }

        for (int i = 0; i < Body.Count; i++)
        {
            if (Body[i] != other.Body[i])
            {
                return false;
            }
        }
        
        return true;
    }
}