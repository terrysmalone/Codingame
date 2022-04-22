﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpringChallenge2022;

internal class Game
{
    private readonly Point _playerBaseLocation;
    private readonly Point _enemyBaseLocation;
    private readonly int _heroesPerPlayer;

    private int _playerBaseHealth;
    private int _enemyBaseHealth;

    private int _mana;

    private List<Monster> _monsters = new List<Monster>();
    private List<Hero> _playerHeroes = new List<Hero>();
    private List<Hero> _enemyHeroes = new List<Hero>();

    private const int _xMax = 17630;
    private const int _yMax = 9000;

    private const int _windSpellDistance = 1280;

    private List<Strategy> _defaultStrategies = new List<Strategy>(0);

    internal Game(Point playerBaseLocation, int heroesPerPlayer)
    {
        _playerBaseLocation = playerBaseLocation;
        _heroesPerPlayer = heroesPerPlayer;

        _enemyBaseLocation = new Point(playerBaseLocation.X == 0 ? _xMax : 0, playerBaseLocation.Y == 0 ? _yMax : 0);

        _defaultStrategies.Add(Strategy.Defend);
        _defaultStrategies.Add(Strategy.Defend);
        _defaultStrategies.Add(Strategy.Attack);
    }

    internal string[] GetMoves()
    {
        var moves = new string[_heroesPerPlayer];

        //Debugger.DisplayPlayerHeroes(_playerHeroes);
        //Debugger.DisplayEnemyHeroes(_enemyHeroes);
        //Debugger.DisplayMonsters(_monsters);

        // Check if we want to change any strategy
            // When we change strategy reset the guard points

        SetGuardPoints();

        // If a monster has died clear it from the current monser ID
        ClearDeadMonsters();

        // At a basic level we want all heros to move towards someone to attack
        AssignMonstersToAttack();

        AssignAttackSpells();

        // Defending the base is priority one. See if we need to fire a defensive wind spell
        AssignDefensiveWindSpell();

        Debugger.DisplayPlayerHeroes(_playerHeroes);

        for (var i = 0; i < moves.Length; i++)
        {
            moves[i] = _playerHeroes[i].CurrentAction;
        }

        return moves;
    }

    private static double CalculateDistance(Point position, Point position2)
    {
        return Math.Sqrt(Math.Pow(position.X - position2.X, 2)
                         + Math.Pow(position.Y - position2.Y, 2));
    }

    private void SetGuardPoints()
    {
        if (_playerHeroes[0].GuardPoint.X == 0 && _playerHeroes[0].GuardPoint.Y == 0)   // or we've changed a Strategy
        {
            var guardPoints = new List<Point>();

            // Assign defenders
            guardPoints.AddRange(GetDefenders());

            Console.Error.WriteLine($"guardPoints.Count: {guardPoints.Count}");

            // Assign others
            guardPoints.AddRange(GetAttackers());

            Console.Error.WriteLine($"guardPoints.Count: {guardPoints.Count}");

            // Set guard points
            if (_playerHeroes.Count != guardPoints.Count)
            {
                Console.Error.WriteLine("ERROR: Player heroes count doesn't match guard point count");
            }

            // At some point we need to make sure we move heroes around to minimise travel to new spots

            Console.Error.WriteLine($"_playerHeroes.Count: {_playerHeroes.Count}");

            for (var i = 0; i < _playerHeroes.Count; i++)
            {
                Console.Error.WriteLine($"i: {i}");
                var hero = _playerHeroes[i];
                hero.GuardPoint = guardPoints[i];
            }
        }
    }

