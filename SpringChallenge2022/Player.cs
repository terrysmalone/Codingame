using System.Drawing;
using System;
using System.Collections.Generic;

namespace SpringChallenge2022;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
internal sealed class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');

        int baseX = int.Parse(inputs[0]); // The corner of the map representing your base
        int baseY = int.Parse(inputs[1]);
        int heroesPerPlayer = int.Parse(Console.ReadLine()); // Always 3

        Game game = new Game(new Point(baseX, baseY), heroesPerPlayer);

        // game loop
        while (true)
        {
            // Don't bother persisting monsters. It's quicker just to re-add them every time.
            // At least until we need to persist them
            List<Hero> enemyHeroes = new List<Hero>();
            List<Monster> monsters = new List<Monster>();

            // Player base stats
            inputs = Console.ReadLine().Split(' ');
            int playerBaseHealth = int.Parse(inputs[0]); // Your base health
            int playerMana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell

            // enemy base stats
            inputs = Console.ReadLine().Split(' ');
            int enemyBaseHealth = int.Parse(inputs[0]); // Your base health
            int enemyMana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell

            int entityCount = int.Parse(Console.ReadLine()); // Amount of heros and monsters you can see

            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');

                int id = int.Parse(inputs[0]); // Unique identifier
                int type = int.Parse(inputs[1]); // 0=monster, 1=your hero, 2=opponent hero
                int x = int.Parse(inputs[2]); // Position of this entity
                int y = int.Parse(inputs[3]);
                int shieldLife = int.Parse(inputs[4]); // Ignore for this league; Count down until shield spell fades
                int isControlled = int.Parse(inputs[5]); // Ignore for this league; Equals 1 when this entity is under a control spell
                int health = int.Parse(inputs[6]); // Remaining health of this monster
                int vx = int.Parse(inputs[7]); // Trajectory of this monster
                int vy = int.Parse(inputs[8]);
                int nearBase = int.Parse(inputs[9]); // 0=monster with no target yet, 1=monster targeting a base
                int threatFor = int.Parse(inputs[10]); // Given this monster's trajectory, is it a threat to 1=your base, 2=your opponent's base, 0=neither

                if (type == 0)
                {
                    ThreatFor threatForEnum;

                    switch (threatFor)
                    {
                        case 0:
                            threatForEnum = ThreatFor.None;
                            break;
                        case 1:
                            threatForEnum = ThreatFor.Player;
                            break;
                        case 2:
                            threatForEnum = ThreatFor.Enemy;
                            break;
                        default:
                            threatForEnum = ThreatFor.None;
                            break;
                    }

                    monsters.Add(new Monster(id, new Point(x, y), health, vx, vy, nearBase != 0, threatForEnum, isControlled == 1, shieldLife));
                }
                else
                {
                    Hero hero = new Hero(id, new Point(x, y), isControlled == 1, shieldLife);

                    if (type == 1)
                    {
                        game.UpdatePlayerHero(hero);
                    }
                    else
                    {
                        enemyHeroes.Add(hero);
                    }
                }
            }

            string[] moves = game.GetMoves(enemyHeroes, monsters, playerMana);

            for (int i = 0; i < moves.Length; i++)
            {
                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                // In the first league: MOVE <x> <y> | WAIT; In later leagues: | SPELL <spellParams>;
                Console.WriteLine(moves[i]);
            }
        }
    }
}
