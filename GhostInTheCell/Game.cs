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

            //DisplayFactories();

            // Basic strategy

            // We can make one move per turn
            // Pick in this order

            // Send from most populace factory
            var sendFromFactory = _factories.Where(f => f.Owner == Owner.Player).OrderByDescending(f => f.NumberOfCyborgs).First();

            var allViableTargetFactories = _factories.Where(f => f.Owner != Owner.Player)
                                                     .OrderByDescending(f => f.Owner == Owner.Neutral).ToList();             // Neutral then opponent

            // closest then furthest
            var viableLinks = sendFromFactory.Links.OrderByDescending(l => l.Distance);

            var targetIndex = 0;
            var targetFound = false;

            var availableTroops = sendFromFactory.NumberOfCyborgs;

            var targetId = 0;
            var troopsToSend = 0;

            while (!targetFound)
            {
                Console.Error.WriteLine($"targetIndex: {targetIndex}");
                Console.Error.WriteLine($"allViableTargetFactories.Count: {allViableTargetFactories.Count}");
                var targetFactory = allViableTargetFactories[targetIndex];
                // TODO: don't send if we're already sending
                    // unless opponent is sending to beat us
                var neededTroops = targetFactory.NumberOfCyborgs + 10; //TODO: add for any en-route

                if(neededTroops <= availableTroops)
                {
                    targetId = targetFactory.Id;
                    troopsToSend = neededTroops;
                    targetFound = true;
                }
                else // Temporary just send what we can
                {
                    targetId = targetFactory.Id;
                    troopsToSend = sendFromFactory.NumberOfCyborgs;
                    targetFound = true;
                }

                targetIndex++;
            }

            // How many to send
            // target amount + enemy troops en-route + 10
                // If we don't have enough pick another one

            move = $"MOVE {sendFromFactory.Id} {targetId} {troopsToSend}";

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
