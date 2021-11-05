using System;
using System.Collections.Generic;
using System.Linq;

namespace GhostInTheCell
{
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

            // We want to keep track of how many cyborgs we can send
            //
            var availableTroops = _factories.Where(f => f.Owner == Owner.Player)
                                            .ToDictionary(f => f.Id, f => f.NumberOfCyborgs);

            //DisplayFactories();

            var playerFactories = _factories.Where(f => f.Owner == Owner.Player).ToList();

            // TODO: Make more sophisticated. We want to go for high producing, close ones first
            var allViableTargetFactories = _factories.Where(f => f.Owner != Owner.Player)
                                                             .OrderByDescending(f => f.Owner == Owner.Neutral) // Neutral then opponent
                                                             .ThenBy(f => f.NumberOfCyborgs)
                                                             .ToList();

            foreach (var targetFactory in allViableTargetFactories)
            {
                // How many troops do we need
                var troopsNeeded = targetFactory.NumberOfCyborgs + 1;

                var linksToPlayerFactories = targetFactory.Links.Where(l => playerFactories.Select(f => f.Id).Contains(l.DestinationFactory))
                                                                  .OrderBy(l => l.Distance).ToList();

                var linkIndex = 0;

                //DisplayFactory(targetFactory);

                //DisplayLinks(linksToPlayerFactories);

                // Get them from the closest place first
                while (linkIndex < linksToPlayerFactories.Count && troopsNeeded > 0)
                {
                    //Console.Error.WriteLine($"linkIndex:{linkIndex}");
                    var closestFactoryId = linksToPlayerFactories[linkIndex].DestinationFactory;
                    var availableAtFactory = availableTroops[closestFactoryId];

                    if (availableAtFactory >= troopsNeeded)
                    {
                        move += $"MOVE {closestFactoryId} {targetFactory.Id} {troopsNeeded};";

                        troopsNeeded = 0;
                        availableTroops[closestFactoryId] -= troopsNeeded;
                    }
                    else
                    {
                        move += $"MOVE {closestFactoryId} {targetFactory.Id} {availableAtFactory};";

                        troopsNeeded -= availableAtFactory;
                        availableTroops[closestFactoryId] = 0;
                    }

                    linkIndex++;
                }

                // Update available troops

                //When we're out of available troops return
            }

            if (move.Length > 0)
            {
                move = move.TrimEnd(';');
            }
            else
            {
                move = "WAIT";
            }

            Console.Error.WriteLine($"move:{move}");

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
                DisplayFactory(factory);
            }
        }

        private static void DisplayFactory(Factory factory)
        {
            Console.Error.WriteLine($"factory.Id:{factory.Id}");
            Console.Error.WriteLine($"factory.Owner:{factory.Owner}");
            Console.Error.WriteLine($"factory.NumberOfCyborgs:{factory.NumberOfCyborgs}");
            Console.Error.WriteLine($"factory.Production:{factory.Production}");
            Console.Error.WriteLine("------------------");

            DisplayLinks(factory.Links);

            Console.Error.WriteLine("================");
        }

        private static void DisplayLinks(List<Link> links)
        {
            foreach (var link in links)
            {
                Console.Error.WriteLine($"Link:{link.SourceFactory}-{link.DestinationFactory}:{link.Distance}");
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
}
