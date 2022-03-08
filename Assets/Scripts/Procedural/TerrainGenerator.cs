using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

static public class TerrainGenerator
{
    static public string file_path = "YangJing1/features_150_32.f";
    static bool is_initial = false;
    static public int x_length;
    static public int z_length;
    static float min_x;
    static float min_z;
    static Vector3[] features;
    static public Material terrain_mat;
    static public bool generate;
    static public int vision_piece = 10;
    static List<GameObject> terrains;
    static public bool need_update = false;
    static public int center_x;
    static public int center_z;
    static public Queue<Vector3> loading_vec3s = new Queue<Vector3>();
    static public Queue<int> generate_center_x = new Queue<int>();
    static public Queue<int> generate_center_z = new Queue<int>();

    static public void loadTerrain()
    {
        //load terrain info
        readFeatureFile(Application.streamingAssetsPath + "//" + file_path);
        is_initial = true;
    }

    static public void generateTerrain(Vector3 position)
    {
        //generate terrain near position
        if (!is_initial)
        {
            loading_vec3s.Enqueue(position);
        }
        else
        {
            while(loading_vec3s.Count > 0)
            {
                Vector3 loading_vec3 = loading_vec3s.Dequeue();
                getAreaTerrain(loading_vec3.x, loading_vec3.z);
            }
            getAreaTerrain(position.x, position.z);
        }
    }

    static public void removeTerrain(Vector3 position)
    {
        //remove terrain not near position
        removeAreaTerrain(position.x, position.z);
    }

    static public void readFeatureFile(string file_path)
    {
        using (StreamReader sr = new StreamReader(file_path))
        {
            string[] inputs = sr.ReadLine().Split(' ');
            x_length = int.Parse(inputs[0]);
            z_length = int.Parse(inputs[1]);
            inputs = sr.ReadLine().Split(' ');
            min_x = float.Parse(inputs[0]);
            min_z = float.Parse(inputs[1]);
            int n = int.Parse(sr.ReadLine());
            features = new Vector3[n];
            for (int f_i = 0; f_i < n; f_i++)
            {
                inputs = sr.ReadLine().Split(' ');
                float x = float.Parse(inputs[0]);
                float y = float.Parse(inputs[1]);
                float z = float.Parse(inputs[2]);
                features[f_i] = new Vector3(x, y, z);
            }
            Debug.Log("Read Feature File Successfully");
        }
        terrains = new List<GameObject>();
    }

    static public void generateSmallIDWTerrain(int x_small_min, int z_small_min, int piece)
    {
        generateSmallIDWTerrain(features, x_small_min, z_small_min, piece + 1, piece + 1);
    }

    static void generateSmallIDWTerrain(Vector3[] features, int x_small_min, int z_small_min, int x_small_length, int z_small_length)
    {
        Debug.Log("Calculating: " + x_small_min + "_" + z_small_min);
        Mesh mesh = new Mesh();
        float[,,] terrain_points = new float[x_small_length, z_small_length, 3];
        Vector3[] vertice = new Vector3[x_small_length * z_small_length];
        Vector2[] uv = new Vector2[x_small_length * z_small_length];
        int[] indices = new int[6 * (x_small_length - 1) * (z_small_length - 1)];
        int indices_index = 0;
        float center_x = min_x + (2 * x_small_min + x_small_length - 1) * PublicOutputInfo.piece_length / 2;
        float center_z = min_z + (2 * z_small_min + z_small_length - 1) * PublicOutputInfo.piece_length / 2;
        float center_y = IDW.inverseDistanceWeighting(features, center_x, center_z);
        Vector3 center = new Vector3(center_x, center_y, center_z);
        for (int i = 0; i < x_small_length; i++)
        {
            for (int j = 0; j < z_small_length; j++)
            {
                terrain_points[i, j, 0] = min_x + (x_small_min + i) * PublicOutputInfo.piece_length;
                terrain_points[i, j, 2] = min_z + (z_small_min + j) * PublicOutputInfo.piece_length;
                terrain_points[i, j, 1] = IDW.inverseDistanceWeighting(features, terrain_points[i, j, 0], terrain_points[i, j, 2]);
                vertice[i * z_small_length + j] = new Vector3(terrain_points[i, j, 0] - center.x, terrain_points[i, j, 1] - center.y, terrain_points[i, j, 2] - center.z);
                uv[i * z_small_length + j] = new Vector2((float)(x_small_min + i) / x_length, (float)(z_small_min + j) / z_length);
            }
        }

        for (int i = 0; i < x_small_length - 1; i++)
        {
            for (int j = 0; j < z_small_length - 1; j++)
            {
                // counter-clockwise
                indices[indices_index++] = i * z_small_length + j;
                indices[indices_index++] = (i + 1) * z_small_length + j + 1;
                indices[indices_index++] = (i + 1) * z_small_length + j;
                indices[indices_index++] = i * z_small_length + j;
                indices[indices_index++] = i * z_small_length + j + 1;
                indices[indices_index++] = (i + 1) * z_small_length + j + 1;
            }
        }

        mesh.vertices = vertice;
        mesh.uv = uv;
        mesh.triangles = indices;
        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        //Name the mesh
        mesh.name = "terrain_mesh";
        GameObject terrain = new GameObject("terrain_IDW_" + x_small_min + "_" + z_small_min);
        MeshFilter mf = terrain.AddComponent<MeshFilter>();
        MeshRenderer mr = terrain.AddComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = terrain_mat;
        terrain.transform.position = center;
        terrains.Add(terrain);
        Debug.Log("Success: " + x_small_min + "_" + z_small_min);
    }

