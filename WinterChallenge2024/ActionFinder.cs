﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace WinterChallenge2024;

internal sealed class ActionFinder
{
    private readonly Game _game;
    private readonly DirectionCalculator _directionCalculator;
    private readonly AStar _aStar;

    private List<Protein> _proteinsToCheck = new List<Protein>();

    public ActionFinder(Game game, DirectionCalculator directionCalculator)
    {
        _game = game;
        _directionCalculator = directionCalculator;
        _aStar = new AStar(game);
    }

    internal List<Action> GetProteinActions(Organism organism, List<Protein> proteins, int maxDistance)
    {
        List<Action> actions = new List<Action>();

        _proteinsToCheck = new List<Protein>();

        foreach (Protein protein in proteins)
        {
            if (!protein.IsHarvested)
            {
                _proteinsToCheck.Add(protein.Clone());
            }
        }

        if (_proteinsToCheck.Count == 0) return actions;

        // TODO: Just get the one move Harvests by checkeing to the sides

        // Search at max distance of 1
        actions.AddRange(GetShortestPathsToProteins(organism, 1, GrowStrategy.ALL_PROTEINS));
        if (_proteinsToCheck.Count == 0) return actions;

        // Search at max distance of 2
        actions.AddRange(GetShortestPathsToProteins(organism, 2, GrowStrategy.NO_PROTEINS));
        if (_proteinsToCheck.Count == 0) return actions;
        actions.AddRange(GetShortestPathsToProteins(organism, 2, GrowStrategy.UNHARVESTED));
        if (_proteinsToCheck.Count == 0) return actions;

        // Search at max distance of 3, being willing to walk over other proteins
        // Search at max distance of 3, being not willing to walk over other proteins
        actions.AddRange(GetShortestPathsToProteins(organism, 3, GrowStrategy.NO_PROTEINS));
        if (_proteinsToCheck.Count == 0) return actions;
        actions.AddRange(GetShortestPathsToProteins(organism, 3, GrowStrategy.UNHARVESTED));
        if (_proteinsToCheck.Count == 0) return actions;

        // Search at max distance of 4, being willing to walk over other proteins
        // Search at max distance of 4, being not willing to walk over other proteins
        actions.AddRange(GetShortestPathsToProteins(organism, 4, GrowStrategy.NO_PROTEINS));
        if (_proteinsToCheck.Count == 0) return actions;
        actions.AddRange(GetShortestPathsToProteins(organism, 4, GrowStrategy.UNHARVESTED));
        if (_proteinsToCheck.Count == 0) return actions;

        // Search at max distance of 5, being willing to walk over other proteins
        // Search at max distance of 5, being not willing to walk over other proteins
        actions.AddRange(GetShortestPathsToProteins(organism, 5, GrowStrategy.NO_PROTEINS));
        if (_proteinsToCheck.Count == 0) return actions;
        actions.AddRange(GetShortestPathsToProteins(organism, 5, GrowStrategy.UNHARVESTED));
        if (_proteinsToCheck.Count == 0) return actions;

        return actions;   
    }

    private IEnumerable<Action> GetShortestPathsToProteins(Organism organism, int maxDistance, GrowStrategy growStrategy)
    {
        List<Action> actions = new List<Action>();

        List<int> proteinsToRemove = new List<int>();
 
        for (int i = 0; i < _proteinsToCheck.Count; i++)
        {
            Protein protein = _proteinsToCheck[i];
            foreach (Organ organ in organism.Organs)
            {
                int manhattanDistance = MapChecker.CalculateManhattanDistance(organ.Position, protein.Position);
                
                if (manhattanDistance > maxDistance)
                {
                    continue;
                }

                List<Point> path = _aStar.GetShortestPath(organ.Position, protein.Position, maxDistance, growStrategy);
                
                if (path.Count > 0)
                {
                    //actions.Add(new Tuple<int, ProteinType, List<Point>>(organ.Id, protein.Type, path));

                    Action? action = CreateAction(organism.RootId, organ.Id, protein.Type, path);

                    if (action != null)
                    {
                        actions.Add(action);
                    }
                   
                    if (!proteinsToRemove.Contains(i) && maxDistance != 1)
                    {
                        proteinsToRemove.Add(i);
                    }
                }
            }
        }

        for (int i = proteinsToRemove.Count - 1; i >= 0; i--)
        {
            _proteinsToCheck.RemoveAt(proteinsToRemove[i]);
        }

        return actions;
    }

