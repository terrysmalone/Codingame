using System;

namespace PlatinumRift
{
    public class Test
    {
        public void Testing()
        {
            var N = int.Parse(Console.ReadLine());

            var numbers = new int[N];

            for (var i = 0; i < N; i++)
            {
                numbers[i] = int.Parse(Console.ReadLine());
            }

            string test = "  ^  ";

            test.IndexOf()

            var count = 0;
            var start = numbers[0];
            var currentIndex = start;

            while (count <= 300)
            {
                var num = numbers[currentIndex];

                currentIndex = num;

                if (currentIndex == start)
                {
                    Console.WriteLine("true");
                    return;
                }

            int.TryParse()
            }

            Console.WriteLine("false");
        }
    }
}
