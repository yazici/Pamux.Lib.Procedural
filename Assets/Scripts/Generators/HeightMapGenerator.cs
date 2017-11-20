﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pamux.Lib.Procedural.Models;

namespace Pamux.Lib.Procedural.Generators
{
    public static class HeightMapGenerator
    {
        public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCentre)
        {
            var values = NoiseGenerator.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCentre);

            AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);

            var minValue = float.MaxValue;
            var maxValue = float.MinValue;

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
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
            return new HeightMap(values, minValue, maxValue);
        }
    }
}