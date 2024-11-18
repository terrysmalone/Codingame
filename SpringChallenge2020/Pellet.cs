namespace SpringChallenge2020;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal struct Pellet(Point position, int value)
{
    internal Point Position = position;

    internal int Value = value;
}
