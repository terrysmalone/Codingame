using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Xml.Linq;
using System.Xml.Schema;

namespace _2026_1_WinterChallenge_SnakeByte;

internal class Game
{
    internal int Width { get; private set; }
    internal int Height { get; private set; }   

    internal List<SnakeBot> MySnakeBots { get; set; }
    internal List<SnakeBot> OpponentSnakeBots { get; set; }

    private Level _level;

    private PathFinder _pathFinder;
    private PositionChecker _positionChecker;

    private const int BASE_POWER_SCORE = 10000;
    private const int BASE_WANDER_SCORE = 500;
    private const int BASE_CRITICAL_MOVE_SCORE = 100000;

    public Game(int width, int height, bool[,] platforms)
    {
        Width = width;
        Height = height;

        _level = new Level(width, height, platforms);
                
        _positionChecker = new PositionChecker(this, _level);
        _pathFinder = new PathFinder(this, _positionChecker);

        MySnakeBots = new List<SnakeBot>();
        OpponentSnakeBots = new List<SnakeBot>();
    }

    internal void AddMySnake(SnakeBot snakeBot)
    {
        MySnakeBots.Add(snakeBot);
    }

    internal void AddOpponentSnake(SnakeBot snakeBot)
    {
        OpponentSnakeBots.Add(snakeBot);
    }

    internal void MarkAllSnakesForRemoval()
    {
       foreach (var snakeBot in MySnakeBots)
        {
            snakeBot.Remove = true;
        }

        foreach (var snakeBot in OpponentSnakeBots)
        {
            snakeBot.Remove = true;
        }
    }

    internal void RemoveMarkedSnakes()
    {
        // Iterate through the snakes backwards to safely remove any where Remove == true
        for (int i = MySnakeBots.Count - 1; i >= 0; --i)
        {
            if (MySnakeBots[i].Remove)
            {
                MySnakeBots.RemoveAt(i);
            }
        }

        for (int i = OpponentSnakeBots.Count - 1; i >= 0; --i)
        {
            if (OpponentSnakeBots[i].Remove)
            {
                OpponentSnakeBots.RemoveAt(i);
            }
        }
    }

    internal SnakeBot GetSnake(int snakebotId)
    {
        return MySnakeBots.FirstOrDefault(s => s.Id == snakebotId) ?? OpponentSnakeBots.FirstOrDefault(s => s.Id == snakebotId);
    }

    internal void AddPowerSource(int x, int y)
    {
        _level.PowerSources.Add(new Point(x, y));
    }

    internal void RemoveAllPowerSources()
    {
        _level.PowerSources.Clear();
    }    

