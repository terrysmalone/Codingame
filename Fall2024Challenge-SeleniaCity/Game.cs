namespace Fall2024Challenge_SeleniaCity;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
internal sealed partial class Game
{
    public int Resources { get; private set; }
    internal List<Tube> Tubes { get; private set; }
    internal List<Teleporter> Teleporters { get; private set; }
    internal List<Pod> Pods { get; private set; }
    public List<LandingPad> LandingPads { get; private set; } = new List<LandingPad>();
    public List<Module> Modules { get; private set; } = new List<Module>();

    internal void SetResources(int resources) => Resources = resources;

    internal void SetTubes(List<Tube> tubes) => Tubes = tubes;

    internal void SetTubes(List<Teleporter> teleporters) => Teleporters = teleporters;

    internal void SetPods(List<Pod> pods) => Pods = pods;

    internal void AddLandingPads(List<LandingPad> landingPads) => LandingPads.AddRange(landingPads);

    internal void AddModules(List<Module> modules) => Modules.AddRange(modules);

    private const int TELEPORTER_COST = 5000;
    private const int POD_COST = 5000;
    private const int DESTROY_REFUND = 750;

    private int currentPodId = 0;
   
    // TUBE | UPGRADE | TELEPORT | POD | DESTROY | WAIT
    // Example - "TUBE 0 1;TUBE 0 2;POD 42 0 1 0 2 0 1 0 2"
    internal string GetActions()
    {
        Display.Summary(this, true);

        //int cost0 = CalculateTubeCost(LandingPads[0], Modules[0]);
        //int cost1 = CalculateTubeCost(LandingPads[0], Modules[1]);

        // Bare minimum implementation
        // Create a tube from every landing pod to every building (that doesn't already exist)

        string actions = string.Empty;

        // Define tubes
        string tubesActions = string.Empty;
        string podsActions = string.Empty;

        foreach (LandingPad landingPad in LandingPads)
        {
            string podPath = string.Empty;

            foreach (Module module in Modules)
            {
                Console.Error.WriteLine($"podPath: {podPath}");
                // If the landing pad has astronauts of the module type
                if (landingPad.Astronauts.Contains(module.Type))
                {
                   // If the tube does not exist (TODO: Later we'll want to check teleporters too)
                   if (!Tubes.Any(a => a.Building1Id == landingPad.Id && a.Building2Id == module.Id))
                    {
                        tubesActions += ($"{nameof(ActionType.TUBE)} {landingPad.Id} {module.Id};");

                        podPath += $"{landingPad.Id} {module.Id} ";
                    }
                }
                Console.Error.WriteLine($"podPath: {podPath}");
            }

            if (podPath != string.Empty)
            {
                podPath = podPath.TrimEnd();
                string podAction = $"{ActionType.POD} {currentPodId} {podPath} {landingPad.Id};";
                currentPodId++;

                podsActions += podAction;
            }
        }

        if (tubesActions != string.Empty)
        {
            actions += tubesActions;
        }

        if (podsActions != string.Empty)
        {
            actions += podsActions;
        }

        if (actions != string.Empty)
        {
            return actions;
        }

        return nameof(ActionType.WAIT);
    }

    private static int CalculateTubeCost(IBuilding building1, IBuilding building2)
    {
        Point point1 = building1.Getposition();
        Point point2 = building2.Getposition();

        double distance = Math.Sqrt(Math.Pow(point1.X-point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2));

        int cost = (int)(Math.Round((distance), MidpointRounding.ToZero) * 10);

        return cost;
    }
}
