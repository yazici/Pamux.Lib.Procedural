using UnityEngine;
using System.Collections;

namespace Pamux.Lib.Procedural.Models
{
    [CreateAssetMenu()]
    public class GlobalSettings : UpdatableData
    {
        public Texture2D AverageTemperaturesMap;
        public Texture2D AveratePrecipitationMap;
        public Texture2D RelativeHumidityMap;
        public Texture2D HeightMap;


#if UNITY_EDITOR

        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif
    }

}