using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public Material material;
    Queue<int> generate_x;
    Queue<int> generate_z;
    bool[] is_generated;
    int piece = 4;
    // Start is called before the first frame update
    void Start()
    {
        TerrainGenerator.terrain_mat = material;
        //terrain
        TerrainGenerator.loadTerrain();
        is_generated = new bool[TerrainGenerator.x_length * TerrainGenerator.z_length];
        generate_x = new Queue<int>();
        generate_z = new Queue<int>();
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
                int center_x = x_index - x_index % piece;
                int center_z = z_index - z_index % piece;
                TerrainGenerator.generate_center_x.Enqueue(center_x);
                TerrainGenerator.generate_center_z.Enqueue(center_z);
                TerrainGenerator.need_update = true;
                Debug.Log(center_x + ", " + center_z);
            }
        }

        if (TerrainGenerator.need_update)
        {
            TerrainGenerator.need_update = false;
            while (TerrainGenerator.generate_center_x.Count > 0)
            {
                for (int i = -2; i <= 2; i++)
                {
                    for (int j = -2; j <= 2; j++)
                    {
                        int center_x = TerrainGenerator.generate_center_x.Peek();
                        int center_z = TerrainGenerator.generate_center_z.Peek();
                        int x_small_min = center_x + i * piece;
                        int z_small_min = center_z + j * piece;
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

        while (generate_x.Count > 0) 
        {
            int x_small_min = generate_x.Dequeue();
            int z_small_min = generate_z.Dequeue();
            if (!is_generated[x_small_min * TerrainGenerator.z_length + z_small_min])
            {
                is_generated[x_small_min * TerrainGenerator.z_length + z_small_min] = true;
                int x_piece = piece;
                int z_piece = piece;
                if (x_small_min + piece > TerrainGenerator.x_length)
                    x_piece = TerrainGenerator.x_length - x_small_min;
                if (z_small_min + piece > TerrainGenerator.z_length)
                    z_piece = TerrainGenerator.z_length - z_small_min;
                TerrainGenerator.generateSmallIDWTerrain(x_small_min, z_small_min, x_piece, z_piece);
                break;
            }
        }
    }
}