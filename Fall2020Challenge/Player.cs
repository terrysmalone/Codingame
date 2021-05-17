using System;
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
            // game loop
            while (true)
            {
                var recipes = CreateRecipes();

                var playerInventories = CreatInventories();

                DisplayRecipes(recipes);
                
                //DisplayPlayerInventories(playerInventories);

                string actionType = string.Empty;
                int recipeId = 0;

                var playerInventory = playerInventories[0];

                // Check recipes
                foreach (var recipe in recipes)
                {
                    if(   playerInventory.GreenIngredients >= recipe.GreenIngredients
                       && playerInventory.BlueIngredients >= recipe.BlueIngredients
                       && playerInventory.YellowIngredients >= recipe.YellowIngredients
                       &&playerInventory.OrangeIngredients >= recipe.OrangeIngredients)
                    {
                        actionType = recipe.ActionType;
                        recipeId = recipe.Id;

                        break;
                    }
                }
            

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                // in the first league: BREW <id> | WAIT; later: BREW <id> | CAST <id> [<times>] | LEARN <id> | REST | WAIT
                Console.WriteLine(actionType + " " + recipeId);
            }
        }

        private static List<Recipe> CreateRecipes()
        {        
            int actionCount = int.Parse(Console.ReadLine()); // the number of spells and recipes in play
            
            List<Recipe> recipes = new List<Recipe>();

            for (int i = 0; i < actionCount; i++)
            {                
                string[] inputs = Console.ReadLine().Split(' ');

                recipes.Add(new Recipe(int.Parse(inputs[0]),
                                       inputs[1],
                                       Math.Abs(int.Parse(inputs[2])),
                                       Math.Abs(int.Parse(inputs[3])),
                                       Math.Abs(int.Parse(inputs[4])),
                                       Math.Abs(int.Parse(inputs[5])),
                                       int.Parse(inputs[6])));                
            }

            recipes = recipes.OrderByDescending(x => x.Price).ToList();

            return recipes;
        }

        private static List<PlayerInventory> CreatInventories()
        {        
            List<PlayerInventory> playerInventories = new List<PlayerInventory>();

            for (int i = 0; i < 2; i++)
            {
                string[] inputs = Console.ReadLine().Split(' ');
                                
                playerInventories.Add(new PlayerInventory(Math.Abs(int.Parse(inputs[0])),
                                       Math.Abs(int.Parse(inputs[1])),
                                       Math.Abs(int.Parse(inputs[2])),
                                       Math.Abs(int.Parse(inputs[3])),
                                       int.Parse(inputs[4])));
            }

            return playerInventories;
        }

        private static void DisplayRecipes(List<Recipe> recipes)
        {
            foreach (Recipe recipe in recipes)
            {  
                Console.Error.WriteLine("actionId: " + recipe.Id);
                Console.Error.WriteLine("actionType: " + recipe.ActionType);   
                Console.Error.WriteLine("blueIngredients: " + recipe.BlueIngredients);   
                Console.Error.WriteLine("greenIngredients: " + recipe.GreenIngredients);   
                Console.Error.WriteLine("orangeIngredients: " + recipe.OrangeIngredients);   
                Console.Error.WriteLine("yellowIngredients: " + recipe.YellowIngredients);   
                Console.Error.WriteLine("Price: " + recipe.Price);   
                
                Console.Error.WriteLine("================================");                     
            }   
        }

        private static void DisplayPlayerInventories(List<PlayerInventory> playerInventories)
        {
            foreach (PlayerInventory playerInventory in playerInventories)
            {             
                Console.Error.WriteLine("blueIngredients: " + playerInventory.BlueIngredients);   
                Console.Error.WriteLine("greenIngredients: " + playerInventory.GreenIngredients);   
                Console.Error.WriteLine("orangeIngredients: " + playerInventory.OrangeIngredients);   
                Console.Error.WriteLine("yellowIngredients: " + playerInventory.YellowIngredients);   
                Console.Error.WriteLine("================================");                     
            }  
        }
    }

    public class Recipe
    {
        public Recipe(int id, 
                      string actionType, 
                      int blueIngredients,
                      int greenIngredients,
                      int orangeIngredients, 
                      int yellowIngredients,
                      int price)
        {
            Id = id;   
            ActionType = actionType;  

            BlueIngredients = blueIngredients;
            GreenIngredients = greenIngredients;
            OrangeIngredients = orangeIngredients;
            YellowIngredients = yellowIngredients;

            Price = price;
        }

        public int Id { get; private set; }
        public string ActionType { get; private set; }
        
        public int BlueIngredients { get; private set; }
        public int GreenIngredients { get; private set; }
        public int OrangeIngredients { get; private set; }
        public int YellowIngredients { get; private set; }

        public int Price { get; private set; }
    }

    public class PlayerInventory
    {
        public PlayerInventory(int blueIngredients,
                               int greenIngredients,
                               int orangeIngredients, 
                               int yellowIngredients,
                               int score)
        {
            BlueIngredients = blueIngredients;
            GreenIngredients = greenIngredients;
            OrangeIngredients = orangeIngredients;
            YellowIngredients = yellowIngredients;

            Score = score;
        }

        public int BlueIngredients { get; private set; }
        public int GreenIngredients { get; private set; }
        public int OrangeIngredients { get; private set; }
        public int YellowIngredients { get; private set; }

        public int Score { get; private set; }
    }
}