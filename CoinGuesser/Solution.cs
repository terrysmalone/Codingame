namespace CoinGuesser;

using System;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    static void Main(string[] args)
    {
        string[] inputs = Console.ReadLine().Split(' ');

        var numberOfCoins = int.Parse(inputs[0]);
        var numberOfConfigurations = int.Parse(inputs[1]);

        var possibilities = new Dictionary<int, List<int>>();

        var odd = 1;

        for (var i=0; i < numberOfCoins; i++)
        {
            possibilities.Add(odd, GetEvenNumbers(numberOfCoins));

            odd += 2;
        }

        for (var i = 0; i < numberOfConfigurations; i++)
        {
            inputs = Console.ReadLine().Split(' ');

            var tosses = new int[numberOfCoins];

            for (var j = 0; j < numberOfCoins; j++)
            {
                tosses[j] = int.Parse(inputs[j]);
            }

            foreach (var oddToss in tosses)
            {
                if (oddToss % 2 != 0) // if it's odd
                {
                    foreach (var evenToss in tosses)
                    {
                        if (evenToss % 2 == 0) // if it's even
                        {
                            possibilities[oddToss].Remove(evenToss);
                        }
                    }
                }
            }
        }

        var confirmed = new Dictionary<int, int>();

        while (confirmed.Count < possibilities.Count)
        {
            // Get confirmed moves
            foreach (var possibility in possibilities)
            {
                if (possibility.Value.Count == 1 && !confirmed.ContainsKey(possibility.Key))
                {
                    confirmed[possibility.Key] = possibility.Value.Single();
                    break;
                }
            }

            // Remove all narrowed down results
            foreach (var possibility in possibilities)
            {
                if (!confirmed.ContainsKey(possibility.Key))
                {
                    foreach (var confirm in confirmed)
                    {
                        possibility.Value.Remove(confirm.Value);
                    }
                }
            }
        }

        confirmed = confirmed.OrderBy(x => x.Key).ToDictionary<KeyValuePair<int, int>, int, int>(c => c.Key, c => c.Value);

        var result = string.Empty;

        foreach (var confirm in confirmed)
        {
            result += confirm.Value + " ";
        }

        Console.WriteLine(result.Trim());
    }

    private static List<int> GetEvenNumbers(int numberOfCoins)
    {
        var numbers = new List<int>();

        var even = 2;

        for (var i = 0; i < numberOfCoins; i++)
        {
            numbers.Add(even);

            even += 2;
        }

        return numbers;
    }
}
