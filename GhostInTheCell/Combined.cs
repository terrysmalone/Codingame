/**************************************************************
  This file was generated by FileConcatenator.
  It combined all classes in the project to work in Codingame.
  This hasn't been put in a namespace to allow for class 
  name duplicates.
***************************************************************/
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;

    internal sealed class Factory
    {
        public int Id { get; }
        public List<Link> Links { get; }

        public Owner Owner { get; private set; }
        public int NumberOfCyborgs { get; private set;  }
        public int Production { get; private set;  }

        public Factory(int id, List<Link> links)
        {
            Id = id;

            Links = NormaliseLinks(links);
        }
        private List<Link> NormaliseLinks(List<Link> links)
        {
            var normalisedLinks = new List<Link>();

            foreach (var link in links)
            {
                if(link.SourceFactory == Id)
                {
                    normalisedLinks.Add(new Link(link.SourceFactory,
                                                     link.DestinationFactory,
                                                     link.Distance));
                }
                else
                {
                    normalisedLinks.Add(new Link(link.DestinationFactory,
                                                     link.SourceFactory,
                                                     link.Distance));
                }
            }

            return normalisedLinks;
        }
        public void Update(Owner owner, int numberOfCyborgs, int factoryProduction)
        {
            Owner = owner;
            NumberOfCyborgs = numberOfCyborgs;
            Production = factoryProduction;
        }
    }

    internal sealed class Game
    {
        private List<Factory> _factories;
        private List<Troop> _playerTroops;
        private List<Troop> _enemyTroops;

        public Game(List<Factory> factories)
        {
            _factories = factories;
        }

        internal string GetMove()
        {
            var move = string.Empty;

            //DisplayFactories();

            // Strategy
            // Very basic flood fill

            // For every one of my factories
                // split

            var myFactories = _factories.Where(f => f.Owner == Owner.Player).OrderByDescending(f => f.NumberOfCyborgs);

            // TODO: Make more sophisticated. We want to go for high producing, close ones first
            var allViableTargetFactoryIds = _factories.Where(f => f.Owner != Owner.Player)
                                                             .OrderByDescending(f => f.Owner == Owner.Neutral) // Neutral then opponent
                                                             .Select(f => f.Id).ToList();

            foreach (var factory in myFactories)
            {
                var troopsInFactory = factory.NumberOfCyborgs;

                // order all targets
                // send to desirable ones first

                var targetAmounts = new int[allViableTargetFactoryIds.Count];

                var counter = 0;
                var targetIndex = 0;
                var maxTargetIndex = allViableTargetFactoryIds.Count - 1;

                while(counter <= troopsInFactory)
                {
                    if (targetIndex > maxTargetIndex)
                    {
                        targetIndex = 0;
                    }

                    targetAmounts[targetIndex]++;

                    targetIndex++;
                    counter++;
                }

                for (var i = 0; i < targetAmounts.Length; i++)
                {
                    if(targetAmounts[i] > 0)
                    {
                        move += $"MOVE {factory.Id} {allViableTargetFactoryIds[i]} {targetAmounts[i]};";
                    }
                }
            }

            if (move.Length > 0)
            {
                move = move.TrimEnd(';');
            }

            return move;
        }

        public void UpdateFactory(int entityId, Owner owner, int numberOfCyborgs, int factoryProduction)
        {
            _factories.Single(f => f.Id == entityId).Update((Owner)owner, numberOfCyborgs, factoryProduction);
        }

        internal void SetPlayerTroops(List<Troop> playerTroops)
        {
            _playerTroops = playerTroops;
        }

        internal void SetEnemyTroops(List<Troop> enemyTroops)
        {
            _enemyTroops = enemyTroops;
        }

        private void DisplayFactories()
        {
            Console.Error.WriteLine("FACTORIES");
            Console.Error.WriteLine("================");

            foreach (var factory in _factories)
            {
                Console.Error.WriteLine($"factory.Id:{factory.Id}");
                Console.Error.WriteLine($"factory.Owner:{factory.Owner}");
                Console.Error.WriteLine($"factory.NumberOfCyborgs:{factory.NumberOfCyborgs}");
                Console.Error.WriteLine($"factory.Production:{factory.Production}");
                Console.Error.WriteLine("------------------");
                foreach (var link in factory.Links)
                {
                    Console.Error.WriteLine($"Link:{link.SourceFactory}-{link.DestinationFactory}:{link.Distance}");
                }

                Console.Error.WriteLine("================");
            }
        }

        private void DisplayPlayerTroops()
        {
            Console.Error.WriteLine("PLAYER TROOPS");
            Console.Error.WriteLine("================");

            foreach (var troop in _playerTroops)
            {
                Console.Error.WriteLine($"troop.SourceFactory:{troop.SourceFactory}");
                Console.Error.WriteLine($"troop.DestinationFactory:{troop.DestinationFactory}");
                Console.Error.WriteLine($"troop.NumberOfCyborgs:{troop.NumberOfCyborgs}");
                Console.Error.WriteLine($"troop.TurnsUntilArrival:{troop.TurnsUntilArrival}");
                Console.Error.WriteLine("================");
            }
        }
    }

    internal sealed class Link
    {
        public int SourceFactory { get; }
        public int DestinationFactory { get; }
        public int Distance { get; }

        public Link(int sourceFactory, int destinationFactory, int distance)
        {
            SourceFactory = sourceFactory;
            DestinationFactory = destinationFactory;
            Distance = distance;
        }
    }

    internal enum Owner
    {
        Neutral = 0,
        Player = 1,
        Enemy = -1
    }

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
                Console.Error.WriteLine($"entityCount:{entityCount}");

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


    internal sealed class Troop
    {
        public int EntityId { get; }
        public int NumberOfCyborgs { get; }
        public int SourceFactory { get; }
        public int DestinationFactory { get; }
        public int TurnsUntilArrival { get; }

        public Troop(int entityId, int numberOfCyborgs, int sourceFactory, int destinationFactory, int turnsUntilArrival)
        {
            EntityId = entityId;
            NumberOfCyborgs = numberOfCyborgs;
            SourceFactory = sourceFactory;
            DestinationFactory = destinationFactory;
            TurnsUntilArrival = turnsUntilArrival;
        }
    }
