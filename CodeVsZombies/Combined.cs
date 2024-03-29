/**************************************************************
  This file was generated by FileConcatenator.
  It combined all classes in the project to work in Codingame.
  This hasn't been put in a namespace to allow for class 
  name duplicates.
***************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;

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

    internal sealed class Human
    {
        public int Id { get; }
        public Point Position { get; }
    
        internal Human(int id, Point position)
        {
            Id = id;
            Position = position;
        }
    }

    /**
 * Save humans, destroy zombies!
 **/
    class Player
    {
        static void Main(string[] args)
        {
            var game = new Game();
        
            string[] inputs;

            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                var x = int.Parse(inputs[0]);
                var y = int.Parse(inputs[1]);
            
                game.SetPlayerCoordinates(x, y);
            
                game.ClearHumans();
                game.ClearZombies();

                var humanCount = int.Parse(Console.ReadLine());
                for (var i = 0; i < humanCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    var humanId = int.Parse(inputs[0]);
                    var humanX = int.Parse(inputs[1]);
                    var humanY = int.Parse(inputs[2]);
                
                    game.AddHuman(humanId, humanX, humanY);
                }

                var zombieCount = int.Parse(Console.ReadLine());
                for (var i = 0; i < zombieCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    var zombieId = int.Parse(inputs[0]);
                    var zombieX = int.Parse(inputs[1]);
                    var zombieY = int.Parse(inputs[2]);
                    var zombieXNext = int.Parse(inputs[3]);
                    var zombieYNext = int.Parse(inputs[4]);
                
                    game.AddZombie(zombieId, zombieX, zombieY, zombieXNext, zombieYNext);
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                var action = game.GetAction();
            
                Console.WriteLine($"{action.X} {action.Y}"); // Your destination coordinates
            }
        }
    }

    internal sealed class Zombie
    {
        public int Id { get; }
        public Point Position { get; }
        public Point NextPosition { get; }
    
        internal Zombie(int id, Point position, Point nextPosition)
        {
            Id = id;
            Position = position;
            NextPosition = nextPosition;
        }
    }
