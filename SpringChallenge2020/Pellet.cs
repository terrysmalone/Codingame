namespace SpringChallenge2020; 

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal struct Pellet
{
    internal Point Position;

    internal int Value;

    public Pellet(Point position, int value)
    {
        Position = position;
        Value = value;
    }
}