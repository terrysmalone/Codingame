using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            var x = int.Parse(inputs[0]);
            var y = int.Parse(inputs[1]);
            var nextCheckpointX = int.Parse(inputs[2]); // x position of the next check point
            var nextCheckpointY = int.Parse(inputs[3]); // y position of the next check point
            var nextCheckpointDist = int.Parse(inputs[4]); // distance to the next checkpoint
            var nextCheckpointAngle = int.Parse(inputs[5]); // angle between your pod orientation and the direction of the next checkpoint
            inputs = Console.ReadLine().Split(' ');
            var opponentX = int.Parse(inputs[0]);
            var opponentY = int.Parse(inputs[1]);

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // You have to output the target position
            // followed by the power (0 <= thrust <= 100)
            // i.e.: "x y thrust"
            Console.WriteLine(nextCheckpointX + " " + nextCheckpointY + " 80");
        }
    }
}