    private IEnumerable<Point> GetDefenders()
    {
        var defendPoints = new List<Point>();

        var numberOfDefenders = _playerHeroes.Count(h => h.Strategy == Strategy.Defend);

        if (numberOfDefenders == 1)
        {
            if (_playerBaseLocation.X == 0)
            {
                defendPoints.Add(new Point(3000, 3000));
            }
            else
            {
                defendPoints.Add(new Point(_xMax - 3000, _yMax - 3000));
            }
        }
        else if (numberOfDefenders == 2)
        {
            if (_playerBaseLocation.X == 0)
            {
                defendPoints.Add(new Point(3500, 2000));
                defendPoints.Add(new Point(2000, 3500));
            }
            else
            {
                defendPoints.Add(new Point(_xMax - 3500, _yMax - 2000));
                defendPoints.Add(new Point(_xMax - 2000, _yMax - 3500));
            }
        }
        else if (numberOfDefenders == 3)
        {
            if (_playerBaseLocation.X == 0)
            {
                defendPoints.Add(new Point(4000, 1000));
                defendPoints.Add(new Point(3000, 3000));
                defendPoints.Add(new Point(1000, 4000));
            }
            else
            {
                defendPoints.Add(new Point(_xMax - 4000, _yMax - 1000));
                defendPoints.Add(new Point(_xMax - 3000, _yMax - 3000));
                defendPoints.Add(new Point(_xMax - 1000, _yMax - 4000));
            }
        }

        return defendPoints;
    }

    private IEnumerable<Point> GetAttackers()
    {
        var numberOfAttackers = _playerHeroes.Count(h => h.Strategy == Strategy.Attack);

        var attackPoints = new List<Point>();

        if (numberOfAttackers == 1)
        {
            if (_playerBaseLocation.X == 0)
            {
                attackPoints.Add(new Point(_xMax - 3750, _yMax - 3750));
            }
            else
            {
                attackPoints.Add(new Point(3750, 3750));
            }
        }

        return attackPoints;
    }

    private void ClearDeadMonsters()
    {
        foreach (var hero in _playerHeroes)
        {
            if (hero.CurrentMonster >= 0)
            {
                if (!_monsters.Any(m => m.Id == hero.CurrentMonster))
                {
                    hero.CurrentMonster = -1;
                }
            }
        }
    }

    private void AssignMonstersToAttack()
    {
        List<Monster> monstersThreateningBase;

        // Define defenders attacks
        var freeDefendingHeroes = _playerHeroes.Where(h => h.Strategy == Strategy.Defend && h.CurrentMonster == -1).ToList();

        if (freeDefendingHeroes.Count > 0)
        {
            monstersThreateningBase = _monsters.Where(m => m.NearBase && m.ThreatFor == ThreatFor.Player)
                                               .OrderBy(m => CalculateDistance(m.Position, _playerBaseLocation))
                                               .ToList();

            var monsterIndex = 0;

            if (monstersThreateningBase.Count > 0)
            {
                var closestMonster = monstersThreateningBase.First();

                foreach (var hero in freeDefendingHeroes)
                {
                    hero.CurrentMonster = closestMonster.Id;
                }
            }
        }

        // Define Attacker moves
        var freeAttackingHeroes = _playerHeroes.Where(h => h.Strategy == Strategy.Attack && h.CurrentMonster == -1).ToList();

        if (freeAttackingHeroes.Count > 0)
        {
            // Get any monsters on the edge of the enemies base
            var monstersOnOutskirts = _monsters.Where(m => CalculateDistance(m.Position, _enemyBaseLocation) > 5000
                                                           && CalculateDistance(m.Position, _enemyBaseLocation) < 7000).ToList();

            // Go to them
            if (monstersOnOutskirts.Count > 1)
            {
                foreach (var freeAttackingHero in freeAttackingHeroes)
                {
                    var closestMonster = monstersOnOutskirts.OrderBy(m => CalculateDistance(freeAttackingHero.Position, m.Position))
                                                            .First();

                    Console.Error.WriteLine($"Moving atacking hero {freeAttackingHero.Id} to monster {closestMonster.Id}");

                    freeAttackingHero.CurrentMonster = closestMonster.Id;
                }
            }
        }

        // Assign actions
        foreach (var hero in _playerHeroes)
        {
            if (hero.CurrentMonster != -1)
            {
                var monsterToAttack = _monsters.Single(m => m.Id == hero.CurrentMonster);

                hero.CurrentAction = $"MOVE {monsterToAttack.Position.X} {monsterToAttack.Position.Y}";
            }
            else
            {
                hero.CurrentAction = $"MOVE {hero.GuardPoint.X} {hero.GuardPoint.Y}";
            }
        }
    }

