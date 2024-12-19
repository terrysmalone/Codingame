using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net;

namespace WinterChallenge2024;

/**
 * Grow and multiply your organisms to end up larger than your opponent.
 **/
partial class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]); // columns in the game grid
        int height = int.Parse(inputs[1]); // rows in the game grid

        Game game = new Game(width, height);

        // game loop
        while (true)
        {
            Organism playerOrganism = new Organism();
            Organism opponentOrganism = new Organism();
            List<Point> walls = new List<Point>();
            List<Protein> proteins = new List<Protein>();

            int entityCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]); // grid coordinate
                string type = inputs[2]; // WALL, ROOT, BASIC, TENTACLE, HARVESTER, SPORER, A, B, C, D
                int owner = int.Parse(inputs[3]); // 1 if your organ, 0 if enemy organ, -1 if neither
                int organId = int.Parse(inputs[4]); // id of this entity if it's an organ, 0 otherwise
                string organDir = inputs[5]; // N,E,S,W or X if not an organ
                int organParentId = int.Parse(inputs[6]);
                int organRootId = int.Parse(inputs[7]);

                switch (type)
                {
                    case "WALL":
                        walls.Add(new Point(x, y));
                        break;
                    case "ROOT":
                        if (owner == 1)
                        {
                            playerOrganism.SetRoot(organId, new Point(x, y));
                        } 
                        else if (owner == 0)
                        {
                            opponentOrganism.SetRoot(organId, new Point(x, y));
                        }
                        break;
                    case "BASIC":
                        if (owner == 1)
                        {
                            playerOrganism.AddOrgan(organId, new Point(x, y));
                        }
                        else if (owner == 0)
                        {
                            opponentOrganism.AddOrgan(organId, new Point(x, y));
                        }
                        break;
                    case "A":
                        proteins.Add(new Protein(ProteinType.A, new Point(x, y)));
                        break;
                    case "B":
                        proteins.Add(new Protein(ProteinType.B, new Point(x, y)));
                        break;
                    case "C":
                        proteins.Add(new Protein(ProteinType.C, new Point(x, y)));
                        break;
                    case "D":
                        proteins.Add(new Protein(ProteinType.D, new Point(x, y)));
                        break;
                }
            }

            game.SetPlayerOrganism(playerOrganism);
            game.SetOpponentOrganism(opponentOrganism);

            game.SetWalls(walls);
            game.SetProteins(proteins);

            ProteinStock playerProteins = GetProteins();
            game.SetPlayerProteinStock(playerProteins);

            ProteinStock opponentProteins = GetProteins();
            game.SetOpponentProteinStock(opponentProteins);

            List<string> actions = game.GetActions();

            int requiredActionsCount = int.Parse(Console.ReadLine()); // your number of organisms, output an action for each one in any order
            for (int i = 0; i < requiredActionsCount; i++)
            {
                Console.WriteLine(actions[i]);

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                // Console.WriteLine("WAIT");
            }
        }
    }

    private static ProteinStock GetProteins()
    {
        string[] inputs = Console.ReadLine().Split(' ');
        int proteinA = int.Parse(inputs[0]);
        int proteinB = int.Parse(inputs[1]);
        int proteinC = int.Parse(inputs[2]);
        int proteinD = int.Parse(inputs[3]);

        ProteinStock proteins = new ProteinStock(proteinA, proteinB, proteinC, proteinD);

        return proteins;
    }
}