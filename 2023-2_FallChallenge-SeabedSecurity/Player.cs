namespace _2023_2_FallChallenge_SeabedSecurity;

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/**
 * Score points by scanning valuable fish faster than your opponent.
 **/
class Player
{
    static void Main(string[] args)
    {
        var game = new Game();
        
        game.InitialiseCreatures(GetCreatures());

        // game loop
        while (true)
        {
            game.VisibleMonsterIds.Clear();

            int myScore = int.Parse(Console.ReadLine());
            game.MyScore = myScore;
            int foeScore = int.Parse(Console.ReadLine());
            game.EnemyScore = foeScore;

            AddSavedScans(game);

            List<Drone> myDrones = GetMyDrones();
            List<Drone> enemyDrones = GetEnemyDrones();

            AddStoredScans(myDrones, enemyDrones);

            UpdateCreatures(game);

            AddRadarBlips(game, myDrones, enemyDrones);

            game.SetMyDrones(myDrones);
            game.SetEnemyDrones(enemyDrones);

            List<string> actions = game.CalculateActions();

            foreach (string action in actions)
            {
                Console.WriteLine(action);
            }

            //Logger.AllDrones("My drones", myDrones);
            //Logger.AllDrones("Enemy drones", enemyDrones);
        }
    }    

    private static List<Creature> GetCreatures()
    {
        int creatureCount = int.Parse(Console.ReadLine());

        List<Creature> creatures = new List<Creature>();

        for (int i = 0; i < creatureCount; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int creatureId = int.Parse(inputs[0]);
            int color = int.Parse(inputs[1]);
            int type = int.Parse(inputs[2]);

            creatures.Add(new Creature(creatureId, color, type));
        }

        return creatures;
    }

    private static void AddSavedScans(Game game)
    {
        int myScanCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < myScanCount; i++)
        {
            int creatureId = int.Parse(Console.ReadLine());
            game.AddScannedCreature(creatureId, true);
        }
        int foeScanCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < foeScanCount; i++)
        {
            int creatureId = int.Parse(Console.ReadLine());
            game.AddScannedCreature(creatureId, false);
        }
    }

    private static List<Drone> GetMyDrones()
    {
        int myDroneCount = int.Parse(Console.ReadLine());
        var myDrones = new List<Drone>();

        for (int i = 0; i < myDroneCount; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int droneId = int.Parse(inputs[0]);
            int droneX = int.Parse(inputs[1]);
            int droneY = int.Parse(inputs[2]);
            int emergency = int.Parse(inputs[3]);
            int battery = int.Parse(inputs[4]);

            myDrones.Add(new Drone(droneId, droneX, droneY, battery));
        }

        return myDrones;
    }

    private static List<Drone> GetEnemyDrones()
    {
        int foeDroneCount = int.Parse(Console.ReadLine());
        var enemyDrones = new List<Drone>();

        for (int i = 0; i < foeDroneCount; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int droneId = int.Parse(inputs[0]);
            int droneX = int.Parse(inputs[1]);
            int droneY = int.Parse(inputs[2]);
            int emergency = int.Parse(inputs[3]);
            int battery = int.Parse(inputs[4]);

            enemyDrones.Add(new Drone(droneId, droneX, droneY, battery));
        }

        return enemyDrones;
    }

    private static void UpdateCreatures(Game game)
    {
        game.SetAllCreaturesAsNotVisible();

        int visibleCreatureCount = int.Parse(Console.ReadLine());

        Console.Error.WriteLine($"Visible creature count: {visibleCreatureCount}");
        for (int i = 0; i < visibleCreatureCount; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int creatureId = int.Parse(inputs[0]);
            int creatureX = int.Parse(inputs[1]);
            int creatureY = int.Parse(inputs[2]);
            int creatureVx = int.Parse(inputs[3]);
            int creatureVy = int.Parse(inputs[4]);

            game.UpdateCreaturePosition(creatureId, creatureX, creatureY, creatureVx, creatureVy);
        }
    }

    private static void AddStoredScans(List<Drone> myDrones, List<Drone> enemyDrones)
    {
        int droneScanCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < droneScanCount; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int droneId = int.Parse(inputs[0]);
            int creatureId = int.Parse(inputs[1]);

            Drone drone = myDrones.FirstOrDefault(d => d.Id == droneId);
            if (drone == null)
            {
                drone = enemyDrones.FirstOrDefault(d => d.Id == droneId);
            }

            if (drone != null)
            {
                drone.ScannedCreaturesIds.Add(creatureId);
            }
        }
    }
    
    private static void AddRadarBlips(Game game, List<Drone> myDrones, List<Drone> enemyDrones)
    {
        int radarBlipCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < radarBlipCount; i++)
        {
            string[] inputs = Console.ReadLine().Split(' ');
            int droneId = int.Parse(inputs[0]);
            int creatureId = int.Parse(inputs[1]);
            string radar = inputs[2];

            Drone drone = myDrones.FirstOrDefault(d => d.Id == droneId);
            if (drone == null)
            {
                drone = enemyDrones.FirstOrDefault(d => d.Id == droneId);
            }

            if (drone != null)
            {
                drone.AddCreatureDirection(creatureId, (CreatureDirection)Enum.Parse(typeof(CreatureDirection), radar));
            }
        }
    }
}
