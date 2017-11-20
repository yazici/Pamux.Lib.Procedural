using UnityEngine;
using System.Collections;
using Pamux.Lib.Procedural.Models;
using Pamux.Lib.Procedural.Enums;

namespace Pamux.Lib.Procedural.Generators
{
    public static class NoiseGenerator
    {
        public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre)
        {
            var noiseMap = new float[mapWidth, mapHeight];

            var prng = new System.Random(settings.seed);
            var octaveOffsets = new Vector2[settings.octaves];

            var maxPossibleHeight = 0f;
            var amplitude = 1f;
            var frequency = 1f;

            for (var i = 0; i < settings.octaves; i++)
            {
                var offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
                var offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);

                maxPossibleHeight += amplitude;
                amplitude *= settings.persistance;
            }

            var maxLocalNoiseHeight = float.MinValue;
            var minLocalNoiseHeight = float.MaxValue;

            var halfWidth = mapWidth / 2f;
            var halfHeight = mapHeight / 2f;


            for (var y = 0; y < mapHeight; y++)
            {
                for (var x = 0; x < mapWidth; x++)
                {

                    amplitude = 1f;
                    frequency = 1f;
                    var noiseHeight = 0f;

                    for (var i = 0; i < settings.octaves; i++)
                    {
                        var sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                        var sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                        var perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= settings.persistance;
                        frequency *= settings.lacunarity;
                    }

                    if (noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }
                    if (noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }
                    noiseMap[x, y] = noiseHeight;

                    if (settings.normalizeMode == NormalizeMode.Global)
                    {
                        var normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
                        noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                    }
                }
            }

            if (settings.normalizeMode == NormalizeMode.Local)
            {
                for (var y = 0; y < mapHeight; y++)
                {
                    for (var x = 0; x < mapWidth; x++)
                    {
                        noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                    }
                }
            }

            return noiseMap;
        }
    }    
}