using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class bgAsset : bgComponent
{
    public string asset_type;
    public string location;
    public (float, float) scale = (1.0f,1.0f);
    public float extrude = 0.0f;

    public float width;
    public float height;
    public RenderTexture image;

    public bgAsset(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter):base(_input_parameter, _component_parameter, _commands, _commands_parameter)
    {
        parse();
        load();
    }
    public void load()
    {
        byte[] bytes = File.ReadAllBytes(location);
        if (asset_type == "image")
        {
            Texture2D tex2d = new Texture2D(2, 2);
            tex2d.LoadImage(bytes);
            //Debug.Log(tex2d.width);
            //Debug.Log(tex2d.height);
            image = new RenderTexture(tex2d.width, tex2d.height, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            Graphics.Blit(tex2d, image);

            float divide = 200.0f;
            width = image.width / divide;
            height = image.height / divide;
        }
        else if (asset_type == "model")
        {
            //TODO
        }
    }
    public override GameObject build() 
    {
        //Debug.Log("type: Asset");
        if (go != null)
        {
            go = GameObject.Instantiate(go);
            go.name = "Asset:" + name;
            //go.transform.localScale = new Vector3(width, height, 1.0f);
        }
        else
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "Asset:" + name;
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            mr.material.mainTexture = image;
            go.transform.localScale = new Vector3(width*scale.Item1,height*scale.Item2, 1.0f);
        }

        return go;
    }
    public override Mesh build_mesh()
    {
        //only for image
        //if (share_mesh != null) return share_mesh;
        share_mesh = new Mesh();
        share_mesh.name = "asset_mesh";
        float hwidth = width / 2.0f;
        float hheight = height / 2.0f;

        vertices = new List<Vector3> {
            new Vector3(center.x-hwidth,center.y-hheight,center.z),
            new Vector3(center.x+hwidth,center.y-hheight,center.z),
            new Vector3(center.x+hwidth,center.y+hheight,center.z),
            new Vector3(center.x-hwidth,center.y+hheight,center.z)
        };
        triangles = new List<int> {
            vertice_index + 0,vertice_index + 3,vertice_index + 1,
            vertice_index + 1,vertice_index + 3,vertice_index + 2
        };
        
        return null;
    }

    public override void parse()
    {
        asset_type = component_parameter[0];
        for (int i = 0; i < commands.Count; i++) {
            if (commands[i] == "Location")
            {
                location = commands_parameter[i][0];
            }
            else if (commands[i] == "Scale")
            {
                scale.Item1 *= float.Parse(commands_parameter[i][0]);
                scale.Item2 *= float.Parse(commands_parameter[i][1]);
            }
            else if (commands[i] == "Extrude")
            {
                extrude += float.Parse(commands_parameter[i][0]);
            }
            else 
            { 
                   //error
                   //should not go to here
            }
        }
    }
}