    internal List<string> GetActions()
    {

        foreach (var snakeBot in MySnakeBots)
        {
            snakeBot.ClearAllPlans();
        }
        // BIG MAD REFACTOR PLAN
        // 
        // GENERAL
        // I want to be able to have a list of plans for each snake, all with their own scores. 
        // After I have plans for them all I work out the best combination of each making sure to
        // check for clashes.
        //
        // SCORING PRIORITIES
        // A plan is scored by two things. 
        // 1. How it affects the game score (eg. Getting a power up gains 1. Hitting an enemy will 
        //    destroy them (so I would lose 1 but they lose 2, meaning a net gain of 2). Destroying a 
        //    an enemy head is neutral but if I'm bigger than them it ultimately helps me, so count
        //    it ias a small gain.
        // 2. Multiplier based on how soon it'll happen. Next turn is guaranteed so should score 
        //    a lot higher. After that score reduces by time to fruition. 
        //
        // I'll add a mechanism to check how safe a move is, which will affect scoring. For example,
        // If no enemy is within range of a power up that I can get in 5 moves, then it's very safe.
        // 
        // Base scores
        // Destroys an enemy 4
        // Gains a power up 2
        // Destroys an enemy head where I'm bigger than them 1
        //
        // DECISION MAKING
        // If there are no clashes just picj the highest from each. 
        // Otherwise get the highest combination I can get from each without a clash

        // Logger.EntireGame(_level.Platforms, MySnakeBots, OpponentSnakeBots, _level.PowerSources);

        List<string> actions = new List<string>();

        foreach (var snakeBot in MySnakeBots)
        {
            // We track power sources we've tried to get to so that we don't keep trying at different depths
            snakeBot.ClearCheckedPowerSources();

            Logger.LogTime($"STARTING FOR SNAKEBOT {snakeBot.Id}. Position:{snakeBot.Body[0].X},{snakeBot.Body[0].Y}");
            // TODO: CHeck for chance toi destroy an opponent snake and do that if possible

            HashSet<Point> excludePoints = new HashSet<Point>();
            
            // If any surrounding points are in danger of the enemy attacking them add to exclude points
            var possibleMoves = new List<Point>()
            {
                new Point(snakeBot.Body[0].X + 1, snakeBot.Body[0].Y),
                new Point(snakeBot.Body[0].X - 1, snakeBot.Body[0].Y),
                new Point(snakeBot.Body[0].X, snakeBot.Body[0].Y + 1),
                new Point(snakeBot.Body[0].X, snakeBot.Body[0].Y - 1)
            };

            var goldenMovesAdded = 0;

            foreach (var possibleMove in possibleMoves)
            {
                (Plan? goldenMove, bool excludeMove) = GetGoldenMove(possibleMove, snakeBot);

                if (goldenMove != null)
                {
                    snakeBot.AddPlan(goldenMove);
                    goldenMovesAdded++;
                    continue;
                }
                
                // TODO: We'll want to add this back in at some point
                if (excludeMove)
                {
                    excludePoints.Add(possibleMove);
                }
                // exclude a move if it seems immediately blocking
                else if (_positionChecker.IsBlocking(possibleMove, snakeBot))
                {
                    excludePoints.Add(possibleMove);
                }
            }

            Logger.LogTime($"Checked for head clash. Added {goldenMovesAdded} plans");

            if (snakeBot.IsStuck())
            {
                excludePoints.Add(snakeBot.GetLastMove());
            }

            // TODO: We'll want to return more than one here at some point
            List<Point> bestPathToPower = GetBestPathToPowerSource(snakeBot, excludePoints);

            Logger.LogTime("Finished path finding");
            if (bestPathToPower.Count != 0)
            {
                snakeBot.AddPlan(new Plan(bestPathToPower, score: BASE_POWER_SCORE, "power", turnsToFruition: bestPathToPower.Count, snakeBot.Id));
            }

            List<Plan> validPlans = GetValidDirectionPointPlans(snakeBot);
            snakeBot.AddPlans(validPlans);
               
            Logger.LogTime($"Finished checking valid directions. Added {validPlans.Count} plans");              
            
        }

        // If any snake has a terrible move (usually meaning it would lower my score)
        // give the same score to any other moves for that snake that has the same first position
        foreach (var snakeBot in MySnakeBots)
        {
            var terribleMoves = snakeBot.GetPlans().Where(p => p.Score < -BASE_CRITICAL_MOVE_SCORE + 10000).ToList();

            foreach (Plan terribleMove in terribleMoves)
            {
                foreach (Plan plan in snakeBot.GetPlans())
                {
                    if (plan.Moves[0] == terribleMove.Moves[0])
                    {
                        plan.Score = terribleMove.Score;
                        Logger.Message($"Made move with first position {plan.Moves[0].X},{plan.Moves[0].Y} just as bad as terrible move with score {plan.Score}");
                    }
                }
            }

            Logger.Plans($"After...Plans for snake {snakeBot.Id}", snakeBot.GetPlans());
        }

        // Get all combinations of all plans. Score them by adding the scores together. Pick the highest scoring combination that doesn't have any clashes.
        Dictionary<List<Plan>, int> planCombinations = GetAllPlanCombinations();
        
        // Logger.PlanCombinations(planCombinations);

        Logger.LogTime($"Got all plan combinations: {planCombinations.Count}");

        // Get the first combination that doesn't have any clashes for the next move
        // TODO: We should check for clashes for the target too, because we don't want
        // snakes going for the same source. Wait until we return multiple power source 
        // plans to do this though.

        foreach (var planCombination in planCombinations.OrderByDescending(pc => pc.Value))
        {
            HashSet<Point> plannedPositions = new HashSet<Point>();
            bool clash = false;

            foreach (var plan in planCombination.Key)
            {
                if (plannedPositions.Contains(plan.Moves[0]))
                {
                    clash = true;
                    break;
                }
                else
                {
                    plannedPositions.Add(plan.Moves[0]);
                }
            }

            if (!clash)
            {
                Logger.LogTime($"Chose plan combination with score {planCombination.Value}");
                foreach (Plan? plan in planCombination.Key)
                {
                    var snakeBot = MySnakeBots.First(s => s.Id == plan.SnakeID);

                    string direction = DirectionHelper.GetDirection(snakeBot.Body[0], plan.Moves[0]);
                    
                    actions.Add($"{snakeBot.Id} {direction} {plan.PlanType}");

                    snakeBot.AddMove(plan.Moves[0]);

                    Point planTarget = plan.Moves[plan.Moves.Count - 1];
                    if (!_positionChecker.IsOutOfMapBounds(planTarget))
                    {
                        actions.Add($"MARK {planTarget.X} {planTarget.Y}");
                    }

                    Logger.Message($"Chose plan {plan.PlanType} with score {plan.Score} and direction {direction}");

                }
                break;
            }
        }

        if (actions.Count == 0)
        {
            actions.Add("WAIT");
        }

        return actions;
    }

