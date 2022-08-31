using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Experimental.TerrainAPI.TerrainUtility;

public class TerrainAnalyzeMaster : MonoBehaviour
{
    [SerializeField]
    Terrain mountain;
    [SerializeField]
    float terrain_interval;
    [SerializeField]
    int sample_rate;
    [SerializeField]
    int method; // 0 Bilinear, 1 IDW
    [SerializeField]
    string file_path = "featureAnalyze/features.f";
    [SerializeField]
    Material terrain_mat;

    [SerializeField]
    bool load;
    [SerializeField]
    bool generate;
    [SerializeField]
    bool do_mse;
    [SerializeField]
    Vector2 corner;
    GameObject terrain_manager;
    // Start is called before the first frame update
    void Start()
    {
        terrain_manager = new GameObject("TerrainManager");
    }

    // Update is called once per frame
    void Update()
    {
        if (load)
        {
            load = false;
            TerrainGenerator.readFeatureFile(Application.streamingAssetsPath + "//" + file_path);
        }

        if (generate)
        {
            generate = false;
            generateAnalyzePatchs();
        }
    }

    void generateAnalyzePatchs()
    {
        generateAnalyzePatch(ref TerrainGenerator.kdtree, corner, new Vector2(1024, 1024));
    }

    void generateAnalyzePatch(ref KDTree kdtree, Vector2 corner, Vector2 length)
    {
        int piece_x_num = Mathf.RoundToInt(length.x / terrain_interval) + 1;
        int piece_z_num = Mathf.RoundToInt(length.y / terrain_interval) + 1;
        Mesh mesh = new Mesh();
        float[,,] terrain_points = new float[piece_x_num, piece_z_num, 3];
        Vector3[] vertice = new Vector3[piece_x_num * piece_z_num];
        //Vector2[] uv = new Vector2[x_small_length * z_small_length];
        int[] indices = new int[6 * (piece_x_num - 1) * (piece_z_num - 1)];
        int indices_index = 0;
        float center_x = corner.x + length.x / 2;
        float center_z = corner.y + length.y / 2;
        //float center_y = min_y + getDEMHeight(center_x, center_z);
        WVec3 center_wvec3 = new WVec3();
        if (!kdtree.findNearestPoint(ref center_wvec3, center_x, center_z))
            Debug.LogWarning($"Not found {center_x} {center_z}");
        float center_y = center_wvec3.y;
        Vector3 center = new Vector3(center_x, center_y, center_z);
        float mse = 0.0f;
        for (int i = 0; i < piece_x_num; i++)
        {
            for (int j = 0; j < piece_z_num; j++)
            {
                terrain_points[i, j, 0] = corner.x + i * terrain_interval;
                terrain_points[i, j, 2] = corner.y + j * terrain_interval;
                if (kdtree.getAreaPoints(terrain_points[i, j, 0] - 1, terrain_points[i, j, 2] - 1, terrain_points[i, j, 0] + 1, terrain_points[i, j, 2] + 1).Length == 1)//(i % sample_rate == 0 && j % sample_rate == 0)
                {
                    WVec3 result = new WVec3();
                    if (!kdtree.findNearestPoint(ref result, terrain_points[i, j, 0], terrain_points[i, j, 2]))
                        Debug.LogWarning($"Not found {terrain_points[i, j, 0]} {terrain_points[i, j, 2]}");
                    terrain_points[i, j, 1] = result.y; // min_y is a bias  -15
                }
                else
                {
                    terrain_points[i, j, 1] = getInterpolate(ref kdtree, corner, terrain_points[i, j, 0], terrain_points[i, j, 2], sample_rate, terrain_interval);
                }
                vertice[i * piece_z_num + j] = new Vector3(terrain_points[i, j, 0] - center.x, terrain_points[i, j, 1] - center.y, terrain_points[i, j, 2] - center.z);
                //uv[i * z_small_length + j] = new Vector2((float)(x_small_min + i) / x_patch_num, (float)(z_small_min + j) / z_patch_num);

                if (do_mse)
                {
                    mse += Mathf.Pow(terrain_points[i, j, 1] - mountain.SampleHeight(new Vector3(terrain_points[i, j, 0], 0, terrain_points[i, j, 2])), 2);
                }
            }
        }

        mse /= piece_x_num * piece_z_num;
        Debug.Log($"MSE: {mse}");

        for (int i = 0; i < piece_x_num - 1; i++)
        {
            for (int j = 0; j < piece_z_num - 1; j++)
            {
                // counter-clockwise
                indices[indices_index++] = i * piece_z_num + j;
                indices[indices_index++] = (i + 1) * piece_z_num + j + 1;
                indices[indices_index++] = (i + 1) * piece_z_num + j;
                indices[indices_index++] = i * piece_z_num + j;
                indices[indices_index++] = i * piece_z_num + j + 1;
                indices[indices_index++] = (i + 1) * piece_z_num + j + 1;
            }
        }

        mesh.vertices = vertice;
        //mesh.uv = uv;
        mesh.triangles = indices;
        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        //Name the mesh
        mesh.name = "terrain_mesh";
        GameObject terrain = new GameObject("terrain_Bilinear");
        MeshFilter mf = terrain.AddComponent<MeshFilter>();
        MeshRenderer mr = terrain.AddComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = terrain_mat;
        terrain.transform.position = center;
        terrain.transform.parent = terrain_manager.transform;
    }

