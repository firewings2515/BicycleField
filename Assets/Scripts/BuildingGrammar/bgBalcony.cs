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
    public float railing_thickness = 0.3f;
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
        create_pillar(ref vertices, ref triangles,new Vector3(1,0,1));
        create_railings(ref vertices, ref triangles);
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return go;
    }

    public void create_floor(ref List<Vector3> vertices,ref List<int> triangles) {
        float hwidth = width / 2.0f;
        int base_index = vertices.Count;
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

        int[] tri_bottom =  { 0, 2, 1, 0, 3, 2 }; add_base_index(ref tri_bottom, base_index);
        int[] tri_top =     { 4, 5, 6, 4, 6, 7 }; add_base_index(ref tri_top, base_index);

        int[] tri_left =    { 0, 4, 7, 0, 7, 3 }; add_base_index(ref tri_left, base_index);
        int[] tri_right =   { 5, 1, 2, 5, 2, 6 }; add_base_index(ref tri_right, base_index);

        int[] tri_front =   { 3, 7, 2, 7, 6, 2 }; add_base_index(ref tri_front, base_index);
        int[] tri_back =    { 4, 0, 5 , 0, 1, 5 }; add_base_index(ref tri_back, base_index);
        triangles.AddRange(tri_bottom);
        triangles.AddRange(tri_top);
        triangles.AddRange(tri_right);
        triangles.AddRange(tri_left);
        triangles.AddRange(tri_front);
        triangles.AddRange(tri_back);
    }

    public void create_pillar(ref List<Vector3> vertices, ref List<int> triangles,Vector3 center)
    {
        float hwidth = 0.1f;
        int base_index = vertices.Count;
        //down
        vertices.Add(new Vector3(center.x- hwidth, center.y, center.z + hwidth));
        vertices.Add(new Vector3(center.x + hwidth, center.y, center.z + hwidth));
        vertices.Add(new Vector3(center.x + hwidth, center.y, center.z - hwidth));
        vertices.Add(new Vector3(center.x- hwidth, center.y, center.z - hwidth));
        //up
        vertices.Add(new Vector3(center.x - hwidth, center.y+height, center.z + hwidth));
        vertices.Add(new Vector3(center.x + hwidth, center.y + height, center.z + hwidth));
        vertices.Add(new Vector3(center.x + hwidth, center.y + height, center.z - hwidth));
        vertices.Add(new Vector3(center.x - hwidth, center.y + height, center.z - hwidth));


        int[] tri_left = { 0, 4, 7, 0, 7, 3 }; add_base_index(ref tri_left, base_index);
        int[] tri_right = { 5, 1, 2, 5, 2, 6 }; add_base_index(ref tri_right, base_index);

        int[] tri_front = { 3, 7, 2, 7, 6, 2 }; add_base_index(ref tri_front, base_index);
        int[] tri_back = { 4, 0, 5, 0, 1, 5 }; add_base_index(ref tri_back, base_index);

        triangles.AddRange(tri_right);
        triangles.AddRange(tri_left);
        triangles.AddRange(tri_front);
        triangles.AddRange(tri_back);
    }

    public void create_railings(ref List<Vector3> vertices, ref List<int> triangles) {
        float hwidth = width / 2.0f;
        float hextrude = extrude / 2.0f;
        create_railing(ref vertices, ref triangles, new Vector3(-hwidth, height, -hextrude), false);
        create_railing(ref vertices, ref triangles, new Vector3(hwidth, height, -hextrude), false);
        create_railing(ref vertices, ref triangles, new Vector3(0, height, -extrude), true);
    }

    public void create_railing(ref List<Vector3> vertices, ref List<int> triangles, Vector3 center, bool front) {
        int base_index = vertices.Count;

        if (!front)
        {
            float hextrude = extrude / 2.0f;
            float hthickness = railing_thickness / 2.0f;
            //down
            vertices.Add(new Vector3(center.x - hthickness, center.y, center.z + hextrude));
            vertices.Add(new Vector3(center.x + hthickness, center.y, center.z + hextrude));
            vertices.Add(new Vector3(center.x + hthickness, center.y, center.z - hextrude + hthickness));
            vertices.Add(new Vector3(center.x - hthickness, center.y, center.z - hextrude + hthickness));
            //up
            vertices.Add(new Vector3(center.x - hthickness, center.y + railing_thickness, center.z + hextrude));
            vertices.Add(new Vector3(center.x + hthickness, center.y + railing_thickness, center.z + hextrude));
            vertices.Add(new Vector3(center.x + hthickness, center.y + railing_thickness, center.z - hextrude + hthickness));
            vertices.Add(new Vector3(center.x - hthickness, center.y + railing_thickness, center.z - hextrude + hthickness));
        }
        else {
            float hwidth = width / 2.0f;
            float hthickness = railing_thickness / 2.0f;
            //down
            vertices.Add(new Vector3(center.x - hwidth - hthickness, center.y, center.z + hthickness));
            vertices.Add(new Vector3(center.x + hwidth + hthickness, center.y, center.z + hthickness));
            vertices.Add(new Vector3(center.x + hwidth + hthickness, center.y, center.z - hthickness));
            vertices.Add(new Vector3(center.x - hwidth - hthickness, center.y, center.z - hthickness));
            //up
            vertices.Add(new Vector3(center.x - hwidth - hthickness, center.y + railing_thickness, center.z + hthickness));
            vertices.Add(new Vector3(center.x + hwidth + hthickness, center.y + railing_thickness, center.z + hthickness));
            vertices.Add(new Vector3(center.x + hwidth + hthickness, center.y + railing_thickness, center.z - hthickness));
            vertices.Add(new Vector3(center.x - hwidth - hthickness, center.y + railing_thickness, center.z - hthickness));
        }
        int[] tri_bottom = { 0, 2, 1, 0, 3, 2 }; add_base_index(ref tri_bottom, base_index);
        int[] tri_top = { 4, 5, 6, 4, 6, 7 }; add_base_index(ref tri_top, base_index);

        int[] tri_left = { 0, 4, 7, 0, 7, 3 }; add_base_index(ref tri_left, base_index);
        int[] tri_right = { 5, 1, 2, 5, 2, 6 }; add_base_index(ref tri_right, base_index);

        int[] tri_front = { 3, 7, 2, 7, 6, 2 }; add_base_index(ref tri_front, base_index);
        int[] tri_back = { 4, 0, 5, 0, 1, 5 }; add_base_index(ref tri_back, base_index);

        if (!front)
        {
            triangles.AddRange(tri_bottom);
            triangles.AddRange(tri_top);
            triangles.AddRange(tri_right);
            triangles.AddRange(tri_left);
        }
        else {
            triangles.AddRange(tri_bottom);
            triangles.AddRange(tri_top);
            triangles.AddRange(tri_right);
            triangles.AddRange(tri_left);
            triangles.AddRange(tri_front);
            triangles.AddRange(tri_back);
        }

    }


    public void add_base_index(ref int[] arr,int base_index) {
        for (int i = 0; i < arr.Length; i++) {
            arr[i] += base_index;
        }
    }
}
