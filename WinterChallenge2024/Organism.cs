using System;
using System.Collections.Generic;
using System.Drawing;

namespace WinterChallenge2024;

internal struct Organism
{
    internal Organ Root { get; private set; }
    internal List<Organ> Organs { get; private set; }

    public Organism()
    {
        Organs = new List<Organ>();
    }

    internal void SetRoot(int id, Point root)
    {
        Root = new Organ(id, root);
    }

    internal readonly void AddOrgan(int organId, Point point)
    {
        Organs.Add(new Organ(organId, point));
    }
}
