using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pamux.Lib.Procedural.Models
{
    public class EdgeConnectionVertexData
    {
        public int vertexIndex;
        public int mainVertexAIndex;
        public int mainVertexBIndex;
        public float dstPercentFromAToB;

        public EdgeConnectionVertexData(int vertexIndex, int mainVertexAIndex, int mainVertexBIndex, float dstPercentFromAToB)
        {
            this.vertexIndex = vertexIndex;
            this.mainVertexAIndex = mainVertexAIndex;
            this.mainVertexBIndex = mainVertexBIndex;
            this.dstPercentFromAToB = dstPercentFromAToB;
        }
    }
}