    private Dictionary<List<Plan>, int> GetAllPlanCombinations()
    {
        var allSnakePlans = MySnakeBots.Select(s => s.GetPlans())
                                       .Where(plans => plans.Count > 0)
                                       .ToList();

        var planCombinations = new Dictionary<List<Plan>, int>();

        if (allSnakePlans.Count == 0)
        {
            return planCombinations;
        }

        GetAllPlanCombinationsRecursive(allSnakePlans, 0, new List<Plan>(), planCombinations);

        return planCombinations;
    }

    private void GetAllPlanCombinationsRecursive(List<List<Plan>> allSnakePlans, int snakeIndex, List<Plan> plans, Dictionary<List<Plan>, int> planCombinations)
    {
        if (snakeIndex == allSnakePlans.Count)
        {
            int score = plans.Sum(p => p.Score);
            planCombinations.Add(plans.ToList(), score);
            return;
        }
        foreach (var plan in allSnakePlans[snakeIndex])
        {
            plans.Add(plan);
            GetAllPlanCombinationsRecursive(allSnakePlans, snakeIndex + 1, plans, planCombinations);
            plans.RemoveAt(plans.Count - 1);
        }
    }

    private List<Point> GetBestPathToPowerSource(SnakeBot snakeBot, HashSet<Point> excludePoints)
    {
        int shortestPathCount = int.MaxValue;
        var shortestPathPoints = new List<Point>();

        // Use an iterative deepening approach to finding targets
        bool stopLooking = false;
        int maxDistance = 5;

        while (stopLooking == false)
        {
            List<Point> path = GetShortestPath(snakeBot, Math.Min(shortestPathCount - 1, maxDistance), excludePoints);

            if (path.Count > 0)
            {
                stopLooking = true;

                shortestPathCount = path.Count;
                shortestPathPoints = path.ToList();
            }

            maxDistance += 5;
            if (maxDistance > 10)
            {
                stopLooking = true;
            }
        }

        return shortestPathPoints;
    }

