using UnityEngine;
using System.Collections;

namespace Pamux.Lib.Procedural.Generators
{
    public static class FalloffGenerator
    {
        public static float[,] GenerateFalloffMap(int size)
        {
            var map = new float[size, size];

            for (var i = 0; i < size; ++i)
            {
                for (var j = 0; j < size; ++j)
                {
                    var x = i / (float)size * 2 - 1;
                    var y = j / (float)size * 2 - 1;

                    var value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                    map[i, j] = Evaluate(value);
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