using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;

public class PolygonPlane
{
    public static GameObject create(List<Vector2> points)
    {
        GameObject go = new GameObject();
        go.name = "Cross";
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshCollider mc = go.AddComponent<MeshCollider>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        //mr.material = new Material(Shader.Find("Standard"));
        mr.material = Resources.Load<Material>("Material/double_side_mesh");
        List<List<Vector2>> holes = new List<List<Vector2>>();
        List<int> indices = null;
        List<Vector3> vertices = null;

        Triangulation.triangulate(points, holes, 0.0f, out indices, out vertices);
        Mesh mesh = mf.mesh;
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();

        go.GetComponent<MeshCollider>().sharedMesh = mesh;

        Vector2[] uvs = new Vector2[mesh.vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].y);
        }
        mesh.uv = uvs;

        return go;
    }
}
