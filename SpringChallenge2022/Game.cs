using System;
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

    private bool _inCollectionPhase = true;

    private int _mana;
    private int _estimatedManaLeft;

    private List<Monster> _monsters = new List<Monster>();
    private List<Hero> _playerHeroes = new List<Hero>();
    private List<Hero> _enemyHeroes = new List<Hero>();

    private const int _xMax = 17630;
    private const int _yMax = 9000;

    private const int _windSpellRange = 1280;
    private const int _controlSpellange = 2200;
    private const int _shieldSpellRange = 2200;

    private const int _outskirtsMinDist = 5000;
    private const int _outskirtsMaxDist = 7000;
    private const int _heroRange = 2200;
    private const int _maxDefenderDistanceFromBase = 7500;
    private const int _baseRadius = 5000;
    private const int _closeToBaseRange = 1000;

    private bool _weGotAController = false; // If our opponent likes to control our defenders make sure they're always shielded


    private List<Strategy> _defaultStrategies = new List<Strategy>(0);

    internal Game(Point playerBaseLocation, int heroesPerPlayer)
    {
        _playerBaseLocation = playerBaseLocation;
        _heroesPerPlayer = heroesPerPlayer;

        _enemyBaseLocation = new Point(playerBaseLocation.X == 0 ? _xMax : 0, playerBaseLocation.Y == 0 ? _yMax : 0);

        _defaultStrategies.Add(Strategy.Defend);
        _defaultStrategies.Add(Strategy.Defend);
        _defaultStrategies.Add(Strategy.Collect);
    }

    internal string[] GetMoves()
    {
        _estimatedManaLeft = _mana;

        var moves = new string[_heroesPerPlayer];

        foreach (var hero in _playerHeroes)
        {
            hero.CurrentAction = string.Empty;
            hero.UsingSpell = false;
            hero.IsShielding = false;
        }

        if (_inCollectionPhase)
        {
            if(_mana > 300)
            {
                _inCollectionPhase = false;

                ClearGuardPoints();
                ChangeCollectorToAttacker();

            }
        }

        SetGuardPoints();

        CheckForController();
        ClearStaleAttacks();

        // At a basic level we want all heros to move towards someone to attack
        AssignMonstersToAttack();

        // Defending the base is priority one. See if we need to fire a defensive wind spell

        if (_weGotAController)
        {
            foreach (var defendingHero in _playerHeroes.Where(h => h.Strategy == Strategy.Defend))
            {
                if (_estimatedManaLeft < 10)
                {
                    break;
                }

                if (defendingHero.ShieldLife == 0)
                {
                    PerformSpell(defendingHero, $"SPELL SHIELD {defendingHero.Id}");

                    defendingHero.IsShielding = true;
                }
            }
        }

        AssignDefensiveWindSpell();

        if (!_inCollectionPhase)
        {
            AssignDefenderControlSpells();

            AssignAttackSpells();
        }

        for (var i = 0; i < moves.Length; i++)
        {
            moves[i] = _playerHeroes[i].CurrentAction;
        }

        return moves;
    }

    private void ClearGuardPoints()
    {
        foreach (var hero in _playerHeroes)
        {
            hero.ClearGuardPoints();
        }
    }

    private void ChangeCollectorToAttacker()
    {
        var heroes = _playerHeroes.Where(h => h.Strategy == Strategy.Collect);

        foreach (var hero in heroes)
        {
            hero.Strategy = Strategy.Attack;
        }
    }

    private static double CalculateDistance(Point position, Point position2)
    {
        return Math.Sqrt(Math.Pow(position.X - position2.X, 2)
                         + Math.Pow(position.Y - position2.Y, 2));
    }

    private void SetGuardPoints()
    {
        if (_playerHeroes[0].GetNumberOfGuardPoints() == 0)   // or we've changed a Strategy
        {
            var guardPoints = new List<List<Point>>();


            guardPoints.AddRange(GetDefenders());

            guardPoints.AddRange(GetCollectors());

            guardPoints.AddRange(GetAttackers());

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
                hero.SetGuardPoints(guardPoints[i]);
            }
        }
    }

    private List<List<Point>> GetDefenders()
    {
        var numberOfDefenders = _playerHeroes.Count(h => h.Strategy == Strategy.Defend);

        var defendPoints = new List<List<Point>>();

        if (numberOfDefenders == 1)
        {
            if (_playerBaseLocation.X == 0)
            {
                defendPoints.Add(new List<Point> { new Point(4000, 4000) });
            }
            else
            {
                defendPoints.Add(new List<Point> { new Point(_xMax - 4000, _yMax - 4000) });
            }
        }
        else if (numberOfDefenders == 2)
        {
            if (_playerBaseLocation.X == 0)
            {
                defendPoints.Add(new List<Point> { new Point(5700, 2500) });
                defendPoints.Add(new List<Point> { new Point(2500, 5700) });
            }
            else
            {
                defendPoints.Add(new List<Point> { new Point(_xMax - 5700, _yMax - 2500) });
                defendPoints.Add(new List<Point> { new Point(_xMax - 2500, _yMax - 5700) });
            }
        }
        else if (numberOfDefenders == 3)
        {
            if (_playerBaseLocation.X == 0)
            {
                defendPoints.Add(new List<Point> { new Point(5000, 2000) });
                defendPoints.Add(new List<Point> { new Point(4000, 4000) });
                defendPoints.Add(new List<Point> { new Point(2000, 5000) });
            }
            else
            {
                defendPoints.Add(new List<Point> { new Point(_xMax - 5000, _yMax - 2000) });
                defendPoints.Add(new List<Point> { new Point(_xMax - 4000, _yMax - 4000) });
                defendPoints.Add(new List<Point> { new Point(_xMax - 2000, _yMax - 5000) });
            }
        }

        return defendPoints;
    }

    private IEnumerable<List<Point>> GetCollectors()
    {
        var numberOfCollectors = _playerHeroes.Count(h => h.Strategy == Strategy.Collect);

        var collectPoints = new List<List<Point>>();

        if (numberOfCollectors == 1)
        {
            collectPoints.Add(new List<Point>
            {
                new Point(_xMax / 2, _yMax / 2)
            });
        }

        return collectPoints;
    }

    private List<List<Point>> GetAttackers()
    {
        var numberOfAttackers = _playerHeroes.Count(h => h.Strategy == Strategy.Attack);

        var attackPoints = new List<List<Point>>();

        if (numberOfAttackers == 1)
        {
            if (_playerBaseLocation.X == 0)
            {
                attackPoints.Add(new List<Point>
                {
                    new Point(_xMax - 3000, _yMax - 2500)
                });
            }
            else
            {
                attackPoints.Add(new List<Point>
                {
                    new Point(3000, 2500)
                });
            }
        }

        return attackPoints;
    }

    private void CheckForController()
    {
        if (!_weGotAController && _playerHeroes.Any(h => h.IsControlled && h.Strategy == Strategy.Defend))
        {
            _weGotAController = true;
        }
    }

    private void ClearStaleAttacks()
    {
        ClearDeadMonsters();

        ClearMonstersIfDefenderIsTooFarAway();
        ClearMonstersIfTheyreAThreatForTheEnemy();

        ClearMonstersFromEnemyOutskirts();
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

    private void ClearMonstersIfDefenderIsTooFarAway()
    {
        foreach (var hero in _playerHeroes.Where(h => h.Strategy == Strategy.Defend))
        {
            if (hero.CurrentMonster >= 0)
            {
                if (CalculateDistance(hero.Position, _playerBaseLocation) > _maxDefenderDistanceFromBase)
                {
                    hero.CurrentMonster = -1;
                }
            }
        }
    }

    private void ClearMonstersIfTheyreAThreatForTheEnemy()
    {
        foreach (var hero in _playerHeroes)
        {
            if (hero.CurrentMonster >= 0)
            {
                if (_monsters.Any(m => m.Id == hero.CurrentMonster && m.ThreatFor == ThreatFor.Enemy))
                {
                    hero.CurrentMonster = -1;
                }
            }
        }
    }

    private void ClearMonstersFromEnemyOutskirts()
    {
        foreach (var hero in _playerHeroes.Where(h => h.Strategy == Strategy.Attack))
        {
            if (hero.CurrentMonster >= 0)
            {
                var currentMonster = _monsters.First(m => m.Id == hero.CurrentMonster);

                if (CalculateDistance(currentMonster.Position, _enemyBaseLocation) < _outskirtsMinDist
                    || CalculateDistance(currentMonster.Position, _enemyBaseLocation) > _outskirtsMaxDist)
                {
                    hero.CurrentMonster = -1;
                }
            }
        }
    }

    private void AssignMonstersToAttack()
    {
        // if a hero is not in the base, and a spider is, drop everything and defend
        var monstersThreateningBase = _monsters.Where(m => m.NearBase && m.ThreatFor == ThreatFor.Player)
                                                          .OrderBy(m => CalculateDistance(m.Position, _playerBaseLocation))
                                                          .ToList();

        var defendingHeroesOutsideOfBase = _playerHeroes.Where(h => h.Strategy == Strategy.Defend
                                                                                    && CalculateDistance(h.Position, _playerBaseLocation) > _baseRadius);

        foreach (var defendingHeroOutsideOfBase in defendingHeroesOutsideOfBase)
        {
            defendingHeroOutsideOfBase.CurrentMonster = -1;
        }

        // Define defenders attacks
        var freeDefendingHeroes = _playerHeroes.Where(h => h.Strategy == Strategy.Defend && h.CurrentMonster == -1).ToList();

        if (freeDefendingHeroes.Count > 0)
        {
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

                foreach (var freeDefendingHero in freeDefendingHeroes)
                {
                    var monsterWithinRange = _monsters.Where(m => CalculateDistance(m.Position, _playerBaseLocation) <= _maxDefenderDistanceFromBase
                                                                      && m.ThreatFor != ThreatFor.Enemy)
                                                      .Select(m => new { m, distance = CalculateDistance(m.Position, freeDefendingHero.Position)})
                                                      .Where(m => m.distance <= _heroRange)
                                                      .OrderBy(m => m.distance)
                                                      .Select(m => m.m)
                                                      .FirstOrDefault();

                    if (monsterWithinRange != null)
                    {
                        freeDefendingHero.CurrentMonster = monsterWithinRange.Id;
                    }
                }

            }
        }

        // Define collector moves
        var collectingHeroes = _playerHeroes.Where(h => h.Strategy == Strategy.Collect && h.CurrentMonster == -1).ToList();

        if (collectingHeroes.Count > 0)
        {
            foreach (var collectingHero in collectingHeroes)
            {
                var closestMonster = _monsters.Where(m => CalculateDistance(m.Position, _playerBaseLocation) > _outskirtsMaxDist)
                                              .Select(m => new { m, distance = CalculateDistance(m.Position, collectingHero.Position)})
                                              .Where(m => m.distance <= _heroRange)
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

        // Define Attacker moves
        var freeAttackingHeroes = _playerHeroes.Where(h => h.Strategy == Strategy.Attack && h.CurrentMonster == -1).ToList();

        if (freeAttackingHeroes.Count > 0)
        {
            // Get any monsters on the edge of the enemies base
            var monstersOnOutskirts = _monsters.Where(m => CalculateDistance(m.Position, _enemyBaseLocation) > _outskirtsMinDist
                                                           && CalculateDistance(m.Position, _enemyBaseLocation) < _outskirtsMaxDist).ToList();

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

    private void AssignDefenderControlSpells()
    {
        var healthCutOff = 10;

        if (_estimatedManaLeft < 10)
        {
            return;
        }

        var defendingHeroesOutsideOfBase = _playerHeroes.Where(h => h.Strategy == Strategy.Defend
                                                                                    && h.IsShielding == false
                                                                                    && CalculateDistance(h.Position, _playerBaseLocation) > _baseRadius);


        foreach (var defendingHeroOutsideOfBase in defendingHeroesOutsideOfBase)
        {
            if (_estimatedManaLeft < 10)
            {
                return;
            }

            var monsterWithinSpellRange = _monsters.Where(m => m.Health > healthCutOff
                                                                      && m.IsControlled == false
                                                                      && m.ThreatFor != ThreatFor.Enemy
                                                                      && m.ShieldLife == 0
                                                                      && CalculateDistance(m.Position, _playerBaseLocation) > _baseRadius)
                                                   .Select(m => new { m, distance = CalculateDistance(m.Position, defendingHeroOutsideOfBase.Position)})
                                                   .Where(m => m.distance <= _controlSpellange)
                                                   .OrderBy(m => m.distance)
                                                   .Select(m => m.m)
                                                   .FirstOrDefault();

            if (monsterWithinSpellRange != null)
            {
                PerformSpell(defendingHeroOutsideOfBase, $"SPELL CONTROL {monsterWithinSpellRange.Id} {_enemyBaseLocation.X} {_enemyBaseLocation.Y}");
            }
        }
    }

    private void AssignAttackSpells()
    {
        foreach (var attackingHero in _playerHeroes.Where(h => h.Strategy == Strategy.Attack))
        {
            if (_estimatedManaLeft < 10)
            {
                return;
            }

            if (CalculateDistance(attackingHero.Position, _enemyBaseLocation) > _outskirtsMaxDist)
            {
                continue;
            }

            var closeEnoughForWindMonster = _monsters.FirstOrDefault(m => CalculateDistance(m.Position, attackingHero.Position) <= _windSpellRange
                                                                                 && m.ShieldLife == 0);

            if (closeEnoughForWindMonster != null)
            {
                Console.Error.WriteLine($"Atacking hero {attackingHero.Id} to cast WIND on monster {closeEnoughForWindMonster.Id}");

                PerformSpell(attackingHero, $"SPELL WIND {_enemyBaseLocation.X} {_enemyBaseLocation.Y}");
            }
            else // If we're not close enough for a wind spell try a shield or control
            {
                var closeEnoughForControlEnemy = _enemyHeroes.Where(e => e.ShieldLife == 0
                                                                                && CalculateDistance(e.Position, attackingHero.Position) <= _controlSpellange)
                                                                  .OrderBy(e => CalculateDistance(e.Position, _enemyBaseLocation))
                                                                  .FirstOrDefault();

                var closeEnoughForSpellMonster = _monsters.FirstOrDefault(m => m.ShieldLife == 0
                                                                                    && m.ThreatFor == ThreatFor.Enemy
                                                                                    && CalculateDistance(m.Position, attackingHero.Position) <= _shieldSpellRange
                                                                                    && CalculateDistance(m.Position, _enemyBaseLocation) <= _outskirtsMinDist);

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

    private void AssignDefensiveWindSpell()
    {
        if (_estimatedManaLeft < 10)
        {
            return;
        }

        var closeDistance = 3000;

        var closestMonster = _monsters.FirstOrDefault(m => CalculateDistance(m.Position, _playerBaseLocation) <= closeDistance
                                                               && m.ShieldLife == 0);

        if (closestMonster != null)
        {
            Debugger.DisplayPlayerHeroes(_playerHeroes);

            var availableHeroes = _playerHeroes.Where(h => h.Strategy == Strategy.Defend && h.IsShielding == false).ToList();

            if (availableHeroes.Count > 0)
            {
                var closestHero = availableHeroes.OrderBy(h => CalculateDistance(h.Position, closestMonster.Position))
                                                 .First();

                if (CalculateDistance(closestHero.Position, closestMonster.Position) <= _windSpellRange)
                {
                    Console.Error.WriteLine("Hero casting wind");
                    PerformSpell(closestHero, $"SPELL WIND {_enemyBaseLocation.X} {_enemyBaseLocation.Y}");
                }
                else
                {
                    // Too far away for wind to work

                    // If he's close and we can control that little shit away do it
                    if (CalculateDistance(closestMonster.Position, _playerBaseLocation) <= _closeToBaseRange
                        && CalculateDistance(closestHero.Position, closestMonster.Position) <= _controlSpellange)
                    {
                        Console.Error.WriteLine("Hero casting control");
                        PerformSpell(closestHero, $"SPELL CONTROL {closestMonster.Id} {_enemyBaseLocation.X} {_enemyBaseLocation.Y}");

                        // Also get the other defender to do a WIND spell just to be sure. At some point lets check if there are other
                        // monsters close to the base too
                        // var otherDefender = _playerHeroes.SingleOrDefault(h => h.Strategy == Strategy.Defend
                        //                                                        && h.Id != closestHero.Id);
                        //
                        // if (otherDefender != null)
                        // {
                        //     Console.Error.WriteLine("Hero casting defensive windwind");
                        //     PerformSpell(otherDefender, $"SPELL WIND {_enemyBaseLocation.X} {_enemyBaseLocation.Y}");
                        // }
                    }
                }
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
            playerHero.IsControlled = hero.IsControlled;
            playerHero.ShieldLife = hero.ShieldLife;
        }
    }
    public void AddEnemyHero(Hero hero)
    {
            _enemyHeroes.Add(hero);
    }

    internal void AddMonster(Monster monster)
    {
        _monsters.Add(monster);
    }

    internal void ClearMonsters()
    {
        _monsters.Clear();
    }

    public void ClearEnemyHeroes()
    {
        _enemyHeroes.Clear();
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
}
