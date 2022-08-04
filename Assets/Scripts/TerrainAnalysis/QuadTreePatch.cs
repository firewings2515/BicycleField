using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class QuadTreePatch
{
    static public double x_dem_interval = 30.9220807759091;
    static public double z_dem_interval = 34.0730199965134;
    static public int interval_peice_num = 16;
    static public double x_dem_step = x_dem_interval / interval_peice_num;
    static public double z_dem_step = z_dem_interval / interval_peice_num;
    static public Dictionary<DEMCoord, int> node_level_dic;
    static public HashSet<(DEMCoord born_corner, DEMCoord opposite_corner)> node_set;
    static public HashSet<(float x, float z, bool is_corner)> dense_node_set;
    static public HashSet<(float x, float z)> sparse_node_set;
    static public bool always_level_one = true;

    static public void initial()
    {
        node_level_dic = new Dictionary<DEMCoord, int>();
        node_set = new HashSet<(DEMCoord, DEMCoord)>();
    }

    static public int fetchNodeLevel(float x, float z)
    {
        var index = TerrainGenerator.getIndex(x, z);
        return fetchNodeLevel(new DEMCoord(index.x, index.z));
    }

    static public int fetchNodeLevel(DEMCoord coord)
    {
        if (node_level_dic.ContainsKey(coord))
            return node_level_dic[coord];
        return 0;
    }

    static public void addNodeLevel(float x, float z, int level)
    {
        var index = TerrainGenerator.getIndex(x, z);
        DEMCoord coord = new DEMCoord(index.x, index.z);
        node_level_dic.Add(coord, level);
        updateNodeLevel(coord, level);
    }

    static public void addNodeLevel(DEMCoord coord, int level)
    {
        node_level_dic.Add(coord, level);
        updateNodeLevel(coord, level);
    }

    static public void updateNodeSize(float x, float z, int level)
    {
        var index = TerrainGenerator.getIndex(x, z);
        updateNodeLevel(new DEMCoord(index.x, index.z), level);
        ////node_dic[new Vector2(x, z)] = size;
        //int upper_level = level * 2; //Mathf.RoundToInt(node_dic[new Vector2(x, z)])
        //float x_bigger_size = (float)(upper_level * x_dem_interval);
        //float z_bigger_size = (float)(upper_level * z_dem_interval);
        //float min_x = Mathf.FloorToInt(x / x_bigger_size) * x_bigger_size;
        //float min_z = Mathf.FloorToInt(z / z_bigger_size) * z_bigger_size;
        //if (Mathf.RoundToInt(findNodeSize(min_x, min_z)) == upper_level)
        //{
        //    for (int x_index = 0; x_index < 2; x_index++)
        //    {
        //        for (int z_index = 0; z_index < 2; z_index++)
        //        {
        //            node_dic[new Vector2((float)(min_x + x_index * x_dem_interval), (float)(min_z + z_index * z_dem_interval))] = level;
        //        }
        //    }
        //}

        //int lower_level = level / 2;
        //if (lower_level < 1)
        //    return;

        //min_x = Mathf.FloorToInt(x / lower_level) * lower_level;
        //min_z = Mathf.FloorToInt(z / lower_level) * lower_level;
        //bool find_smaller = false;
        //for (int x_index = 0; x_index < step && !find_smaller; x_index++)
        //{
        //    for (int z_index = 0; z_index < step && !find_smaller; z_index++)
        //    {
        //        if (Mathf.RoundToInt(findNodeSize(min_x + x_index * lower_level, min_z + z_index * lower_level)) == lower_level)
        //        {
        //            node_dic[new Vector2(x, z)] = lower_level;
        //            find_smaller = true;
        //        }
        //    }
        //}
        //if (find_smaller)
        //{
        //    for (int x_index = 0; x_index < step; x_index++)
        //    {
        //        for (int z_index = 0; z_index < step; z_index++)
        //        {
        //            node_dic[new Vector2(min_x + x_index * lower_level, min_z + z_index * lower_level)] = lower_level;
        //        }
        //    }
        //}
    }

    static public void updateNodeLevel(DEMCoord coord, int level)
    {
        int upper_level = level * 2;
        int upper_x_min = (coord.x / upper_level) * upper_level;
        int upper_z_min = (coord.z / upper_level) * upper_level;
        if (fetchNodeLevel(upper_x_min, upper_z_min) == upper_level)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    node_level_dic[(upper_x_min + i, upper_z_min + j)] = level;
                }
            }
        }

        int lower_level = level / 2;
        if (lower_level < 1)
            return;

        int lower_x_min = (coord.x / lower_level) * lower_level;
        int lower_z_min = (coord.z / lower_level) * lower_level;
        bool find_smaller = false;
        for (int i = 0; i < 2 && !find_smaller; i++)
        {
            for (int j = 0; j < 2 && !find_smaller; j++)
            {
                if (fetchNodeLevel(lower_x_min + i, lower_z_min + j) == lower_level)
                {
                    node_level_dic[(lower_x_min + i, lower_z_min + j)] = lower_level;
                    find_smaller = true;
                }
            }
        }
        if (find_smaller)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    node_level_dic[(lower_x_min + i, lower_z_min + j)] = lower_level;
                }
            }
        }
    }

    static public int calcLevel(float size_x)
    {
        return Mathf.RoundToInt((float)(size_x / x_dem_interval));
    }

    static public void getAllNode()
    {
        foreach (var node_level in node_level_dic)
        {
            node_set.Add((node_level.Key, new DEMCoord(node_level.Key.x + 1, node_level.Key.z + 1)));
            node_set.Add((new DEMCoord(node_level.Key.x + 1, node_level.Key.z), new DEMCoord(node_level.Key.x + 1 + 1, node_level.Key.z + 1)));
            node_set.Add((new DEMCoord(node_level.Key.x, node_level.Key.z + 1), new DEMCoord(node_level.Key.x + 1, node_level.Key.z + 1 + 1)));
            node_set.Add((new DEMCoord(node_level.Key.x + 1, node_level.Key.z + 1), new DEMCoord(node_level.Key.x + 1 + 1, node_level.Key.z + 1 + 1)));
        }
    }

    static public void denseNode(int subdivide)
    {
        dense_node_set = new HashSet<(float x, float z, bool is_corner)>();
        float step = 1.0f / subdivide;
        foreach (var node in node_set)
        {
            for (int i = 0; i < subdivide; i++)
            {
                for (int j = 0; j < subdivide; j++)
                {
                    dense_node_set.Add((node.born_corner.x + i * step, node.born_corner.z + j * step, i == 0 && j == 0));
                }
            }
        }
    }

    static public void sparseNode(int interval)
    {
        sparse_node_set = new HashSet<(float x, float z)>();
        foreach (var node in node_set)
        {
            if (node.born_corner.x % interval == 0 && node.born_corner.z % interval == 0)
            {
                sparse_node_set.Add((node.born_corner.x, node.born_corner.z));
            }
        }
    }
}

public struct DEMCoord
{
    public int x;
    public int z;

    public DEMCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public override bool Equals(object obj)
    {
        return obj is DEMCoord other &&
               x == other.x &&
               z == other.z;
    }

    public override int GetHashCode()
    {
        int hashCode = 1553271884;
        hashCode = hashCode * -1521134295 + x.GetHashCode();
        hashCode = hashCode * -1521134295 + z.GetHashCode();
        return hashCode;
    }

    public void Deconstruct(out int x, out int z)
    {
        x = this.x;
        z = this.z;
    }

    public static implicit operator (int x, int z)(DEMCoord value)
    {
        return (value.x, value.z);
    }

    public static implicit operator DEMCoord((int x, int z) value)
    {
        return new DEMCoord(value.x, value.z);
    }
}