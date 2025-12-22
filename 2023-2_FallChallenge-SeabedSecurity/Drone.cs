namespace _2023_2_FallChallenge_SeabedSecurity;
using System.Drawing;

internal sealed class Drone
{
    internal int Id { get; private set; }

    internal Point Position { get; set; }
    internal int BatteryLevel { get; set; }
    
    internal Drone(int id, int xPos, int yPos, int batteryLevel)
    {
        Id = id;
        Position = new Point(xPos, yPos);
        BatteryLevel = batteryLevel;
    }
}