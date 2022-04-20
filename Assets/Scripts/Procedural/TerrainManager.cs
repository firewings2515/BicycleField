using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public Material terrain_idw_mat;                        // Setting TerrainGenerator: Use the standard material. The vertices are calculated by CPU
    public Material terrain_mat;                            // Setting TerrainGenerator: Use the material with IDW shader
    public int terrain_mode = 0;                            // Setting TerrainGenerator: 0 is DEM 1 is IDW
    //public GameObject feature_ball_prefab;
    Queue<int> queue_generate_patch_x = new Queue<int>();   // Patch Queue
    Queue<int> queue_generate_patch_z = new Queue<int>();   // Patch Queue
    bool loop_begin = false;                                // Begin InvokeRepeating("generateTerrainPatch", 0.0f, 0.01666f)

    // Start is called before the first frame update
    void Start()
    {
        TerrainGenerator.terrain_mat = terrain_mat;
        TerrainGenerator.terrain_idw_mat = terrain_idw_mat;
        TerrainGenerator.terrain_mode = terrain_mode;
        //TerrainGenerator.feature_ball_prefab = feature_ball_prefab;
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
                    int x_index = patch_x_index + i * TerrainGenerator.piece_num;
                    int z_index = patch_z_index + j * TerrainGenerator.piece_num;
                    if (x_index < 0 || x_index >= TerrainGenerator.x_index_length || z_index < 0 || z_index >= TerrainGenerator.z_index_length)
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
            if (!TerrainGenerator.is_generated[x_index * TerrainGenerator.z_index_length + z_index])
            {
                TerrainGenerator.is_generated[x_index * TerrainGenerator.z_index_length + z_index] = true;
                int x_piece_num = TerrainGenerator.piece_num;
                int z_piece_num = TerrainGenerator.piece_num;
                if (x_index + TerrainGenerator.piece_num > TerrainGenerator.x_index_length)
                    x_piece_num = TerrainGenerator.x_index_length - x_index;
                if (z_index + TerrainGenerator.piece_num > TerrainGenerator.z_index_length)
                    z_piece_num = TerrainGenerator.z_index_length - z_index;
                StartCoroutine(TerrainGenerator.generateTerrainPatch(x_index, z_index, x_piece_num, z_piece_num));
                break;
            }
        }
    }
}