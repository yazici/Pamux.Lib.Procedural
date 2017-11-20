﻿using UnityEngine;
using System.Collections;
using Pamux.Lib.Procedural.Models;

namespace Pamux.Lib.Procedural.Generators
{
    public static class MeshGenerator
    {
        public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, int levelOfDetail)
        {
            var skipIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
            var numVertsPerLine = meshSettings.numVertsPerLine;

            var topLeft = new Vector2(-1, 1) * meshSettings.meshWorldSize / 2f;

            var meshData = new MeshData(numVertsPerLine, skipIncrement, meshSettings.useFlatShading);

            var vertexIndicesMap = new int[numVertsPerLine, numVertsPerLine];
            var meshVertexIndex = 0;
            var outOfMeshVertexIndex = -1;

            for (var y = 0; y < numVertsPerLine; y++)
            {
                for (var x = 0; x < numVertsPerLine; x++)
                {
                    var isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
                    var isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);
                    if (isOutOfMeshVertex)
                    {
                        vertexIndicesMap[x, y] = outOfMeshVertexIndex;
                        outOfMeshVertexIndex--;
                    }
                    else if (!isSkippedVertex)
                    {
                        vertexIndicesMap[x, y] = meshVertexIndex;
                        meshVertexIndex++;
                    }
                }
            }

            for (var y = 0; y < numVertsPerLine; y++)
            {
                for (var x = 0; x < numVertsPerLine; x++)
                {
                    var isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % skipIncrement != 0 || (y - 2) % skipIncrement != 0);

                    if (!isSkippedVertex)
                    {
                        var isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;
                        var isMeshEdgeVertex = (y == 1 || y == numVertsPerLine - 2 || x == 1 || x == numVertsPerLine - 2) && !isOutOfMeshVertex;
                        var isMainVertex = (x - 2) % skipIncrement == 0 && (y - 2) % skipIncrement == 0 && !isOutOfMeshVertex && !isMeshEdgeVertex;
                        var isEdgeConnectionVertex = (y == 2 || y == numVertsPerLine - 3 || x == 2 || x == numVertsPerLine - 3) && !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;

                        var vertexIndex = vertexIndicesMap[x, y];
                        var percent = new Vector2(x - 1, y - 1) / (numVertsPerLine - 3);
                        var vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * meshSettings.meshWorldSize;
                        var height = heightMap[x, y];

                        if (isEdgeConnectionVertex)
                        {
                            var isVertical = x == 2 || x == numVertsPerLine - 3;
                            var dstToMainVertexA = ((isVertical) ? y - 2 : x - 2) % skipIncrement;
                            var dstToMainVertexB = skipIncrement - dstToMainVertexA;
                            var dstPercentFromAToB = dstToMainVertexA / (float)skipIncrement;

                            var coordA = new Coord((isVertical) ? x : x - dstToMainVertexA, (isVertical) ? y - dstToMainVertexA : y);
                            var coordB = new Coord((isVertical) ? x : x + dstToMainVertexB, (isVertical) ? y + dstToMainVertexB : y);

                            var heightMainVertexA = heightMap[coordA.x, coordA.y];
                            var heightMainVertexB = heightMap[coordB.x, coordB.y];

                            height = heightMainVertexA * (1 - dstPercentFromAToB) + heightMainVertexB * dstPercentFromAToB;

                            var edgeConnectionVertexData = new EdgeConnectionVertexData(vertexIndex, vertexIndicesMap[coordA.x, coordA.y], vertexIndicesMap[coordB.x, coordB.y], dstPercentFromAToB);
                            meshData.DeclareEdgeConnectionVertex(edgeConnectionVertexData);
                        }

                        meshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), percent, vertexIndex);

                        var createTriangle = x < numVertsPerLine - 1 && y < numVertsPerLine - 1 && (!isEdgeConnectionVertex || (x != 2 && y != 2));

                        if (createTriangle)
                        {
                            var currentIncrement = (isMainVertex && x != numVertsPerLine - 3 && y != numVertsPerLine - 3) ? skipIncrement : 1;

                            var a = vertexIndicesMap[x, y];
                            var b = vertexIndicesMap[x + currentIncrement, y];
                            var c = vertexIndicesMap[x, y + currentIncrement];
                            var d = vertexIndicesMap[x + currentIncrement, y + currentIncrement];
                            meshData.AddTriangle(a, d, c);
                            meshData.AddTriangle(d, a, b);
                        }
                    }
                }
            }
            meshData.ProcessMesh();
            return meshData;
        }
    }
}