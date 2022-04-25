using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpringChallenge2022;

internal sealed class MovementGenerator
{
    private readonly Point _playerBaseLocation;
    private readonly Point _enemyBaseLocation;
    private readonly ValuesProvider _valuesProvider;

    public MovementGenerator(Point playerBaseLocation,
                             Point enemyBaseLocation,
                             ValuesProvider valuesProvider)
    {
        _playerBaseLocation = playerBaseLocation;
        _enemyBaseLocation = enemyBaseLocation;
        _valuesProvider = valuesProvider;
    }

    internal void AssignHeroMovement(List<Hero> playerHeroes, List<Monster> monsters)
    {
        var defendingHeroesOutsideOfBase = playerHeroes.Where(h => h.Strategy == Strategy.Defend
                                                                   && CalculateDistance(h.Position, _playerBaseLocation) > _valuesProvider.BaseRadius);

        foreach (var defendingHeroOutsideOfBase in defendingHeroesOutsideOfBase)
        {
            defendingHeroOutsideOfBase.CurrentMonster = -1;
        }

        CalculateDefenderMovement(playerHeroes, monsters);

        CalculateCollectorMovement(playerHeroes, monsters);

        CalculateAttackerMovement(playerHeroes, monsters);

        // Assign actions
        foreach (var hero in playerHeroes)
        {
            if (hero.CurrentMonster != -1)
            {
                var monsterToAttack = monsters.Single(m => m.Id == hero.CurrentMonster);

                hero.CurrentAction = $"MOVE {monsterToAttack.Position.X} {monsterToAttack.Position.Y}";
            }
            else
            {
                var currentGuardPoint = hero.GetCurrentGuardPoint();

                if (!(hero.Position.X == currentGuardPoint.X && hero.Position.Y == currentGuardPoint.Y))
                {
                    hero.CurrentAction = $"MOVE {currentGuardPoint.X} {currentGuardPoint.Y}";
                }
                else
                {
                    var nextGuardPoint = hero.GetNextGuardPoint();
                    hero.CurrentAction = $"MOVE {nextGuardPoint.X} {nextGuardPoint.Y}";
                }
            }
        }
    }
    private void CalculateAttackerMovement(IEnumerable<Hero> playerHeroes, IEnumerable<Monster> monsters)
    {
        var freeAttackingHeroes = playerHeroes.Where(h => h.Strategy == Strategy.Attack && h.CurrentMonster == -1).ToList();

        if (freeAttackingHeroes.Count <= 0)
        {
            return;
        }

        // Get any monsters on the edge of the enemies base
        var monstersOnOutskirts = monsters.Where(m => CalculateDistance(m.Position, _enemyBaseLocation) > _valuesProvider.OutskirtsMinDist
                                                      && CalculateDistance(m.Position, _enemyBaseLocation) < _valuesProvider.OutskirtsMaxDist).ToList();

        // Go to them
        if (monstersOnOutskirts.Count > 1)
        {
            foreach (var freeAttackingHero in freeAttackingHeroes)
            {
                var closestMonster = monstersOnOutskirts.OrderBy(m => CalculateDistance(freeAttackingHero.Position, m.Position))
                                                        .First();

                freeAttackingHero.CurrentMonster = closestMonster.Id;
            }
        }
    }

    private void CalculateDefenderMovement(IReadOnlyCollection<Hero> playerHeroes, List<Monster> monsters)
    {
        // if a hero is not in the base, and a spider is, drop everything and defend
        var monstersThreateningBase = monsters.Where(m => m.ThreatFor == ThreatFor.Player
                                                                         && CalculateDistance(m.Position, _playerBaseLocation) <= 6000)
                                                         .OrderBy(m => CalculateDistance(m.Position, _playerBaseLocation))
                                                         .ToList();

        var freeDefendingHeroes = playerHeroes.Where(h => h.Strategy == Strategy.Defend && h.CurrentMonster == -1).ToList();

        if (monstersThreateningBase.Count == 0 && freeDefendingHeroes.Count <= 0)
        {
            return;
        }

        if (monstersThreateningBase.Count > 0)
        {
            var closestMonster = monstersThreateningBase.First();

            foreach (var hero in freeDefendingHeroes)
            {
                hero.CurrentMonster = closestMonster.Id;
            }
        }
        else
        {
            // Monsters on outskirts might have to be considered

            Console.Error.WriteLine($"freeDefendingHeroes.Count: {freeDefendingHeroes.Count}");

            foreach (var freeDefendingHero in freeDefendingHeroes)
            {
                var monsterWithinRange = monsters.Where(m => CalculateDistance(m.Position, _playerBaseLocation) <= _valuesProvider.MaxDefenderDistanceFromBase
                                                             && m.ThreatFor != ThreatFor.Enemy)
                                                 .Select(m => new { m, distance = CalculateDistance(m.Position, freeDefendingHero.Position) })
                                                 .Where(m => m.distance <= _valuesProvider.HeroRange)
                                                 .OrderBy(m => m.distance)
                                                 .Select(m => m.m)
                                                 .FirstOrDefault();

                if (monsterWithinRange != null)
                {
                    Console.Error.WriteLine($"Hero {freeDefendingHero.Id} attacking monster {monsterWithinRange.Id}");
                    freeDefendingHero.CurrentMonster = monsterWithinRange.Id;
                }
            }
        }
    }

    private void CalculateCollectorMovement(IEnumerable<Hero> playerHeroes, IEnumerable<Monster> monsters)
    {

        var collectingHeroes = playerHeroes.Where(h => h.Strategy == Strategy.Collect && h.CurrentMonster == -1).ToList();

        if (collectingHeroes.Count > 0)
        {
            foreach (var collectingHero in collectingHeroes)
            {
                var closestMonster = monsters.Where(m => CalculateDistance(m.Position, _playerBaseLocation) > _valuesProvider.OutskirtsMaxDist)
                                             .Select(m => new { m, distance = CalculateDistance(m.Position, collectingHero.Position) })
                                             .Where(m => m.distance <= _valuesProvider.HeroRange)
                                             .OrderBy(m => m.distance)
                                             .Select(m => m.m)
                                             .FirstOrDefault();

                if (closestMonster != null)
                {
                    collectingHero.CurrentMonster = closestMonster.Id;
                }
                else
                {
                    var currentGuardPoint = collectingHero.GetCurrentGuardPoint();

                    if (!(collectingHero.Position.X == currentGuardPoint.X && collectingHero.Position.Y == currentGuardPoint.Y))
                    {
                        collectingHero.CurrentAction = $"MOVE {currentGuardPoint.X} {currentGuardPoint.Y}";
                    }
                    else
                    {
                        var nextGuardPoint = collectingHero.GetNextGuardPoint();
                        collectingHero.CurrentAction = $"MOVE {nextGuardPoint.X} {nextGuardPoint.Y}";
                    }
                }
            }
        }
    }

    private static double CalculateDistance(Point position, Point position2)
    {
        return Math.Sqrt(Math.Pow(position.X - position2.X, 2)
                         + Math.Pow(position.Y - position2.Y, 2));
    }
}

