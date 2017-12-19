using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pamux.Lib.Procedural.Models;

namespace Pamux.Lib.Procedural.Generators
{
    public static class HeightMapGenerator
    {
        private static float[,] falloffMap;

        public static HeightMap GenerateHeightMap(int width, int height, GlobalSettings globalSettings, HeightMapSettings heightMapSettings, Vector2 sampleCentre)
        {
            var values = NoiseGenerator.GenerateNoiseMap(width, height, globalSettings, heightMapSettings.noiseSettings, sampleCentre);

            AnimationCurve heightCurve_threadsafe = new AnimationCurve(heightMapSettings.heightCurve.keys);

            var minValue = float.MaxValue;
            var maxValue = float.MinValue;

            //if (settings.useFalloff)
            //{
            //    var falloffMap = FalloffGenerator.GenerateFalloffMap(width, height);

            //    for (var y = 0; y < height; ++y)
            //    {
            //        for (var x = 0; x < width; ++x)
            //        {
            //            values[x, y] = Mathf.Clamp01(values[x, y] - falloffMap[x, y]);
            //        }
            //    }
            //}

            for (var x = 0; x < width; ++x)
            {
                for (var y = 0; y < height; ++y)
                {
                    values[x, y] *= heightCurve_threadsafe.Evaluate(values[x, y]) * heightMapSettings.heightMultiplier;
         
                    if (values[x, y] > maxValue)
                    {
                        maxValue = values[x, y];
                    }
                    if (values[x, y] < minValue)
                    {
                        minValue = values[x, y];
                    }
                }
            }

            return new HeightMap(values, minValue, maxValue);
        }
    }
}