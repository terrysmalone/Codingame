namespace CoinGuesser;

using System;
using System.Collections.Generic;
using System.Linq;

public class Solution
{
    static void Main(string[] args)
    {
        string[] inputs = Console.ReadLine().Split(' ');

        int numberOfCoins = int.Parse(inputs[0]);
        int numberOfConfigurations = int.Parse(inputs[1]);

        Dictionary<int, List<int>> possibilities = new Dictionary<int, List<int>>();

        int odd = 1;

        for (int i =0; i < numberOfCoins; i++)
        {
            possibilities.Add(odd, GetEvenNumbers(numberOfCoins));

            odd += 2;
        }

        for (int i = 0; i < numberOfConfigurations; i++)
        {
            inputs = Console.ReadLine().Split(' ');

            int[] tosses = new int[numberOfCoins];

            for (int j = 0; j < numberOfCoins; j++)
            {
                tosses[j] = int.Parse(inputs[j]);
            }

            foreach (int oddToss in tosses)
            {
                if (oddToss % 2 != 0) // if it's odd
                {
                    foreach (int evenToss in tosses)
                    {
                        if (evenToss % 2 == 0) // if it's even
                        {
                            possibilities[oddToss].Remove(evenToss);
                        }
                    }
                }
            }
        }

        Dictionary<int, int> confirmed = new Dictionary<int, int>();

        while (confirmed.Count < possibilities.Count)
        {
            // Get confirmed moves
            foreach (KeyValuePair<int, List<int>> possibility in possibilities)
            {
                if (possibility.Value.Count == 1 && !confirmed.ContainsKey(possibility.Key))
                {
                    confirmed[possibility.Key] = possibility.Value.Single();
                    break;
                }
            }

            // Remove all narrowed down results
            foreach (KeyValuePair<int, List<int>> possibility in possibilities)
            {
                if (!confirmed.ContainsKey(possibility.Key))
                {
                    foreach (KeyValuePair<int, int> confirm in confirmed)
                    {
                        possibility.Value.Remove(confirm.Value);
                    }
                }
            }
        }

        confirmed = confirmed.OrderBy(x => x.Key).ToDictionary<KeyValuePair<int, int>, int, int>(c => c.Key, c => c.Value);

        string result = string.Empty;

        foreach (KeyValuePair<int, int> confirm in confirmed)
        {
            result += confirm.Value + " ";
        }

        Console.WriteLine(result.Trim());
    }

    private static List<int> GetEvenNumbers(int numberOfCoins)
    {
        List<int> numbers = new List<int>();

        int even = 2;

        for (int i = 0; i < numberOfCoins; i++)
        {
            numbers.Add(even);

            even += 2;
        }

        return numbers;
    }
}
