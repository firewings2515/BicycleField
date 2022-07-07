using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public Material terrain_mat;                            // Setting TerrainGenerator: Use the standard material. The vertices are calculated by CPU
    public Material terrain_idw_mat;                        // Setting TerrainGenerator: Use the material with IDW shader
    public Material terrain_nni_mat;                        // Setting TerrainGenerator: Use the material with NNI shader
    public int terrain_mode = 0;                            // Setting TerrainGenerator: 0 is DEM 1 is IDW
    public GameObject feature_ball_prefab;
    Queue<int> queue_generate_patch_x = new Queue<int>();   // Patch Queue
    Queue<int> queue_generate_patch_z = new Queue<int>();   // Patch Queue
    bool loop_begin = false;                                // Begin InvokeRepeating("generateTerrainPatch", 0.0f, 0.01666f)
    //public Material heightmap_mat;
    public ComputeShader compute_shader;
    public Texture2D main_tex;

    // Start is called before the first frame update
    void Start()
    {
        GameObject terrain_manager = new GameObject("TerrainManager");
        TerrainGenerator.terrain_manager = terrain_manager;
        TerrainGenerator.terrain_mat = terrain_mat;
        TerrainGenerator.terrain_idw_mat = terrain_idw_mat;
        TerrainGenerator.terrain_nni_mat = terrain_nni_mat;
        TerrainGenerator.terrain_mode = terrain_mode;
        TerrainGenerator.feature_ball_prefab = feature_ball_prefab;
        //TerrainGenerator.heightmap_mat = heightmap_mat;
        TerrainGenerator.compute_shader = compute_shader;
        TerrainGenerator.main_tex = main_tex;
        TerrainGenerator.loadTerrain();
    }

    // Update is called once per frame
    void Update()
    {
        if (TerrainGenerator.is_initial && !loop_begin)
        {
            loop_begin = true;

            // Pop all Loading Queue 
            while (TerrainGenerator.loading_vec3s.Count > 0)
            {
                Vector3 loading_vec3 = TerrainGenerator.loading_vec3s.Dequeue();
                TerrainGenerator.generateTerrain(loading_vec3);
            }

            InvokeRepeating("generateTerrainPatch", 0.0f, 0.01666f);
        }

        if (TerrainGenerator.need_update)
        {
            TerrainGenerator.need_update = false;
            queuePatchesInView();
        }
    }

    /// <summary>
    /// Find patches in view.
    /// </summary>
    void queuePatchesInView()
    {
        while (TerrainGenerator.queue_patch_x_index.Count > 0)
        {
            int patch_x_index = TerrainGenerator.queue_patch_x_index.Peek();
            int patch_z_index = TerrainGenerator.queue_patch_z_index.Peek();
            for (int i = -TerrainGenerator.vision_patch_num; i <= TerrainGenerator.vision_patch_num; i++)
            {
                for (int j = -TerrainGenerator.vision_patch_num; j <= TerrainGenerator.vision_patch_num; j++)
                {
                    if (Mathf.Abs(i) + Mathf.Abs(j) > TerrainGenerator.vision_patch_num)
                        continue;
                    int x_index = patch_x_index + i;
                    int z_index = patch_z_index + j;
                    if (x_index < 0 || x_index >= TerrainGenerator.x_patch_num || z_index < 0 || z_index >= TerrainGenerator.z_patch_num)
                        continue;
                    queue_generate_patch_x.Enqueue(x_index);
                    queue_generate_patch_z.Enqueue(z_index);
                }
            }
            TerrainGenerator.queue_patch_x_index.Dequeue();
            TerrainGenerator.queue_patch_z_index.Dequeue();
        }
    }

    /// <summary>
    /// Generate patch in Patches Queue.
    /// </summary>
    void generateTerrainPatch()
    {
        while (queue_generate_patch_x.Count > 0)
        {
            int x_index = queue_generate_patch_x.Dequeue();
            int z_index = queue_generate_patch_z.Dequeue();
            TerrainGenerator.trigger_num_in_view[x_index * TerrainGenerator.z_patch_num + z_index]++;
            if (!TerrainGenerator.is_loaded[x_index * TerrainGenerator.z_patch_num + z_index])
            {
                TerrainGenerator.is_loaded[x_index * TerrainGenerator.z_patch_num + z_index] = true;
                int x_piece_num = PublicOutputInfo.piece_num;
                int z_piece_num = PublicOutputInfo.piece_num;
                if (x_index == TerrainGenerator.x_patch_num - 1)
                    x_piece_num = Mathf.FloorToInt((TerrainGenerator.max_x - (TerrainGenerator.min_x + x_index * PublicOutputInfo.patch_length)) / PublicOutputInfo.piece_length);
                if (z_index == TerrainGenerator.z_patch_num - 1)
                    z_piece_num = Mathf.FloorToInt((TerrainGenerator.max_z - (TerrainGenerator.min_z + z_index * PublicOutputInfo.patch_length)) / PublicOutputInfo.piece_length);
                //if (x_index + TerrainGenerator.piece_num > TerrainGenerator.x_patch_num)
                //    x_piece_num = TerrainGenerator.x_patch_num - x_index;
                //if (z_index + TerrainGenerator.piece_num > TerrainGenerator.z_patch_num)
                //    z_piece_num = TerrainGenerator.z_patch_num - z_index;
                StartCoroutine(TerrainGenerator.generateTerrainPatchTex(x_index, z_index, x_piece_num, z_piece_num));
                break;
            }
        }

        TerrainGenerator.is_queue_generate_patch_empty = (queue_generate_patch_x.Count == 0);
    }
}