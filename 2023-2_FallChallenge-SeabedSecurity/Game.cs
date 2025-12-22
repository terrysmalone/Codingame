namespace _2023_2_FallChallenge_SeabedSecurity;

using System;
using System.Collections.Generic;
using System.Drawing;

internal class Game
{
    internal int MyScore { get; set; }
    internal int EnemyScore { get; set; }

    private List<Creature> creatures = [];
    private List<Drone> myDrones = [];
    private List<Drone> enemyDrones = [];

    private HashSet<int> myScannedCreatureIds = [];
    private HashSet<int> enemyScannedCreatureIds = [];

    internal void InitialiseCreatures(List<Creature> creatures)
    {
        this.creatures = creatures;
    }

    internal void AddScannedCreature(int creatureId, bool isMyDrone)
    {
        if (isMyDrone)
        {
            myScannedCreatureIds.Add(creatureId);
        }
        else
        {
            enemyScannedCreatureIds.Add(creatureId);
        }
    }

    internal void SetMyDrones(List<Drone> drones)
    {
        myDrones = drones;
    }

    internal void SetEnemyDrones(List<Drone> drones)
    {
        enemyDrones = drones;
    }

    internal void UpdateCreaturePosition(int creatureId, int creatureX, int creatureY, int creatureVx, int creatureVy)
    {
        Creature creature = creatures.Find(c => c.Id == creatureId);
        if (creature != null)
        {
            creature.Position = new Point(creatureX, creatureY);
            creature.Velocity = new Point(creatureVx, creatureVy);
        }
    }

    internal List<string> CalculateActions()
    {
        var actions = new List<string>();

        for (var i=0; i<myDrones.Count; i++)
        {
            actions.Add("WAIT 1"); // MOVE <x> <y> <light (1|0)> | WAIT <light (1|0)>
        }

        return actions;
    }
}
