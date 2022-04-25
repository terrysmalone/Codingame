﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpringChallenge2022;

internal sealed class SpellGenerator
{
    private int _estimatedManaLeft;
    
    private readonly Point _playerBaseLocation;
    private readonly Point _enemyBaseLocation;
    private readonly ValuesProvider _valuesProvider;

    public SpellGenerator(Point playerBaseLocation,
                          Point enemyBaseLocation,
                          ValuesProvider valuesProvider)
    {
        _playerBaseLocation = playerBaseLocation;
        _enemyBaseLocation = enemyBaseLocation;
        _valuesProvider = valuesProvider;
    }

    internal void CastProtectiveShieldSpells(IEnumerable<Hero> playerHeroes, Strategy strategy)
    {
        foreach (var hero in playerHeroes.Where(h => h.Strategy == strategy))
        {
            if (_estimatedManaLeft < 10)
            {
                break;
            }

            if (hero.ShieldLife == 0)
            {
                PerformSpell(hero, $"SPELL SHIELD {hero.Id}");

                hero.IsShielding = true;
            }
        }
    }

    internal void AssignDefensiveWindSpell(List<Hero> playerHeroes, IEnumerable<Monster> monsters)
    {
        if (_estimatedManaLeft < 10)
        {
            return;
        }

        var closeDistance = 3000;

        var closestMonster = monsters.FirstOrDefault(m => CalculateDistance(m.Position, _playerBaseLocation) <= closeDistance
                                                               && m.ShieldLife == 0);

        if (closestMonster != null)
        {
            Debugger.DisplayPlayerHeroes(playerHeroes);

            var availableHeroes = playerHeroes.Where(h => h.Strategy == Strategy.Defend && h.IsShielding == false).ToList();

            if (availableHeroes.Count > 0)
            {
                var closestHero = availableHeroes.OrderBy(h => CalculateDistance(h.Position, closestMonster.Position))
                                                 .First();

                if (CalculateDistance(closestHero.Position, closestMonster.Position) <= ValuesProvider.WindSpellRange)
                {
                    Console.Error.WriteLine("Hero casting wind");
                    PerformSpell(closestHero, $"SPELL WIND {_enemyBaseLocation.X} {_enemyBaseLocation.Y}");
                }
                else
                {
                    // Too far away for wind to work

                    // If he's close and we can control that little shit away do it
                    if (CalculateDistance(closestMonster.Position, _playerBaseLocation) <= _valuesProvider.CloseToBaseRange
                        && CalculateDistance(closestHero.Position, closestMonster.Position) <= _valuesProvider.ControlSpellange)
                    {
                        Console.Error.WriteLine("Hero casting control");
                        PerformSpell(closestHero, $"SPELL CONTROL {closestMonster.Id} {_enemyBaseLocation.X} {_enemyBaseLocation.Y}");
                    }
                }
            }
        }
    }

    internal void AssignDefenderControlSpells(IEnumerable<Hero> playerHeroes, IEnumerable<Monster> monsters)
    {
        const int healthCutOff = 10;

        if (_estimatedManaLeft < 10)
        {
            return;
        }

        var defendingHeroesOutsideOfBase =
            playerHeroes.Where(h => h.Strategy == Strategy.Defend
                                      && h.IsShielding == false
                                      && CalculateDistance(h.Position, _playerBaseLocation) > _valuesProvider.BaseRadius);

        foreach (var defendingHeroOutsideOfBase in defendingHeroesOutsideOfBase)
        {
            if (_estimatedManaLeft < 10)
            {
                return;
            }

            var monsterWithinSpellRange =
                monsters.Where(m => m.Health > healthCutOff
                                        && m.IsControlled == false
                                        && m.ThreatFor != ThreatFor.Enemy
                                        && m.ShieldLife == 0
                                        && CalculateDistance(m.Position, _playerBaseLocation) > _valuesProvider.BaseRadius)
                        .Select(m => new { m, distance = CalculateDistance(m.Position, defendingHeroOutsideOfBase.Position)})
                        .Where(m => m.distance <= _valuesProvider.ControlSpellange)
                        .OrderBy(m => m.distance)
                        .Select(m => m.m)
                        .FirstOrDefault();

            if (monsterWithinSpellRange != null)
            {
                PerformSpell(defendingHeroOutsideOfBase, $"SPELL CONTROL {monsterWithinSpellRange.Id} {_enemyBaseLocation.X} {_enemyBaseLocation.Y}");
            }
        }
    }

