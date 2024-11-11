namespace Fall2024Challenge_SeleniaCity;

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{   
    static void Main(string[] args)
    {
        Game game = new Game();

        // game loop
        while (true)
        {
            int resources = int.Parse(Console.ReadLine());
            game.SetResources(resources);

            ParseTravelRoutes(game);
            ParsePods(game);
            ParseBuildings(game);

            string actions = game.GetActions();

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine(actions);
        }
    }

    private static void ParseTravelRoutes(Game game)
    {
        List<Tube> tubes = new List<Tube>();
        List<Teleporter> teleporters = new List<Teleporter>();

        int numTravelRoutes = int.Parse(Console.ReadLine());

        for (int i = 0; i < numTravelRoutes; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int buildingId1 = int.Parse(inputs[0]);
            int buildingId2 = int.Parse(inputs[1]);
            int capacity = int.Parse(inputs[2]);

            if (capacity == 0)
            {
                teleporters.Add(new Teleporter(buildingId1, buildingId2));

            }
            else
            {
                tubes.Add(new Tube(buildingId1, buildingId2, capacity));
            }
        }

        game.SetTubes(tubes);
    }

    private static void ParsePods(Game game)
    {
        List<Pod> pods = new List<Pod>();
        int numPods = int.Parse(Console.ReadLine());
        for (int i = 0; i < numPods; i++)
        {
            string podProperties = Console.ReadLine();

            string[] properties = podProperties.Split(" ");

            int[] path = new int[properties.Length - 2];

            for (int j = 0; j < path.Length; j++)
            {
                path[j] = int.Parse(properties[j + 2]);
            }

            pods.Add(new Pod(int.Parse(properties[0]), int.Parse(properties[1]), path));
        }

        game.SetPods(pods);
    }

    private static void ParseBuildings(Game game)
    {
        List<Module> modules = new List<Module>();
        List<LandingPad> landingPads = new List<LandingPad>();

        int numNewBuildings = int.Parse(Console.ReadLine());
        for (int i = 0; i < numNewBuildings; i++)
        {
            string buildingProperties = Console.ReadLine();

            string[] props = buildingProperties.Split(" ");

            int type = int.Parse(props[0]);
            int id = int.Parse(props[1]);
            Point position = new Point(int.Parse(props[2]), int.Parse(props[3]));

            if (type == 0)
            {
                int[] astronauts = new int[props.Length - 4];

                for (int j = 0; j < astronauts.Length; j++)
                {
                    astronauts[j] = int.Parse(props[j + 4]);
                }

                landingPads.Add(new LandingPad(id, position, astronauts));
            }
            else
            {
                modules.Add(new Module(id, type, position));
            }
        }

        game.AddLandingPads(landingPads);
        game.AddModules(modules);
    }
}
