using System;
using System.Linq;
using System.Collections.Generic;

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
                game.board.Add(cell);
            }
    
            // game loop
            while (true)
            {
                game.day = int.Parse(Console.ReadLine()); // the game lasts 24 days: 0-23
                game.nutrients = int.Parse(Console.ReadLine()); // the base score you gain from the next COMPLETE action
                inputs = Console.ReadLine().Split(' ');
                game.mySun = int.Parse(inputs[0]); // your sun points
                game.myScore = int.Parse(inputs[1]); // your current score
                inputs = Console.ReadLine().Split(' ');
                game.opponentSun = int.Parse(inputs[0]); // opponent's sun points
                game.opponentScore = int.Parse(inputs[1]); // opponent's score
                game.opponentIsWaiting = inputs[2] != "0"; // whether your opponent is asleep until the next day
    
                game.trees.Clear();
                var numberOfTrees = int.Parse(Console.ReadLine()); // the current amount of trees
                for (var i = 0; i < numberOfTrees; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    var cellIndex = int.Parse(inputs[0]); // location of this tree
                    var size = int.Parse(inputs[1]); // size of this tree: 0-3
                    var isMine = inputs[2] != "0"; // 1 if this is your tree
                    var isDormant = inputs[3] != "0"; // 1 if this tree is dormant
                    var tree = new Tree(cellIndex, size, isMine, isDormant);
                    game.trees.Add(tree);
                }
    
                game.possibleActions.Clear();
    
                var numberOfPossibleMoves = int.Parse(Console.ReadLine());
                
                for (var i = 0; i < numberOfPossibleMoves; i++)
                {
                    var possibleMove = Console.ReadLine();
                    game.possibleActions.Add(Action.Parse(possibleMove));
                }
    
                var action = game.GetNextAction();
                Console.WriteLine(action);
            }
        }
    }
    
    internal sealed class Cell
    {
        public int Index { get; }
         public int Richness { get; }
        public int[] Neighbours { get; }

        public Cell(int index, int richness, int[] neighbours)
        {
            Index = index;
            Richness = richness;
            Neighbours = neighbours;
        }
    }
    
    internal sealed class Tree
    {
        public int CellIndex { get; }
        public int Size { get; }
        public bool IsMine { get; }
        public bool IsDormant { get; }
        
        public Tree(int cellIndex, int size, bool isMine, bool isDormant)
        {
            CellIndex = cellIndex;
            Size = size;
            IsMine = isMine;
            IsDormant = isDormant;
        }
    }
    
    internal sealed class Action
    {
        public string Type { get; }
        public int SourceCellIdx { get; }
        public int TargetCellIdx { get; }

        private Action(string type, int sourceCellIdx, int targetCellIdx)
        {
            Type = type;
            SourceCellIdx = sourceCellIdx;
            TargetCellIdx = targetCellIdx;
        }

        private Action(string type, int targetCellIdx)
            : this(type, 0, targetCellIdx)
        {
        }

        private Action(string type)
            : this(type, 0, 0)
        {
        }
        
        public static Action Parse(string action)
        {
            var parts = action.Split(" ");
            
            switch (parts[0])
            {
                case "WAIT":
                    return new Action("WAIT");
                case "SEED":
                    return new Action("SEED", int.Parse(parts[1]), int.Parse(parts[2]));
                case "GROW":
                case "COMPLETE":
                default:
                    return new Action(parts[0], int.Parse(parts[1]));
            }
        }
    
        public override string ToString()
        {
            switch (Type)
            {
                case "WAIT":
                    return "WAIT";
                case "SEED":
                    return $"SEED {SourceCellIdx} {TargetCellIdx}";
                default:
                    return $"{Type} {TargetCellIdx}";
            }
        }
    }
    
    internal sealed class Game
    {
        public int day;
        public int nutrients;
        public List<Cell> board;
        public List<Action> possibleActions;
        public List<Tree> trees;
        public int mySun, opponentSun;
        public int myScore, opponentScore;
        public bool opponentIsWaiting;
    
        private readonly DistanceCalculator _distanceCalculator;
    
        public Game()
        {
            board = new List<Cell>();
            possibleActions = new List<Action>();
            trees = new List<Tree>();
    
            _distanceCalculator = new DistanceCalculator(board);
        }
    
        public Action GetNextAction()
        {      
    
            // TO Do
            // Try to complete and reseed in one move *******
            // Tidy up and refactor  
            // Move weighting scores to class level sso we can split out some methods    
    
            // COMPLETE
            // 
            // GROW
    
            // Shadows
            // Do something with them
    
            // Weighting parameters
            // All scores are weighted to give multipliers between 1 and 2. They can be weighted here 
            // COMPLETE
            var completeMultipler = 0.8;
            var richnessCompletionMultipier = 1.5;
            var edgeCompletionMultiplier = 1.0;
            // GROW
            var generalGrowMultiplier = 1.0;
            var smallestCostWeighting = 2.0;
            var richnessWeighting = 1.5;
    
            var toSize3Multiplier = 1.0;
            var toSize2Multiplier = 1.0;
            var toSize1Multiplier = 1.0;        
            
            //SEED
            var generalSeedMultiplier = 1.3;
            var seedNearCentreWeighting = 1.6;
            var seedDistanceApartWeighting = 2.0;
            var seedRichnessWeighting = 1.0;
            var numberOfSeedsWeighting = 1.8;           
                
            var actionsWithScores = new List<Tuple<Action, double>>();
    
            var numberOfTrees = CountTreeSizes(); 
    
            var totalRounds = 24;
            var allOutCompleteMultiplier = 1.0;
    
            var completeActions = possibleActions.Count(a => a.Type == "COMPLETE");
    
            // If we can't get another complete before the end just wait        
            var waitScore = 1.0;
    
            if(day == totalRounds-1)
            {
                // Work out if we can get a complete. Otherwise just wait
                // Can we complete?
                    // If so do it
                // If not can we grow one to 3 then complete
                    // If so do it
                // If not can we grow one to 2 then 3 then complete
                    // If so do it
                // If not can we grow one to 1 then 2 then 3 then complete
            
                //if(completeActions < 1)
                //{
                    //waitScore = 100000.0;
                //}
            }
    
            // If there's 0 or 1 seeds prioritise planting more
            if(numberOfTrees[0] < 2)
            {
                generalSeedMultiplier *=5;
            }
    
            // Grow and seed
            // Try to reseed as soon as we grow
            {
            
    
            }
    
            // If we have multiple trees to complete go all out
            if(completeActions>= 3)
            {
                allOutCompleteMultiplier = 100.0;
            }
            // If the number of trees that can be completed is the same as the number of rounds left just complete
            else if(numberOfTrees[3] >= (totalRounds - day))
            {
                allOutCompleteMultiplier = 1000.0;
            }
    
            //Console.Error.WriteLine($"numberOfSeeds:{numberOfSeeds}"); 
            Console.Error.WriteLine($"Day: {day}");
            // Score every action and then order them
            foreach(var action in possibleActions)
            {  
                var actionScore = 1.0;
    
                var treeCell = board.Find(b => b.Index == action.TargetCellIdx);
    
                if(action.Type == "WAIT")
                { 
                    actionScore *= waitScore;
                }
                else if(action.Type == "COMPLETE")
                { 
                    // Higher score for completing rich soil trees
                    var richnessScore = GetScaledValue((double)treeCell.Richness, 1.0, 3.0, 1.0, 2.0) * richnessCompletionMultipier;
                    actionScore *= richnessScore;
    
                    // Higher score for completing near the edges
                    //var distanceFromCentre = _distanceCalculator.GetDistanceFromCentre(action.targetCellIdx);
                    //var edgeBonus = GetScaledValue((double)distanceFromCentre, 1.0, 4.0, 1.0, 2.0) * edgeCompletionMultiplier;
                    //actionScore *= edgeBonus;
    
                    // More likely to complete near the end of the game
                    // We don't want to scale this because it should be heavily biased towards completing at the end of the game
                    var dayScore = 1.0;
                    
                    if(day > 22)
                    {
                        dayScore = 4.0;                 
                    }
                    else if(day > 20)
                    {
                        dayScore = 3.0;                
                    }
                    else if(day > 18)
                    {
                        dayScore = 2.0;                
                    }
                    else
                    {
                        dayScore = 1.0;
                    }   
    
                    actionScore *= dayScore;
                    
                    // General weighting
                    actionScore *= completeMultipler;
                                  
                    actionScore *= allOutCompleteMultiplier;
    
                }
                else if(action.Type == "SEED")
                {  
                    // The more seeds there are the less likely we are to seed
                    // Mote: This is very crude. There should be a better way to do this (maybe scale it)               
                    actionScore /= ((numberOfTrees[0] + 1) * numberOfSeedsWeighting);
    
                    //Prefer planting seeds in the centre                
                    // The target cell can be 0-3 away. If 3 away we want a 1 * multiplier, if 0 away we want 2. 
                    var distanceFromCentre = _distanceCalculator.GetDistanceFromCentre(action.TargetCellIdx);
    
                    var centreBonus = GetScaledValue(distanceFromCentre, 4.0, 0.0, 1.0, 2.0);
                    centreBonus *= seedNearCentreWeighting;
                    actionScore *= centreBonus;  
    
                    // Higher score for seeding far away from tree
                    var distanceApart = _distanceCalculator.GetDistanceBetweenCells(action.SourceCellIdx, action.TargetCellIdx);
                    var distanceApartScore = GetScaledValue(distanceApart, 1.0, 3.0, 1.0, 2.0);
                    distanceApartScore *= seedDistanceApartWeighting;
    
                    actionScore *= distanceApartScore;
    
                    // Try to plant on richer soil
                    var richnessScore = GetScaledValue((double)treeCell.Richness, 1.0, 3.0, 1.0, 2.0) * seedRichnessWeighting;
                    actionScore *= richnessScore;
    
                    // General weighting
                    actionScore *= generalSeedMultiplier; 
                }
                else if(action.Type == "GROW")
                {
                    var tree = trees.Find(t => t.CellIndex == action.TargetCellIdx);                
                    
                    // Prioritise growing by richness  
                    //Console.Error.WriteLine("---------------------------------------------");
                    //Console.Error.WriteLine($"Richness score: {GetScaledValue(treeCell.richness, 1.0, 3.0, 1.0, 2.0) * richnessWeighting}");                                    
                    actionScore *= GetScaledValue(treeCell.Richness, 1.0, 3.0, 1.0, 2.0) * richnessWeighting;
    
                    // We prefer to grow fewer trees of the same size   
                    // get scaling min and max
                    var minTreeCount = Math.Min(numberOfTrees[1], Math.Min(numberOfTrees[2], numberOfTrees[3]));
                    var maxTreeCount = Math.Max(numberOfTrees[1], Math.Max(numberOfTrees[2], numberOfTrees[3]));
    
                    var sizeGoingTo = tree.Size + 1;
    
                    var amountOfThisSize =  numberOfTrees[tree.Size + 1];
    
                    // Invert it because smaller is better
                    var smallestCostMultiplier = GetScaledValue(amountOfThisSize, maxTreeCount, minTreeCount, 1.0, 2.0);
                    smallestCostMultiplier *= smallestCostWeighting;
                    Console.Error.WriteLine($"smallestCostMultiplier: {smallestCostMultiplier}");
                    actionScore *= smallestCostMultiplier;
                    Console.Error.WriteLine($"generalGrowMultiplier: {generalGrowMultiplier}");
    
                    // If this is a sizeTo3 add the final grow multiplier. This'll do nothing unless growing will take it to 3 for the last move
                    //if (sizeGoingTo == 3)
                    //{
                    //    actionScore *= toSize3Multiplier;
                    //}
                    //else if (sizeGoingTo == 2)
                    //{
                    // *= toSize2Multiplier;
                    //
    
                    //ingTo == 1)
                    //
                    // *= toSize1Multiplier;
                    //
    
                    // General weighting
                    actionScore *= generalGrowMultiplier;                  
                }
    
                actionsWithScores.Add(new Tuple<Action, double> ( action, actionScore));
            }
    
            // Output all actions with scores
            OutputActionsAndScores(actionsWithScores);
    
            Console.Error.WriteLine($"{actionsWithScores.OrderBy(a => a.Item2).Last().Item1.Type}");
    
            return actionsWithScores.OrderBy(a => a.Item2).Last().Item1;
        }
    
        private static double GetScaledValue(double valueToScale, double inputMin, double inputMax, double outputMin, double outputMax)
        {
            if(inputMax == inputMin) { return 1.0; }
    
            return ((valueToScale - inputMin) / (inputMax - inputMin)) * (outputMax - outputMin) + outputMin;         
        }
    
        private int[] CountTreeSizes()
        {
            var numberOfTrees = new int[4];
    
            numberOfTrees[0] = trees.Count(t => t.Size == 0 && t.IsMine);
            numberOfTrees[1] = trees.Count(t => t.Size == 1 && t.IsMine);
            numberOfTrees[2] = trees.Count(t => t.Size == 2 && t.IsMine);
            numberOfTrees[3] = trees.Count(t => t.Size == 3 && t.IsMine); 
    
            return numberOfTrees;
        }
    
        // Console error output
    
        private static void OutputTree(Tree tree)
        {
            if(tree != null)
            {
                Console.Error.WriteLine($"bestTree.cellIndex: {tree.CellIndex}");
                Console.Error.WriteLine($"bestTree.size: {tree.Size}");
            }
        }
    
        private static void OutputActionsAndScores(List<Tuple<Action, double>> actionsWithScores)
        {
            foreach(var actionWithScore in actionsWithScores)
            {
                Console.Error.WriteLine($"Action type: {actionWithScore.Item1.Type}");
                Console.Error.WriteLine($"targetCellIdx: {actionWithScore.Item1.TargetCellIdx}");
                Console.Error.WriteLine($"sourceCellIdx: {actionWithScore.Item1.SourceCellIdx}");
                Console.Error.WriteLine($"Score: {actionWithScore.Item2}");    
                Console.Error.WriteLine($"-------------------------------------------------");         
            }
        }
    
        private void OutputIndexes(List<int> indexes)
        {        
            foreach(int index in indexes)
            {
                Console.Error.Write($"{index} ,");
            }
            Console.Error.WriteLine("");
            Console.Error.WriteLine("");
        }
    }
    
    internal sealed class DistanceCalculator
    {
        private bool[] _hasBeenChecked;
        private readonly List<Cell> _cells;
    
        internal DistanceCalculator(List<Cell> cells)
        {
            _cells = cells;
    
            _hasBeenChecked = new bool[38];
        }
    
        internal int GetDistanceFromCentre(int cellIndex)
        {
            return GetDistanceBetweenCells(0, cellIndex);
        }
        
    
        internal int GetDistanceBetweenCells(int index1, int index2)
        {
            if(index1 == index2)
            {
                return 0;
            }
    
            _hasBeenChecked = new bool[38];
    
            var cell1 = _cells.Find(c => c.Index == index1);
    
            _hasBeenChecked[cell1.Index] = true;
    
            var toCheck = new List<int>
            {
                cell1.Index
            };
    
            int distance = 1;
            
    
            while (distance <= 6)
            {
                // get ones to check 
                var neighbouringIndexes = GetNeighbouringIndexes(toCheck);
    
                // check them
                if(neighbouringIndexes.Contains(index2))
                {
                    return distance;
                }
    
                // mark them as checked
                foreach(int index in neighbouringIndexes)
                {              
                    _hasBeenChecked[index] = true;
                }
    
                // update 
                toCheck = neighbouringIndexes;
    
                distance++;
            }
    
            // We should never get here
            return 7;
        }
    
        private List<int> GetNeighbouringIndexes(List<int> indexes)
        {        
            var neighbouringIndexes = new List<int>();
    
            foreach(int index in indexes)
            {
                var neighbourIndexes = _cells.Find(c => c.Index == index).Neighbours;
    
                foreach(int neighbourIndex in neighbourIndexes)
                {
                    if(neighbourIndex != -1 && !_hasBeenChecked[neighbourIndex] && !neighbouringIndexes.Contains(neighbourIndex))
                    {
                        neighbouringIndexes.Add(neighbourIndex);
                    }
                }        
            }
    
            return neighbouringIndexes;
        }
    }
}