using System;
using System.Collections.Generic;
using System.Linq;

namespace Fall2020Challenge
{
    internal sealed class Game
    {   
        internal List<Recipe> Recipes { get; private set; }
        internal List<Spell> Spells { get; private set; }
        internal Inventory PlayerInventory { get; private set; }
        internal Inventory OpponentInventory { get; private set; }
        
        public Game()
        {
            Recipes = new List<Recipe>();
        }
        
        public void SetRecipes(List<Recipe> recipes)
        {
            Recipes = recipes;
        }
        
        public void SetSpells(List<Spell> spells)
        {
            Spells = spells;
        }
        
        public void SetPlayerInventory(Inventory inventoryItems)
        {
            PlayerInventory = inventoryItems;
        }
        
        public void SetOpponentInventory(Inventory inventoryItems)
        {
            OpponentInventory = inventoryItems;
        }
        public string GetAction()
        {
            //DisplayRecipes();
            //DisplaySpells();
            
            // Very basic implementation
            foreach (var recipe in Recipes.Where(recipe =>    PlayerInventory.Ingredients[0] >= recipe.Ingredients[0] 
                                                              && PlayerInventory.Ingredients[1] >= recipe.Ingredients[1] 
                                                              && PlayerInventory.Ingredients[2] >= recipe.Ingredients[2] 
                                                              && PlayerInventory.Ingredients[3] >= recipe.Ingredients[3]))
            {
                return $"{Recipe.ActionType} {recipe.Id}";
            }
            
            // Pick the most expensive spell
            var targetRecipe = Recipes.OrderByDescending(s => s.Price).First();
            
            Console.Error.WriteLine($"targetRecipe: {targetRecipe.Id}");
            
            //var neededForRecipe = CalculateNeededIngredients(targetRecipe.Ingredients);

            //var  currentlyNeeded = neededForRecipe[0] + neededForRecipe[1] + neededForRecipe[2] + neededForRecipe[3];
            
            var bestSpellId = -1;
            var lowestNeeds = int.MaxValue;

            foreach (var spell in Spells.Where(s => s.Castable))
            {
                if(!CanSpellBeCast(spell.IngredientsChange)) { continue; }

                var needsAfterSpell = CalculateNeededIngredientsAfterChange(targetRecipe.Ingredients, spell.IngredientsChange);
                
                Console.Error.WriteLine($"Needs after spell {spell.Id}: [{needsAfterSpell[0]},{needsAfterSpell[1]},{needsAfterSpell[2]},{needsAfterSpell[3]}]");
                var totalNeedsAfterSpell = needsAfterSpell[0] + needsAfterSpell[1] + needsAfterSpell[2] + needsAfterSpell[3];
                
                // If this spell makes it brewable just cast it
                if(totalNeedsAfterSpell == 0)
                {
                    return $"{Spell.ActionType} {spell.Id}";
                }
                
                // Check if this spell makes it more castable
                if(totalNeedsAfterSpell < lowestNeeds)
                {
                    lowestNeeds = totalNeedsAfterSpell;
                    bestSpellId = spell.Id;
                }
            }
            
            if(bestSpellId != -1)
            {
                return $"{Spell.ActionType} {bestSpellId}";
            }

            //return $"{Spell.ActionType} {Spells.First(s => s.Castable).Id}";
            return "REST";
            
        }
        private bool CanSpellBeCast(int[] spellIngredientsChange)
        {
            var inventoryIngredients = PlayerInventory.Ingredients;
            
            return    inventoryIngredients[0] + spellIngredientsChange[0] >= 0
                      && inventoryIngredients[1] + spellIngredientsChange[1] >= 0
                      && inventoryIngredients[2] + spellIngredientsChange[2] >= 0
                      && inventoryIngredients[3] + spellIngredientsChange[3] >= 0;

        }

        private int[] CalculateNeededIngredients(int[] targetIngredients)
        {
            return CalculateNeededIngredientsAfterChange(targetIngredients, new int[] { 0, 0, 0, 0 });
        }
        
        private int[] CalculateNeededIngredientsAfterChange(IReadOnlyList<int> targetIngredients, IReadOnlyList<int> ingredientsChange)
        {
            var blueDiff = targetIngredients[0] - (PlayerInventory.Ingredients[0] + ingredientsChange[0]);
            var greenDiff = targetIngredients[1] - (PlayerInventory.Ingredients[1] + ingredientsChange[1]);
            var orangeDiff = targetIngredients[2] - (PlayerInventory.Ingredients[2] + ingredientsChange[2]);
            var yellowDiff = targetIngredients[3] - (PlayerInventory.Ingredients[3] + ingredientsChange[3]);
            
            var blueNeeds = blueDiff >= 0 ? blueDiff : 0;
            var greenNeeds = greenDiff >= 0 ? greenDiff : 0;
            var orangeNeeds = orangeDiff >= 0 ? orangeDiff : 0;
            var yellowNeeds = yellowDiff >= 0 ? yellowDiff : 0;

            return new []
            {
                blueNeeds,
                greenNeeds,
                orangeNeeds,
                yellowNeeds
            };
        }

        private void DisplayRecipes()
        {
            Console.Error.WriteLine("Recipes");
            
            foreach (var recipe in Recipes)
            {  
                Console.Error.WriteLine("actionId: " + recipe.Id);
                Console.Error.WriteLine("blueIngredients:   " + recipe.Ingredients[0]);   
                Console.Error.WriteLine("greenIngredients:  " + recipe.Ingredients[1]);   
                Console.Error.WriteLine("orangeIngredients: " + recipe.Ingredients[2]);   
                Console.Error.WriteLine("yellowIngredients: " + recipe.Ingredients[3]);   
                Console.Error.WriteLine("Price: " + recipe.Price);  
                Console.Error.WriteLine();                    
            }   
        }
        
        private void DisplaySpells()
        {
            Console.Error.WriteLine("Spells");
            
            foreach (var spell in Spells)
            {  
                Console.Error.WriteLine("actionId: " + spell.Id);
                Console.Error.WriteLine("Blue Ingredients change:   " + spell.IngredientsChange[0]);   
                Console.Error.WriteLine("Green Ingredients change:  " + spell.IngredientsChange[1]);   
                Console.Error.WriteLine("Orange Ingredients change: " + spell.IngredientsChange[2]);   
                Console.Error.WriteLine("Yellow Ingredients change: " + spell.IngredientsChange[3]);
                Console.Error.WriteLine();        
            }   
        }
    }
}