    private void AssignAttackSpells()
    {
        foreach (var attackingHero in _playerHeroes.Where(h => h.Strategy == Strategy.Attack))
        {
            var closeMonster = _monsters.FirstOrDefault(m => CalculateDistance(m.Position, attackingHero.Position) <= _windSpellDistance);

            if (closeMonster != null)
            {
                Console.Error.WriteLine($"Atacking hero {attackingHero.Id} to cast WIND on monster {closeMonster.Id}");

                attackingHero.CurrentAction = $"SPELL WIND {_enemyBaseLocation.X} {_enemyBaseLocation.Y}";
                attackingHero.CurrentMonster = -1;
            }
        }
    }

    private void AssignDefensiveWindSpell()
    {
        if (_mana < 10)
        {
            return;
        }

        var closeDistance = 2000;

        var closestMonster = _monsters.FirstOrDefault(m => CalculateDistance(m.Position, _playerBaseLocation) <= closeDistance);

        if (closestMonster != null)
        {
            var closeHeroes = _playerHeroes.Where(h => h.Strategy == Strategy.Defend)
                                                               .OrderBy(h => CalculateDistance(h.Position, closestMonster.Position));

            // All defenders to use wind
            // foreach (var closeHero in closeHeroes)
            // {
            //     if (CalculateDistance(closeHero.Position, closestMonster.Position) <= windSpellDistance)
            //     {
            //         int xNew, yNew;
            //
            //         // very crude vector calc
            //         if (_playerBaseLocation.X == 0)
            //         {
            //             xNew = closeHero.Position.X + 1;
            //             yNew = closeHero.Position.Y + 1;
            //         }
            //         else
            //         {
            //             xNew = closeHero.Position.X - 1;
            //             yNew = closeHero.Position.Y - 1;
            //         }
            //
            //         closeHero.CurrentAction = $"SPELL WIND {xNew} {yNew}";
            //     }
            // }

            var closestHero = _playerHeroes.Where(h => h.Strategy == Strategy.Defend)
                                           .OrderBy(h => CalculateDistance(h.Position, closestMonster.Position))
                                           .First();

            if (CalculateDistance(closestHero.Position, closestMonster.Position) <= _windSpellDistance)
            {
                int xNew, yNew;

                Console.Error.WriteLine($"_playerBaseLocation.X:{_playerBaseLocation.X}");
                // very crude vector calc
                if (_playerBaseLocation.X == 0)
                {
                    xNew = closestHero.Position.X + 1;
                    yNew = closestHero.Position.Y + 1;
                }
                else
                {
                    xNew = closestHero.Position.X - 1;
                    yNew = closestHero.Position.Y - 1;
                }

                closestHero.CurrentAction = $"SPELL WIND {xNew} {yNew}";
            }
        }
    }

    internal void SetMana(int mana)
    {
        _mana = mana;
    }

    internal void SetPlayerBaseHealth(int playerBaseHealth)
    {
        _playerBaseHealth = playerBaseHealth;
    }

    internal void SetEnemyBaseHealth(int playerBaseHealth)
    {
        _enemyBaseHealth = playerBaseHealth;
    }

    public void UpdatePlayerHero(Hero hero)
    {
        var playerHero = _playerHeroes.SingleOrDefault(h => h.Id == hero.Id);

        if (playerHero == null)
        {
            // Assign a strategy
            Console.Error.WriteLine($"Assigning strategy {_defaultStrategies[_playerHeroes.Count]}");
            hero.Strategy = _defaultStrategies[_playerHeroes.Count];

            _playerHeroes.Add(hero);
        }
        else
        {
            playerHero.Position = hero.Position;
        }
    }
    public void UpdateEnemyHero(Hero hero)
    {
        var enemyHero = _enemyHeroes.SingleOrDefault(h => h.Id == hero.Id);

        if (enemyHero == null)
        {
            _enemyHeroes.Add(hero);
        }
        else
        {
            enemyHero.Position = hero.Position;
        }
    }

    internal void AddMonster(Monster monster)
    {
        _monsters.Add(monster);
    }

    internal void ClearMonsters()
    {
        _monsters.Clear();
    }
}
