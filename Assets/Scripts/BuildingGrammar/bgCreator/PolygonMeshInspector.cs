using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(PolygonMesh2D))]
public class PolygonMeshInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PolygonMesh2D polygon = (PolygonMesh2D) target;
        if (GUILayout.Button("reset"))
        {
            DestroyImmediate(polygon.gameObject);
            GameObject obj = new GameObject();
            polygon = obj.AddComponent<PolygonMesh2D>();
        }

        if (GUILayout.Button("get polygon")) {
            Vector2[] points = polygon.get_points();
            for (int i = 0; i < points.Length; i++) {
                Debug.Log(points[i]);
            }
        }
    }
}