    static void getAreaTerrain(float x, float z)
    {
        int x_index = Mathf.FloorToInt((x - min_x) / PublicOutputInfo.piece_length);
        int z_index = Mathf.FloorToInt((z - min_z) / PublicOutputInfo.piece_length);
        int piece = 4;
        center_x = x_index - x_index % piece;
        center_z = z_index - z_index % piece;
        generate_center_x.Enqueue(center_x);
        generate_center_z.Enqueue(center_z);
        need_update = true;
        Debug.Log(center_x + ", " + center_z);
        //int x_begin_index = x_index - x_index % piece;
        //int z_begin_index = z_index - z_index % piece;

        //for (int i = -2; i <= 2; i++)
        //{
        //    for (int j = -2; j <= 2; j++)
        //    {
        //        int x_small_min = x_begin_index + i * piece;
        //        int z_small_min = z_begin_index + j * piece;
        //        if (x_small_min < 0 || x_small_min >= x_length || z_small_min < 0 || z_small_min >= z_length)
        //            continue;
        //        if (!is_generated[x_small_min * z_length + z_small_min])
        //        {
        //            is_generated[x_small_min * z_length + z_small_min] = true;
        //            generateSmallIDWTerrain(features, x_begin_index + i * piece, z_begin_index + j * piece, piece + 1, piece + 1);
        //        }
        //    }
        //}
    }

    static void removeAreaTerrain(float x, float z)
    {
        ////int piece = 4;
        ////int x_index = Mathf.FloorToInt((current_x - min_x) / PublicOutputInfo.piece_length);
        ////int z_index = Mathf.FloorToInt((current_z - min_z) / PublicOutputInfo.piece_length);
        ////int x_begin_index = x_index - x_index % piece;
        ////int z_begin_index = z_index - z_index % piece;
        ////bool[] is_reserved = new bool[x_length * z_length];
        ////for (int i = -2; i <= 2; i++)
        ////{
        ////    for (int j = -2; j <= 2; j++)
        ////    {
        ////        int x_small_min = x_begin_index + i * piece;
        ////        int z_small_min = z_begin_index + j * piece;
        ////        if (x_small_min < 0 || x_small_min >= x_length || z_small_min < 0 || z_small_min >= z_length)
        ////            continue;
        ////        is_reserved[x_small_min * z_length + z_small_min] = true;
        ////    }
        ////}

        //x_index = Mathf.FloorToInt((x - min_x) / PublicOutputInfo.piece_length);
        //z_index = Mathf.FloorToInt((z - min_z) / PublicOutputInfo.piece_length);
        //x_begin_index = x_index - x_index % piece;
        //z_begin_index = z_index - z_index % piece;
        //for (int i = -2; i <= 2; i++)
        //{
        //    for (int j = -2; j <= 2; j++)
        //    {
        //        int x_small_min = x_begin_index + i * piece;
        //        int z_small_min = z_begin_index + j * piece;
        //        if (x_small_min < 0 || x_small_min >= x_length || z_small_min < 0 || z_small_min >= z_length)
        //            continue;
        //        //if (is_generated[x_small_min * z_length + z_small_min] && !is_reserved[x_small_min * z_length + z_small_min])
        //        //{
        //        //    is_generated[x_small_min * z_length + z_small_min] = false;
        //        //    Debug.Log("Delete" + x_small_min + "_" + z_small_min);
        //        //    for (int terrain_index = 0; terrain_index < terrains.Count; terrain_index++)
        //        //    {
        //        //        if (terrains[terrain_index].name == "terrain_IDW_" + x_small_min + "_" + z_small_min)
        //        //        {
        //        //            GameObject.Destroy(terrains[terrain_index]);
        //        //            terrains.RemoveAt(terrain_index);
        //        //        }    
        //        //    }
        //        //}
        //    }
        //}
    }
}