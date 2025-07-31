using System;
using System.Collections.Generic;

namespace _2023_1_SpringChallenge_Ants;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        int numberOfCells = int.Parse(Console.ReadLine()); // amount of hexagonal cells in this map
                
        Game game = new Game(numberOfCells);

        for (int i = 0; i < numberOfCells; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int type = int.Parse(inputs[0]); // 0 for empty, 1 for eggs, 2 for crystal
            int initialResources = int.Parse(inputs[1]); // the initial amount of eggs/crystals on this cell
            int neigh0 = int.Parse(inputs[2]); // the index of the neighbouring cell for each direction
            int neigh1 = int.Parse(inputs[3]);
            int neigh2 = int.Parse(inputs[4]);
            int neigh3 = int.Parse(inputs[5]);
            int neigh4 = int.Parse(inputs[6]);
            int neigh5 = int.Parse(inputs[7]);

            int[] neighbourIds = new int[] { neigh0, neigh1, neigh2, neigh3, neigh4, neigh5 };

            var cellType = (CellType)type;

            var eggCount = 0;
            var crystalCount = 0;

            if (cellType == CellType.Egg)
            {
                eggCount = initialResources;
            }
            else if (cellType == CellType.Crystal)
            {
                crystalCount = initialResources;
            }

            var cell = new Cell(i, neighbourIds, cellType, eggCount, crystalCount);

            game.AddCell(i, cell);

        }

        int numberOfBases = int.Parse(Console.ReadLine());
        inputs = Console.ReadLine().Split(' ');
        for (int i = 0; i < numberOfBases; i++)
        {
            int myBaseIndex = int.Parse(inputs[i]);
            game.AddPlayerBase(myBaseIndex);
        }
        inputs = Console.ReadLine().Split(' ');
        for (int i = 0; i < numberOfBases; i++)
        {
            int oppBaseIndex = int.Parse(inputs[i]);
            game.AddOpponentBase(oppBaseIndex);
        }

        // game loop
        while (true)
        {
            Console.Error.WriteLine(Console.ReadLine());

            game.ResetCounts();

            for (int i = 0; i < numberOfCells; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int resources = int.Parse(inputs[0]); // the current amount of eggs/crystals on this cell
                int myAnts = int.Parse(inputs[1]); // the amount of your ants on this cell
                int oppAnts = int.Parse(inputs[2]); // the amount of opponent ants on this cell

                game.UpdateCell(i, resources, myAnts, oppAnts);
                game.IncreasePlayerAntCount(myAnts);
                game.IncreaseOpponentAntCount(oppAnts);
            }

            List<string> actions = game.GetActions();

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // WAIT | LINE <sourceIdx> <targetIdx> <strength> | BEACON <cellIdx> <strength> | MESSAGE <text>
            var actionList = String.Join("; ", actions);
            Console.WriteLine(actionList);
        }
    }
}