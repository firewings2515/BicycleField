using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgBalcony : bgComponent
{
    public float height = 2.0f;
    public float width = 3.0f;
    public float extrude = 3.0f;
    public int pillar_count = 10;
    public float floor_thickness = 0.5f;
    public bgBalcony(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter) : base(_input_parameter, _component_parameter, _commands, _commands_parameter)
    {

    }

    public override GameObject build()
    {
        go = new GameObject("Balcony:" + name);
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Diffuse"));
        Mesh mesh = mf.mesh;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        create_floor(ref vertices, ref triangles);
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return go;
    }

    public void create_floor(ref List<Vector3> vertices,ref List<int> triangles) {
        float hwidth = width / 2.0f;
        //down
        vertices.Add(new Vector3(-hwidth, 0, 0));
        vertices.Add(new Vector3(hwidth, 0, 0));
        vertices.Add(new Vector3(hwidth, 0, -extrude));
        vertices.Add(new Vector3(-hwidth, 0, -extrude));
        //up
        vertices.Add(new Vector3(-hwidth, floor_thickness, 0));
        vertices.Add(new Vector3(hwidth, floor_thickness, 0));
        vertices.Add(new Vector3(hwidth, floor_thickness, -extrude));
        vertices.Add(new Vector3(-hwidth, floor_thickness, -extrude));

        int[] tri_bottom =  { 0, 2, 1, 0, 3, 2 };
        int[] tri_top =     { 4, 5, 6, 4, 6, 7 };

        int[] tri_left =    { 0, 4, 7, 0, 7, 3 };
        int[] tri_right =   { 5, 1, 2, 5, 2, 6 };

        int[] tri_front =   { 3, 7, 2, 7, 6, 2 };
        int[] tri_back =    { 4, 0, 5 , 0, 1, 5 };
        triangles.AddRange(tri_bottom);
        triangles.AddRange(tri_top);
        triangles.AddRange(tri_right);
        triangles.AddRange(tri_left);
        triangles.AddRange(tri_front);
        triangles.AddRange(tri_back);
    }
}
