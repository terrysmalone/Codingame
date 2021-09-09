using System;
using System.Collections.Generic;
using System.Linq;

namespace Spring2021Challenge
{
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

        private int _moveNum = 0;
    
        public Action GetNextAction()
        {
            Console.Error.WriteLine($"_moveNum: {_moveNum}");
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
                    if(   Round >= _totalRounds-5 
                          || numberOfTrees[0] > 0
                          || SeedHasDirectNeighbour(targetCell))
                    {
                        actionScore = 0;
                    }
                    else
                    {
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
                        
                        // If we have only 2 rounds left prioritise getting size 2 trees to size 3
                        if(Round == _totalRounds-2 && tree.Size == 2)
                        {
                            actionScore *= 10; 
                        }
                        // If we have only 3 rounds left prioritise getting size 1 trees to size 2
                        else if(Round == _totalRounds-3 && tree.Size == 1)
                        {
                            actionScore *= 10;
                        }
                        else if(Round == _totalRounds-4 && tree.Size == 0)
                        {
                            actionScore *= 10;
                        }
                    }
                }
    
                actionsWithScores.Add(new Tuple<Action, double> ( action, actionScore));
            }
    
            // Output all actions with scores
            OutputActionsAndScores(actionsWithScores.OrderBy(a => a.Item2).ToList(), true);
    
            var highestScoringAction = actionsWithScores.OrderBy(a => a.Item2).Last().Item1;

            _moveNum++;

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
}