    private List<Plan> GetValidDirectionPointPlans(SnakeBot snakeBot)
    {        
        // Prioritise moving towards the nearest powersource
        // TODO: We should also prioritise climbing. For example, if I'm below a platform and I can get up there,
        // I should prioritise that as it opens up the map and gives more options.
        Point nearestPowerSource = GetNearestPowerSource(snakeBot);

        List<string> possibleDirections = nearestPowerSource.X > snakeBot.Body[0].X ? new List<string>() { "RIGHT", "UP", "DOWN", "LEFT" } 
                                                                                    : new List<string>() { "LEFT", "UP", "DOWN", "RIGHT" };

        Dictionary<Point, int> directionScores = new Dictionary<Point, int>();

        foreach (string direction in possibleDirections)
        {
            Point checkPoint = DirectionHelper.GetNewPosition(snakeBot.Body[0], direction);
            directionScores.Add(checkPoint, 0);
        }

        RemoveAllHardNos(directionScores, snakeBot);

        if (directionScores.Count == 0)
        {
            return new List<Plan>();
        }
        
        UpdateForOtherSnakeBodyPositions(directionScores, snakeBot);
         
        UpdateForHeadDangerPositions(directionScores, snakeBot);
        
        UpdateForStuckDirections(directionScores, snakeBot);
        
        // Use flood fill to either move to a more open space, or to give the opponent less space
        // Score the current position:
        // For every direction score all my snake flood fills minus all opponent square flood fills.
        // The highest one wins.
        // TODO: At some point check for all opponent moves here too. 
        // For example, If I go left, give a score for all opponent moves. Count the worse one for me as the score. 
        foreach (var directionScore in directionScores)
        {
            int score = BASE_WANDER_SCORE;

            // TODO: Simulate the movement (just adding a head and removing a tail. At some point we might want to think about
            // simulating gravity but not yet
            List<Point> newSnakeBody = new List<Point>() { directionScore.Key };
            newSnakeBody.AddRange(snakeBot.Body.Take(snakeBot.Body.Count - 1));
            
            // For the flood fill I want to exclude this snake ID, but include newSnakeBody
            foreach (var mySnake in MySnakeBots)
            {
                if (mySnake.Id == snakeBot.Id)
                {
                    score += _positionChecker.FloodFillCount(newSnakeBody[0], snakeBot.Id, newSnakeBody, 20);
                }
                else
                {
                    score += _positionChecker.FloodFillCount(mySnake.Body[0], snakeBot.Id, newSnakeBody, 20);
                }
            }

            foreach (var opponentSnake in OpponentSnakeBots)
            { 
                score -= _positionChecker.FloodFillCount(opponentSnake.Body[0], snakeBot.Id, newSnakeBody, 20);
            }

            directionScores[directionScore.Key] = directionScores[directionScore.Key] += score;
        }

        // Add small position bonuses
        // At the start of the game move towards the centre and up. When there are hardly any 
        // power sources left, head towards the nearest one
        // TODO: Add bonus for heading towards the most power sources
        foreach (var directionScore in directionScores)
        {
            if (_level.PowerSources.Count > 2)
            {
                // If the head is out of bounds, and this move will bring it back in, give it a stronger bonus
 
                if (_positionChecker.IsOutOfMapBounds(snakeBot.Body[0]) && !_positionChecker.IsOutOfMapBounds(directionScore.Key))
                {
                    directionScores[directionScore.Key] = directionScores[directionScore.Key] + 10;
                }

                int distanceFromCentre = CalculationUtil.GetManhattanDistance(directionScore.Key, new Point(Width / 2, Height / 2));
                directionScores[directionScore.Key] = directionScores[directionScore.Key] - distanceFromCentre;

                // Add a small bomus for moving towards the top of the map
                int distanceFromTop = directionScore.Key.Y;
                directionScores[directionScore.Key] = directionScores[directionScore.Key] - distanceFromTop;
            }
            else
            {
                // Bonus for moving nearer to the nearest powersource
                int distanceToPowerSource = CalculationUtil.GetManhattanDistance(
                    directionScore.Key, 
                    GetNearestPowerSource(directionScore.Key));
                directionScores[directionScore.Key] = directionScores[directionScore.Key] - distanceToPowerSource;

            }
        }


        // Make plans from the direction scores

        List<Plan> plans = new List<Plan>();

        foreach (var directionScore in directionScores)
        {
            plans.Add(new Plan(new List<Point> { directionScore.Key }, directionScore.Value, "wander", turnsToFruition: 1, snakeBot.Id));
        }

        return plans;
    }

