namespace Fall2024Challenge_SeleniaCity; 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
internal sealed class Game
{
    public int Resources { get; private set; }
    internal List<Tube> Tubes { get; private set; }
    internal List<Teleporter> Teleporters { get; private set; }
    internal List<Pod> Pods { get; private set; }
    public List<LandingPad> LandingPads { get; private set; }
    public List<Module> Modules { get; private set; }

    internal void SetResources(int resources) => Resources = resources;

    internal void SetTubes(List<Tube> tubes) => Tubes = tubes;

    internal void SetTubes(List<Teleporter> teleporters) => Teleporters = teleporters;

    internal void SetPods(List<Pod> pods) => Pods = pods;

    internal void SetLandingPads(List<LandingPad> landingPads) => LandingPads = landingPads;

    internal void SetModules(List<Module> modules) => Modules = modules;

    // TUBE | UPGRADE | TELEPORT | POD | DESTROY | WAIT
    // Example - "TUBE 0 1;TUBE 0 2;POD 42 0 1 0 2 0 1 0 2"
    internal string GetActions()
    {
        if (Tubes.Count == 0)
        {
            return "TUBE 0 1;TUBE 0 2;POD 42 0 1 0 2 0 1 0 2";
        }
        else
        {
            return "UPGRADE 0 1;UPGRADE 0 2";
        }
    }
}
