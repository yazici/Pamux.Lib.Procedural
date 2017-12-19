using UnityEngine;
using System.Collections;
using Pamux.Lib.Procedural.Models;
using Pamux.Lib.Procedural.Generators;
using Pamux.Lib.Procedural.Enums;

namespace Pamux.Lib.Procedural.Utilities
{
    public class MapPreview : MonoBehaviour
    {
        public Renderer textureRender;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public DrawModes drawMode;

        public MeshSettings meshSettings;
        public GlobalSettings globalSettings;
        public HeightMapSettings heightMapSettings;
        public TextureSettings textureData;

        public Material terrainMaterial;

        [Range(0, MeshSettings.numSupportedLods - 1)]
        public int editorPreviewLod;
        public bool autoUpdate;

        public void DrawMapInEditor()
        {
            textureData.ApplyToMaterial(terrainMaterial);
            textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

            globalSettings.Initialize();

            var heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, globalSettings, heightMapSettings, Vector2.zero);

            if (drawMode == DrawModes.NoiseMap)
            {
                DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap));
            }
            else if (drawMode == DrawModes.Mesh)
            {
                DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLod));
            }
            else if (drawMode == DrawModes.FalloffMap)
            {
                DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine), 0, 1)));
            }
        }

        public void DrawTexture(Texture2D texture)
        {
            textureRender.sharedMaterial.mainTexture = texture;
            textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height) / 10f;

            textureRender.gameObject.SetActive(true);
            meshFilter.gameObject.SetActive(false);
        }

        public void DrawMesh(MeshData meshData)
        {
            meshFilter.sharedMesh = meshData.CreateMesh();

            textureRender.gameObject.SetActive(false);
            meshFilter.gameObject.SetActive(true);
        }

        void OnValuesUpdated()
        {
            if (!Application.isPlaying)
            {
                DrawMapInEditor();
            }
        }

        void OnTextureValuesUpdated()
        {
            textureData.ApplyToMaterial(terrainMaterial);
        }

        void OnValidate()
        {
            if (meshSettings != null)
            {
                meshSettings.OnValuesUpdated -= OnValuesUpdated;
                meshSettings.OnValuesUpdated += OnValuesUpdated;
            }
            if (heightMapSettings != null)
            {
                heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
                heightMapSettings.OnValuesUpdated += OnValuesUpdated;
            }
            if (textureData != null)
            {
                textureData.OnValuesUpdated -= OnTextureValuesUpdated;
                textureData.OnValuesUpdated += OnTextureValuesUpdated;
            }
        }
    }
}