using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class PublicOutputInfo
{
    public static Vector3 origin_pos;
    static public int piece_num = 64;                                                                   // The number of piece in a patch    ver2 = 8 ver3 = 64
    public static float piece_length = 2.0f;
    public static float patch_length = piece_num * piece_length;                                        // 128 is well
    public static int piece_num_in_chunk = 192;
    public static float editor_chunk_piece_length = 16.0f;
    public static float editor_chunk_patch_length = piece_num_in_chunk * editor_chunk_piece_length;
    public static Vector2 boundary_min;                                                                 // for terrain to getDEMHeight
    public static int gaussian_m = 16;
    public static int tex_size = 129;                                                                   
    public static int height_buffer_row_size = 136;
    public static int pregaussian_tex_size = tex_size + 2 * gaussian_m;                                 // 129 + 32 = 161

    /// <summary>
    /// all terrains in a feature file
    /// </summary>
    /// <param name="file_path"></param>
    /// <param name="features"></param>
    static public void writeFeatureFile(string file_path, WVec3[] features, int[] building_point_count, Vector3 boundary_min, Vector3 terrain_min, Vector3 terrain_max)
    {
        KDTree kdtree = new KDTree();
        kdtree.buildKDTree(features);

        Debug.Log("Writing " + file_path);
        using (StreamWriter sw = new StreamWriter(file_path))
        {
            sw.WriteLine(boundary_min.x + " " + boundary_min.y);
            sw.WriteLine(origin_pos.x + " " + origin_pos.y + " " + origin_pos.z);
            sw.WriteLine(terrain_min.x.ToString() + " " + terrain_min.y.ToString() + " " + terrain_min.z.ToString() + " " + terrain_max.x.ToString() + " " + terrain_max.y.ToString() + " " + terrain_max.z.ToString());
            sw.WriteLine(features.Length);
            for (int point_index = 0; point_index < features.Length; point_index++)
            {
                Vector3 feature_out = new Vector3(kdtree.nodes[point_index].x - origin_pos.x, kdtree.nodes[point_index].y, kdtree.nodes[point_index].z - origin_pos.z);
                sw.WriteLine(feature_out.x + " " + feature_out.y + " " + feature_out.z + " " + kdtree.nodes[point_index].w + " " + kdtree.parent[point_index] + " " + kdtree.left[point_index] + " " + kdtree.right[point_index]);
            }
            sw.WriteLine(building_point_count.Length);
            for (int building_point_index = 0; building_point_index < building_point_count.Length; building_point_index++)
            {
                sw.WriteLine(building_point_count[building_point_index]);
            }
        }
        Debug.Log("Write " + file_path + " Successfully!");
    }
}