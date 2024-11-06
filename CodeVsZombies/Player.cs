namespace CodeVsZombies;
    
using System;

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

// Notes:
// Ash can move 1000
//Zombies can move 400
