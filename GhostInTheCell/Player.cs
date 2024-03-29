﻿using System;
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
            var factoryCount = int.Parse(Console.ReadLine()); // the number of factories

            var linkCount = int.Parse(Console.ReadLine()); // the number of links between factories

            var links = new List<Link>();

            for (var i = 0; i < linkCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var factory1 = int.Parse(inputs[0]);
                var factory2 = int.Parse(inputs[1]);
                var distance = int.Parse(inputs[2]);

                links.Add(new Link(factory1, factory2, distance));
            }

            var factories = new List<Factory>();

            for (var i = 0; i < factoryCount; i++)
            {
                factories.Add(new Factory(i, links.Where(l => l.SourceFactory == i || l.DestinationFactory == i).ToList()));
            }


            var game = new Game(factories);

            // game loop
            while (true)
            {
                var playerTroops = new List<Troop>();
                var enempyTroops = new List<Troop>();

                var entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. factories and troops)


                for (var i = 0; i < entityCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    var entityId = int.Parse(inputs[0]);
                    string entityType = inputs[1];

                    var owner = int.Parse(inputs[2]);

                    if(entityType == "FACTORY")
                    {
                        var numberOfCyborgs = int.Parse(inputs[3]);
                        var factoryProduction = int.Parse(inputs[4]);
                        var unused1 = int.Parse(inputs[5]);
                        var unused2 = int.Parse(inputs[6]);

                        game.UpdateFactory(entityId, (Owner)owner, numberOfCyborgs, factoryProduction);
                    }
                    else if(entityType == "TROOP")
                    {
                        var sourceFactory = int.Parse(inputs[3]);
                        var destinationFactory = int.Parse(inputs[4]);
                        var numberOfCyborgs = int.Parse(inputs[5]);
                        var turnsUntilArrival = int.Parse(inputs[6]);

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

                var move = game.GetMove();

                // Any valid action, such as "WAIT" or "MOVE source destination cyborgs"
                Console.WriteLine(move);
            }
        }
    }

}
