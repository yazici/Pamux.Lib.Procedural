using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pamux.Lib.Utilities;
using Pamux.Lib.Procedural.Utilities;

namespace Pamux.Lib.Procedural.Models
{
    [RequireComponent(typeof(ThreadedDataRequester))]

    public class TerrainViewer : Singleton<TerrainViewer>
    {
        private const float moveThresholdForChunkUpdate = 25f;
        private const float moveThresholdForChunkUpdateSquared = moveThresholdForChunkUpdate * moveThresholdForChunkUpdate;

        public GlobalSettings globalSettings;
        public MeshSettings meshSettings;
        public HeightMapSettings heightMapSettings;
        public TextureSettings textureSettings;

        public int colliderLodIndex;
        public LodInfo[] detailLevels;

        public Material mapMaterial;

        private bool isReady = false;

        private Transform chunkParent;
        public Transform ChunkParent => chunkParent;

        private Vector2 currentPosition2d;
        private Vector2 chunkVisibilityEvaluationPosition2d;

        private IList<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();
        private IDictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

        private bool IsOnChunkVisibilityEvaluationPosition => currentPosition2d == chunkVisibilityEvaluationPosition2d;
        private bool ShouldReevaluateChunkVisibility => (chunkVisibilityEvaluationPosition2d - currentPosition2d).sqrMagnitude > moveThresholdForChunkUpdateSquared;

        private int chunksVisibleInViewDst;

        public float GetSquareOfDistanceFromNearestEdge(Bounds bounds)
        {
            return bounds.SqrDistance(currentPosition2d);
        }

        public float GetDistanceFromNearestEdge(Bounds bounds)
        {
            return Mathf.Sqrt(GetSquareOfDistanceFromNearestEdge(bounds));
        }


        private void Start()
        {
            chunkParent = new GameObject("Terrain").transform;

            textureSettings.ApplyToMaterial(mapMaterial);
            textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

            SetCurrentPosition2d();

            var maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
            chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshSettings.meshWorldSize);

            UpdateVisibleChunks();

            gameObject.GetComponent<Rigidbody>().useGravity = true;
            isReady = true; // TODO
        }

        private void SetCurrentPosition2d()
        {
            currentPosition2d = new Vector2(transform.position.x, transform.position.z);
        }

        private void Update()
        {
            if (!isReady)
            {
                return;
            }

            SetCurrentPosition2d();

            if (IsOnChunkVisibilityEvaluationPosition)
            {
                return;
            }

            foreach (var chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }

            if (ShouldReevaluateChunkVisibility)
            {
                UpdateVisibleChunks();
            }
        }

        void UpdateVisibleChunks()
        {
            chunkVisibilityEvaluationPosition2d = currentPosition2d;

            var alreadyUpdatedChunkCoords = new HashSet<Vector2>();
            for (var i = visibleTerrainChunks.Count - 1; i >= 0; i--)
            {
                alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
                visibleTerrainChunks[i].UpdateTerrainChunk();
            }

            var currentChunkCoordX = Mathf.RoundToInt(currentPosition2d.x / meshSettings.meshWorldSize);
            var currentChunkCoordY = Mathf.RoundToInt(currentPosition2d.y / meshSettings.meshWorldSize);

            for (var yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
            {
                for (var xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
                {
                    var viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                    if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                    {
                        if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                        {
                            terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                        }
                        else
                        {
                            var newChunk = new TerrainChunk(this, viewedChunkCoord, globalSettings, heightMapSettings, meshSettings, detailLevels, colliderLodIndex, mapMaterial);
                            terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                            newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                            newChunk.Load();
                        }
                    }

                }
            }
        }

        private void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
        {
            if (isVisible)
            {
                visibleTerrainChunks.Add(chunk);
            }
            else
            {
                visibleTerrainChunks.Remove(chunk);
            }
        }
    }
}