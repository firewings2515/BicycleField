using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode]
public class HeightmapCompress : MonoBehaviour
{
    public Texture2D heightmap_edge;
    public Texture2D heightmap;
    public Vector3[] vertice;
    public float[] edges;
    public int x_length;
    public int z_length;
    public float map_size_width = 22.5f;
    public float map_size_height = 22.5f;
    public float piece_length = 32.0f; //2048
    public float min_x;
    public float min_z;
    public float gray_height;
    public bool get_feature;
    public GameObject blue_ball;
    public GameObject red_ball;
    public float threshold;
    public float epsilon;
    Vector3[] point_cloud;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (get_feature)
        {
            get_feature = false;
            Debug.Log("Get feature start");
            GameObject feature_manager = new GameObject("feature_manager");
            List<Vector3> point_cloud_list = new List<Vector3>();

            // W8D in heightmap
            //for (float edge_x = 0.0f; edge_x < map_size_width; edge_x += piece_length)
            //{
            //    for (float edge_z = 0.0f; edge_z < map_size_height; edge_z += piece_length)
            //    {
            //        Color gray = heightmap_edge.GetPixel(Mathf.FloorToInt(edge_x / map_size_width * heightmap_edge.width), Mathf.FloorToInt(edge_z / map_size_height * heightmap_edge.height));
            //        if (gray.r > threshold)
            //        {
            //            List<List<Vector3>> w8d = W8D(map_size_width, map_size_height, new Vector3(edge_x, 0.0f, edge_z), point_cloud_list); // need to limit boundary
            //            Vector3[] w8d_center = new Vector3[1];
            //            Color height = heightmap.GetPixel(Mathf.FloorToInt(edge_x / map_size_width * heightmap.width), Mathf.FloorToInt(edge_z / map_size_height * heightmap.height));
            //            w8d_center[0] = new Vector3(min_x + edge_x, height.r * gray_height, min_z + edge_z);
            //            showPoint(w8d_center, "Feature_Center", feature_manager.transform, red_ball, 16.0f);
            //            for (int w8d_index = 0; w8d_index < w8d.Count; w8d_index++)
            //            {
            //                point_cloud_list.AddRange(w8d[w8d_index]);
            //            }
            //        }
            //    }
            //}

            // W8D in terrain edge detection
            bool[] flag = new bool[vertice.Length];
            for (int x = 0; x < x_length; x++)
            {
                for (int z = 0; z < z_length; z++)
                {
                    if (edges[x * z_length + z] > threshold)
                    {
                        List<List<Vector3>> w8d = W8DGrid(x_length, z_length, x, z, point_cloud_list, flag);
                        Vector3[] w8d_center = new Vector3[1];
                        w8d_center[0] = vertice[x * z_length + z];
                        showPoint(w8d_center, "Feature_Center", feature_manager.transform, red_ball, 16.0f);
                        for (int w8d_index = 0; w8d_index < w8d.Count; w8d_index++)
                        {
                            point_cloud_list.AddRange(w8d[w8d_index]);
                        }
                    }
                }
            }

            point_cloud = point_cloud_list.ToArray();
            showPoint(point_cloud, "Feature", feature_manager.transform, blue_ball, 8.0f);

            using (StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "//features.f"))
            {
                sw.WriteLine(x_length + " " + z_length);
                sw.WriteLine((min_x - PublicOutputInfo.origin_pos.x).ToString() + " " + (min_z - PublicOutputInfo.origin_pos.z).ToString());
                sw.WriteLine(point_cloud.Length);
                for (int point_index = 0; point_index < point_cloud.Length; point_index++)
                {
                    Vector3 feature_out = point_cloud[point_index] - PublicOutputInfo.origin_pos;
                    sw.WriteLine(feature_out.x + " " + feature_out.y + " " + feature_out.z);
                }
            }
            Debug.Log("Get feature finish");
        }
    }

    List<List<Vector3>> W8DGrid(int x_length, int z_length, int center_x, int center_z, List<Vector3> point_cloud_list, bool[] flag)
    {
        int[] dx = new int[8] { 1, 1, 1, 0, -1, -1, -1, 0 };
        int[] dz = new int[8] { 1, 0, -1, -1, -1, 0, 1, 1 };
        List<List<Vector3>> terrain_feature_points = new List<List<Vector3>>();

        for (int dir = 0; dir < 8; dir++)
        {
            terrain_feature_points.Add(new List<Vector3>());
            int d = 0;
            while (true)
            {
                int x = center_x + d * dx[dir];
                int z = center_z + d * dz[dir];
                if (x < 0 || x >= x_length || z < 0 || z >= z_length)
                    break;
                flag[x * z_length + z] = true;
                terrain_feature_points[dir].Add(vertice[x * z_length + z]);
                d++;
            }

            if (terrain_feature_points[dir].Count > 1)
                terrain_feature_points[dir] = DouglasPeuckerAlgorithm.DouglasPeucker(terrain_feature_points[dir], epsilon);
        }

        return terrain_feature_points;
    }

    List<List<Vector3>> W8D(float width_boundary, float height_boundary, Vector3 center, List<Vector3> point_cloud_list)
    {
        Vector3[] directions = new Vector3[8] { new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.707f, 0.0f, 0.707f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(-0.707f, 0.0f, 0.707f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(-0.707f, 0.0f, -0.707f), new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.707f, 0.0f, -0.707f) };
        List<List<Vector3>> terrain_feature_points = new List<List<Vector3>>();

        for (int dir = 0; dir < 8; dir++)
        {
            List<Vector3> terrain_feature_readys = new List<Vector3>();
            terrain_feature_points.Add(new List<Vector3>());
            for (float d = 0.0f; d < width_boundary * height_boundary; d += piece_length)
            {
                Vector3 terrain_feature_ready = center + d * directions[dir];
                if (terrain_feature_ready.x < 0.0f || terrain_feature_ready.z < 0.0f || terrain_feature_ready.x > width_boundary || terrain_feature_ready.z > height_boundary)
                    break;
                Color gray = heightmap.GetPixel(Mathf.FloorToInt(terrain_feature_ready.x / map_size_width * heightmap.width), Mathf.FloorToInt(terrain_feature_ready.z / map_size_height * heightmap.height));
                //terrain_feature_lonlats.Add(terrain_feature_lonlat);
                terrain_feature_ready.y = gray.r * gray_height;
                terrain_feature_ready += new Vector3(min_x, 0.0f, min_z);
                terrain_feature_readys.Add(terrain_feature_ready);
                // no near detection
                terrain_feature_points[dir].Add(terrain_feature_ready);
            }
            //for (int point_index = 0; point_index < terrain_feature_readys.Count; point_index++)
            //{
            //    bool is_too_near = false;
            //    for (int point_cloud_index = 0; point_cloud_index < point_cloud_list.Count; point_cloud_index++)
            //    {
            //        if (distance2D(point_cloud_list[point_cloud_index].x, point_cloud_list[point_cloud_index].z, terrain_feature_readys[point_index].x, terrain_feature_readys[point_index].z) < 0.1f)
            //        {
            //            is_too_near = true;
            //            break;
            //        }
            //    }
            //    if (!is_too_near)
            //        terrain_feature_points[dir].Add(terrain_feature_readys[point_index]);
            //}

            if (terrain_feature_points[dir].Count > 1)
                terrain_feature_points[dir] = DouglasPeuckerAlgorithm.DouglasPeucker(terrain_feature_points[dir], epsilon);
        }

        return terrain_feature_points;
    }

    float distance2D(float x1, float z1, float x2, float z2)
    {
        return Mathf.Sqrt(Mathf.Pow(x1 - x2, 2) + Mathf.Pow(z1 - z2, 2));
    }

    void showPoint(List<Vector3> path_points_dp, string tag, Transform parent, GameObject ball_prefab, float ball_size)
    {
        showPoint(path_points_dp.ToArray(), tag, parent, ball_prefab, ball_size);
    }

    void showPoint(Vector3[] path_points_dp, string tag, Transform parent, GameObject ball_prefab, float ball_size)
    {
        for (int point_index = 0; point_index < path_points_dp.Length; point_index++)
        {
            GameObject ball = Instantiate(ball_prefab, path_points_dp[point_index], Quaternion.identity);
            ball.transform.localScale = new Vector3(ball_size, ball_size, ball_size);
            ball.name = tag + "_" + point_index.ToString();
            ball.transform.parent = parent;
        }
    }
}