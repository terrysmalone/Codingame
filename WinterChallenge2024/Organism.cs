using System;
using System.Collections.Generic;
using System.Drawing;

namespace WinterChallenge2024;

internal class Organism
{
    internal int RootId { get; private set; }

    internal List<Organ> Organs { get; private set; }

    internal Organism(int rootId)
    {
        Organs = new List<Organ>();
        RootId = rootId;
    }

    internal void AddOrgan(Organ organ)
    {
        Organs.Add(organ);
    }


}
