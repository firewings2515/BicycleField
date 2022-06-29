using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode]
public class FeatureLineReconstruction : MonoBehaviour
{
    public string file_path = "features.f";
    public Texture2D heightmap_edge;
    public Texture2D heightmap;
    public Vector3[] vertice;
    public float[] edges;
    public int x_length = 256;
    public int z_length = 256;
    public float map_size_width = 256;
    public float map_size_height = 256;
    public bool get_line_feature;
    public bool generate_heightmap;
    public bool generate_IDW;
    public GameObject blue_ball;
    public GameObject red_ball;
    public float threshold;
    public float epsilon;
    Vector3[] point_cloud;
    public Material terrain_mat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (get_line_feature)
        {
            get_line_feature = false;
            Debug.Log("Get feature line start");
            GameObject feature_manager = new GameObject("feature_manager");
            List<Vector3> point_cloud_list = new List<Vector3>();

            // W8D in terrain edge detection
            x_length = heightmap.width;
            z_length = heightmap.height;
            bool[] flag = new bool[x_length * z_length];
            edges = new float[x_length * z_length];
            vertice = new Vector3[x_length * z_length];
            for (int x = 0; x < x_length; x++)
            {
                for (int z = 0; z < z_length; z++)
                {
                    Color height = heightmap.GetPixel(x, z);
                    vertice[x * z_length + z] = new Vector3(x, height.r * 255, z);
                    height = heightmap_edge.GetPixel(x, z);
                    edges[x * z_length + z] = height.r;
                }
            }

            for (int x = 0; x < x_length; x++)
            {
                for (int z = 0; z < z_length; z++)
                {
                    if (edges[x * z_length + z] > threshold)
                    {
                        List<List<Vector3>> w8d = W8DGrid(x_length, z_length, x, z, point_cloud_list, flag);
                        Vector3[] w8d_center = new Vector3[1];
                        w8d_center[0] = vertice[x * z_length + z];
                        showPoint(w8d_center, "Feature_Center", feature_manager.transform, red_ball, 2.0f);
                        for (int w8d_index = 0; w8d_index < w8d.Count; w8d_index++)
                        {
                            point_cloud_list.AddRange(w8d[w8d_index]);
                        }
                    }
                }
            }

            point_cloud = point_cloud_list.ToArray();
            WVec3[] w_vec3 = new WVec3[point_cloud.Length];
            for (int i = 0; i < w_vec3.Length; i++)
            {
                w_vec3[i].x = point_cloud[i].x;
                w_vec3[i].y = point_cloud[i].y;
                w_vec3[i].z = point_cloud[i].z;
                w_vec3[i].w = 1;
            }
            TerrainGenerator.features = w_vec3;
            PublicOutputInfo.piece_length = 1;
            TerrainGenerator.min_x = 0;
            TerrainGenerator.min_y = 0;
            TerrainGenerator.min_z = 0;
            TerrainGenerator.x_patch_num = 256;
            TerrainGenerator.z_patch_num = 256;
            TerrainGenerator.terrains = new GameObject[1];
            TerrainGenerator.terrain_mat = terrain_mat;
            showPoint(point_cloud, "Feature", feature_manager.transform, blue_ball, 1.0f);
            Debug.Log(point_cloud.Length);
            Debug.Log("Get feature finish");
            TerrainGenerator.kdtree = new KDTree();
            TerrainGenerator.kdtree.buildKDTree(w_vec3);
        }

        if (generate_heightmap)
        {
            generate_heightmap = false;

            TerrainGenerator.generateSmallHeightmapTerrain(heightmap, 0, 0, 255, 255);
        }

        if (generate_IDW)
        {
            generate_IDW = false;
            
            TerrainGenerator.generateTerrainPatch(0, 0, 255, 255);
            exportSmallTexture(x_length, z_length, vertice, 255);
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

    Texture2D exportSmallTexture(int x_length, int z_length, Vector3[] vertice, float max_height)
    {
        //first Make sure you're using RGB24 as your texture format
        Texture2D texture2D = new Texture2D(x_length, z_length, TextureFormat.RGBA32, false);

        for (int i = 0; i < x_length; i++)
        {
            for (int j = 0; j < z_length; j++)
            {
                float gray = vertice[i * z_length + j].y / max_height;
                texture2D.SetPixel(i, j, new Color(gray, gray, gray));
            }
        }

        //then Save To Disk as PNG
        byte[] bytes = texture2D.EncodeToPNG();
        var dirPath = Application.dataPath + "/Resources/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "FeatureLineTerrainImage" + ".png", bytes);

        return texture2D;
    }
}