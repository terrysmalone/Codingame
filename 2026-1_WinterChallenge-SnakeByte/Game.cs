using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Numerics;
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

    private Dictionary<Point, int> _closestSnakeToPowerSourceMap = new Dictionary<Point, int>();

    private const int BASE_POWER_SCORE = 100000;
    private const int PATH_LENGTH_PENALTY = 100;
    private const int BASE_CLIMBABLE_LEDGE_SCORE = 20000;
    private const int BASE_WANDER_SCORE = 5000;
    private const int BASE_CRITICAL_MOVE_SCORE = 1000000;
    private const int CLOSER_TO_POWER_SCORE = 500;

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
        _closestSnakeToPowerSourceMap = _positionChecker.GetClosestPowerSourceToOpponentSnakeMap();

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
                (Plan? headClashMove, bool excludeMove) = GetCriticalHeadClashMove(possibleMove, snakeBot);

                if (headClashMove != null)
                {
                    Logger.Plans("Added head clash move", new List<Plan> { headClashMove });
                    snakeBot.AddPlan(headClashMove);
                    goldenMovesAdded++;
                    continue;
                }
                
                if (excludeMove)
                {
                    excludePoints.Add(possibleMove);
                }
                // exclude a move if it seems immediately blocking
                else if (_positionChecker.IsBlocking(possibleMove, snakeBot))
                {
                    excludePoints.Add(possibleMove);
                }

                // Add sensible exclude moves here
                // 1. If move is blocking it should be critical exclude path
                // 2. If any opponent snake move can trap me it's an excludeMove
            }

            Logger.LogTime($"Checked for head clash. Added {goldenMovesAdded} plans");            

            if (snakeBot.IsStuck())
            {
                excludePoints.Add(snakeBot.GetLastMove());
            }

            // TODO: We'll want to return more than one here at some point
            List<Plan> bestPlansToPowerSources = GetBestPlansToPowerSources(snakeBot, excludePoints);
            UpdateScores(bestPlansToPowerSources, snakeBot);

            snakeBot.AddPlans(bestPlansToPowerSources);
            Logger.LogTime("Finished path finding");
            
            // Before wandering randomly, try to get some paths to nearby platforms that are above me
            List<Plan> climbableLedgePlans = GetClimbableLedgePlans(snakeBot, excludePoints);
            UpdateScores(climbableLedgePlans, snakeBot);
            snakeBot.AddPlans(climbableLedgePlans);

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
                    }
                }
            }

            Logger.Plans($"Plans for snake {snakeBot.Id}", snakeBot.GetPlans());
        }

        // Get all combinations of all plans. Score them by adding the scores together. Pick the highest scoring combination that doesn't have any clashes.
        Dictionary<List<Plan>, int> planCombinations = GetAllPlanCombinations();
        
        // Logger.PlanCombinations(planCombinations);

        Logger.LogTime($"Got all plan combinations: {planCombinations.Count}");

        // Get the first combination that doesn't have any clashes for the next move
        foreach (var planCombination in planCombinations.OrderByDescending(pc => pc.Value))
        {
            HashSet<Point> plannedStartMoves = new HashSet<Point>();
            HashSet<Point> plannedEndMoves = new HashSet<Point>();
            bool clash = false;

            // TODO: At some point we might want to consider amount
            // For example, if there are 2  power ups, we shouldn't go for both this turn
            Point? lastPowerSource = null;

            if (_level.PowerSources.Count == 1 && GetMyScore() - GetEnemyScore() < 0)
            {
                lastPowerSource = _level.PowerSources[0];
            }


            foreach (var plan in planCombination.Key)
            {
                if (plannedStartMoves.Contains(plan.Moves[0])
                    || plannedEndMoves.Contains(plan.Moves[plan.Moves.Count - 1])
                    || lastPowerSource == plan.Moves[0])
                {
                    clash = true;
                    break;
                }
                else
                {
                    plannedStartMoves.Add(plan.Moves[0]);
                    plannedEndMoves.Add(plan.Moves[plan.Moves.Count - 1]);
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

    private List<Plan> GetClimbableLedgePlans(SnakeBot snakeBot, HashSet<Point> excludePoints)
    {
        List<Plan> plans = new List<Plan>();
        var shortestPathPoints = new List<Point>();

        //bool stopLooking = false;
        int maxDistance = 5;

        plans = GetShortestPathToClimbableLedgePlans(snakeBot, maxDistance, excludePoints);

        return plans;
    }

    private List<Plan> GetShortestPathToClimbableLedgePlans(SnakeBot snakeBot, int maxDistance, HashSet<Point> excludePoints)
    {
        List<Plan> plans = new List<Plan>();

        // Get lowest point on SnakeBot body
        var lowestSnakePoint = snakeBot.Body.OrderByDescending(p => p.Y).First();

        foreach (Point ledge in _level.GetWalkableLedges())
        {
            if(ledge.Y >= lowestSnakePoint.Y)
            {
                continue;
            }

            // Don't bother trying if it's further away than maxDistance
            int manhattanDistance = CalculationUtil.GetManhattanDistance(snakeBot.Body[0], ledge);
            if (manhattanDistance >= maxDistance)
            {
                continue;
            }

            // Don't try if something is on the ledge
            if(_positionChecker.IsPointInAnySnake(ledge, countTails: false))
            {
                continue;
            }

            List<Point> path = _pathFinder.GetShortestPath(snakeBot.Body.First(), ledge, snakeBot, excludePoints.ToList());

            if (path?.Count > 0)
            {
                // Create a plan for this path
                int score = BASE_CLIMBABLE_LEDGE_SCORE;

                plans.Add(new Plan(path, score, "climbing", turnsToFruition: path.Count, snakeBot.Id));
                
                if (maxDistance >= 10)
                {
                    // Exit early. Paths of 10 can be expensive
                    break;
                }
            }
        }

        Logger.LogTime($"Finished looking for ledges with max distance {maxDistance}");

        return plans;
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

    private List<Plan> GetBestPlansToPowerSources(SnakeBot snakeBot, HashSet<Point> excludePoints)
    {
        List<Plan> plans = new List<Plan>();
        var shortestPathPoints = new List<Point>();

        // Use an iterative deepening approach to finding targets
        bool stopLooking = false;
        int maxDistance = 5;

        while (stopLooking == false)
        {
            plans = GetShortestPathToPowerSourcePlans(snakeBot, maxDistance, excludePoints);

            if (plans.Count > 0)
            {
                stopLooking = true;
            }

            maxDistance += 5;
            if (maxDistance > 10)
            {
                stopLooking = true;
            }
        }

        return plans;
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
            directionScores.Add(checkPoint, BASE_WANDER_SCORE);
        }

        RemoveAllHardNos(directionScores, snakeBot);

        if (directionScores.Count == 0)
        {
            return new List<Plan>();
        }

        UpdateScores(directionScores, snakeBot);
         
        // Make plans from the direction scores

        List<Plan> plans = new List<Plan>();

        foreach (var directionScore in directionScores)
        {
            plans.Add(new Plan(new List<Point> { directionScore.Key }, directionScore.Value, "wander", turnsToFruition: 1, snakeBot.Id));
        }

        return plans;
    }

    private void UpdateScores(List<Plan> plans, SnakeBot snakeBot)
    {
        int scoreChange = 0;
        foreach (Plan plan in plans)
        {
            Point newHeadPosition = plan.Moves[0];

            scoreChange += ScoreChangeForOtherSnakeBodyPositions(newHeadPosition, snakeBot);
            scoreChange += ScoreChangeForSpaceCreated(newHeadPosition, snakeBot);
            scoreChange += ScoreChangeForStuckDirections(newHeadPosition, snakeBot);
            scoreChange += ScoreChangeForPosition(newHeadPosition, snakeBot);
            
            plan.Score = plan.Score += scoreChange;
        }
    }

   

    private void UpdateScores(Dictionary<Point, int> directionScores, SnakeBot snakeBot)
    {
        foreach (var direction in directionScores)
        {
            int scoreChange = 0;
            Point newHeadPosition = direction.Key;

            scoreChange += ScoreChangeForOtherSnakeBodyPositions(newHeadPosition, snakeBot);
            scoreChange +=  ScoreChangeForSpaceCreated(newHeadPosition, snakeBot);
            scoreChange += ScoreChangeForStuckDirections(newHeadPosition, snakeBot);
            scoreChange += ScoreChangeForPosition(newHeadPosition, snakeBot);

            directionScores[direction.Key] = directionScores[direction.Key] += scoreChange;
        }
    }

    private int ScoreChangeForOtherSnakeBodyPositions(Point movePoint, SnakeBot snakeBot)
    {
        int scoreChange = 0;

        if (_positionChecker.IsPointInAnySnake(movePoint, countTails: true, snakeBot.Id)
            || _positionChecker.IsPointInGivenSnake(snakeBot.Body, movePoint, countTails: false))
        {
            // We never want to do this unless it's the only choice. Give it a preposterously low score
            scoreChange =  -BASE_CRITICAL_MOVE_SCORE;
        }

        return scoreChange;
        
    }

    private int ScoreChangeForSpaceCreated(Point movePoint, SnakeBot snakeBot)
    {
        // Use flood fill to either move to a more open space, or to give the opponent less space
        // Score the current position:
        // For every direction score all my snake flood fills minus all opponent square flood fills.
        // The highest one wins.
        // TODO: At some point check for all opponent moves here too. 
        // For example, If I go left, give a score for all opponent moves. Count the worse one for me as the score. 

        // TODO: Simulate the movement (just adding a head and removing a tail. At some point we might want to think about
        // simulating gravity but not yet
        List<Point> newSnakeBody = new List<Point>() { movePoint };
        newSnakeBody.AddRange(snakeBot.Body.Take(snakeBot.Body.Count - 1));

        int scoreChange = 0;

        // For the flood fill I want to exclude this snake ID, but include newSnakeBody
        foreach (var mySnake in MySnakeBots)
        {
            if (mySnake.Id == snakeBot.Id)
            {
                scoreChange += _positionChecker.FloodFillCount(newSnakeBody[0], snakeBot.Id, newSnakeBody, 20);
            }
            else
            {
                scoreChange += _positionChecker.FloodFillCount(mySnake.Body[0], snakeBot.Id, newSnakeBody, 20);
            }
        }

        foreach (var opponentSnake in OpponentSnakeBots)
        {
            scoreChange -= _positionChecker.FloodFillCount(opponentSnake.Body[0], snakeBot.Id, newSnakeBody, 20);
        }

        return scoreChange; 
    }

    private int ScoreChangeForPosition(Point movePoint, SnakeBot snakeBot)
    {
        int scoreChange = 0;
        // Add small position bonuses
        // At the start of the game move towards the centre and up. When there are hardly any 
        // power sources left, head towards the nearest one
        // TODO: Add bonus for heading towards the most power sources
  
        if (_level.PowerSources.Count > 2)
        {
            // If the head is out of bounds, and this move will bring it back in, give it a stronger bonus

            if (_positionChecker.IsOutOfMapBounds(snakeBot.Body[0]) && !_positionChecker.IsOutOfMapBounds(movePoint))
            {
                scoreChange += 10;
            }

            int distanceFromCentre = CalculationUtil.GetManhattanDistance(movePoint, new Point(Width / 2, Height / 2));
            scoreChange -= distanceFromCentre;

            // Add a small bonus for moving towards the top of the map
            int distanceFromTop = movePoint.Y;
            scoreChange -= distanceFromTop;
        }
        else
        {
            // Bonus for moving nearer to the nearest powersource
            int distanceToPowerSource = CalculationUtil.GetManhattanDistance(
                movePoint,
                GetNearestPowerSource(movePoint));

            scoreChange -= distanceToPowerSource;
        }

        return scoreChange;
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

    

    private int ScoreChangeForStuckDirections(Point movePoint, SnakeBot snakeBot)
    {
        int scoreChange = 0;

        if (_positionChecker.IsStuckMove(movePoint, snakeBot))
        {
            scoreChange = -50;
        }

        return scoreChange;
    }

    // Create plans for any clashing moves that would result in a positive headclash
    // For example, if I can move into a position where an enemy snake could move into
    // on their next turn but they are smaller than me, then it's worth it
    // TODO: Only make it a critical move if it's guaranteed. If the opponent can avoid it just make it a good
    // // move
    private (Plan?, bool) GetCriticalHeadClashMove(Point newHeadPosition, SnakeBot snakeBot)
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
        else if (diff == 0 && powerUpOnSpot)
        {
            score = BASE_CRITICAL_MOVE_SCORE + 3500;
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

    private bool CheckForHeadClash(Point newHeadPosition, SnakeBot snakeBot)
    {
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
            return false;
        }


        /**
         * 
         * int myLossOnImpact = 0;
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
        else if (diff == 0 && powerUpOnSpot)
        {
            score = BASE_CRITICAL_MOVE_SCORE + 3500;
        }
        else if (diff == 0 && snakeBot.Body.Count == clashingEnemyBodySizes.Min() && GetMyScore() > GetEnemyScore())
        {
            score = BASE_CRITICAL_MOVE_SCORE + 3000;
        }
        else if (diff < 0)
        {
            excludeMove = true;
        }
         * 
         * 
         * 
         * 
         * 
         */

        bool powerUpOnSpot = _level.PowerSources.Contains(newHeadPosition);

        if (powerUpOnSpot)
        {// If there is a power up the only reason not to take it is if the enemy snake is bigger
         // and they can destroy me on their next turn. 
            if (snakeBot.Body.Count < clashingSnakeSize && snakeBot.Body.Count <= 3)
            {
                return true;
            }
            else
            {
                return true;
            }
        }
        else
        {
            // TODO: At some point we'll want to split out more than and equal to, since more than
            // should score way higher
            if (snakeBot.Body.Count >= clashingSnakeSize)
            {
                return false;
            }
            else
            {
                return false;
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

    private List<Plan> GetShortestPathToPowerSourcePlans(SnakeBot snakeBot, int maxDistance, HashSet<Point> excludePoints)
    {
        List<Plan> plans = new List<Plan>();

        foreach (Point powerSource in _level.PowerSources)
        {
            if (snakeBot.HasCheckedPowerSource(powerSource))
            {
                continue;
            }

            // Don't bother trying if it's further away than maxDistance
            int manhattanDistance = CalculationUtil.GetManhattanDistance(snakeBot.Body[0], powerSource);
            if (manhattanDistance >= maxDistance)
            {
                continue;
            }

            // If the snake can't reach the power source from a platform don't even bother trying
            // TODO: We should really check more than just power to the nearest platform. We need to check if an entire path can be made.
            //       For exaample. Power to ledge1, ledge1 to ledge2, ledge 2 to ledge that snake is already on.
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

            if (path?.Count > 0)
            {
                // Create a plan for this path
                int score = BASE_POWER_SCORE - (path.Count * PATH_LENGTH_PENALTY); // Small penalty for longer paths

                // If it's a small path and doing this blocks an opponent from getting to a power source
                // give bonus points
                if (path.Count <= 5)
                {
                    if (_closestSnakeToPowerSourceMap.ContainsKey(powerSource))
                    {
                        int closestSnakeId;
                        _closestSnakeToPowerSourceMap.TryGetValue(powerSource, out closestSnakeId);

                        SnakeBot closestSnake = OpponentSnakeBots.First(s => s.Id == closestSnakeId);

                        if (closestSnake != null)
                        {
                            int closestSnakeDistance = int.MaxValue;

                            List<Point> closestSnakePath = _pathFinder.GetShortestPath(closestSnake.Body.First(), powerSource, closestSnake, new List<Point>());
                            if (closestSnakePath?.Count > 0)
                            {
                                if (closestSnakePath.Count < path.Count)
                                {
                                    // Enemy is closer
                                    score -= CLOSER_TO_POWER_SCORE;

                                }
                                else
                                {
                                    // I'm closer, or we'll draw
                                    // TODO: If it's a draw we only sometimes want to go for it
                                    score += CLOSER_TO_POWER_SCORE;
                                }
                            }
                        }
                    }
                }

                plans.Add(new Plan(path, score, "power", turnsToFruition: path.Count, snakeBot.Id));
                Logger.LogTime($"Added path to power source {powerSource.X}, {powerSource.Y}. Path: {string.Join(", ", path.Select(p => $"({p.X},{p.Y})"))}. Score:{score}");


                if (maxDistance >= 10)
                {
                    // Exit early. Paths of 10 can be expensive
                    break;
                }
            }
            else
            {
                Logger.LogTime($"No path to power source {powerSource.X}, {powerSource.Y}.");
            }
        }

        Logger.LogTime($"Finished looking for power sources with max distance {maxDistance}");

        return plans;
    }

    internal List<Point> GetPowerSources()
    {
        return _level.PowerSources;
    }

    internal HashSet<Point> GetAllPlatformPositions()
    {
        return _level.GetAllPlatformPositions();
    }
}