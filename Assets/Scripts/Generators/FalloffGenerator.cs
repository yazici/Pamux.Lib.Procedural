using UnityEngine;
using System.Collections;

namespace Pamux.Lib.Procedural.Generators
{
    public static class FalloffGenerator
    {
        public static float[,] GenerateFalloffMap(int width, int height)
        {
            var map = new float[width, height];

            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    var dx = x / (float)width * 2 - 1;
                    var dy = y / (float)height * 2 - 1;

                    var value = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
                    map[x, y] = Evaluate(value);
                }
            }

            return map;
        }

        private static float Evaluate(float value)
        {
            // TODO: Magic values? Mentioned in the tutorial?
            var a = 3;
            var b = 2.2f;

            return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
        }
    }
}