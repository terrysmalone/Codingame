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
            var move = "WAIT";

            DisplayFactories();

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

    }
}