    float getInterpolate(ref KDTree kdtree, Vector2 corner, float x, float z, int sample_rate, float interval)
    {
        if (method == 0) // Bilinear
        {
            x -= corner.x;
            z -= corner.y;
            float small_corner_x = Mathf.FloorToInt(x / (interval * sample_rate)) * (interval * sample_rate);
            float small_corner_z = Mathf.FloorToInt(z / (interval * sample_rate)) * (interval * sample_rate);
            float x_rate = 1 - (x - small_corner_x) / (interval * sample_rate);
            float z_rate = 1 - (z - small_corner_z) / (interval * sample_rate);
            float small_end_x = Mathf.CeilToInt(x / (interval * sample_rate)) * (interval * sample_rate);
            float small_end_z = Mathf.CeilToInt(z / (interval * sample_rate)) * (interval * sample_rate);
            WVec3 result = new WVec3();
            small_corner_x += corner.x;
            small_corner_z += corner.y;
            small_end_x += corner.x;
            small_end_z += corner.y;
            if (!kdtree.findNearestPoint(ref result, small_corner_x, small_corner_z))
                Debug.LogWarning($"I Not found {small_corner_x} {small_corner_z}");
            float a = result.y;
            if (!kdtree.findNearestPoint(ref result, small_corner_x, small_end_z))
                Debug.LogWarning($"I Not found {small_corner_x} {small_end_z}");
            float b = result.y;
            if (!kdtree.findNearestPoint(ref result, small_end_x, small_end_z))
                Debug.LogWarning($"I Not found {small_end_x} {small_end_z}");
            float c = result.y;
            if (!kdtree.findNearestPoint(ref result, small_end_x, small_corner_z))
                Debug.LogWarning($"I Not found {small_end_x} {small_corner_z}");
            float d = result.y;

            return a * x_rate * z_rate + b * x_rate * (1 - z_rate) + c * (1 - x_rate) * (1 - z_rate) + d * (1 - x_rate) * z_rate;
        }
        else // IDW
        {
            IDW.dist_threshold = 40.0f;
            float extend = 40.0f;
            int min_feature_num = 21;
            int[] area_features_index;
            do
            {
                area_features_index = kdtree.getAreaPoints(x - extend, z - extend, x + extend, z + extend);
                extend += 6.0f;
                IDW.dist_threshold = extend;
            }
            while (area_features_index.Length < min_feature_num);
            Vector4[] area_features = new Vector4[area_features_index.Length];
            for (int area_features_index_index = 0; area_features_index_index < area_features_index.Length; area_features_index_index++)
            {
                WVec3 feature = kdtree.nodes[area_features_index[area_features_index_index]];
                area_features[area_features_index_index] = new Vector4(feature.x, feature.y, feature.z, feature.w);
            }
            return IDW.inverseDistanceWeighting(area_features, x, z);
        }
    }
}