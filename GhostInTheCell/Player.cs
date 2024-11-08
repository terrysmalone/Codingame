using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace GhostInTheCell
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
            int factoryCount = int.Parse(Console.ReadLine()); // the number of factories

            int linkCount = int.Parse(Console.ReadLine()); // the number of links between factories

            List<Link> links = new List<Link>();

            for (int i = 0; i < linkCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int factory1 = int.Parse(inputs[0]);
                int factory2 = int.Parse(inputs[1]);
                int distance = int.Parse(inputs[2]);

                links.Add(new Link(factory1, factory2, distance));
            }

            List<Factory> factories = new List<Factory>();

            for (int i = 0; i < factoryCount; i++)
            {
                factories.Add(new Factory(i, links.Where(l => l.SourceFactory == i || l.DestinationFactory == i).ToList()));
            }


            Game game = new Game(factories);

            // game loop
            while (true)
            {
                List<Troop> playerTroops = new List<Troop>();
                List<Troop> enempyTroops = new List<Troop>();

                int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. factories and troops)


                for (int i = 0; i < entityCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int entityId = int.Parse(inputs[0]);
                    string entityType = inputs[1];

                    int owner = int.Parse(inputs[2]);

                    if(entityType == "FACTORY")
                    {
                        int numberOfCyborgs = int.Parse(inputs[3]);
                        int factoryProduction = int.Parse(inputs[4]);
                        int unused1 = int.Parse(inputs[5]);
                        int unused2 = int.Parse(inputs[6]);

                        game.UpdateFactory(entityId, (Owner)owner, numberOfCyborgs, factoryProduction);
                    }
                    else if(entityType == "TROOP")
                    {
                        int sourceFactory = int.Parse(inputs[3]);
                        int destinationFactory = int.Parse(inputs[4]);
                        int numberOfCyborgs = int.Parse(inputs[5]);
                        int turnsUntilArrival = int.Parse(inputs[6]);

                        if((Owner)owner == Owner.Player)
                        {
                            playerTroops.Add(new Troop(entityId, numberOfCyborgs, sourceFactory, destinationFactory, turnsUntilArrival));
                        }
                        else
                        {
                            enempyTroops.Add(new Troop(entityId, numberOfCyborgs, sourceFactory, destinationFactory, turnsUntilArrival));
                        }
                    }
                }

                game.SetPlayerTroops(playerTroops);
                game.SetEnemyTroops(enempyTroops);

                string move = game.GetMove();

                // Any valid action, such as "WAIT" or "MOVE source destination cyborgs"
                Console.WriteLine(move);
            }
        }
    }

}