    private void RemoveAllHardNos(Dictionary<Point, int> directionScores, SnakeBot snakeBot)
    {
        HashSet<Point> pointsToRemove = new HashSet<Point>();

        foreach (var directionScore in directionScores)
        {
            Point newHeadPosition = directionScore.Key;

            if (newHeadPosition.X < -1
                || newHeadPosition.X > Width
                || newHeadPosition.Y < -1
                || newHeadPosition.Y > Height
                || _positionChecker.IsPlatform(newHeadPosition)
                || _positionChecker.IsPointInAnySnake(newHeadPosition, countTails: false))
            {
                pointsToRemove.Add(newHeadPosition);
            }

            // If the new position is out of bounds, And the tail is the only one in bounds, hard no the move.
            if (_positionChecker.IsOutOfMapBounds(newHeadPosition))
            {
                int inBoundsCount = 0;

                foreach (var bodyPart in snakeBot.Body)
                {
                    if (!_positionChecker.IsOutOfMapBounds(bodyPart))
                    {
                        inBoundsCount++;
                    }
                }

                if (inBoundsCount <= 1)
                {
                    pointsToRemove.Add(newHeadPosition);
                }
            }
        }

       
          

        foreach (var point in pointsToRemove)
        {
            directionScores.Remove(point);
        }
    }

    private void UpdateForOtherSnakeBodyPositions(Dictionary<Point, int> possibleDirections, SnakeBot snakeBot)
    {
        foreach (var direction in possibleDirections)
        {
            Point newHeadPosition = direction.Key;

            if (_positionChecker.IsPointInAnySnake(newHeadPosition, countTails: true, snakeBot.Id)
                || _positionChecker.IsPointInGivenSnake(snakeBot.Body, newHeadPosition, countTails: false))
            {
                // We never want to do this unless it's the only choice. Give it a preposterously low score
                possibleDirections[direction.Key] = possibleDirections[direction.Key] - BASE_CRITICAL_MOVE_SCORE;
            }
        }        
    }

    private void UpdateForHeadDangerPositions(Dictionary<Point, int> possibleDirections, SnakeBot snakeBot)
    {
        foreach (var direction in possibleDirections)
        {
            Point newHeadPosition = direction.Key;

            // We don't have to worry about using the move here, since we check it as part
            // of the pathfinding move
            (_, var excludeMove) = CheckForHeadClash(newHeadPosition, snakeBot);

            if (excludeMove)
            {
                possibleDirections[direction.Key] = possibleDirections[direction.Key] - BASE_CRITICAL_MOVE_SCORE;
            }
        }        
    }

    private void UpdateForStuckDirections(Dictionary<Point, int> possibleDirections, SnakeBot snakeBot)
    {
        foreach (var direction in possibleDirections)
        {
            Point newHeadPosition = direction.Key;

            if (_positionChecker.IsStuckMove(newHeadPosition, snakeBot))
            {
                possibleDirections[direction.Key] = possibleDirections[direction.Key] - 50;
            }
            
        }
    }

