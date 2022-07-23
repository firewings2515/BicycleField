using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class QuadTreePatch
{
    static public Dictionary<Vector2, float> node_dic;

    static public void initial()
    {
        node_dic = new Dictionary<Vector2, float>();
    }

    static public float findNodeSize(float x, float z)
    {
        if (node_dic.ContainsKey(new Vector2(x, z)))
            return node_dic[new Vector2(x, z)];
        return 0;
    }

    static public void addNodeSize(float x, float z, float size)
    {
        node_dic.Add(new Vector2(x, z), size);
        updateNodeSize(x, z, size);
    }


    static public void updateNodeSize(float x, float z, float size)
    {
        //node_dic[new Vector2(x, z)] = size;
        int bigger_size = Mathf.RoundToInt(size * 2); //Mathf.RoundToInt(node_dic[new Vector2(x, z)])
        int step = Mathf.RoundToInt(bigger_size / size);
        float min_x = Mathf.FloorToInt(x / bigger_size) * bigger_size;
        float min_z = Mathf.FloorToInt(z / bigger_size) * bigger_size;
        if (Mathf.RoundToInt(findNodeSize(min_x, min_z)) == bigger_size)
        {
            for (int x_index = 0; x_index < step; x_index++)
            {
                for (int z_index = 0; z_index < step; z_index++)
                {
                    node_dic[new Vector2(min_x + x_index * size, min_z + z_index * size)] = size;
                }
            }
        }

        int smaller_size = Mathf.RoundToInt(size / 2);
        if (smaller_size < 32)
            return;

        min_x = Mathf.FloorToInt(x / smaller_size) * smaller_size;
        min_z = Mathf.FloorToInt(z / smaller_size) * smaller_size;
        bool find_smaller = false;
        for (int x_index = 0; x_index < step && !find_smaller; x_index++)
        {
            for (int z_index = 0; z_index < step && !find_smaller; z_index++)
            {
                if (Mathf.RoundToInt(findNodeSize(min_x + x_index * smaller_size, min_z + z_index * smaller_size)) == smaller_size)
                {
                    node_dic[new Vector2(x, z)] = smaller_size;
                    find_smaller = true;
                }
            }
        }
        if (find_smaller)
        {
            for (int x_index = 0; x_index < step; x_index++)
            {
                for (int z_index = 0; z_index < step; z_index++)
                {
                    node_dic[new Vector2(min_x + x_index * smaller_size, min_z + z_index * smaller_size)] = smaller_size;
                }
            }
        }
    }
}