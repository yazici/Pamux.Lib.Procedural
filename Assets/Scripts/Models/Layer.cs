using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pamux.Lib.Procedural.Models
{
    [System.Serializable]
    public class Layer
    {
        public Texture2D texture;

        public Color tint;

        [Range(0, 1)]
        public float tintStrength;

        [Range(0, 1)]
        public float startHeight;

        [Range(0, 1)]
        public float blendStrength;

        public float textureScale;
    }
}