using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TriLibCore;
using TriLibCore.General;
public class bgAsset : bgComponent
{
    public string asset_type;
    public string location;
    string file_extension = "";
    public (float, float,float) scale = (1.0f,1.0f,1.0f);
    public float extrude = 0.0f;

    public float width;
    public float height;
    public Texture2D image;
    public Material mat;
    AssetLoaderContext xx;
    //使用RenderTexture會導致shader無法使用
    public bgAsset(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter):base(_input_parameter, _component_parameter, _commands, _commands_parameter)
    {
        parse();
        load();
    }
    public void load()
    {
        
        if (asset_type == "image")
        {
            byte[] bytes = File.ReadAllBytes(location);
            image = new Texture2D(2, 2);
            image.LoadImage(bytes);
            image.Apply();
            
            //Debug.Log(tex2d.width);
            //Debug.Log(tex2d.height);

            //使用RenderTexture會導致shader無法使用
            //image = new RenderTexture(tex2d.width, tex2d.height, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            //Graphics.Blit(tex2d, image);

            float divide = 150.0f;
            width = image.width / divide;
            height = image.height / divide;
            mat = new Material(Shader.Find("Diffuse - Worldspace"));
            mat.SetTexture("_MainTex",image);
        }
        else if (asset_type == "model")
        {
            //TODO
            
            for (int i = location.Length - 1; i >= 0; i--) {
                if (location[i] == '.') break;
                file_extension = location[i] + file_extension;
            }
            //Debug.Log("ex:" + file_extension);
            if (file_extension == "obj")
            {
                go = new Dummiesman.OBJLoader().Load(location);
                go.SetActive(false);
            }
            else if (file_extension == "fbx")
            {
                //go = AssetLoader.LoadModelFromFile(location).RootGameObject;
                var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
                xx = AssetLoader.LoadModelFromFileNoThread(location,OnError, null, assetLoaderOptions);
                //fbx_importer = new FBXImporter();
                //go = fbx_importer.ParseFBX(location);
                go = xx.RootGameObject;
            }

        }
    }
    public override GameObject build() 
    {
        if (go != null)
        {
            go = GameObject.Instantiate(go);
            go.name = "Asset:" + name;
            //go.transform.localScale = new Vector3(width, height, 1.0f);
            
            if (asset_type == "model")
            {
                go.SetActive(true);
                go.transform.localScale = new Vector3(scale.Item1, scale.Item2, scale.Item3);
                go.transform.localRotation = rotate;
            }
        }
        else
        {


            if (asset_type == "image")
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                GameObject.Destroy(go.GetComponent<MeshCollider>());
                MeshRenderer mr = go.GetComponent<MeshRenderer>();
                mr.material.mainTexture = image;
                go.transform.localScale = new Vector3(width * scale.Item1, height * scale.Item2, 1.0f);
            }
            else if (asset_type == "model")
            {
                if (file_extension == "obj")
                {
                    go = new Dummiesman.OBJLoader().Load(location);

                }
                else if (file_extension == "fbx")
                {
                    //fbx_importer = new FBXImporter();
                    //go = fbx_importer.ParseFBX(location);
                    var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
                    xx = AssetLoader.LoadModelFromFileNoThread(location, OnError, null, assetLoaderOptions);
                    go = xx.RootGameObject;
                }
                go.transform.localScale = new Vector3(scale.Item1, scale.Item2, scale.Item3);
                go.transform.localRotation = rotate;
            }
            go.name = "Asset:" + name;
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
                if (commands_parameter[i].Count > 2) {
                    scale.Item3 *= float.Parse(commands_parameter[i][2]);
                }
            }
            else if (commands[i] == "Extrude")
            {
                extrude += float.Parse(commands_parameter[i][0]);
            }
            else if (commands[i] == "Rotate")
            {
                float rx = float.Parse(commands_parameter[i][0]);
                float ry = float.Parse(commands_parameter[i][1]);
                float rz = float.Parse(commands_parameter[i][2]);
                rotate = Quaternion.Euler(rx,ry,rz);
            }
            else 
            { 
                   //error
                   //should not go to here
            }
        }
    }






    /// <summary>
    /// Called when any error occurs.
    /// </summary>
    /// <param name="obj">The contextualized error, containing the original exception and the context passed to the method where the error was thrown.</param>
    private void OnError(IContextualizedError obj)
    {
        Debug.LogError($"An error occurred while loading your Model: {obj.GetInnerException()}");
    }

    /// <summary>
    /// Called when the Model loading progress changes.
    /// </summary>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    /// <param name="progress">The loading progress.</param>
    private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
    {
        Debug.Log($"Loading Model. Progress: {progress:P}");
    }

    /// <summary>
    /// Called when the Model (including Textures and Materials) has been fully loaded.
    /// </summary>
    /// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Materials loaded. Model fully loaded.");
    }

    /// <summary>
    /// Called when the Model Meshes and hierarchy are loaded.
    /// </summary>
    /// <remarks>The loaded GameObject is available on the assetLoaderContext.RootGameObject field.</remarks>
    /// <param name="assetLoaderContext">The context used to load the Model.</param>
    private void OnLoad(AssetLoaderContext assetLoaderContext)
    {
        Debug.Log("Model loaded. Loading materials.");
    }
}