    // Create plans for any clashing moves that would result in a positive headclash
    // For example, if I can move into a position where an enemy snake could move into
    // on their next turn but they are smaller than me, then it's worth it
    private (Plan?, bool) GetGoldenMove(Point newHeadPosition, SnakeBot snakeBot)
    {
        var excludeMove = false;

        List<int> clashingEnemyBodySizes = new List<int>();

        bool powerUpOnSpot = _level.PowerSources.Contains(newHeadPosition);

        foreach (var opponentSnake in OpponentSnakeBots)
        {
            var possibleHeadMoves = new List<Point>()
            {
                new Point(opponentSnake.Body[0].X + 1, opponentSnake.Body[0].Y),
                new Point(opponentSnake.Body[0].X - 1, opponentSnake.Body[0].Y),
                new Point(opponentSnake.Body[0].X, opponentSnake.Body[0].Y + 1),
                new Point(opponentSnake.Body[0].X, opponentSnake.Body[0].Y - 1)
            };

            foreach (Point possibleMove in possibleHeadMoves)
            {
               // Don't check invalid moves
                if (possibleMove.X < -1
                    || possibleMove.X > Width
                    || possibleMove.Y < -1
                    || possibleMove.Y > Height
                    || _positionChecker.IsPlatform(possibleMove)
                    || _positionChecker.IsPointInAnySnake(possibleMove, countTails: powerUpOnSpot))
                {
                    continue;
                }

                if (possibleMove == newHeadPosition)
                {
                    clashingEnemyBodySizes.Add(opponentSnake.Body.Count);
                }
            }
        }

        if (clashingEnemyBodySizes.Count == 0)
        {
            return (null, false);
        }

        int myLossOnImpact = 0;
        int enemyLossOnImpact = 0;

        if (powerUpOnSpot)
        {
            myLossOnImpact = 1;

            foreach (var enemyBodySize in clashingEnemyBodySizes)
            {
                enemyLossOnImpact += 1;
            }
        }
        else
        {
            myLossOnImpact = snakeBot.Body.Count <= 3 ? 3 : 1;

            foreach (var enemyBodySize in clashingEnemyBodySizes)
            {
                enemyLossOnImpact += enemyBodySize <= 3 ? 3 : 1;
            }
        }

        // If I win overall, it's a golden move (highest score)
        // If it's neutral, and I'm bigger than the enemy snake, it's a golden move (mid score)
        // If it's 100% neutral, it's only a golden move if I have the highest score (the game ending 
        // is in my favour.

        int diff = enemyLossOnImpact - myLossOnImpact;

        Console.Error.WriteLine($"Checking for head clash at {newHeadPosition.X},{newHeadPosition.Y}. My loss: {myLossOnImpact}, Enemy loss: {enemyLossOnImpact}, Diff: {diff}");
        int score = 0;

        if (diff > 0)
        {
            score = BASE_CRITICAL_MOVE_SCORE + 5000;           
        }
        else if (diff == 0 && snakeBot.Body.Count > clashingEnemyBodySizes.Min())
        {
            score = BASE_CRITICAL_MOVE_SCORE + 4000;
        }
        else if (diff == 0 && snakeBot.Body.Count == clashingEnemyBodySizes.Min() && GetMyScore() > GetEnemyScore())
        {
            score = BASE_CRITICAL_MOVE_SCORE + 3000;
        }
        else if (diff < 0)
        {
            excludeMove = true;
        }

        Plan? plan = null;
        
        if (score > 0)
        {
            score += _positionChecker.FloodFillCount(newHeadPosition, snakeBot.Id, snakeBot.Body, 20);
            plan = new Plan(new List<Point> { newHeadPosition }, score, "attack", turnsToFruition: 1, snakeBot.Id);
        }

        return (plan, excludeMove);
    }

    private int GetEnemyScore()
    {
        return GetTotalBodyCount(OpponentSnakeBots);
    }

    private int GetMyScore()
    {
        return GetTotalBodyCount(MySnakeBots);
    }

    private static int GetTotalBodyCount(List<SnakeBot> snakes)
    {
        int bodyCount = 0;

        foreach (var snake in snakes)
        {
            bodyCount += snake.Body.Count;
        }

        return bodyCount;
    }

    private (bool useMove, bool excludeMove) CheckForHeadClash(Point newHeadPosition, SnakeBot snakeBot)
    {
        // If an enemy snake's head could move to the new head position decide how to deal with it
        // 
        // If there is a power up on that spot
        //    If their snake is smaller than mine, it's not a problem
        //    If the snakes are the same, it's not a problem as we would both die and take each other out
        //    If my snake is smaller than theirs
        //       If my snake will die, let them have it
        //       Else go for it, we don't want to give them an extra point
        // If there is not a power up on that spot
        //    If their snake is smaller than mine, it's not a problem
        //    If the snakes are the same, it's neutral but we probably want to avoid it as we could easily get unlucky and lose
        //    If my snake is smaller than theirs, it's a problem, avoid it

        bool headClash = false;

        // Track the size of the biggest snake. That's the one that matters
        // TODO: Technically, there's can be more advantage in clashing with
        // multiple snakes, since I can theoretically destroy multiple snakes
        // at once if I'm bigger than them
        int clashingSnakeSize = 0;

        foreach (var opponentSnake in OpponentSnakeBots)
        {
            var possibleHeadMoves = new List<Point>()
            {
                new Point(opponentSnake.Body[0].X + 1, opponentSnake.Body[0].Y),
                new Point(opponentSnake.Body[0].X - 1, opponentSnake.Body[0].Y),
                new Point(opponentSnake.Body[0].X, opponentSnake.Body[0].Y + 1),
                new Point(opponentSnake.Body[0].X, opponentSnake.Body[0].Y - 1)
            };

            // Exclude any points that are not valid moves. I don't want to count them as head clashes
            for (int i = 3; i >= 0; i--)
            {
                Point possibleMove = possibleHeadMoves[i];
                if (possibleMove.X < -1
                    || possibleMove.X > Width
                    || possibleMove.Y < -1
                    || possibleMove.Y > Height
                    || _positionChecker.IsPlatform(possibleMove)
                    || _positionChecker.IsPointInAnySnake(possibleMove, countTails: false))
                {
                    possibleHeadMoves.RemoveAt(i);
                }
            }

            if (possibleHeadMoves.Contains(newHeadPosition))
            {
                headClash = true;
                if (opponentSnake.Body.Count > clashingSnakeSize)
                {
                    clashingSnakeSize = opponentSnake.Body.Count;
                }
            }
        }

        if (!headClash)
        {
            return (useMove: false, excludeMove: false);
        }

        bool powerUpOnSpot = _level.PowerSources.Contains(newHeadPosition);

        if (powerUpOnSpot)
        {// If there is a power up the only reason not to take it is if the enemy snake is bigger
         // and they can destroy me on their next turn. 
            if (snakeBot.Body.Count < clashingSnakeSize && snakeBot.Body.Count <= 3)
            {
                return (useMove: false, excludeMove: true);
            }
            else
            {
                return (useMove: true, excludeMove: true);
            }
        }
        else
        {
            // TODO: At some point we'll want to split out more than and equal to, since more than
            // should score way higher
            if (snakeBot.Body.Count >= clashingSnakeSize)
            {
                return (useMove: true, excludeMove: false);
            }
            else
            {
                return (useMove: false, excludeMove: true);
            }
        }
    }

