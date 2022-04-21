using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpringChallenge2022;

internal class Game
{
    private readonly Point _playerBaseLocation;

    private int _playerBaseHealth;
    private int _enemyBaseHealth;

    private List<Monster> _monsters = new List<Monster>();
    private List<Hero> _playerHeroes = new List<Hero>();
    private List<Hero> _enemyHeroes = new List<Hero>();

    internal Game(Point playerBaseLocation)
    {
        _playerBaseLocation = playerBaseLocation;
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
            playerHero.Postion = hero.Postion;
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
            enemyHero.Postion = hero.Postion;
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
