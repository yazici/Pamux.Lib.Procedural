using UnityEngine;
using Pamux.Lib.Procedural.Generators;
using Pamux.Lib.Procedural.Utilities;

namespace Pamux.Lib.Procedural.Models
{
    public class TerrainChunk
    {
        private const float colliderGenerationDistanceThreshold = 5;
        private const float colliderGenerationSquareOfDistanceThreshold = colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold;

        public event System.Action<TerrainChunk, bool> onVisibilityChanged;

        public Vector2 coord;

        private GameObject meshObject;
        private Vector2 sampleCentre;
        private Bounds bounds;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        private LodInfo[] detailLevels;
        private LodMesh[] lodMeshes;
        private int colliderLodIndex;

        private HeightMap heightMap;
        private bool heightMapReceived;
        private int previousLodIndex = -1;
        private bool hasSetCollider;
        private float maxViewDst;

        private HeightMapSettings heightMapSettings;
        private MeshSettings meshSettings;
        private TerrainViewer terrainViewer;

        public TerrainChunk(TerrainViewer terrainViewer, Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LodInfo[] detailLevels, int colliderLodIndex, Material material)
        {
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.colliderLodIndex = colliderLodIndex;
            this.heightMapSettings = heightMapSettings;
            this.meshSettings = meshSettings;
            this.terrainViewer = terrainViewer;

            var position = coord * meshSettings.meshWorldSize;
            sampleCentre = position / meshSettings.meshScale;
            bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);


            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.position = new Vector3(position.x, 0, position.y);
            meshObject.transform.parent = terrainViewer.ChunkParent;
            SetVisible(false);

            lodMeshes = new LodMesh[detailLevels.Length];
            for (var i = 0; i < detailLevels.Length; ++i)
            {
                lodMeshes[i] = new LodMesh(detailLevels[i].lod);
                lodMeshes[i].updateCallback += UpdateTerrainChunk;
                if (i == colliderLodIndex)
                {
                    lodMeshes[i].updateCallback += UpdateCollisionMesh;
                }
            }

            maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;

        }

        public void Load()
        {
            ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
        }

        void OnHeightMapReceived(object heightMapObject)
        {
            this.heightMap = (HeightMap)heightMapObject;
            heightMapReceived = true;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk()
        {
            if (!heightMapReceived)
            {
                return;
            }

            var viewerDstFromNearestEdge = terrainViewer.GetDistanceFromNearestEdge(bounds);

            var wasVisible = IsVisible();
            var visible = viewerDstFromNearestEdge <= maxViewDst;

            if (visible)
            {
                int lodIndex = 0;

                for (var i = 0; i < detailLevels.Length - 1; ++i)
                {
                    if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (lodIndex != previousLodIndex)
                {
                    var lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLodIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }


            }

            if (wasVisible != visible)
            {

                SetVisible(visible);
                if (onVisibilityChanged != null)
                {
                    onVisibilityChanged(this, visible);
                }
            }
        }

        public void UpdateCollisionMesh()
        {
            if (hasSetCollider)
            {
                return;
            }

            var sqrDstFromViewerToEdge = terrainViewer.GetSquareOfDistanceFromNearestEdge(bounds);

            if (sqrDstFromViewerToEdge < detailLevels[colliderLodIndex].sqrVisibleDstThreshold)
            {
                if (!lodMeshes[colliderLodIndex].hasRequestedMesh)
                {
                    lodMeshes[colliderLodIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            if (sqrDstFromViewerToEdge < colliderGenerationSquareOfDistanceThreshold)
            {
                if (lodMeshes[colliderLodIndex].hasMesh)
                {
                    meshCollider.sharedMesh = lodMeshes[colliderLodIndex].mesh;
                    hasSetCollider = true;
                }
            }
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }    
}