using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

/**
 * Save the Planet.
 * Use less Fossil Fuel.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;

        List<Point> surface = new List<Point>();

        int numberOfSurfacePoints = int.Parse(Console.ReadLine());

        for (int i = 0; i < numberOfSurfacePoints; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int landX = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
            int landY = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.
            
            surface.Add(new Point(landX, landY));
        }
        
        // Get landing location
        surface = surface.OrderBy(s => s.X).ToList();

        Point previousPoint = surface[0];

        Point landingSpaceStart = new Point(-1, -1);
        Point landingSpaceEnd = new Point(-1, -1);
        
        for (int i = 1; i < surface.Count; i++)
        {
            Console.Error.WriteLine($"{surface[i].X}, {surface[i].Y}");
            
            if(previousPoint.Y == surface[i].Y)
            {
                landingSpaceStart = previousPoint;
                landingSpaceEnd = surface[i];
                
                break;
            }
            
            previousPoint = surface[i];
        }
        
        Console.Error.WriteLine($"start {landingSpaceStart.X}, {landingSpaceStart.Y}");
        Console.Error.WriteLine($"end {landingSpaceEnd.X}, {landingSpaceEnd.Y}");

        //var landingTarget = new Point(landingSpaceStart.X + ((landingSpaceEnd.X - landingSpaceStart.X) / 2), landingSpaceStart.Y);


        int targetThrust = 4;
        bool straightenUp = false;
        
        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            Point landerPoint = new Point(int.Parse(inputs[0]), int.Parse(inputs[1]));

            int horizontalSpeed = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
            int verticalSpeed = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.
            int fuel = int.Parse(inputs[4]); // the quantity of remaining fuel in liters.
            int rotation = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
            int thrust = int.Parse(inputs[6]); // the thrust power (0 to 4).
            
            // Basic implementation
            
            if(straightenUp)
            {
                // We need to get horizontal speed to 0
                if(horizontalSpeed > 0)
                {
                    int angle = 70;
                    int speed;

                    if (horizontalSpeed > -30) { speed = 1; }
                    else if (horizontalSpeed > -60) { speed = 2; }
                    else if (horizontalSpeed > -90) { speed = 3; }
                    else { speed = 4; }

                    Console.WriteLine($"{angle} {speed}");
                }
                else if(horizontalSpeed < 0)
                {
                    int angle = -70;
                    int speed;
                    
                    if (horizontalSpeed < 30) { speed = 1; }
                    else if (horizontalSpeed < 60) { speed = 2; }
                    else if (horizontalSpeed < 90) { speed = 3; }
                    else { speed = 4; }
                    
                    Console.WriteLine($"{angle} {speed}");
                }
                else
                {
                    straightenUp = false;
                    Console.WriteLine($"{0} {0}");
                }
            }
            else
            {
                // Move above the landing
                if(landerPoint.X < landingSpaceStart.X) // Go right
                {
                    Console.WriteLine($"{-60} {3}");
                } 
                else if (landerPoint.X > landingSpaceEnd.X) // Go left
                {
                    Console.WriteLine($"{60} {3}");
                }
                else // Make an approach
                {
                    straightenUp = true;
                    Console.WriteLine($"{0} {0}");
                    
                    //targetThrust = 2;
                    //Console.WriteLine($"{0} {targetThrust}");
                }
            }
            
            
            
            

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            
            // R P. R is the desired rotation angle. P is the desired thrust power.
            //Console.WriteLine($"{rotation} {thrust}");
        }
    }
}