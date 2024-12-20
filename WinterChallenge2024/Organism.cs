using System;
using System.Collections.Generic;
using System.Drawing;

namespace WinterChallenge2024;

internal struct Organism
{
    internal List<Organ> Organs { get; private set; }

    public Organism()
    {
        Organs = new List<Organ>();
    }

    internal void AddRoot(int id, Point root)
    {
        Organs.Add(new Organ(id, OrganType.ROOT, root));
    }

    internal readonly void AddBasicOrgan(int organId, Point point)
    {
        Organs.Add(new Organ(organId, OrganType.BASIC, point));
    }

    internal readonly void AddHarvesterOrgan(int organId, Point point, OrganDirection direction)
    {
        Organs.Add(new Organ(organId, OrganType.HARVESTER, point, direction));
    }
}
