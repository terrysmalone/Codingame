using System;

namespace SkynetRevolution
{
    /**
     * Auto-generated code below aims at helping you parse
     * the standard input according to the problem statement.
    **/
    class Player
    {
        static void Main(string[] args)
        {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');
            var numberOfNodes = int.Parse(inputs[0]); // the total number of nodes in the level, including the gateways
            var numberOfLinks = int.Parse(inputs[1]); // the number of links
            var numberOfExitGateways = int.Parse(inputs[2]); // the number of exit gateways
            for (var i = 0; i < numberOfLinks; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var nodeLink1 = int.Parse(inputs[0]); // N1 and N2 defines a link between these nodes
                var nodeLink2 = int.Parse(inputs[1]);
            }
            for (var i = 0; i < numberOfExitGateways; i++)
            {
                var exitGatewayIndex = int.Parse(Console.ReadLine()); // the index of a gateway node
            }

            // game loop
            while (true)
            {
                var skynetAgentPosition = int.Parse(Console.ReadLine()); // The index of the node on which the Skynet agent is positioned this turn
                Console.Error.WriteLine($"skynetAgentPosition: {skynetAgentPosition}");
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");


                // Example: 0 1 are the indices of the nodes you wish to sever the link between
                Console.WriteLine("1 2");
            }
        }
    }