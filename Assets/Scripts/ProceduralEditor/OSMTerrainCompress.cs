using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;

public class OSMTerrainCompress : MonoBehaviour
{
    OSMEditor osm_editor;
    public bool is_initial = false;
    public GameObject test_ball;
    Vector3[] point_cloud;

    // Start is called before the first frame update
    void Start()
    {
        osm_editor = GetComponent<OSMEditor>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!is_initial && osm_editor.is_initial)
        {
            // OSMRoadRender is initail
            is_initial = true;

            // process roads board
            //for (int road_index = 0; road_index < osm_editor.osm_reader.pathes.Count; road_index++)
            {
                //List<Vector3> path_points = osm_editor.osm_reader.toPositions(osm_editor.osm_reader.pathes[osm_editor.osm_reader.getPathIndex("28492749")].ref_node);
                //List<Vector3> path_points_dp = DouglasPeuckerAlgorithm.DouglasPeucker(path_points, 16.0f);

                //showPoint(path_points_dp);
                // roads
                //generateRoadBoards(osm_editor.osm_reader.pathes[road_index]);
            }

            // terrain board points test
            List<Vector3> terrain_board_points = new List<Vector3>() { new Vector3(8302.3f, 0.0f, 7605.3f), new Vector3(8459.0f, 0.0f, 8052.4f), new Vector3(8640.0f, 0.0f, 8133.8f), new Vector3(9204.0f, 0.0f, 8206.0f), new Vector3(9387.0f, 0.0f, 8022.0f), new Vector3(9411.0f, 0.0f, 7412.7f) };
            List<Vector3> terrain_board_lonlats = new List<Vector3>();
            foreach (Vector3 terrain_board_point in terrain_board_points)
            {
                Vector3 terrain_board_lonlat = new Vector3();
                osm_editor.osm_reader.toLonAndLat(terrain_board_point.x, terrain_board_point.z, out terrain_board_lonlat.x, out terrain_board_lonlat.z);
                terrain_board_lonlats.Add(terrain_board_lonlat);
            }
            //////////////////////////////get elevations/////////////////////////////////////////
            List<float> all_elevations = new List<float>();
            List<EarthCoord> all_coords = new List<EarthCoord>();
            for (int point_index = 0; point_index < terrain_board_lonlats.Count; point_index++)
            {
                all_coords.Add(new EarthCoord(terrain_board_lonlats[point_index].x, terrain_board_lonlats[point_index].z));
            }
            all_elevations = HgtReader.getElevations(all_coords);
            ///////////////////////////////////////////////////////////////////////////////////
            for (int point_index = 0; point_index < terrain_board_lonlats.Count; point_index++)
            {
                float unity_x, unity_z;
                osm_editor.osm_reader.toUnityLocation(terrain_board_lonlats[point_index].x, terrain_board_lonlats[point_index].z, out unity_x, out unity_z);
                terrain_board_points[point_index] = new Vector3(unity_x, all_elevations[point_index], unity_z);
            }
            //showPoint(terrain_board_points);

            List<Vector3> point_cloud_list = new List<Vector3>();

            // W8D
            for (int point_index = 0; point_index < terrain_board_points.Count; point_index++)
            {
                List<List<Vector3>> w8d = W8D(terrain_board_points[point_index]); // need to limit boundary
                foreach (List<Vector3> points in w8d)
                {
                    point_cloud_list.AddRange(points);
                }
            }

            point_cloud = point_cloud_list.ToArray();

            generateTINTerrain(point_cloud);
        }
    }

    void showPoint(List<Vector3> path_points_dp)
    {
        foreach (Vector3 point in path_points_dp)
        {
            GameObject ball = Instantiate(test_ball, point, Quaternion.identity);
            ball.transform.localScale = new Vector3(10, 10, 10);
            ball.name = "ball";
        }
    }

    void generateRoadBoards(List<Vector3> path_points_dp) // generate pieces of road
    {
        
    }

    List<List<Vector3>> W8D(Vector3 center)
    {
        Vector3[] directions = new Vector3[8] { new Vector3(1.0f,0.0f,0.0f), new Vector3(0.707f, 0.0f, 0.707f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(-0.707f, 0.0f, 0.707f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(-0.707f, 0.0f, -0.707f), new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.707f, 0.0f, -0.707f) };
        List<List<Vector3>> terrain_feature_points = new List<List<Vector3>>();
        
        for (int dir = 0; dir < 8; dir++)
        {
            List<Vector3> terrain_feature_lonlats = new List<Vector3>();
            terrain_feature_points.Add(new List<Vector3>());
            for (float d = 0.0f; d < 1300.0f; d += 50.0f)
            {
                Vector3 terrain_feature_point = center + d * directions[dir];
                Vector3 terrain_feature_lonlat = new Vector3();
                osm_editor.osm_reader.toLonAndLat(terrain_feature_point.x, terrain_feature_point.z, out terrain_feature_lonlat.x, out terrain_feature_lonlat.z);
                terrain_feature_lonlats.Add(terrain_feature_lonlat);
            }
            //////////////////////////////get elevations/////////////////////////////////////////
            List<float> all_elevations = new List<float>();
            List<EarthCoord> all_coords = new List<EarthCoord>();
            for (int point_index = 0; point_index < terrain_feature_lonlats.Count; point_index++)
            {
                all_coords.Add(new EarthCoord(terrain_feature_lonlats[point_index].x, terrain_feature_lonlats[point_index].z));
            }
            all_elevations = HgtReader.getElevations(all_coords);
            ///////////////////////////////////////////////////////////////////////////////////
            for (int point_index = 0; point_index < terrain_feature_lonlats.Count; point_index++)
            {
                float unity_x, unity_z;
                osm_editor.osm_reader.toUnityLocation(terrain_feature_lonlats[point_index].x, terrain_feature_lonlats[point_index].z, out unity_x, out unity_z);
                terrain_feature_points[dir].Add(new Vector3(unity_x, all_elevations[point_index], unity_z));
            }

            terrain_feature_points[dir] = DouglasPeuckerAlgorithm.DouglasPeucker(terrain_feature_points[dir], 4.0f);
            showPoint(terrain_feature_points[dir]);
        }
        
        return terrain_feature_points;
    }

    List<List<int>> DeWall(List<int> point_index_list, List<List<int>> afl, bool is_vertical_alpha)
    {
        List<List<int>> simplex_list = new List<List<int>>();

        float alpha = 0.0f;
        List<int> point1_index_list = new List<int>();
        List<int> point2_index_list = new List<int>();
        pointsetPartition(ref point_index_list, ref alpha, is_vertical_alpha, ref point1_index_list, ref point2_index_list);

        if (afl.Count == 0)
        {

        }

        return simplex_list;
    }

    void pointsetPartition(ref List<int> point_index_list, ref float alpha_pxz, bool is_vertical_alpha, ref List<int> point1_index_list, ref List<int> point2_index_list)
    {
        mergeSortForPointCloud(ref point_index_list, 0, point_index_list.Count, point_index_list, is_vertical_alpha);
        if (is_vertical_alpha)
        {
            alpha_pxz = (point_cloud[point_index_list[point_index_list.Count / 2 - 1]].x + point_cloud[point_index_list[point_index_list.Count / 2]].x) / 2;

            foreach (int point_index in point_index_list)
            {
                if (point_cloud[point_index].x < alpha_pxz)
                    point1_index_list.Add(point_index);
                else
                    point2_index_list.Add(point_index);
            }
            //float max_x = point_cloud[point_index_list[0]].x;
            //float min_x = point_cloud[point_index_list[0]].x;
            //foreach (int point_index in point_index_list)
            //{
            //    max_x = Mathf.Max(max_x, point_cloud[point_index].x);
            //    min_x = Mathf.Min(min_x, point_cloud[point_index].x);
            //}

            //int p1_count, p2_count;
            //alpha_pxz = (max_x + min_x) / 2;
            //while (true)
            //{
            //    p1_count = 0;
            //    p2_count = 0;

            //}
        }
        else
        {
            alpha_pxz = (point_cloud[point_index_list[point_index_list.Count / 2 - 1]].z + point_cloud[point_index_list[point_index_list.Count / 2]].z) / 2;

            foreach (int point_index in point_index_list)
            {
                if (point_cloud[point_index].z < alpha_pxz)
                    point1_index_list.Add(point_index);
                else
                    point2_index_list.Add(point_index);
            }
        }
    }

    void makeFirstSimplex()
    {

    }

    float circumCircleRadius(Vector3 P1, Vector3 P2, Vector3 P3)
    {
        float a = Vector3.Distance(P1, P2);
        float b = Vector3.Distance(P2, P3);
        float c = Vector3.Distance(P1, P3);
        float s = (a + b + c) / 2;
        float A = Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
        return a * b * c / (4 * A);
    }

    void generateTINTerrain(Vector3[] point_cloud)
    {
        List<int> point_index_list = new List<int>();
        for (int point_index = 0; point_index < point_cloud.Length; point_index++)
        {
            point_index_list.Add(point_index);
        }

        DeWall(point_index_list, new List<List<int>>(), true);

        Mesh mesh = new Mesh();
        Vector3[] vertice = new Vector3[point_cloud.Length];

        //int[] indices = new int[6 * resolution * resolution];
        //int indices_index = 0;
        //for (int i = 0; i < resolution + 1; i++)
        //{
        //    for (int j = 0; j < resolution + 1; j++)
        //    {
        //        //float pos_x, pos_z;
        //        //osm_editor.osm_reader.toUnityLocation(terrain_points[i, j].x, terrain_points[i, j].z, out pos_x, out pos_z);
        //        terrain_points[i, j, 0] = MercatorProjection.lonToX(terrain_points[i, j, 0]) - osm_editor.osm_reader.boundary_min.x;
        //        terrain_points[i, j, 1] = all_elevations[i * (resolution + 1) + j];
        //        terrain_points[i, j, 2] = MercatorProjection.latToY(terrain_points[i, j, 2]) - osm_editor.osm_reader.boundary_min.y;
        //        //terrain_points[i, j].x = pos_x;
        //        //terrain_points[i, j].y = all_elevations[i * (resolution + 1) + j];
        //        //terrain_points[i, j].z = pos_z;
        //        vertice[i * (resolution + 1) + j] = new Vector3((float)terrain_points[i, j, 0], (float)terrain_points[i, j, 1], (float)terrain_points[i, j, 2]);
        //        //vertice2D[i * (resolution + 1) + j] = new Vector2(terrain_points[i, j].x, terrain_points[i, j].z);
        //    }
        //}
        //for (int i = 0; i < resolution; i++)
        //{
        //    for (int j = 0; j < resolution; j++)
        //    {
        //        // counter-clockwise
        //        indices[indices_index++] = i * (resolution + 1) + j;
        //        indices[indices_index++] = (i + 1) * (resolution + 1) + j + 1;
        //        indices[indices_index++] = (i + 1) * (resolution + 1) + j;
        //        indices[indices_index++] = i * (resolution + 1) + j;
        //        indices[indices_index++] = i * (resolution + 1) + j + 1;
        //        indices[indices_index++] = (i + 1) * (resolution + 1) + j + 1;
        //    }
        //}
        ////for (int i = 0; i < resolution * resolution * 6; i++)
        ////{
        ////    if (i % 6 == 0)
        ////        Debug.Log("==============");
        ////    Debug.Log(indices[i]);
        ////}
        //// Use the triangulator to get indices for creating triangles
        ////Triangulator tr = new Triangulator(vertice2D);
        ////int[] indices = tr.Triangulate();
        ////Assign data to mesh
        //mesh.vertices = vertice;
        //mesh.triangles = indices;
        ////Recalculations
        //mesh.RecalculateNormals();
        //mesh.RecalculateBounds();
        //mesh.Optimize();
        ////Name the mesh
        //mesh.name = "terrain_mesh";
        //GameObject terrain = new GameObject("terrain");
        //MeshFilter mf = terrain.AddComponent<MeshFilter>();
        //MeshRenderer mr = terrain.AddComponent<MeshRenderer>();
        //mf.mesh = mesh;
        //mr.material = terrain_mat;
    }

    void mergeSortForPointCloud(ref List<int> point_index_list, int x, int y, List<int> point_index_list_t, bool is_vertical_alpha)
    {
        if (y - x > 1)
        {
            int m = x + (y - x) / 2;
            mergeSortForPointCloud(ref point_index_list, x, m, point_index_list, is_vertical_alpha);
            mergeSortForPointCloud(ref point_index_list, m, y, point_index_list, is_vertical_alpha);
            int p = x, q = m;
            int index = x;
            while (p < m && q < y)
            {
                if ((is_vertical_alpha && point_cloud[point_index_list[p]].x < point_cloud[point_index_list[q]].x) ||
                    (point_cloud[point_index_list[p]].z < point_cloud[point_index_list[q]].z))
                    point_index_list_t[index++] = point_index_list[p++];
                else
                    point_index_list_t[index++] = point_index_list[q++];
            }
            while (p < m)
                point_index_list_t[index++] = point_index_list[p++];
            while (q < y)
                point_index_list_t[index++] = point_index_list[q++];
            for (int i = x; i < y; i++)
                point_index_list[i] = point_index_list_t[i];
        }
    }
}