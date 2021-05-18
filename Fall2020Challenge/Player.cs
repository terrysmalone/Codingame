﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Fall2020Challenge
{
    internal sealed class Player
    {
        static void Main(string[] args)
        {
            var game = new Game();
            
            // game loop
            while (true)
            {
                ParseActions(game);

                game.SetPlayerInventory(GetInventoryItems());
                game.SetOpponentInventory(GetInventoryItems());
                
                //DisplayInventory(game.PlayerInventory, true);
                //DisplayInventory(game.OpponentInventory, false);

                var action = game.GetAction();
                Console.WriteLine(action);

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                // in the first league: BREW <id> | WAIT; later: BREW <id> | CAST <id> [<times>] | LEARN <id> | REST | WAIT
                //Console.WriteLine(actionType + " " + recipeId);
            }
        }

        private static void ParseActions(Game game)
        {        
            var actionCount = int.Parse(Console.ReadLine()); // the number of spells and recipes in play
            
            var recipes = new List<Recipe>();
            var spells = new List<Spell>();
            

            for (var i = 0; i < actionCount; i++)
            {               
                var input = Console.ReadLine();
                var inputs = input.Split(' ');
                
                if(inputs[1] == "BREW")
                {
                    recipes.Add(new Recipe(int.Parse(inputs[0]),
                                           new int[] { Math.Abs(int.Parse(inputs[2])),
                                                       Math.Abs(int.Parse(inputs[3])),
                                                       Math.Abs(int.Parse(inputs[4])),
                                                       Math.Abs(int.Parse(inputs[5]))},
                                           int.Parse(inputs[6])));   
                }
                else if(inputs[1] == "CAST")
                {
                    spells.Add(new Spell(int.Parse(inputs[0]),
                                         new int[] { int.Parse(inputs[2]),
                                                     int.Parse(inputs[3]),
                                                     int.Parse(inputs[4]),
                                                     int.Parse(inputs[5])},
                                         int.Parse(inputs[9]) == 1));   
                }       
            }
            
            game.SetRecipes(recipes);
            game.SetSpells(spells);
        }
        
        private static Inventory GetInventoryItems()
        {
            var input = Console.ReadLine();
            var inputs = input.Split(' ');
            
            var inventory = new Inventory(new int[] {Math.Abs(int.Parse(inputs[0])),
                                          Math.Abs(int.Parse(inputs[1])),
                                          Math.Abs(int.Parse(inputs[2])),
                                          Math.Abs(int.Parse(inputs[3]))},
                                          int.Parse(inputs[4]));

            return inventory;
        }

        private static void DisplayInventory (Inventory inventory, bool mine)
        {
            Console.Error.WriteLine(mine ? "My inventory" : "Opponent inventory");
            Console.Error.WriteLine("-----------");
            
            Console.Error.WriteLine("blueIngredients: " + inventory.Ingredients[0]);   
            Console.Error.WriteLine("greenIngredients: " + inventory.Ingredients[1]);   
            Console.Error.WriteLine("orangeIngredients: " + inventory.Ingredients[2]);   
            Console.Error.WriteLine("yellowIngredients: " + inventory.Ingredients[3]); 
        }
    }

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

    internal sealed class Recipe
    {
        public const string ActionType = "BREW";
        
        internal int Id { get; }
        public int[] Ingredients { get; }
        internal int Price { get; }
        
        internal Recipe(int id,
                        int[] ingredients,
                        int price)
        {
            Id = id;
            Ingredients = ingredients;
            Price = price;
        }

        
    }
    
    internal sealed class Spell
    {
        internal const string ActionType = "CAST";
        
        internal int Id { get; }
        public int[] IngredientsChange { get; }
        internal bool Castable { get; }

        internal Spell(int id,
                       int[] ingredientsChange,
                       bool castable)
        {
            Id = id;
            IngredientsChange = ingredientsChange;
            Castable = castable;

        }
    }

    internal sealed class Inventory
    {
        public int[] Ingredients { get; }
        internal int Score { get; }
        
        internal Inventory(int[] ingredients,
                           int score)
        {
            Ingredients = ingredients;
            Score = score;
        }
    }
}

// for (int i = 0; i < actionCount; i++)
// {
//     inputs = Console.ReadLine().Split(' ');
//     int actionId = int.Parse(inputs[0]); // the unique ID of this spell or recipe
//     string actionType = inputs[1]; // in the first league: BREW; later: CAST, OPPONENT_CAST, LEARN, BREW
//     int delta0 = int.Parse(inputs[2]); // tier-0 ingredient change
//     int delta1 = int.Parse(inputs[3]); // tier-1 ingredient change
//     int delta2 = int.Parse(inputs[4]); // tier-2 ingredient change
//     int delta3 = int.Parse(inputs[5]); // tier-3 ingredient change
//     int price = int.Parse(inputs[6]); // the price in rupees if this is a potion
//     int tomeIndex = int.Parse(inputs[7]); // in the first two leagues: always 0; later: the index in the tome if this is a tome spell, equal to the read-ahead tax; For brews, this is the value of the current urgency bonus
//     int taxCount = int.Parse(inputs[8]); // in the first two leagues: always 0; later: the amount of taxed tier-0 ingredients you gain from learning this spell; For brews, this is how many times you can still gain an urgency bonus
//     bool castable = inputs[9] != "0"; // in the first league: always 0; later: 1 if this is a castable player spell
//     bool repeatable = inputs[10] != "0"; // for the first two leagues: always 0; later: 1 if this is a repeatable player spell
// }
// for (int i = 0; i < 2; i++)
// {
//     inputs = Console.ReadLine().Split(' ');
//     int inv0 = int.Parse(inputs[0]); // tier-0 ingredients in inventory
//     int inv1 = int.Parse(inputs[1]);
//     int inv2 = int.Parse(inputs[2]);
//     int inv3 = int.Parse(inputs[3]);
//     int score = int.Parse(inputs[4]); // amount of rupees
// }