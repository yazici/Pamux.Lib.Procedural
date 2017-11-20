using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pamux.Lib.Procedural.Utilities;
using Pamux.Lib.Procedural.Generators;

namespace Pamux.Lib.Procedural.Models
{
    public class LodMesh
    {
        public event System.Action updateCallback;

        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;

        private int lod;

        public LodMesh(int lod)
        {
            this.lod = lod;
        }

        private void OnMeshDataReceived(object meshDataObject)
        {
            mesh = ((MeshData)meshDataObject).CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
        {
            hasRequestedMesh = true;
            ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
        }
    }
}