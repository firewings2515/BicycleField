using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ComputeHightmap : MonoBehaviour
{
    public Material material;
    public ComputeShader compute_shader;
    public RenderTexture tex;
    public RenderTexture constraints_tex;
    public Texture2D main_tex;
    public bool calc;
    public bool show_feature;
    public bool set_cube_to_height;
    public bool test_subdivision;
    WVec3[] features;
    public GameObject features_manager;
    public GameObject features_prefab;
    public GameObject test_cube;
    Texture2D heightmap;
    Texture2D constraintsmap;
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

        if (test_subdivision)
        {
            test_subdivision = false;
            //TerrainGenerator.heightmap_mat = material;
            TerrainGenerator.compute_shader = compute_shader;
            TerrainGenerator.main_tex = main_tex;
            TerrainGenerator.loadTerrain();
            testSubdivision();
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
        compute_shader.SetTexture(kernelHandler, "Constraintsmap", constraints_tex);
        compute_shader.SetVectorArray("features", area_features);
        compute_shader.SetInt("features_count", area_features.Length);
        compute_shader.SetVectorArray("constraints", area_constraints.ToArray());
        compute_shader.SetInt("constraints_count", area_constraints.Count);
        compute_shader.SetFloat("x", -8.0f);
        compute_shader.SetFloat("z", -8.0f);
        compute_shader.SetFloat("resolution", PublicOutputInfo.patch_length / PublicOutputInfo.tex_size); // patch_length / tex_length
        compute_shader.Dispatch(kernelHandler, PublicOutputInfo.tex_size / 8, PublicOutputInfo.tex_size / 8, 1);

        heightmap = new Texture2D(PublicOutputInfo.tex_size, PublicOutputInfo.tex_size, TextureFormat.RGB24, false);
        Rect rectReadPicture = new Rect(0, 0, PublicOutputInfo.tex_size, PublicOutputInfo.tex_size);
        RenderTexture.active = tex;
        // Read pixels
        heightmap.ReadPixels(rectReadPicture, 0, 0);
        heightmap.Apply();
        RenderTexture.active = null; // added to avoid errors 

        constraintsmap = new Texture2D(PublicOutputInfo.tex_size, PublicOutputInfo.tex_size, TextureFormat.RGB24, false);
        RenderTexture.active = constraints_tex;
        // Read pixels
        constraintsmap.ReadPixels(rectReadPicture, 0, 0);
        constraintsmap.Apply();
        RenderTexture.active = null; // added to avoid errors 
    }

    void testSubdivision()
    {
        int x_index = 13;
        int z_index = 7;
        for (int i = -5; i <= 5; i++)
        {
            for (int j = -5; j <= 5; j++)
            {
                if (!TerrainGenerator.is_generated[(x_index + i) * TerrainGenerator.z_patch_num + (z_index + j)])
                {
                    TerrainGenerator.is_generated[(x_index + i) * TerrainGenerator.z_patch_num + (z_index + j)] = true;
                    int x_piece_num = 64;
                    int z_piece_num = 64;
                    PublicOutputInfo.piece_length = 2;
                    StartCoroutine(TerrainGenerator.generateTerrainPatchWithTex(x_index + i, z_index + j, x_piece_num, z_piece_num));
                }
            }
        }
    }
}