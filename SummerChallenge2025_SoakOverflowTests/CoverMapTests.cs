using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace SummerChallenge2025_SoakOverflow.Tests
{
    [TestFixture]
    public class CoverMapTests
    {
        [Test]
        public void CreateCoverMap_NorthProtection()
        {
            double[,] expected = GetFilledArray(5, 5);

            // Agent position
            expected[1, 3] = 0.0; 

            // North protection
            expected[0, 0] = 0.25;
            expected[1, 0] = 0.25;
            expected[2, 0] = 0.25; 
            expected[3, 0] = 0.25; 
            expected[4, 0] = 0.25;

            expected[3, 1] = 0.25;
            expected[4, 1] = 0.25;

            // expected
            // 0.25, 0.25, 0.25, 0.25, 0.25
            // 1.0, 1.0, 1.0, 0.25, 0.25 
            // 1.0, 1.0, 1.0, 1.0, 1.0
            // 1.0, 1.0, 1.0, 1.0, 1.0
            // 1.0, 1.0, 1.0, 1.0, 1.0

            var cover = new int[5, 5];
            cover[1, 2] = 2; // High cover north

            var map = new CoverMap();
            var result = map.CreateCoverMap(1, 3, cover);

            // North row should be filled with 0.5 except adjacent tiles
            Assert.AreEqual(expected, result);
        }

        private static double[,] GetFilledArray(int width, int height)
        {
            var arr = new double[width, height];
            // Populate all elements with 1.0
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    arr[i, j] = 1.0;
                }
            }

            return arr;
        }
    }
}
