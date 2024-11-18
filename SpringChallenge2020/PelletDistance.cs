using System.Drawing;

namespace SpringChallenge2020;

internal struct PelletDistance(Point position, double[] distances)
{
    internal Point Position = position;
 
    internal double[] Distances = distances;
}