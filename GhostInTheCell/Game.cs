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
}
