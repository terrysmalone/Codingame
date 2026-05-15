using System;
using System.Drawing;
using System.Linq;

namespace SpringChallenge2026;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
partial class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);

        Game game = new Game(width, height);

        for (int y = 0; y < height; y++)
        {
            string line = Console.ReadLine();

            Logger.Message(line);

            char[] rowText = line.ToCharArray();


            for (int x = 0; x < width; x++)
            {
                if(rowText[x] == '.')
                {
                    game.SetIsWalkable(x, y, true);
                }

                if (rowText[x] == '0')
                {
                    game.AddPlayerShack(x, y);
                }
                else if (rowText[x] == '1')
                {
                    game.AddEnemyShack(x, y);
                }
            }
        }

        // game loop
        while (true)
        {
            game.ClearTrees();
            game.ClearTrolls();

            for (int i = 0; i < 2; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int plum = int.Parse(inputs[0]);
                int lemon = int.Parse(inputs[1]);
                int apple = int.Parse(inputs[2]);
                int banana = int.Parse(inputs[3]);
                int iron = int.Parse(inputs[4]);
                int wood = int.Parse(inputs[5]);

                if (i == 0)
                {
                    game.SetPlayerInventory(new Inventory
                    {
                        Plum = plum,
                        Lemon = lemon,
                        Apple = apple,
                        Banana = banana,
                        Iron = iron,
                        Wood = wood
                    });
                }
                else
                {
                    game.SetEnemyInventory(new Inventory
                    {
                        Plum = plum,
                        Lemon = lemon,
                        Apple = apple,
                        Banana = banana,
                        Iron = iron,
                        Wood = wood
                    });
                }
            }

            int treesCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < treesCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                string type = inputs[0];
                int x = int.Parse(inputs[1]);
                int y = int.Parse(inputs[2]);
                int size = int.Parse(inputs[3]);
                int health = int.Parse(inputs[4]);
                int fruits = int.Parse(inputs[5]);
                int cooldown = int.Parse(inputs[6]);

                game.AddTree(new Tree
                {
                    Type = type,
                    Position = new Point(x, y),
                    Size = size,
                    Health = health,
                    Fruits = fruits,
                    Cooldown = cooldown
                });
            }


            int trollsCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < trollsCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]);
                int player = int.Parse(inputs[1]);
                int x = int.Parse(inputs[2]);
                int y = int.Parse(inputs[3]);
                int movementSpeed = int.Parse(inputs[4]);
                int carryCapacity = int.Parse(inputs[5]);
                int harvestPower = int.Parse(inputs[6]);
                int chopPower = int.Parse(inputs[7]);
                int carryPlum = int.Parse(inputs[8]);
                int carryLemon = int.Parse(inputs[9]);
                int carryApple = int.Parse(inputs[10]);
                int carryBanana = int.Parse(inputs[11]);
                int carryIron = int.Parse(inputs[12]);
                int carryWood = int.Parse(inputs[13]);

                var troll = new Troll
                {
                    Id = id,
                    Position = new Point(x, y),
                    MovementSpeed = movementSpeed,
                    CarryCapacity = carryCapacity,
                    HarvestPower = harvestPower,
                    ChopPower = chopPower,
                    CarryPlum = carryPlum,
                    CarryLemon = carryLemon,
                    CarryApple = carryApple,
                    CarryBanana = carryBanana,
                    CarryIron = carryIron,
                    CarryWood = carryWood,
                    TotalCarry = carryPlum + carryLemon + carryApple + carryBanana + carryIron + carryWood
                };
                if (player == 0)
                {
                    game.AddPlayerTroll(troll);
                }
                else
                {
                    game.AddEnemyTroll(troll);
                }
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            // valid actions:
            // MOVE <id> <x> <y>
            // HARVEST <id> - when you are on the same cell as a tree
            // DROP <id> - when you are next to your shack and carry items
            string actions = string.Join(";", game.GetActions());

            Console.WriteLine(actions);

            // Console.WriteLine("MOVE 0 7 7");
        }
    }
}
