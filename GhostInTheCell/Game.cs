using System;
using System.Collections.Generic;
using System.Linq;

namespace GhostInTheCell
{
    // TODO
    // Focus fire on a place where I just sent a bomb
    // Try to make sure I don't bomb myself
    // Spread out troops
    internal sealed class Game
    {
        private List<Factory> _factories;
        private List<Troop> _playerTroops;
        private List<Troop> _enemyTroops;

        private int _bombCount = 2;
        private int _firstBombTarget = -1;

        public Game(List<Factory> factories)
        {
            _factories = factories;
        }

        internal string GetMove()
        {
            var move = string.Empty;

            var playerFactories = _factories.Where(f => f.Owner == Owner.Player).ToList();

            // Get bomb moves
            if(_bombCount > 0)
            {
                var enemyFactory = _factories.Where(f => f.Owner == Owner.Enemy && f.Production > 0 && f.Id != _firstBombTarget).OrderByDescending(f => f.Production).FirstOrDefault();

                 if(enemyFactory != null)
                 {
                     var sendFrom = playerFactories.OrderBy(f => f.NumberOfCyborgs).Select(f => f.Id).First();

                     if(_firstBombTarget == -1)
                     {
                         _firstBombTarget = enemyFactory.Id;
                     }

                     move += $"BOMB {sendFrom} {enemyFactory.Id};";
                     _bombCount--;
                 }
            }

            move += GetTroopMoves(playerFactories);

            move = move.Length > 0 ? move.TrimEnd(';') : "WAIT";

            return move;
        }

        private string GetTroopMoves(List<Factory> playerFactories)
        {
            var move = string.Empty;

            // We want to keep track of how many cyborgs we can send
            //
            var availableTroops = _factories.Where(f => f.Owner == Owner.Player)
                                                          .ToDictionary(f => f.Id, f => f.NumberOfCyborgs);

            //move += AddDefensiveMoves(playerFactories, availableTroops);

            // TODO: Make more sophisticated. We want to go for high producing, close ones first
            var allViableTargetFactories = _factories.Where(f => f.Owner != Owner.Player && f.Production > 0)
                                                     .OrderByDescending(f => f.Owner == Owner.Neutral) // Neutral then opponent
                                                     .ThenByDescending(f => f.Production)
                                                     .ThenBy(f => f.NumberOfCyborgs)
                                                     .ToList();

            foreach (var targetFactory in allViableTargetFactories)
            {
                var playerTroopsEnRoute = _playerTroops.Where(t => t.DestinationFactory == targetFactory.Id)
                                                       .Select(f => f.NumberOfCyborgs)
                                                       .Sum();

                var enemyTroopsEnRoute = _enemyTroops.Where(t => t.DestinationFactory == targetFactory.Id)
                                                     .Select(f => f.NumberOfCyborgs)
                                                     .Sum();

                // How many troops do we need
                var troopsNeeded = (targetFactory.NumberOfCyborgs + 1 +  enemyTroopsEnRoute) - playerTroopsEnRoute;

                var linksToPlayerFactories = targetFactory.Links.Where(l => playerFactories.Select(f => f.Id).Contains(l.DestinationFactory))
                                                          .OrderBy(l => l.Distance).ToList();

                var linkIndex = 0;

                // Get them from the closest place first
                //
                while (linkIndex < linksToPlayerFactories.Count && troopsNeeded > 0)
                {
                    var closestFactoryId = linksToPlayerFactories[linkIndex].DestinationFactory;
                    var availableAtFactory = availableTroops[closestFactoryId];

                    if (availableAtFactory >= troopsNeeded)
                    {
                        move += $"MOVE {closestFactoryId} {targetFactory.Id} {troopsNeeded};";

                        availableTroops[closestFactoryId] -= troopsNeeded;

                        troopsNeeded = 0;

                    }
                    else
                    {
                        move += $"MOVE {closestFactoryId} {targetFactory.Id} {availableAtFactory};";

                        troopsNeeded -= availableAtFactory;
                        availableTroops[closestFactoryId] = 0;
                    }

                    linkIndex++;
                }
            }

            return move;
        }
        private string AddDefensiveMoves(List<Factory> playerFactories, Dictionary<int, int> availableTroops)
        {
            var move = string.Empty;

            // Before attacking anyone, lets see if we need to defend
            // If I'm holding a high production site that is being attacked
                // send it some troops
            if (playerFactories.Count > 1)
            {
                // Lets try just protecting one factory at a time
                //
                var highProdPlayerFactory = playerFactories.Where(f => f.Production > 1).OrderByDescending(f => f.Production).First();

                // (troops in factory + my troops on the way) - enemy troops on the way
                var playerTroopsEnRoute = _playerTroops.Where(t => t.DestinationFactory == highProdPlayerFactory.Id)
                                                          .Select(f => f.NumberOfCyborgs)
                                                          .Sum();

                var enemyTroopsEnRoute = _enemyTroops.Where(t => t.DestinationFactory == highProdPlayerFactory.Id)
                                                        .Select(f => f.NumberOfCyborgs)
                                                        .Sum();

                var projectedTroops = (highProdPlayerFactory.NumberOfCyborgs + playerTroopsEnRoute) - enemyTroopsEnRoute;

                // if above is negative send some troops
                if (projectedTroops < 1)
                {
                    var troopsNeeded = 1 - projectedTroops;
                    var sourceFactories = playerFactories.Where(f => f.NumberOfCyborgs > 1 && f.Id != highProdPlayerFactory.Id).OrderByDescending(f => f.NumberOfCyborgs).ToList();

                    var factoryIndex = 0;

                    // Get them from the closest place first
                    //
                    while (factoryIndex < sourceFactories.Count && troopsNeeded > 0)
                    {
                        var closestFactoryId = sourceFactories[factoryIndex].Id;
                        var availableAtFactory = availableTroops[closestFactoryId];

                        if (availableAtFactory >= troopsNeeded)
                        {
                            Console.Error.WriteLine($"IF DEFENSE MOVE:{closestFactoryId} {highProdPlayerFactory.Id} {troopsNeeded}");
                            move += $"MOVE {closestFactoryId} {highProdPlayerFactory.Id} {troopsNeeded};";

                            availableTroops[closestFactoryId] -= troopsNeeded;

                            troopsNeeded = 0;

                        }
                        else
                        {
                            Console.Error.WriteLine($"ELSE DEFENSE MOVE:{closestFactoryId} {highProdPlayerFactory.Id} {availableAtFactory}");
                            move += $"MOVE {closestFactoryId} {highProdPlayerFactory.Id} {availableAtFactory};";

                            troopsNeeded -= availableAtFactory;
                            availableTroops[closestFactoryId] = 0;
                        }

                        factoryIndex++;
                    }
                }
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
