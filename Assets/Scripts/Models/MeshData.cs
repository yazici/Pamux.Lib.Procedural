using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pamux.Lib.Procedural.Models
{
    public class MeshData
    {
        private Vector3[] vertices;
        private int[] triangles;
        private Vector2[] uvs;
        private Vector3[] bakedNormals;

        private Vector3[] outOfMeshVertices;
        private int[] outOfMeshTriangles;

        private int triangleIndex;
        private int outOfMeshTriangleIndex;

        private EdgeConnectionVertexData[] edgeConnectionVertices;
        private int edgeConnectionVertexIndex;

        private bool useFlatShading;

        public MeshData(int numVertsPerLine, int skipIncrement, bool useFlatShading)
        {
            this.useFlatShading = useFlatShading;

            var numMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;
            var numEdgeConnectionVertices = (skipIncrement - 1) * (numVertsPerLine - 5) / skipIncrement * 4;
            var numMainVerticesPerLine = (numVertsPerLine - 5) / skipIncrement + 1;
            var numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;

            vertices = new Vector3[numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices];
            uvs = new Vector2[vertices.Length];
            edgeConnectionVertices = new EdgeConnectionVertexData[numEdgeConnectionVertices];

            var numMeshEdgeTriangles = 8 * (numVertsPerLine - 4);
            var numMainTriangles = (numMainVerticesPerLine - 1) * (numMainVerticesPerLine - 1) * 2;
            triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];

            outOfMeshVertices = new Vector3[numVertsPerLine * 4 - 4];
            outOfMeshTriangles = new int[24 * (numVertsPerLine - 2)];
        }

        public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
        {
            if (vertexIndex < 0)
            {
                outOfMeshVertices[-vertexIndex - 1] = vertexPosition;
            }
            else
            {
                vertices[vertexIndex] = vertexPosition;
                uvs[vertexIndex] = uv;
            }
        }

        public void AddTriangle(int a, int b, int c)
        {
            if (a < 0 || b < 0 || c < 0)
            {
                outOfMeshTriangles[outOfMeshTriangleIndex] = a;
                outOfMeshTriangles[outOfMeshTriangleIndex + 1] = b;
                outOfMeshTriangles[outOfMeshTriangleIndex + 2] = c;
                outOfMeshTriangleIndex += 3;
            }
            else
            {
                triangles[triangleIndex] = a;
                triangles[triangleIndex + 1] = b;
                triangles[triangleIndex + 2] = c;
                triangleIndex += 3;
            }
        }

        public void DeclareEdgeConnectionVertex(EdgeConnectionVertexData edgeConnectionVertexData)
        {
            edgeConnectionVertices[edgeConnectionVertexIndex] = edgeConnectionVertexData;
            edgeConnectionVertexIndex++;
        }

        private Vector3[] CalculateNormals()
        {

            var vertexNormals = new Vector3[vertices.Length];
            var triangleCount = triangles.Length / 3;
            for (var i = 0; i < triangleCount; i++)
            {
                var normalTriangleIndex = i * 3;
                var vertexIndexA = triangles[normalTriangleIndex];
                var vertexIndexB = triangles[normalTriangleIndex + 1];
                var vertexIndexC = triangles[normalTriangleIndex + 2];

                var triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                vertexNormals[vertexIndexA] += triangleNormal;
                vertexNormals[vertexIndexB] += triangleNormal;
                vertexNormals[vertexIndexC] += triangleNormal;
            }

            var borderTriangleCount = outOfMeshTriangles.Length / 3;
            for (var i = 0; i < borderTriangleCount; i++)
            {
                var normalTriangleIndex = i * 3;
                var vertexIndexA = outOfMeshTriangles[normalTriangleIndex];
                var vertexIndexB = outOfMeshTriangles[normalTriangleIndex + 1];
                var vertexIndexC = outOfMeshTriangles[normalTriangleIndex + 2];

                var triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                if (vertexIndexA >= 0)
                {
                    vertexNormals[vertexIndexA] += triangleNormal;
                }
                if (vertexIndexB >= 0)
                {
                    vertexNormals[vertexIndexB] += triangleNormal;
                }
                if (vertexIndexC >= 0)
                {
                    vertexNormals[vertexIndexC] += triangleNormal;
                }
            }


            for (var i = 0; i < vertexNormals.Length; i++)
            {
                vertexNormals[i].Normalize();
            }

            return vertexNormals;

        }

        private void ProcessEdgeConnectionVertices()
        {
            foreach (var e in edgeConnectionVertices)
            {
                bakedNormals[e.vertexIndex] = bakedNormals[e.mainVertexAIndex] * (1 - e.dstPercentFromAToB) + bakedNormals[e.mainVertexBIndex] * e.dstPercentFromAToB;
            }
        }

        private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
        {
            var pointA = (indexA < 0) ? outOfMeshVertices[-indexA - 1] : vertices[indexA];
            var pointB = (indexB < 0) ? outOfMeshVertices[-indexB - 1] : vertices[indexB];
            var pointC = (indexC < 0) ? outOfMeshVertices[-indexC - 1] : vertices[indexC];

            var sideAB = pointB - pointA;
            var sideAC = pointC - pointA;
            return Vector3.Cross(sideAB, sideAC).normalized;
        }

        public void ProcessMesh()
        {
            if (useFlatShading)
            {
                FlatShading();
            }
            else
            {
                BakeNormals();
                ProcessEdgeConnectionVertices();
            }
        }

        private void BakeNormals()
        {
            bakedNormals = CalculateNormals();
        }

        private void FlatShading()
        {
            var flatShadedVertices = new Vector3[triangles.Length];
            var flatShadedUvs = new Vector2[triangles.Length];

            for (var i = 0; i < triangles.Length; i++)
            {
                flatShadedVertices[i] = vertices[triangles[i]];
                flatShadedUvs[i] = uvs[triangles[i]];
                triangles[i] = i;
            }

            vertices = flatShadedVertices;
            uvs = flatShadedUvs;
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            if (useFlatShading)
            {
                mesh.RecalculateNormals();
            }
            else
            {
                mesh.normals = bakedNormals;
            }
            return mesh;
        }
    }
}