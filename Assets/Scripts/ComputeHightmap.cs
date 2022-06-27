using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ComputeHightmap : MonoBehaviour
{
    public Material material;
    public ComputeShader compute_shader;
    public RenderTexture tex;
    public Texture2D main_tex;
    public bool calc;
    public bool show_feature;
    public bool set_cube_to_height;
    WVec3[] features;
    public GameObject features_manager;
    public GameObject features_prefab;
    public GameObject test_cube;
    Texture2D heightmap;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (calc)
        {
            calc = false;
            dispatchComputeShader();
        }

        if (show_feature)
        {
            show_feature = false;
            TerrainGenerator.showPoint(features, "features", features_manager.transform, features_prefab, 8.0f);
        }

        if (set_cube_to_height)
        {
            set_cube_to_height = false;
            float height = heightmap.GetPixel(384, 384).r * 3000;
            Debug.Log("set test cube's height to " + height.ToString());
            test_cube.transform.position = new Vector3(test_cube.transform.position.x, height, test_cube.transform.position.z);
        }
    }

    void dispatchComputeShader()
    {
        TerrainGenerator.loadTerrain();
        // -328, -328, 440, 440
        int[] area_features_index = TerrainGenerator.kdtree.getAreaPoints(-328, -328, 440, 440);
        Vector4[] area_features = new Vector4[area_features_index.Length];
        features = new WVec3[area_features_index.Length];
        List<Vector4> area_constraints = new List<Vector4>();
        for (int area_features_index_index = 0; area_features_index_index < area_features_index.Length; area_features_index_index++)
        {
            WVec3 feature = TerrainGenerator.kdtree.nodes[area_features_index[area_features_index_index]];
            area_features[area_features_index_index] = new Vector4(feature.x, feature.y, feature.z, feature.w);
            features[area_features_index_index] = feature;
            if (feature.w > -1)
                area_constraints.Add(area_features[area_features_index_index]);
        }
        area_constraints.Sort(delegate(Vector4 a, Vector4 b)
        {
            return a.w.CompareTo(b.w);
        });

        tex = new RenderTexture(768, 768, 24);
        tex.enableRandomWrite = true;
        tex.Create();
        material.SetTexture("_MainTex", tex);
        //material.SetTexture("_MainTex", tex);
        material.SetTexture("_HeightmapTex", tex);
        
        int kernelHandler = compute_shader.FindKernel("CSMain");
        compute_shader.SetTexture(kernelHandler, "Result", tex);
        compute_shader.SetVectorArray("features", area_features);
        compute_shader.SetInt("features_count", area_features.Length);
        compute_shader.SetVectorArray("constraints", area_constraints.ToArray());
        compute_shader.SetInt("constraints_count", area_constraints.Count);
        compute_shader.SetFloat("x", -328.0f);
        compute_shader.SetFloat("z", -328.0f);
        compute_shader.SetFloat("patch_length", 768.0f);
        compute_shader.Dispatch(kernelHandler, 768 / 8, 768 / 8, 1);

        heightmap = new Texture2D(768, 768, TextureFormat.RGB24, false);
        Rect rectReadPicture = new Rect(0, 0, 768, 768);
        RenderTexture.active = tex;
        // Read pixels
        heightmap.ReadPixels(rectReadPicture, 0, 0);
        heightmap.Apply();
        RenderTexture.active = null; // added to avoid errors 
    }
}