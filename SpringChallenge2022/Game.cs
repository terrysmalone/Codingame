using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpringChallenge2022;

internal class Game
{
    private readonly Point _playerBaseLocation;
    private readonly int _heroesPerPlayer;

    private int _playerBaseHealth;
    private int _enemyBaseHealth;

    private int _mana;

    private List<Monster> _monsters = new List<Monster>();
    private List<Hero> _playerHeroes = new List<Hero>();
    private List<Hero> _enemyHeroes = new List<Hero>();

    private const int _xMax = 17630;
    private const int _yMax = 9000;

    internal Game(Point playerBaseLocation, int heroesPerPlayer)
    {
        _playerBaseLocation = playerBaseLocation;
        _heroesPerPlayer = heroesPerPlayer;
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

        ClearDeadMonsters();

        AssignMonstersToAttack();

        AsignWindSpell();

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

            // Assign others

            // Set guard points
            if (_playerHeroes.Count != guardPoints.Count)
            {
                Console.Error.WriteLine("ERROR: Player heroes count doesn't match guard point count");
            }

            // At some point we need to make sure we move heroes around to minimise travel to new spots

            for (var i = 0; i < _playerHeroes.Count; i++)
            {
                var hero = _playerHeroes[i];
                hero.GuardPoint = guardPoints[i];
            }
        }
    }

    private IEnumerable<Point> GetDefenders()
    {
        var defendPoints = new List<Point>();

        var numberOfDefenders = _playerHeroes.Count(h => h.Strategy == Strategy.Defend);
        if (numberOfDefenders == 3)
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
        List<Monster> viableMonsters;

        viableMonsters = _monsters.Where(m => m.NearBase && m.ThreatFor == ThreatFor.Player)
                                  .OrderBy(m => CalculateDistance(m.Position, _playerBaseLocation))
                                  .ToList();

        var monsterIndex = 0;
        var freeHeroes = _playerHeroes.Where(h => h.CurrentMonster == -1).ToList();

        if (viableMonsters.Count > 0)
        {
            var closestMonster = viableMonsters.First();

            foreach (var hero in freeHeroes)
            {
                hero.CurrentMonster = closestMonster.Id;
            }
        }

        // This only assigns one one hero per monster per turn
        //
        // while (freeHeroes.Count > 0 && monsterIndex < viableMonsters.Count)
        // {
        //     Console.Error.WriteLine("Assigning hero to monster");
        //
        //     var closestMonster = viableMonsters[monsterIndex];
        //
        //     var closestHero = freeHeroes.OrderBy(h => CalculateDistance(h.Position, closestMonster.Position)).First();
        //
        //     closestHero.CurrentMonster = closestMonster.Id;
        //
        //     Console.Error.WriteLine($"Assigning hero {closestHero.Id} to monster {closestMonster.Id}");
        //
        //     //viableMonsters.Remove(closestMonster);
        //     freeHeroes.Remove(closestHero);
        //     monsterIndex++;
        // }

        // Assign actions
        foreach (var hero in _playerHeroes)
        {
            if (hero.CurrentMonster != -1)
            {
                var hero1 = hero;
                var monsterToAttack = _monsters.Single(m => m.Id == hero1.CurrentMonster);

                hero.CurrentAction = $"MOVE {monsterToAttack.Position.X} {monsterToAttack.Position.Y}";
            }
            else
            {
                hero.CurrentAction = $"MOVE {hero.GuardPoint.X} {hero.GuardPoint.Y}";
            }
        }
    }

    private void AsignWindSpell()
    {
        if (_mana < 10)
        {
            return;
        }

        var closeDistance = 2000;
        var windDistance = 1280;

        var closestMonster = _monsters.FirstOrDefault(m => CalculateDistance(m.Position, _playerBaseLocation) <= closeDistance);

        if (closestMonster != null)
        {
            var closestHero = _playerHeroes.OrderBy(h => CalculateDistance(h.Position, closestMonster.Position)).First();

            if (CalculateDistance(closestHero.Position, closestMonster.Position) <= windDistance)
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
