namespace SpringChallenge2020; 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
internal class Game
{
    private const int pelletValue = 1;
    private const int superPelletValue = 10;

    public List<Pellet> Pellets { get; private set; }

    internal void SetPellets(List<Pellet> pellets) => Pellets = pellets;
    
    // MOVE <pacId> <x> <y>
    internal string GetCommand()
    {
        Pellet pellet = new Pellet();

        if (Pellets.Any(p => p.Value == superPelletValue))
        {
            pellet = Pellets.First(p => p.Value == superPelletValue);
        }
        else
        {
            pellet = Pellets.First();
        }

        return $"MOVE 0 {pellet.X} {pellet.Y}";    
    }
}