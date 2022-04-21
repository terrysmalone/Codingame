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

    private List<Monster> _monsters = new List<Monster>();
    private List<Hero> _playerHeroes = new List<Hero>();
    private List<Hero> _enemyHeroes = new List<Hero>();

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

        SetGuardPoints();

        ClearDeadMonsters();

        // get all viable targets
        var viableMonsters = _monsters.Where(m => m.NearBase && m.ThreatFor == 1)
                                                 .OrderBy(m => CalculateDistance(m.Position, _playerBaseLocation))
                                                 .ToList();

        var monsterIndex = 0;
        var freeHeroes = _playerHeroes.Where(h => h.CurrentMonster == -1).ToList();

        while (freeHeroes.Count > 0 && monsterIndex < viableMonsters.Count)
        {
            Console.Error.WriteLine("Assigning hero to monster");

            var closestMonster = viableMonsters[monsterIndex];

            var closestHero = freeHeroes.OrderBy(h => CalculateDistance(h.Position, closestMonster.Position)).First();

            closestHero.CurrentMonster = closestMonster.Id;

            viableMonsters.Remove(closestMonster);
            freeHeroes.Remove(closestHero);
            monsterIndex++;
        }

        Debugger.DisplayPlayerHeroes(_playerHeroes);

        for (var i = 0; i < moves.Length; i++)
        {
            var hero = _playerHeroes[i];

            if (hero.CurrentMonster != -1)
            {
                var monsterToAttack = _monsters.Single(m => m.Id == hero.CurrentMonster);

                moves[i] = $"MOVE {monsterToAttack.Position.X} {monsterToAttack.Position.Y}";
            }
            else
            {
                moves[i] = $"MOVE {hero.GuardPoint.X} {hero.GuardPoint.Y}";
            }
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
        if (_playerHeroes[0].GuardPoint.X == 0 && _playerHeroes[0].GuardPoint.Y == 0)
        {
            _playerHeroes[0].GuardPoint = new Point(4000, 1000);
            _playerHeroes[1].GuardPoint = new Point(3000, 3000);
            _playerHeroes[2].GuardPoint = new Point(1000, 4000);
        }
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
