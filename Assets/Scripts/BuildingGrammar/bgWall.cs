using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgWall : bgComponent
{
    List<Vector3> positions;
    string dir;
    string height_type; // const/ratio
    float height_parameter_val = 10.0f;//from component parameter
    public float height = 10.0f; //may modify
    public float width = 4.0f; //come from upper level
    
    public bgWall(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter) : base(_input_parameter, _component_parameter, _commands, _commands_parameter)
    {
        parse();
        positions = new List<Vector3>();
    }

    public override void parse()
    {
        dir = component_parameter[0];
        height_type = component_parameter[1];
        height_parameter_val = float.Parse(component_parameter[2]);

    }
    public override GameObject build()
    {

        Debug.Log("type: Wall");
        if (go != null)
        {
            go = GameObject.Instantiate(go);
            go.transform.localScale = new Vector3(width, height, 0.5f);
            go.name = "Wall:" + name;
            int i = 0;
            foreach(Transform child in go.transform) {
                child.localPosition = positions[i++];
            }
            
        }
        else {
            if (height_type == "const")
            {
                height = height_parameter_val;
            }
            else if (height_type == "ratio")
            {
                height = width * height_parameter_val;
            }

            go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "Wall:" + name;

            MeshRenderer mr = go.GetComponent<MeshRenderer>();

            string background_name = commands[0];
            Debug.Log(background_name);
            //bgAsset background = builder.get_asset(background_name);
            Material mat = Resources.Load<Material>("Material/tillable");

            Material newmat = new Material(mat);
            mr.sharedMaterial = newmat;
            go.transform.localScale = new Vector3(width, height, 0.5f);
            Vector3 pos = new Vector3();
            pos.z = -0.55f;
            for (int i = 1; i < commands.Count; i++)
            {
                if (commands[i] == "pos")
                {
                    pos.x = (float.Parse(commands_parameter[i][0]) - 0.5f);// * width;
                    pos.y = (float.Parse(commands_parameter[i][1]) - 0.5f);// * height;
                    positions.Add(pos);
                }
                else
                {
                    bgAsset asset = builder.get_asset(commands[i]);
                    GameObject obj = asset.build();
                    obj.transform.parent = go.transform;
                    obj.transform.localPosition = pos;
                }
            }
            
        }
        /*
        float hheight = height / 2.0f;
        float hwidth = width / 2.0f;

        Vector3[] vertices = {
            new Vector3(-hwidth,0,0),
            new Vector3(hwidth,0,0),
            new Vector3(hwidth,height,0),
            new Vector3(-hwidth,height,0),
        };
        //°f®É°w
        //int[] triangles = {
        //    0,1,3,
        //    1,2,3
        //};
        //¶¶®É°w
        int[] triangles = {
            0,3,1,
            1,3,2
        };
        Vector2[] uv = new Vector2[]
{
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1),
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        MeshFilter mf = go.AddComponent<MeshFilter>();        
        mf.mesh = mesh;
        */
        Debug.Log(height);
        
        return go;
    }
}
