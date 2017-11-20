using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Pamux.Lib.Procedural.Utilities
{
    [CustomEditor(typeof(MapPreview))]
    public class MapPreviewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var mapPreview = (MapPreview)target;

            if (DrawDefaultInspector())
            {
                if (mapPreview.autoUpdate)
                {
                    mapPreview.DrawMapInEditor();
                }
            }

            if (GUILayout.Button("Generate"))
            {
                mapPreview.DrawMapInEditor();
            }
        }
    }
}