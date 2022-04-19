using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public Material terrain_idw_mat;
    public Material terrain_mat;
    public int terrain_mode = 0;
    //public GameObject feature_ball_prefab;
    Queue<int> generate_x = new Queue<int>();
    Queue<int> generate_z = new Queue<int>();
    int piece_num = 4;
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
        if (TerrainGenerator.is_initial)
        {
            while (TerrainGenerator.loading_vec3s.Count > 0)
            {
                Vector3 loading_vec3 = TerrainGenerator.loading_vec3s.Dequeue();
                int x_index = Mathf.FloorToInt((loading_vec3.x - TerrainGenerator.min_x) / PublicOutputInfo.piece_length);
                int z_index = Mathf.FloorToInt((loading_vec3.z - TerrainGenerator.min_z) / PublicOutputInfo.piece_length);
                int center_x = x_index - x_index % piece_num;
                int center_z = z_index - z_index % piece_num;
                TerrainGenerator.generate_center_x.Enqueue(center_x);
                TerrainGenerator.generate_center_z.Enqueue(center_z);
                TerrainGenerator.need_update = true;
                InvokeRepeating("generateTerrainPatch", 0.0f, 0.01666f);
            }
        }

        if (TerrainGenerator.need_update)
        {
            TerrainGenerator.need_update = false;
            while (TerrainGenerator.generate_center_x.Count > 0)
            {
                int center_x = TerrainGenerator.generate_center_x.Peek();
                int center_z = TerrainGenerator.generate_center_z.Peek();
                for (int i = -TerrainGenerator.vision_piece; i <= TerrainGenerator.vision_piece; i++)
                {
                    for (int j = -TerrainGenerator.vision_piece; j <= TerrainGenerator.vision_piece; j++)
                    {
                        if (Mathf.Abs(i) + Mathf.Abs(j) > TerrainGenerator.vision_piece)
                            continue;
                        int x_small_min = center_x + i * piece_num;
                        int z_small_min = center_z + j * piece_num;
                        if (x_small_min < 0 || x_small_min >= TerrainGenerator.x_length || z_small_min < 0 || z_small_min >= TerrainGenerator.z_length)
                            continue;
                        generate_x.Enqueue(x_small_min);
                        generate_z.Enqueue(z_small_min);
                    }
                }
                TerrainGenerator.generate_center_x.Dequeue();
                TerrainGenerator.generate_center_z.Dequeue();
            }
        }
    }

    void generateTerrainPatch()
    {
        while (generate_x.Count > 0)
        {
            int x_small_min = generate_x.Dequeue();
            int z_small_min = generate_z.Dequeue();
            if (!TerrainGenerator.is_generated[x_small_min * TerrainGenerator.z_length + z_small_min])
            {
                TerrainGenerator.is_generated[x_small_min * TerrainGenerator.z_length + z_small_min] = true;
                int x_piece_num = piece_num;
                int z_piece_num = piece_num;
                if (x_small_min + piece_num > TerrainGenerator.x_length)
                    x_piece_num = TerrainGenerator.x_length - x_small_min;
                if (z_small_min + piece_num > TerrainGenerator.z_length)
                    z_piece_num = TerrainGenerator.z_length - z_small_min;
                StartCoroutine(TerrainGenerator.generateTerrainPatch(x_small_min, z_small_min, x_piece_num, z_piece_num));
                break;
            }
        }
    }
}