using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

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

internal sealed class Game
{
    Point _playerPosition;
    
    List<Human> _humans = new List<Human>();
    List<Zombie> _zombies = new List<Zombie>();
    
    internal Point GetAction()
    {
        var moveToPoint = _playerPosition;
        
        // If I at a person 
            // How many turns until a different person needs me
            // Kill nearby zombies until I have to move
            // Go to a new person
        // else
            // Who is in the most danger
            // Go to them 
        
        // Am I at a person
        if(_humans.Any(h => h.Position.X == _playerPosition.X && h.Position.Y == _playerPosition.Y))
        {
            var currentHuman = _humans.First(h => h.Position.X == _playerPosition.X && h.Position.Y == _playerPosition.Y);
            var closestZombie = GetClosestZombie(currentHuman);
            
            var currentClosestDistance = CalculateDistance(currentHuman, closestZombie);
            
            Human mostInDangerHuman = null;
            
            foreach (var human in _humans)
            {
                var closest = GetClosestZombie(currentHuman);
                
                var distance = CalculateDistance(human, closest);
                
                if(distance < currentClosestDistance)
                {
                    currentClosestDistance = distance;
                    mostInDangerHuman = human;
                }
            }
            
            if(mostInDangerHuman != null)
            {
                return mostInDangerHuman.Position;
            }
            else
            {
                return currentHuman.Position;
            }
        }
        else
        {
            var closestZombieDistance = double.MaxValue;
            var closestHumanPoint = new Point(0,0);
        
            // Find the player with the closest Zombie
            foreach (var human in _humans)
            {
                foreach (var zombie in _zombies)
                {
                    var distance = CalculateDistance(human, zombie);
                
                    if(distance < closestZombieDistance)
                    {
                        closestZombieDistance = distance;
                        moveToPoint = human.Position;
                    }
                }
            }
        }
        
        return moveToPoint;
    }
    private Zombie GetClosestZombie(Human human)
    {
        var closestZombieDistance = double.MaxValue;
        Zombie closestZombie = null;

        foreach (var zombie in _zombies)
        {
            var distance = CalculateDistance(human, zombie);
            
            if(distance < closestZombieDistance)
            {
                closestZombieDistance = distance;
                closestZombie = zombie;
            }
        }
        
        return closestZombie;
    }

    private static double CalculateDistance(Human human, Zombie zombie)
    { 
        return Math.Sqrt(  Math.Pow(Math.Abs(human.Position.X - zombie.Position.X), 2) 
                         + Math.Pow(Math.Abs(human.Position.Y - zombie.Position.Y), 2));
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

// Notes:
// Ash can move 1000
//Zombies can move 400
