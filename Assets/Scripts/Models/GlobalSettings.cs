using UnityEngine;
using System.Collections;
using System;

namespace Pamux.Lib.Procedural.Models
{
    [CreateAssetMenu()]
    public class GlobalSettings : UpdatableData
    {
        public float WorldE2WSize = 200000f;
        public float WorldN2SSize = 200000f;

        public float WorldE2WHalfSize = 100000f;
        public float WorldN2SHalfSize = 100000f;

        public Texture2D AverageTemperaturesMap;
        public Texture2D AveratePrecipitationMap;
        public Texture2D RelativeHumidityMap;
        public Texture2D HeightMap;
        
        private byte[,] averageTemperaturesMap;
        private byte[,] averatePrecipitationMap;
        private byte[,] relativeHumidityMap;
        private byte[,] heightMap;

        private int mapE2WLength;
        private int mapN2SLength;

        public void Initialize()
        {           
            if (heightMap != null)
            {
                return;
            }
            WorldE2WHalfSize = WorldE2WSize / 2;
            WorldN2SHalfSize = WorldN2SSize / 2;
            mapE2WLength = HeightMap.width;
            mapN2SLength = HeightMap.height;

            heightMap = new byte[mapE2WLength, mapN2SLength];

            for (var x = 0; x < mapE2WLength; ++x)
            {
                for (var y = 0; y < mapN2SLength; ++y)
                {
                    var pixel = HeightMap.GetPixel(x, y);

                    if (pixel.a == 0)
                    { 
                        heightMap[x, y] = 0;
                    }
                    else
                    {
                        heightMap[x, y] = (byte) (Mathf.FloorToInt(pixel.grayscale *254)+1);
                        
                    }
                }
            }
            
        }

        int toXIndex(float x)
        {
            
            return (int)((x + WorldE2WHalfSize) * mapE2WLength / WorldE2WSize);
  
        }

        int toYIndex(float y)
        {
            return (int)((y + WorldN2SHalfSize) * mapN2SLength / WorldN2SSize);
        }

        internal bool IsWater(float x, float y)
        {
            var ix = toXIndex(x);
            if (ix < 0 || ix >= mapE2WLength)
            {
                Debug.Log($"x {x}, ix {ix}");
                return false;
            }

            var iy = toYIndex(y);
            if (iy < 0 || iy >= mapN2SLength)
            {
                Debug.Log($"y {y}, iy {iy}");
                return false;
            }

            return heightMap[ix, iy] ==  0;
        }


#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();
        }

        
#endif
    }

}