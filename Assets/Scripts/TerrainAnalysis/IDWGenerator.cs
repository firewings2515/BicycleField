using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDWGenerator : MonoBehaviour
{
    public string file_path = "featureAnalyze/features.f";
    public Material terrain_mat;                            // Setting TerrainGenerator: Use the standard material. The vertices are calculated by CPU
    public int terrain_mode = 0;                            // Setting TerrainGenerator: 0 is DEM 1 is IDW
    Queue<int> queue_generate_patch_x = new Queue<int>();   // Patch Queue
    Queue<int> queue_generate_patch_z = new Queue<int>();   // Patch Queue
    bool loop_begin = false;                                // Begin InvokeRepeating("generateTerrainPatch", 0.0f, 0.01666f)
    //public Material heightmap_mat;
    public ComputeShader compute_shader;
    public Texture2D main_tex;
    public Material building_polygon_mat;
    public bool generate;
    public bool mse_analyze;
    public float power;
    public bool clean;
    public int terrain_case;
    public Terrain hill;
    public Terrain cliff;
    public Terrain mountain;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (generate)
        {
            generate = false;
            TerrainGenerator.file_path = file_path;
            TerrainGenerator.terrain_manager = new GameObject("TerrainManager");
            TerrainGenerator.terrain_mat = terrain_mat;
            TerrainGenerator.terrain_mode = terrain_mode;
            //TerrainGenerator.heightmap_mat = heightmap_mat;
            TerrainGenerator.compute_shader = compute_shader;
            TerrainGenerator.main_tex = main_tex;
            TerrainGenerator.building_polygons_manager = new GameObject("BuildingPolygonsManager");
            TerrainGenerator.building_polygon_mat = building_polygon_mat;
            TerrainGenerator.power = power;
            TerrainGenerator.need_mse = mse_analyze;
            if (terrain_case == 0)
                TerrainGenerator.origin_terrain = hill;
            else if (terrain_case == 1)
                TerrainGenerator.origin_terrain = cliff;
            else
                TerrainGenerator.origin_terrain = mountain;
            TerrainGenerator.loadTerrain();
            int x_index = 0;
            int z_index = 0;
            TerrainGenerator.is_loaded[x_index * TerrainGenerator.z_patch_num + z_index] = true;
            int x_piece_num = PublicOutputInfo.piece_num;
            int z_piece_num = PublicOutputInfo.piece_num;
            StartCoroutine(TerrainGenerator.generateTerrainPatchTex(x_index, z_index, x_piece_num, z_piece_num));
        }

        if (clean)
        {
            clean = false;
            GameObject.DestroyImmediate(GameObject.Find("TerrainManager"));
            GameObject.DestroyImmediate(GameObject.Find("BuildingPolygonsManager"));
            GameObject.DestroyImmediate(GameObject.Find("ConstraintsCameraManager"));
            GameObject.DestroyImmediate(GameObject.Find("feature_manager"));
        }
    }
}