    internal void AssignAttackSpells(IEnumerable<Hero> playerHeroes, IEnumerable<Hero> enemyHeroes, IEnumerable<Monster> monsters)
    {
        foreach (var attackingHero in playerHeroes.Where(h => h.Strategy == Strategy.Attack))
        {
            Console.Error.WriteLine($"_estimatedManaLeft: {_estimatedManaLeft}");

            if (_estimatedManaLeft < 10)
            {
                return;
            }

            if (CalculateDistance(attackingHero.Position, _enemyBaseLocation) > _valuesProvider.OutskirtsMaxDist)
            {
                continue;
            }

            var closeEnoughForWindMonster = monsters.FirstOrDefault(m => CalculateDistance(m.Position, attackingHero.Position) <= ValuesProvider.WindSpellRange
                                                                                 && m.ShieldLife == 0);

            if (closeEnoughForWindMonster != null)
            {
                Console.Error.WriteLine($"Atacking hero {attackingHero.Id} to cast WIND on monster {closeEnoughForWindMonster.Id}");

                PerformSpell(attackingHero, $"SPELL WIND {_enemyBaseLocation.X} {_enemyBaseLocation.Y}");
            }
            else // If we're not close enough for a wind spell try a shield or control
            {
                var closeEnoughForControlEnemy =
                        enemyHeroes.Where(e => e.ShieldLife == 0
                                                 && CalculateDistance(e.Position, attackingHero.Position) <= _valuesProvider.ControlSpellange
                                                 && CalculateDistance(e.Position, _enemyBaseLocation) <= _valuesProvider.BaseRadius)
                                   .OrderBy(e => CalculateDistance(e.Position, _enemyBaseLocation))
                                   .FirstOrDefault();

                var closeEnoughForSpellMonster =
                        monsters.FirstOrDefault(m => m.ShieldLife == 0
                                                         && m.ThreatFor == ThreatFor.Enemy
                                                         && CalculateDistance(m.Position, attackingHero.Position) <= _valuesProvider.ShieldSpellRange
                                                         && CalculateDistance(m.Position, _enemyBaseLocation) <= _valuesProvider.OutskirtsMinDist);

                if (closeEnoughForControlEnemy != null && closeEnoughForSpellMonster != null)
                {
                    if (new Random().Next(1) == 0)
                    {
                        PerformSpell(attackingHero, $"SPELL SHIELD {closeEnoughForSpellMonster.Id}");
                    }
                    else
                    {
                        PerformSpell(attackingHero, $"SPELL CONTROL {closeEnoughForControlEnemy.Id} {_playerBaseLocation.X} {_playerBaseLocation.Y}");
                    }
                }
                else if (closeEnoughForSpellMonster != null)
                {
                    PerformSpell(attackingHero, $"SPELL SHIELD {closeEnoughForSpellMonster.Id}");
                }
                else if (closeEnoughForControlEnemy != null)
                {
                    PerformSpell(attackingHero, $"SPELL CONTROL {closeEnoughForControlEnemy.Id} {_playerBaseLocation.X} {_playerBaseLocation.Y}");
                }
            }
        }
    }

    private void PerformSpell(Hero hero, string action)
    {
        hero.CurrentAction = action;
        hero.CurrentMonster = -1;

        if (hero.UsingSpell == false)
        {
            _estimatedManaLeft -= 10;
            hero.UsingSpell = true;
        }
    }
    
    private static double CalculateDistance(Point position, Point position2)
    {
        return Math.Sqrt(Math.Pow(position.X - position2.X, 2)
                         + Math.Pow(position.Y - position2.Y, 2));
    }

    internal void SetEstimatedMana(int estimate)
    {
        _estimatedManaLeft = estimate;
    }
}
