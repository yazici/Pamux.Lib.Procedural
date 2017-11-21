using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pamux.Lib.Procedural.Models;

namespace Pamux.Lib.Procedural.Generators
{
    public static class HeightMapGenerator
    {
        private static float[,] falloffMap;

        public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre)
        {
            var values = NoiseGenerator.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);



            AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

            var minValue = float.MaxValue;
            var maxValue = float.MinValue;

            for (var i = 0; i < width; ++i)
            {
                for (var j = 0; j < height; ++j)
                {
                    values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]) * settings.heightMultiplier;

                    if (values[i, j] > maxValue)
                    {
                        maxValue = values[i, j];
                    }
                    if (values[i, j] < minValue)
                    {
                        minValue = values[i, j];
                    }
                }
            }



            if (settings.useFalloff)
            {
                var falloffMap = FalloffGenerator.GenerateFalloffMap(width, height);

                for (var y = 0; y < height; ++y)
                {
                    for (var x = 0; x < width; ++x)
                    {
                        values[x, y] = Mathf.Clamp01(values[x, y] - falloffMap[x, y]);
                    }
                }
            }

            return new HeightMap(values, minValue, maxValue);
        }
    }
}