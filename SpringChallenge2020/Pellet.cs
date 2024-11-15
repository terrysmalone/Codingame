namespace SpringChallenge2020; 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal struct Pellet
{
    internal int X;
    internal int Y;

    internal int Value;

    public Pellet(int x, int y, int value)
    {
        X = x;
        Y = y;
        Value = value;
    }
}