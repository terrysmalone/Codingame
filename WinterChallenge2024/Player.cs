using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WinterChallenge2024;

partial class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        var width = int.Parse(inputs[0]);
        var height = int.Parse(inputs[1]);

        Game game = new Game(width, height);

        while (true)
        {
            var unsortedPlayerOrgans = new List<Organ>();
            var unsortedOpponentOrgans = new List<Organ>();
            var proteins = new List<Protein>();
            var walls = new bool[width, height]; 

            int entityCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]);
                string type = inputs[2];
                int owner = int.Parse(inputs[3]);
                int organId = int.Parse(inputs[4]); 
                string organDir = inputs[5];
                int organParentId = int.Parse(inputs[6]);
                int organRootId = int.Parse(inputs[7]);

                if (Enum.TryParse(type, out OrganType organTypeEnum))
                {
                    switch (type)
                    {
                        case "BASIC":
                        case "ROOT":
                            if (owner == 1)
                            {
                                unsortedPlayerOrgans.Add(
                                    CreateOrgan(
                                        organId,
                                        organRootId,
                                        organTypeEnum,
                                        new Point(x, y),
                                        organParentId));
                            }
                            else if (owner == 0)
                            {
                                unsortedOpponentOrgans.Add(
                                    CreateOrgan(
                                        organId,
                                        organRootId,
                                        organTypeEnum,
                                        new Point(x, y),
                                        organParentId));
                            }

                            break;

                        case "HARVESTER":
                        case "SPORER":
                        case "TENTACLE":
                            OrganDirection dirEnum;
                            if (Enum.TryParse(organDir, out dirEnum))
                            {
                                if (owner == 1)
                                {
                                    unsortedPlayerOrgans.Add(
                                        CreateDirectionOrgan(
                                            organId,
                                            organRootId,
                                            organTypeEnum,
                                            new Point(x, y),
                                            organParentId,
                                            dirEnum));
                                }
                                else if (owner == 0)
                                {
                                    unsortedOpponentOrgans.Add(
                                        CreateDirectionOrgan(
                                            organId,
                                            organRootId,
                                            organTypeEnum,
                                            new Point(x, y),
                                            organParentId,
                                            dirEnum));
                                }
                            }
                            break;
                    }
                }
                else
                {
                    switch (type)
                    {
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
                        case "WALL":
                            walls[x, y] = true;
                            break;
                    }
                }
            }

            List<Organism> playerOrganisms = SortOrgans(unsortedPlayerOrgans);
            game.SetPlayerOrganisms(playerOrganisms);
            List<Organism> opponentOrganisms = SortOrgans(unsortedOpponentOrgans);
            game.SetOpponentOrganisms(opponentOrganisms);

            game.SetWalls(walls);
            game.SetProteins(proteins);

            ProteinStock playerProteins = GetProteins();
            game.SetPlayerProteinStock(playerProteins);

            ProteinStock opponentProteins = GetProteins();
            game.SetOpponentProteinStock(opponentProteins);

            List<Action> actions = game.GetActions();

            int requiredActionsCount = int.Parse(Console.ReadLine()); 
            for (int i = 0; i < requiredActionsCount; i++)
            {
                Console.WriteLine(actions[i].ToString());
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

    private static Organ CreateOrgan(int organId, int rootId, OrganType organType, Point point, int parentId)
    {
        return new Organ(organId, rootId, organType, point, parentId);
    }

    private static Organ CreateDirectionOrgan(int organId, int rootId, OrganType organType, Point point, int parentId, OrganDirection direction)
    {
        return new Organ(organId, rootId, organType, point, parentId, direction);
    }

    private static List<Organism> SortOrgans(List<Organ> unsortedOrgans)
    {
        List<Organism> organisms = new List<Organism>();

        unsortedOrgans = unsortedOrgans.OrderBy(o => o.Type != OrganType.ROOT).ToList();

        foreach (Organ organ in unsortedOrgans)
        {
            if (organ.Type == OrganType.ROOT)
            {
                Organism organism = new Organism(organ.Id);
                organism.AddOrgan(organ);
                organisms.Add(new Organism(organ.Id));
            }

            organisms.Single(o => o.RootId == organ.RootId).AddOrgan(organ);
        }

        return organisms;
    }
}