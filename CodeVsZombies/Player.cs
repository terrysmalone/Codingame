namespace CodeVsZombies;
    
using System;

class Player
{
    static void Main(string[] args)
    {
        Game game = new Game();
        
        string[] inputs;

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            
            game.SetPlayerCoordinates(x, y);
            
            game.ClearHumans();
            game.ClearZombies();

            int humanCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < humanCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int humanId = int.Parse(inputs[0]);
                int humanX = int.Parse(inputs[1]);
                int humanY = int.Parse(inputs[2]);
                
                game.AddHuman(humanId, humanX, humanY);
            }

            int zombieCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < zombieCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int zombieId = int.Parse(inputs[0]);
                int zombieX = int.Parse(inputs[1]);
                int zombieY = int.Parse(inputs[2]);
                int zombieXNext = int.Parse(inputs[3]);
                int zombieYNext = int.Parse(inputs[4]);
                
                game.AddZombie(zombieId, zombieX, zombieY, zombieXNext, zombieYNext);
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            System.Drawing.Point action = game.GetAction();
            
            Console.WriteLine($"{action.X} {action.Y}"); // Your destination coordinates
        }
    }
}

// Notes:
// Ash can move 1000
//Zombies can move 400
