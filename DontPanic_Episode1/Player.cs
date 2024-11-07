using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;


// https://www.codingame.com/training/medium/don't-panic-episode-1
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        var nbFloors = int.Parse(inputs[0]); // number of floors
        var width = int.Parse(inputs[1]); // width of the area
        var nbRounds = int.Parse(inputs[2]); // maximum number of rounds
        var exitFloor = int.Parse(inputs[3]); // floor on which the exit is found
        var exitPos = int.Parse(inputs[4]); // position of the exit on its floor
        var nbTotalClones = int.Parse(inputs[5]); // number of generated clones
        var nbAdditionalElevators = int.Parse(inputs[6]); // ignore (always zero)
        var nbElevators = int.Parse(inputs[7]); // number of elevators
        
        var elevators = new Dictionary<int,int>();
        
        for (var i = 0; i < nbElevators; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            var elevatorFloor = int.Parse(inputs[0]); // floor on which this elevator is found
            var elevatorPos = int.Parse(inputs[1]); // position of the elevator on its floor
            
            elevators.Add(elevatorFloor, elevatorPos);
        }

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            var cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
            var clonePos = int.Parse(inputs[1]); // position of the leading clone on its floor
            var direction = inputs[2]; // direction of the leading clone: LEFT or RIGHT
            
            if(cloneFloor == exitFloor)
            {
                if(direction == "LEFT" && clonePos < exitPos)
                {
                    Console.WriteLine("BLOCK");
                }
                else if(direction == "RIGHT" && clonePos > exitPos)
                {
                    Console.WriteLine("BLOCK");
                }
                else
                {
                    Console.WriteLine("WAIT");
                }
            }
            else
            {
                var elevatorPos = -1;
                
                if(elevators.ContainsKey(cloneFloor))
                {
                    elevatorPos = elevators[cloneFloor];
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                if(direction == "LEFT" && (elevatorPos != -1 && clonePos < elevatorPos) || clonePos == 0)
                {
                    Console.WriteLine("BLOCK");
                }
                else if(direction == "RIGHT" &&  (elevatorPos != -1 && clonePos > elevatorPos) || clonePos > width-2)
                {
                    Console.WriteLine("BLOCK");
                }
                else
                {
                    Console.WriteLine("WAIT"); // action: WAIT or BLOCK
                }
            }
        }
    }
}