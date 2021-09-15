using System;
using System.Collections.Generic;
using System.Drawing;

namespace CodeVsZombies
{
    internal sealed class Game
    {
        Point _playerPosition;
    
        List<Human> _humans = new List<Human>();
        List<Zombie> _zombies = new List<Zombie>();
    
        internal Point GetAction()
        {
            var closestZombieDistance = double.MaxValue;
            var closestHumanPoint = new Point(0,0);
            var closesZombiePoint = new Point(0,0);

            // Find the player with the closest Zombie
            foreach (var human in _humans)
            {
                foreach (var zombie in _zombies)
                {
                    var distance = CalculateDistance(human.Position, zombie.Position);
            
                    if(distance < closestZombieDistance)
                    {
                        if(CanHumanBeSaved(human,zombie))
                        {
                            closestZombieDistance = distance;
                            closestHumanPoint = human.Position;
                            closesZombiePoint = zombie.Position;
                        }
                    }
                }
            }
        
            return closesZombiePoint;
        }
        private bool CanHumanBeSaved(Human human, Zombie zombie)
        {
            var myTurnsToTarget = CalculateDistance(_playerPosition, human.Position) / 1000;

            var zombieTurnsToTarget = CalculateDistance(human.Position, zombie.Position) / 400;
        
            return myTurnsToTarget <= zombieTurnsToTarget;
        }
        private Zombie GetClosestZombie(Human human)
        {
            var closestZombieDistance = double.MaxValue;
            Zombie closestZombie = null;

            foreach (var zombie in _zombies)
            {
                var distance = CalculateDistance(human.Position, zombie.Position);
            
                if(distance < closestZombieDistance)
                {
                    closestZombieDistance = distance;
                    closestZombie = zombie;
                }
            }
        
            return closestZombie;
        }

        private static double CalculateDistance(Point point1, Point point2)
        { 
            return Math.Sqrt(  Math.Pow(Math.Abs(point1.X - point2.X), 2) 
                               + Math.Pow(Math.Abs(point1.Y - point2.Y), 2));
        }

        public void SetPlayerCoordinates(int xPosition, int yPosition)
        {
            _playerPosition = new Point(xPosition, yPosition);
        }

        public void AddHuman(int humanId, int humanX, int humanY)
        {
            _humans.Add(new Human(humanId, new Point(humanX, humanY)));
        }
    
        public void AddZombie(int zombieId, int zombieX, int zombieY, int zombieXNext, int zombieYNext)
        {
            _zombies.Add(new Zombie(zombieId, new Point(zombieX, zombieY), new Point(zombieXNext, zombieYNext)));
        }

        public void ClearHumans()
        {
            _humans = new List<Human>();
        }
    
        public void ClearZombies()
        {
            _zombies = new List<Zombie>();
        }
    }
}
