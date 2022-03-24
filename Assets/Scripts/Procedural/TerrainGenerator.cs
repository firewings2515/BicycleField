using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

static public class TerrainGenerator
{
    static public string file_path = "YangJing1/features_100_16.f"; // _150_32
    static public bool is_initial = false;
    static public int x_length;
    static public int z_length;
    static public float min_x;
    static public float min_y;
    static public float min_z;
    static float boundary_min_x;
    static float boundary_min_z;
    static float origin_x;
    static float origin_y;
    static float origin_z;
    static public Vector3[] features;
    static public Material terrain_mat;
    static public bool generate;
    static public int vision_piece = 10;
    static public List<GameObject> terrains;
    static public bool need_update = false;
    static public int center_x;
    static public int center_z;
    static public Queue<Vector3> loading_vec3s = new Queue<Vector3>();
    static public Queue<int> generate_center_x = new Queue<int>();
    static public Queue<int> generate_center_z = new Queue<int>();
    static public bool[] is_generated;
    static public List<int> generated_x_list;
    static public List<int> generated_z_list;
    static public KDTree kdtree;
    //static public GameObject feature_ball_prefab;

    static public void loadTerrain()
    {
        //load terrain info
        readFeatureFile(Application.streamingAssetsPath + "//" + file_path);
        is_generated = new bool[x_length * z_length];
        generated_x_list = new List<int>();
        generated_z_list = new List<int>();
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
            getAreaTerrain(position.x, position.z);

            //remove terrain not near position
            removeAreaTerrain(position.x, position.z);
        }
    }

    static public void removeTerrain(Vector3 position)
    {
        removeAreaTerrain(position.x, position.z);
    }

    static public void readFeatureFile(string file_path)
    {
        using (StreamReader sr = new StreamReader(file_path))
        {
            string[] inputs = sr.ReadLine().Split(' ');
            boundary_min_x = float.Parse(inputs[0]);
            boundary_min_z = float.Parse(inputs[1]);
            inputs = sr.ReadLine().Split(' ');
            origin_x = float.Parse(inputs[0]);
            origin_y = float.Parse(inputs[1]);
            origin_z = float.Parse(inputs[2]);
            inputs = sr.ReadLine().Split(' ');
            x_length = int.Parse(inputs[0]);
            z_length = int.Parse(inputs[1]);
            inputs = sr.ReadLine().Split(' ');
            min_x = float.Parse(inputs[0]);
            min_y = float.Parse(inputs[1]);
            min_z = float.Parse(inputs[2]);
            int n = int.Parse(sr.ReadLine());
            kdtree = new KDTree();
            kdtree.nodes = new Vector3[n];
            kdtree.parent = new int[n];
            kdtree.left = new int[n];
            kdtree.right = new int[n];
            for (int f_i = 0; f_i < n; f_i++)
            {
                inputs = sr.ReadLine().Split(' ');
                float x = float.Parse(inputs[0]);
                float y = float.Parse(inputs[1]);
                float z = float.Parse(inputs[2]);
                kdtree.nodes[f_i] = new Vector3(x, y, z);
                int p = int.Parse(inputs[3]);
                kdtree.parent[f_i] = p;
                int l = int.Parse(inputs[4]);
                kdtree.left[f_i] = l;
                int r = int.Parse(inputs[5]);
                kdtree.right[f_i] = r;
            }
            Debug.Log("Read Feature File Successfully");

            GameObject feature_manager = new GameObject("feature_manager");
            //showPoint(kdtree.nodes, "feature", feature_manager.transform, feature_ball_prefab, 1.0f);
        }
        terrains = new List<GameObject>();
    }

    static public Vector3[] getAreaFeatures(int x_small_min, int z_small_min, int x_piece, int z_piece)
    {
        float expanded_length = vision_piece * PublicOutputInfo.piece_length * 4;
        int[] area_features_index = kdtree.getAreaPoints(x_small_min - expanded_length, z_small_min - expanded_length, x_small_min + (x_piece + 1) * PublicOutputInfo.piece_length + expanded_length, z_small_min + (z_piece + 1) * PublicOutputInfo.piece_length + expanded_length);
        Vector3[] area_features = new Vector3[area_features_index.Length];
        for (int area_features_index_index = 0; area_features_index_index < area_features_index.Length; area_features_index_index++)
        {
            area_features[area_features_index_index] = kdtree.nodes[area_features_index[area_features_index_index]];
        }
        return area_features;
    }

    static public IEnumerator generateSmallIDWTerrain(int x_small_min, int z_small_min, int x_piece, int z_piece)
    {
        Vector3[] area_features = getAreaFeatures(x_small_min, z_small_min, x_piece, z_piece);
        generateSmallIDWTerrain(area_features, x_small_min, z_small_min, x_piece + 1, z_piece + 1);
        yield return null;
    }

    static void generateSmallIDWTerrain(Vector3[] features, int x_small_min, int z_small_min, int x_small_length, int z_small_length)
    {
        //Debug.Log("Calculating: " + x_small_min + "_" + z_small_min);
        Mesh mesh = new Mesh();
        float[,,] terrain_points = new float[x_small_length, z_small_length, 3];
        Vector3[] vertice = new Vector3[x_small_length * z_small_length];
        Vector2[] uv = new Vector2[x_small_length * z_small_length];
        int[] indices = new int[6 * (x_small_length - 1) * (z_small_length - 1)];
        int indices_index = 0;
        float center_x = min_x + (2 * x_small_min + x_small_length - 1) * PublicOutputInfo.piece_length / 2;
        float center_z = min_z + (2 * z_small_min + z_small_length - 1) * PublicOutputInfo.piece_length / 2;
        //float center_y = min_y + getDEMHeight(center_x, center_z);
        float center_y = min_y + IDW.inverseDistanceWeighting(features, center_x, center_z) - 15; // -15
        Vector3 center = new Vector3(center_x, center_y, center_z);
        for (int i = 0; i < x_small_length; i++)
        {
            for (int j = 0; j < z_small_length; j++)
            {
                terrain_points[i, j, 0] = min_x + (x_small_min + i) * PublicOutputInfo.piece_length;
                terrain_points[i, j, 2] = min_z + (z_small_min + j) * PublicOutputInfo.piece_length;
                //terrain_points[i, j, 1] = min_y + getDEMHeight(terrain_points[i, j, 0], terrain_points[i, j, 2]); // min_y is a bias
                terrain_points[i, j, 1] = min_y + IDW.inverseDistanceWeighting(features, terrain_points[i, j, 0], terrain_points[i, j, 2]) - 15; // min_y is a bias  -15
                vertice[i * z_small_length + j] = new Vector3(terrain_points[i, j, 0] - center.x, terrain_points[i, j, 1] - center.y, terrain_points[i, j, 2] - center.z);
                //uv[i * z_small_length + j] = new Vector2((float)(x_small_min + i) / x_length, (float)(z_small_min + j) / z_length);
                uv[i * z_small_length + j] = new Vector2((float)i / (x_small_length - 1), (float)j / (z_small_length - 1));
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
        generated_x_list.Add(x_small_min);
        generated_z_list.Add(z_small_min);
        //Debug.Log("Success: " + x_small_min + "_" + z_small_min);

        terrain.AddComponent<TerrainView>();
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
        //Debug.Log(center_x + ", " + center_z);
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
        for (int generated_list_index = 0; generated_list_index < generated_x_list.Count; generated_list_index++)
        {
            int ddist = Mathf.Abs(generated_x_list[generated_list_index] - center_x) / 4 + Mathf.Abs(generated_z_list[generated_list_index] - center_z) / 4;
            if (ddist > vision_piece * 2)
            {
                is_generated[generated_x_list[generated_list_index] * z_length + generated_z_list[generated_list_index]] = false;
                GameObject.Destroy(terrains[generated_list_index]);
                generated_x_list.RemoveAt(generated_list_index);
                generated_z_list.RemoveAt(generated_list_index);
                terrains.RemoveAt(generated_list_index);
                generated_list_index--;
            }
        }
    }

    static float getDEMHeight(float x, float z)
    {
        x += boundary_min_x + origin_x;
        z += boundary_min_z + origin_z;
        float lon = (float)MercatorProjection.xToLon(x);
        float lat = (float)MercatorProjection.yToLat(z);
        List<EarthCoord> all_coords = new List<EarthCoord>();
        all_coords.Add(new EarthCoord(lon, lat));
        return HgtReader.getElevations(all_coords)[0];
    }

    static float getIDWHeight(float x, float z)
    {
        getAreaTerrain(x, z);
        Vector3[] area_features = getAreaFeatures(center_x, center_z, 0, 0);
        return min_y + IDW.inverseDistanceWeighting(area_features, center_x, center_z) - 15;
    }

    static public void generateSmallHeightmapTerrain(Texture2D heightmap, int x_small_min, int z_small_min, int x_small_length, int z_small_length)
    {
        //Debug.Log("Calculating: " + x_small_min + "_" + z_small_min);
        Mesh mesh = new Mesh();
        float[,,] terrain_points = new float[x_small_length, z_small_length, 3];
        Vector3[] vertice = new Vector3[x_small_length * z_small_length];
        Vector2[] uv = new Vector2[x_small_length * z_small_length];
        int[] indices = new int[6 * (x_small_length - 1) * (z_small_length - 1)];
        int indices_index = 0;
        float center_x = min_x + (2 * x_small_min + x_small_length - 1) * PublicOutputInfo.piece_length / 2;
        float center_z = min_z + (2 * z_small_min + z_small_length - 1) * PublicOutputInfo.piece_length / 2;
        //float center_y = min_y + getDEMHeight(center_x, center_z);
        float center_y = min_y; // -15
        Vector3 center = new Vector3(center_x, center_y, center_z);
        for (int i = 0; i < x_small_length; i++)
        {
            for (int j = 0; j < z_small_length; j++)
            {
                terrain_points[i, j, 0] = min_x + (x_small_min + i) * PublicOutputInfo.piece_length;
                terrain_points[i, j, 2] = min_z + (z_small_min + j) * PublicOutputInfo.piece_length;
                //terrain_points[i, j, 1] = min_y + getDEMHeight(terrain_points[i, j, 0], terrain_points[i, j, 2]); // min_y is a bias
                terrain_points[i, j, 1] = min_y + heightmap.GetPixel(i, j).r * 255; // min_y is a bias  -15
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
        GameObject terrain = new GameObject("terrain_Heightmap_" + x_small_min + "_" + z_small_min);
        MeshFilter mf = terrain.AddComponent<MeshFilter>();
        MeshRenderer mr = terrain.AddComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = terrain_mat;
        terrain.transform.position = center;
        terrains.Add(terrain);
        //Debug.Log("Success: " + x_small_min + "_" + z_small_min);
    }

    static void showPoint(Vector3[] points, string tag, Transform parent, GameObject ball_prefab, float ball_size)
    {
        for (int point_index = 0; point_index < points.Length; point_index++)
        {
            GameObject ball = GameObject.Instantiate(ball_prefab, points[point_index], Quaternion.identity);
            ball.transform.localScale = new Vector3(ball_size, ball_size, ball_size);
            ball.name = tag + "_" + point_index.ToString();
            ball.transform.parent = parent;
        }
    }
}