    private Action? CreateAction(int organismId, int organId, ProteinType proteinType, List<Point> path)
    {
        Action? action = new Action();

        action.ActionType = ActionType.GROW;
        action.TargetPosition = path[0];
        action.OrganismId = organismId;
        action.OrganId = organId;
        
        action.GoalProteinType = proteinType;

        if (path.Count == 1)
        {
            action.TurnsToGoal = 1;
            action.GoalType = GoalType.CONSUME;

            action.OrganType = GetOrgan(path[0]);
          
            if (action.OrganType != OrganType.BASIC)
            {
                action.OrganDirection = _directionCalculator.CalculateClosestOpponentDirection(path[0]);
            }
        }
        else if (path.Count == 2)
        {
            if (!CostCalculator.CanProduceOrgan(OrganType.HARVESTER, _game.PlayerProteinStock))
            {
                return null;
            }
            
            action.TurnsToGoal = 1;
            action.GoalType = GoalType.HARVEST;

            action.OrganType = OrganType.HARVESTER;

            action.OrganDirection = _directionCalculator.GetDirection(path[0], path[1]);

            // Check if there are any consumed proteins on the path 
            if (_game.hasAnyProtein[path[0].X, path[0].Y])
            {
                ProteinType pType = _game.proteinTypes[path[0].X, path[0].Y];

                action.ConsumedProteins.TryGetValue(pType, out var currentCount);
                action.ConsumedProteins[pType] = currentCount + 1;
            }
        }
        else
        {
            if (!CostCalculator.CanProduceOrgan(OrganType.HARVESTER, _game.PlayerProteinStock))
            {
                return null;
            }

            action.TurnsToGoal = path.Count - 1;
            action.GoalType = GoalType.HARVEST;

            // Check if there are any consumed proteins on the path 
            if (_game.hasAnyProtein[path[0].X, path[0].Y])
            {
                ProteinType pType = _game.proteinTypes[path[0].X, path[0].Y];

                action.ConsumedProteins.TryGetValue(pType, out var currentCount);
                action.ConsumedProteins[pType] = currentCount + 1;
            }

            action.OrganType = GetOrgan(path[0]);

            if (action.OrganType != OrganType.BASIC)
            {
                action.OrganDirection = _directionCalculator.CalculateClosestOpponentDirection(path[0]);
            }
        }

        return action;
    }

    private OrganType GetOrgan(Point point)
    {
        bool hasProtein = _game.hasAnyProtein[point.X, point.Y];
        // If we can make it a tentacle and still have some spare proteins then do it
        if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, _game.PlayerProteinStock, 3) && !hasProtein)
        {
            return OrganType.TENTACLE;
        }
        else if (CostCalculator.CanProduceOrgan(OrganType.BASIC, _game.PlayerProteinStock))
        {
            return OrganType.BASIC;
        }
        else if (CostCalculator.CanProduceOrgan(OrganType.SPORER, _game.PlayerProteinStock))
        {
            return OrganType.SPORER;
        }
        else if (CostCalculator.CanProduceOrgan(OrganType.HARVESTER, _game.PlayerProteinStock))
        {
            return OrganType.HARVESTER;
        }
        else if (CostCalculator.CanProduceOrgan(OrganType.TENTACLE, _game.PlayerProteinStock))
        {
            return OrganType.TENTACLE;
        }

        return OrganType.BASIC;
    }
}