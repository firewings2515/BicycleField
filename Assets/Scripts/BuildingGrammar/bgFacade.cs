using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgFacade : bgComponent
{
    public float height = 0.0f;
    public float width = 4.0f; //come from upper level
    List<float> widths;
    List<Vector3> positions;
    public bgFacade(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter) : base(_input_parameter, _component_parameter, _commands, _commands_parameter)
    {
        positions = new List<Vector3>();
        widths = new List<float>();

    }

    public override GameObject build()
    {
        go = new GameObject("Facade:" + name);
        height = 0.0f;
        for (int i = 0; i < commands.Count; i++)
        {
            if (commands[i] == "empty") {
                height += float.Parse(commands_parameter[i][2]);
                create_floor_split();
                continue;
            }
            bgWall wall = builder.get_wall(commands[i]);
            wall.width = this.width;
            
            wall.random_background = this.random_background;
            widths.Add(width);
            GameObject obj = wall.build();
            height += wall.height / 2.0f;
            obj.transform.localPosition = new Vector3(0, height, 0);
            positions.Add(new Vector3(0, height, 0));
            height += wall.height / 2.0f;

            create_floor_split();
            //floor_split.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
            //floor_split.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", new Color(133, 94, 66));
            obj.transform.parent = go.transform;

        }

        return go;
    }

    void create_floor_split() {
        GameObject floor_split = GameObject.CreatePrimitive(PrimitiveType.Quad);
        GameObject.Destroy(floor_split.GetComponent<MeshCollider>());
        floor_split.transform.localScale = new Vector3(width, 0.1f, 1.0f);
        floor_split.transform.localPosition = new Vector3(0, height, -0.01f);
        floor_split.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/brown");
        floor_split.transform.parent = go.transform;
    }

    public override Mesh build_mesh()
    {
        share_mesh = new Mesh();
        share_mesh.name = "facade_mesh";
        
        vertices = new List<Vector3>();
        vertices.Clear();
        triangles = new List<int>();
        triangles.Clear();
        float height = 0.0f;
        for (int i = 0; i < commands.Count; i++)
        {
            bgWall wall = builder.get_wall(commands[i]);
            wall.width = this.width;
            widths.Add(width);

            wall.center = center + new Vector3(0,height,0);
            wall.vertice_index = vertices.Count;

            wall.build_mesh();
            height += wall.height;

            vertices.AddRange(wall.vertices);
            triangles.AddRange(wall.triangles);
        }

        share_mesh.vertices = vertices.ToArray();
        share_mesh.triangles = triangles.ToArray();
        return share_mesh;
    }
}
