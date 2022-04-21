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

        Debugger.DisplayPlayerHeroes(_playerHeroes);
        Debugger.DisplayEnemyHeroes(_enemyHeroes);
        Debugger.DisplayMonsters(_monsters);

        for (var i = 0; i < moves.Length; i++)
        {
            moves[i] = "WAIT";
        }

        return moves;
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
