using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pamux.Lib.Procedural.Models
{
    public class TerrainViewer : MonoBehaviour
    {
        public Vector2 position2d
        {
            get
            {
                return new Vector2(transform.position.x, transform.position.z);
            }
        }

        public float GetSquareOfDistanceFromNearestEdge(Bounds bounds)
        {
            return bounds.SqrDistance(position2d);
        }

        public float GetDistanceFromNearestEdge(Bounds bounds)
        {
            return Mathf.Sqrt(GetSquareOfDistanceFromNearestEdge(bounds));
        }

        void Start()
        {

        }
        void Update()
        {

        }
    }
}