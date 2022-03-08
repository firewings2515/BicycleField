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
        if (TerrainGenerator.need_update)
        {
            TerrainGenerator.need_update = false;
            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    int x_small_min = TerrainGenerator.center_x + i * piece;
                    int z_small_min = TerrainGenerator.center_z + j * piece;
                    if (x_small_min < 0 || x_small_min >= TerrainGenerator.x_length || z_small_min < 0 || z_small_min >= TerrainGenerator.z_length)
                        continue;
                    generate_x.Enqueue(x_small_min);
                    generate_z.Enqueue(z_small_min);
                }
            }
        }

        while (generate_x.Count > 0) 
        {
            int x_small_min = generate_x.Dequeue();
            int z_small_min = generate_z.Dequeue();
            if (!is_generated[x_small_min * TerrainGenerator.z_length + z_small_min])
            {
                is_generated[x_small_min * TerrainGenerator.z_length + z_small_min] = true;
                TerrainGenerator.generateSmallIDWTerrain(x_small_min, z_small_min, piece);
                break;
            }
        }
    }
}