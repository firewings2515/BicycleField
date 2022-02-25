using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HeightmapCompress : MonoBehaviour
{
    public Texture2D heightmap_edge;
    float map_size = 22.5f;
    int resolution = 2048;
    public bool get_feature;
    public GameObject test_ball;
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
            GameObject feature_manager = new GameObject("feature_manager");
            List<Vector3> point_cloud_list = new List<Vector3>();

            // W8D
            //for (int point_index = 0; point_index < terrain_board_points.Length; point_index++)
            //{
            for (float edge_x = 0.0f; edge_x < map_size; edge_x += 0.1f)
            {
                for (float edge_z = 0.0f; edge_z < map_size; edge_z += 0.1f)
                {
                    Color gray = heightmap_edge.GetPixel(Mathf.FloorToInt(edge_x / map_size * heightmap_edge.width), Mathf.FloorToInt(edge_z / map_size * heightmap_edge.height));
                    if (gray.r > 0.5f)
                    {
                        List<List<Vector3>> w8d = W8D(map_size, map_size, new Vector3(edge_x, 0.0f, edge_z), point_cloud_list); // need to limit boundary
                        for (int w8d_index = 0; w8d_index < w8d.Count; w8d_index++)
                        {
                            point_cloud_list.AddRange(w8d[w8d_index]);
                        }
                    }
                }
            }
            //}

            point_cloud = point_cloud_list.ToArray();
            showPoint(point_cloud, "Feature", feature_manager.transform);
        }
    }

    List<List<Vector3>> W8D(float width_boundary, float height_boundary, Vector3 center, List<Vector3> point_cloud_list)
    {
        Vector3[] directions = new Vector3[8] { new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.707f, 0.0f, 0.707f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(-0.707f, 0.0f, 0.707f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(-0.707f, 0.0f, -0.707f), new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.707f, 0.0f, -0.707f) };
        List<List<Vector3>> terrain_feature_points = new List<List<Vector3>>();

        for (int dir = 0; dir < 8; dir++)
        {
            List<Vector3> terrain_feature_readys = new List<Vector3>();
            terrain_feature_points.Add(new List<Vector3>());
            for (float d = 0.0f; d < map_size * map_size; d += 0.125f)
            {
                Vector3 terrain_feature_ready = center + d * directions[dir];
                if (terrain_feature_ready.x < 0.0f || terrain_feature_ready.z < 0.0f || terrain_feature_ready.x > width_boundary || terrain_feature_ready.z > height_boundary)
                    break;
                Color gray = heightmap_edge.GetPixel(Mathf.FloorToInt(terrain_feature_ready.x / map_size * heightmap_edge.width), Mathf.FloorToInt(terrain_feature_ready.z / map_size * heightmap_edge.height));
                //terrain_feature_lonlats.Add(terrain_feature_lonlat);
                terrain_feature_ready.y = gray.r;
                terrain_feature_readys.Add(terrain_feature_ready);
                // no near detection
                //terrain_feature_points[dir].Add(terrain_feature_ready);
            }
            for (int point_index = 0; point_index < terrain_feature_readys.Count; point_index++)
            {
                bool is_too_near = false;
                for (int point_cloud_index = 0; point_cloud_index < point_cloud_list.Count; point_cloud_index++)
                {
                    if (distance2D(point_cloud_list[point_cloud_index].x, point_cloud_list[point_cloud_index].z, terrain_feature_readys[point_index].x, terrain_feature_readys[point_index].z) < 0.1f)
                    {
                        is_too_near = true;
                        break;
                    }
                }
                if (!is_too_near)
                    terrain_feature_points[dir].Add(terrain_feature_readys[point_index]);
            }

            if (terrain_feature_points[dir].Count > 1)
                terrain_feature_points[dir] = DouglasPeuckerAlgorithm.DouglasPeucker(terrain_feature_points[dir], 0.1f);
        }

        return terrain_feature_points;
    }

    float distance2D(float x1, float z1, float x2, float z2)
    {
        return Mathf.Sqrt(Mathf.Pow(x1 - x2, 2) + Mathf.Pow(z1 - z2, 2));
    }

    void showPoint(List<Vector3> path_points_dp, string tag, Transform parent)
    {
        showPoint(path_points_dp.ToArray(), tag, parent);
    }

    void showPoint(Vector3[] path_points_dp, string tag, Transform parent)
    {
        for (int point_index = 0; point_index < path_points_dp.Length; point_index++)
        {
            GameObject ball = Instantiate(test_ball, path_points_dp[point_index], Quaternion.identity);
            ball.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            ball.name = tag + "_" + point_index.ToString();
            ball.transform.parent = parent;
        }
    }
}