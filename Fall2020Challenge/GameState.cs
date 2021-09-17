using System.Collections.Generic;

namespace Fall2020Challenge
{
    internal class GameState
    {
        public List<Spell> AvailableSpells { get; }
        public int[] PlayerIngredients { get; }
        public List<string> Actions { get; }

        public GameState(List<Spell> availableSpells, int[] playerIngredients, List<string> actions)
        {
            AvailableSpells = availableSpells;
            PlayerIngredients = playerIngredients;
            Actions = actions;
        }
    }
}
