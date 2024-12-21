﻿using System.ComponentModel;
using System.Drawing;

namespace WinterChallenge2024;

internal struct Organ
{
    internal int Id { get; private set; }

    public OrganType Type { get; set; }

    internal Point Position { get; private set; }

    internal OrganDirection Direction { get; private set; }

    public Organ(int id, OrganType type, Point position) : this()
    {
        Id = id;
        Type = type;
        Position = position;
    }

    public Organ(int id, OrganType type, Point position, OrganDirection direction) : this(id, type, position)
    {
        Direction = direction;
    }
}