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
        public int Size { get; set; }
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
        public int Round { get; set; }
        public int Nutrients { get; set; }
        public int MySun { get; set; }
        public int OpponentSun { get; set; }
        public int MyScore { get; set; }
        public int OpponentScore { get; set; }
        public bool OpponentIsWaiting { get; set; }
        
        public List<Cell> Board { get; }
        public List<Action> PossibleActions { get; }
        public List<Tree> Trees { get; }
    
        private readonly DistanceCalculator _distanceCalculator;
        private readonly int _totalRounds = 24;

        private int _sunDirection = 0;
    
        public Game()
        {
            Board = new List<Cell>();
            PossibleActions = new List<Action>();
            Trees = new List<Tree>();
    
            _distanceCalculator = new DistanceCalculator(Board);
        }
        int _lastPlantedOnRound = 0;
        
    
        public Action GetNextAction()
        {
            _sunDirection = Round % 6;
            
            //------------------------------------------------------------------------
            var nextSunDirection = _sunDirection + 1;

            if (nextSunDirection > 5)
            {
                nextSunDirection = 0;
            }
            
            var sunPointCalculator = new SunPointCalculator(Board, Trees, nextSunDirection);
            
            //------------------------------------------------------------------------
            
            //Console.Error.WriteLine("==========================================");
            //Console.Error.WriteLine($"Round: {Round}");
            
            // TO Do
            // Try to complete and reseed in one move. If we can't, seed on the turn after a complete
            // Tidy up and refactor  

            // COMPLETE
            // Is it sensible to all out COMPLETE after 3 size 3 trees. It seems too rigid a heuristic
            // GROW
            //
            // SEED
            // Relax the no seed next door rule after a while. Maybe when there are a certain number of trees
            // If we have 2 points left spew seeds. It could win us a draw

          var actionsWithScores = new List<Tuple<Action, double>>();
    
            var numberOfTrees = CountTreeSizes(); 
    
            // If we can't get another complete before the end just wait        
            var waitScore = 1.0;
            
            // Score every action and then order them
            foreach(var action in PossibleActions)
            {  
                var actionScore = 1.0;
    
                var targetCell = Board.Find(b => b.Index == action.TargetCellIdx);
                
                if(action.Type == "WAIT")
                { 
                    actionScore *= waitScore;
                }
                else if(action.Type == "COMPLETE")
                {
                    // Until endgame Never complete if we have 3 or fewer size 3 trees 
                    if(Round < 18 && numberOfTrees[3] <= 3)
                    {
                        actionScore = 0;
                    }
                    else
                    {
                        // Higher score for completing rich soil trees
                        var richnessScore = GetScaledValue(targetCell.Richness, 1.0, 3.0, 1.0, 2.0);
                        actionScore *= richnessScore;
                        
                        // Adding plus 3 is a simple way to scale it to 0, since completing seems like a loss. 
                        //var sunPointScore = CalculateSunPointScore(sunPointCalculator, action, false) + 3;

                        //actionScore *= sunPointScore;

                        // More likely to complete near the end of the game
                        // We don't want to scale this because it should be heavily biased towards completing at the end of the game
                        var dayScore = 1.0;

                        if (Round > 22)
                        {
                            dayScore = 8.0;
                        }
                        else if (Round > 20)
                        {
                            dayScore = 6.0;
                        }
                        else if (Round > 19)
                        {
                            dayScore = 4.0;
                        }
                        else if (Round == 18 || Round == 19)
                        {
                            dayScore = 100.0;
                        }
                        

                        actionScore *= dayScore;
                    }
                }
                else if(action.Type == "SEED")
                {  
                    // Hard no rules
                    //
                    // 1. We never seed right nest to another tree
                    // 2. If there are less than 5 days left there's no point in planting new seeds because they can't complete
                    //
                    // Day    | t-5 | t-4 | t-3 | t-2 | t-1 |
                    // Action |  S  |  1  |  2  |  3  |  C  |

                    // We never want to seed next to ourselves
                    // Note: this is covered by the rule below but we may want to distinguish between them
                    // at some point so it stays
                    //if(_distanceCalculator.GetDistanceBetweenCells(action.SourceCellIdx, action.TargetCellIdx) == 1)
                    //{
                    //    actionScore = 0;
                    //}
                    
                    // Change to 
                    // Trees <= 6 && direct neighbour
                    // Trees >6 && Trees <= 8 && >1 neighbour
                    // Trees > 8 && direct neighbour
                    if(   Round >= _totalRounds-5 
                       || Round == _lastPlantedOnRound
                       || SeedHasDirectNeighbour(targetCell))
                    {
                        actionScore = 0;
                    }
                    else
                    {
                        // The more seeds there are the less likely we are to seed
                        // Mote: This is very crude. There should be a better way to do this (maybe scale it)               
                        actionScore /= ((numberOfTrees[0] + 1));
        
                        //Prefer planting seeds in the centre                
                        // The target cell can be 0-3 away. If 3 away we want a 1 * multiplier, if 0 away we want 2. 
                        var distanceFromCentre = _distanceCalculator.GetDistanceFromCentre(action.TargetCellIdx);
        
                        var centreBonus = GetScaledValue(distanceFromCentre, 4.0, 0.0, 1.0, 2.0);
                        actionScore *= centreBonus;  
        
                        // Higher score for seeding far away from tree
                        var distanceApart = _distanceCalculator.GetDistanceBetweenCells(action.SourceCellIdx, action.TargetCellIdx);
                        var distanceApartScore = GetScaledValue(distanceApart, 1.0, 3.0, 1.0, 2.0);
                        actionScore *= distanceApartScore;
        
                        // Try to plant on richer soil
                        var richnessScore = GetScaledValue(targetCell.Richness, 1.0, 3.0, 1.0, 2.0);
                        actionScore *= richnessScore;
                        
                        // If there's 0 or 1 seeds prioritise planting more
                        //if(numberOfTrees[0] < 2)
                        //{
                        //    actionScore *=5;
                        //}
                    }
                }
                else if(action.Type == "GROW")
                {
                    var tree = Trees.Find(t => t.CellIndex == action.TargetCellIdx);    
                                        
                    // Day    | t-5 | t-4 | t-3 | t-2 | t-1 |
                    // Action |  S  |  1  |  2  |  3  |  C  |
                    //
                    // On the last day don't grow. We won't be able to complete
                    if (Round == _totalRounds - 1)
                    {
                        // If we're on the last round don't grow or seed. We can't complete
                        actionScore = 0;
                    }
                    // If we have only 2 rounds left we can only grow a 2 to completion by the end (since we have to wait a day to complete)
                    if(Round == _totalRounds-2 && tree.Size <= 1)
                    {
                        actionScore = 0;
                    }
                    // If we have only 3 rounds left we can only grow a 1 to completion by the end (since we have to wait a day to complete)
                    else if(Round == _totalRounds-3 && tree.Size <= 0)
                    {
                        actionScore = 0;
                    }  

                    // If we've hard blocked growing by this stage don't bother scoring it
                    if (actionScore != 0)
                    {
                        var sunPointScore = CalculateSunPointScore(sunPointCalculator, action, false);

                        // We don't want any factors other than sun score moving this up by more than 1 decimal place.
                        // Scale all other scores between 0 and 0.99

                        // Prioritise growing by richness
                        var nonSunScore = GetScaledValue(targetCell.Richness, 1.0, 3.0, 1.0, 2.0);

                        // We prefer to grow fewer trees of the same size   
                        // get scaling min and max
                        var minTreeCount = Math.Min(numberOfTrees[1], Math.Min(numberOfTrees[2], numberOfTrees[3]));
                        var maxTreeCount = Math.Max(numberOfTrees[1], Math.Max(numberOfTrees[2], numberOfTrees[3]));
                        var amountOfThisSize =  numberOfTrees[tree.Size + 1];

                        // Invert it because smaller is better
                        nonSunScore += GetScaledValue(amountOfThisSize, maxTreeCount, minTreeCount, 1.0, 2.0);
  
                        actionScore = sunPointScore + GetScaledValue(nonSunScore, 0, 2, 0, 0.99);
                    }
                }
    
                actionsWithScores.Add(new Tuple<Action, double> ( action, actionScore));
            }
    
            // Output all actions with scores
            OutputActionsAndScores(actionsWithScores.OrderBy(a => a.Item2).ToList(), true);
    
            var highestScoringAction = actionsWithScores.OrderBy(a => a.Item2).Last().Item1;

            if(highestScoringAction.Type == "SEED")
            {
                _lastPlantedOnRound = Round;
            }

            return highestScoringAction;
        }
        
        private static int CalculateSunPointScore(SunPointCalculator sunPointCalculator, Action action, bool outputDebugging)
        {
            // Baseline is how many points we'll get if we do nothing
            var baseLineSunPoints = sunPointCalculator.CalculateSunPoints();
            var baseLineScore = baseLineSunPoints.Item1 - baseLineSunPoints.Item2;

            if (outputDebugging)
            {
                Console.Error.WriteLine("--------------------------------");
                OutputAction(action);
            }

            sunPointCalculator.DoAction(action);
            var sunPoints = sunPointCalculator.CalculateSunPoints();

            if (outputDebugging)
            {
                Console.Error.WriteLine($"My       points: {sunPoints.Item1}");
                Console.Error.WriteLine($"Opponent points: {sunPoints.Item2}");
            }

            sunPointCalculator.UndoLastAction();

            return (sunPoints.Item1 - sunPoints.Item2) - baseLineScore;
        }
        
        private static double GetScaledValue(double valueToScale, double inputMin, double inputMax, double outputMin, double outputMax)
        {
            if(inputMax == inputMin) { return 1.0; }
    
            return ((valueToScale - inputMin) / (inputMax - inputMin)) * (outputMax - outputMin) + outputMin;         
        }
    
        private int[] CountTreeSizes()
        {
            var numberOfTrees = new int[4];
    
            numberOfTrees[0] = Trees.Count(t => t.Size == 0 && t.IsMine);
            numberOfTrees[1] = Trees.Count(t => t.Size == 1 && t.IsMine);
            numberOfTrees[2] = Trees.Count(t => t.Size == 2 && t.IsMine);
            numberOfTrees[3] = Trees.Count(t => t.Size == 3 && t.IsMine); 
    
            return numberOfTrees;
        }
        
        private bool SeedHasDirectNeighbour(Cell cell)
        {
            foreach (var neighbourindex in cell.Neighbours)
            {
                if(Trees.Find(t => t.CellIndex == neighbourindex && t.IsMine) != null)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private int NumberOfSurroundingTrees(Cell cell)
        {
            var count = 0;
            
            foreach (var neighbourindex in cell.Neighbours)
            {
                if(Trees.Find(t => t.CellIndex == neighbourindex && t.IsMine) != null)
                {
                    count++;
                }
            }
            
            return count;
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

        private static void OutputActionsAndScores(List<Tuple<Action, double>> actionsWithScores, bool showZeroScoredActions = true)
        {
            foreach(var actionWithScore in actionsWithScores)
            {
                if(!showZeroScoredActions && actionWithScore.Item2 == 0) { continue; }
                
                Console.Error.WriteLine($"Action type: {actionWithScore.Item1.Type}");
                Console.Error.WriteLine($"targetCellIdx: {actionWithScore.Item1.TargetCellIdx}");
                Console.Error.WriteLine($"sourceCellIdx: {actionWithScore.Item1.SourceCellIdx}");
                Console.Error.WriteLine($"Score: {actionWithScore.Item2}");    
                Console.Error.WriteLine($"--------------");         
            }
        }
        
        private static void OutputAction(Action action)
        { 
            Console.Error.WriteLine($"Action type: {action.Type}");
            Console.Error.WriteLine($"targetCellIdx: {action.TargetCellIdx}");
            Console.Error.WriteLine($"sourceCellIdx: {action.SourceCellIdx}");
            Console.Error.WriteLine("");
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
    
    // PLAN
    //
    // 1. As a first pass take the necessary parameters and calculate the sun points
    //      * Consideration: we'll need to deep copy something
    // 2. Pass in a next move and calculate the sun points. 
    // 3. Pass in an opponent move and see the sun points
    // 4. Pass in multiple moves and calculate sun points
    internal sealed class SunPointCalculator
    {
        private readonly List<Cell> _boardCells;
        private readonly List<Tree> _trees;
        private readonly int _sunDirection;

        private bool[] _inSpookyShadow;

        internal SunPointCalculator(List<Cell> boardCells, List<Tree> trees, int sunDirection)
        {
            _boardCells = boardCells;
            _trees = trees;
            _sunDirection = sunDirection;

            _inSpookyShadow = new bool[37];
        }

        internal Tuple<int, int> CalculateSunPoints()
        {
            CalculateShadowedCells();

            return CalculatePoints();
        }
        
        private void CalculateShadowedCells()
        {
            // foreach tree calculate it's shadow
            foreach (var tree in _trees)
            {
                if (tree.Size > 0)
                {
                    // Calculate first shadow
                    var treeCell = _boardCells.Find(c => c.Index == tree.CellIndex);
                    
                    if (treeCell == null) { continue; }
                    
                    var shadowIndex = treeCell.Neighbours[_sunDirection];

                    if (IsTreeInSpookyShadow(tree.Size, shadowIndex))
                    {
                        _inSpookyShadow[shadowIndex] = true;
                    }

                    // If size is 2 calculate 2nd shadow
                    if (tree.Size > 1)
                    {
                        var shadowCell = _boardCells.Find(c => c.Index == shadowIndex);

                        if (shadowCell == null) { continue; }
                        
                        shadowIndex = shadowCell.Neighbours[_sunDirection];

                        if (IsTreeInSpookyShadow(tree.Size, shadowIndex))
                        {
                            _inSpookyShadow[shadowIndex] = true;
                        }
                    }

                    // If size is 3 calculate 3rd shadow
                    if (tree.Size > 2)
                    {
                        var shadowCell = _boardCells.Find(c => c.Index == shadowIndex);
                        
                        if (shadowCell == null) { continue; }
                        
                        shadowIndex = shadowCell.Neighbours[_sunDirection];

                        if (IsTreeInSpookyShadow(tree.Size, shadowIndex))
                        {
                            _inSpookyShadow[shadowIndex] = true;
                        }
                    }
                }
            }
        }
        
        private bool IsTreeInSpookyShadow(int castingTreeSize, int shadowedTreeIndex)
        {
            // If the tree casts a shadow on a tree that's smaller than or equal to it, then it's spooky
            if (_trees.Find(t => t.CellIndex == shadowedTreeIndex) != null
                && _trees.Find(t => t.CellIndex == shadowedTreeIndex).Size <= castingTreeSize)
            {
                return true;
            }

            return false;
        }
        
        private Tuple<int, int> CalculatePoints()
        {
            var mySunPoints = 0;
            var opponentSunPoints = 0;
            
            foreach (var tree in _trees.Where(tree => !_inSpookyShadow[tree.CellIndex]))
            {
                //Console.Error.WriteLine("============================================");
                
                if (tree.IsMine)
                {
                    //Console.Error.WriteLine("My tree");
                    //Console.Error.WriteLine($"tree.CellIndex: {tree.CellIndex}");
                    //Console.Error.WriteLine($"tree.Size: {tree.Size}");

                    mySunPoints += tree.Size;
                }
                else
                {
                    //Console.Error.WriteLine("Opponents tree");
                    //Console.Error.WriteLine($"tree.CellIndex: {tree.CellIndex}");
                    //Console.Error.WriteLine($"tree.Size: {tree.Size}"); 
                    opponentSunPoints += tree.Size;
                }
            }

            return new Tuple<int, int>(mySunPoints, opponentSunPoints);
        }
        
        Action _lastAction;
        Tree _lastRemovedTree;
        Tree _lastSeededTree;

        internal void DoAction(Action action)
        {
            _lastAction = action;
            
            if(action.Type == "COMPLETE")
            {
                _lastRemovedTree = _trees.Find(t => t.CellIndex == action.TargetCellIdx);        // We probably need to deep copy here
                
                _trees.Remove(_lastRemovedTree);
            }
            if(action.Type == "SEED")
            {
                // We have to assume it's not
                _lastSeededTree = new Tree(action.TargetCellIdx, 0, true, false);
                   
                _trees.Add(_lastSeededTree);
            }
            else if (action.Type == "GROW")
            {
                var tree = _trees.Find(t => t.CellIndex == action.TargetCellIdx);

                tree.Size++;
            }
        }
        
        internal void UndoLastAction()
        {
            if(_lastAction.Type == "COMPLETE")
            {
                _trees.Add(_lastRemovedTree);
            }
            if(_lastAction.Type == "SEED")
            {
                _trees.Remove(_lastSeededTree);
            }
            else if (_lastAction.Type == "GROW")
            {
                var tree = _trees.Find(t => t.CellIndex == _lastAction.TargetCellIdx);

                tree.Size--;
            }
        }
    }
}

// Previous Rank: 993
// Current Rank: 