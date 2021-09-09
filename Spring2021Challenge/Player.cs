using System;

namespace Spring2021Challenge
{
    internal sealed class Player
    {
        static void Main(string[] args)
        {
            string[] inputs;
    
            var game = new Game();
    
            var  numberOfCells = int.Parse(Console.ReadLine()); // 37
            
            for (var i = 0; i < numberOfCells; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var index = int.Parse(inputs[0]); // 0 is the center cell, the next cells spiral outwards
                var richness = int.Parse(inputs[1]); // 0 if the cell is unusable, 1-3 for usable cells
                var neigh0 = int.Parse(inputs[2]); // the index of the neighbouring cell for each direction
                var neigh1 = int.Parse(inputs[3]);
                var neigh2 = int.Parse(inputs[4]);
                var neigh3 = int.Parse(inputs[5]);
                var neigh4 = int.Parse(inputs[6]);
                var neigh5 = int.Parse(inputs[7]);
                var neighs = new int[] { neigh0, neigh1, neigh2, neigh3, neigh4, neigh5 };
                
                var cell = new Cell(index, richness, neighs);
                game.Board.Add(cell);
            }
    
            // game loop
            while (true)
            {
                game.Round = int.Parse(Console.ReadLine()); // the game lasts 24 days: 0-23
                game.Nutrients = int.Parse(Console.ReadLine()); // the base score you gain from the next COMPLETE action
                inputs = Console.ReadLine().Split(' ');
                game.MySun = int.Parse(inputs[0]); // your sun points
                game.MyScore = int.Parse(inputs[1]); // your current score
                inputs = Console.ReadLine().Split(' ');
                game.OpponentSun = int.Parse(inputs[0]); // opponent's sun points
                game.OpponentScore = int.Parse(inputs[1]); // opponent's score
                game.OpponentIsWaiting = inputs[2] != "0"; // whether your opponent is asleep until the next day
    
                game.Trees.Clear();
                var numberOfTrees = int.Parse(Console.ReadLine()); // the current amount of trees
                for (var i = 0; i < numberOfTrees; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    var cellIndex = int.Parse(inputs[0]); // location of this tree
                    var size = int.Parse(inputs[1]); // size of this tree: 0-3
                    var isMine = inputs[2] != "0"; // 1 if this is your tree
                    var isDormant = inputs[3] != "0"; // 1 if this tree is dormant
                    var tree = new Tree(cellIndex, size, isMine, isDormant);
                    game.Trees.Add(tree);
                }
    
                game.PossibleActions.Clear();
    
                var numberOfPossibleMoves = int.Parse(Console.ReadLine());
                
                for (var i = 0; i < numberOfPossibleMoves; i++)
                {
                    var possibleMove = Console.ReadLine();
                    game.PossibleActions.Add(Action.Parse(possibleMove));
                }
    
                var action = game.GetNextAction();
                Console.WriteLine(action);
            }
        }
    }

    // PLAN
    //
    // 1. As a first pass take the necessary parameters and calculate the sun points
    //      * Consideration: we'll need to deep copy something
    // 2. Pass in a next move and calculate the sun points. 
    // 3. Pass in an opponent move and see the sun points
    // 4. Pass in multiple moves and calculate sun points
}