    private Point GetNearestPowerSource(SnakeBot snakeBot)
    {
        return GetNearestPowerSource(snakeBot.Body[0]);
    }

    private Point GetNearestPowerSource(Point point)
    {
        int nearestDistance = int.MaxValue;
        Point nearestPowerSource = new Point(-1, -1);

        foreach (var powerSource in _level.PowerSources)
        {
            int distance = CalculationUtil.GetManhattanDistance(point, powerSource);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPowerSource = powerSource;
            }
        }

        return nearestPowerSource;
    }

    private List<Point> GetShortestPath(SnakeBot snakeBot, int maxDistance, HashSet<Point> excludePoints)
    {
        int shortestPathCount = int.MaxValue;
        int shortestManhattanDistanceCount = int.MaxValue;
        var shortestPathPoints = new List<Point>();

        var checkedSources = 0;
        foreach (Point powerSource in _level.PowerSources)
        {
            if (snakeBot.HasCheckedPowerSource(powerSource))
            {
                continue;
            }

            // Don't bother trying if it's further away than the shortest one we've found
            int manhattanDistance = CalculationUtil.GetManhattanDistance(snakeBot.Body[0], powerSource);
            if (manhattanDistance >= maxDistance 
                || manhattanDistance >= shortestPathCount)
                //|| manhattanDistance >= shortestManhattanDistanceCount)
            {
                continue;
            }

            // If the snake can't reach the power source from a platform don't even bother trying
            if (snakeBot.Body.Count < _positionChecker.GetNearestPlatformDistance(powerSource, snakeBot.Id) - 1)
            {
                continue;
            }

            if (snakeBot.GetAttemptsAtPowerSource(powerSource) > 20)
            {
                snakeBot.ClearAttemptsAtPowerSource(powerSource);
                continue;                
            }

            snakeBot.AddAttemptAtPowerSource(powerSource);

            List<Point> path = _pathFinder.GetShortestPath(snakeBot.Body.First(), powerSource, snakeBot, excludePoints.ToList());

            snakeBot.AddCheckedPowerSource(powerSource);
            checkedSources++;

            if (path != null && path.Count > 0 && path.Count < shortestPathCount)
            {
                shortestManhattanDistanceCount = manhattanDistance;
                shortestPathCount = path.Count;
                shortestPathPoints = path.ToList();
            }
        }

        Logger.LogTime($"Finished looking with max distance {maxDistance}. Checked {checkedSources} power sources.");

        return shortestPathPoints;
    }

    internal List<Point> GetPowerUps()
    {
        return _level.PowerSources;
    }

    internal HashSet<Point> GetAllPlatformPositions()
    {
        return _level.GetAllPlatformPositions();
    }
}