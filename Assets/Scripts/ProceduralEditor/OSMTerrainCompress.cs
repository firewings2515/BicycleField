using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;
using System.Linq;

public class OSMTerrainCompress : MonoBehaviour
{
    OSMEditor osm_editor;
    public bool is_initial = false;
    public GameObject test_ball;
    Vector3[] point_cloud;
    bool[] point_cloud_valid;
    bool[] point_cloud_inside;
    public Material terrain_mat;
    int p_c = 0;

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
            Vector3[] terrain_board_points = new Vector3[] { new Vector3(8302.3f, 0.0f, 7605.3f), new Vector3(8459.0f, 0.0f, 8052.4f), new Vector3(8640.0f, 0.0f, 8133.8f), new Vector3(9204.0f, 0.0f, 8206.0f), new Vector3(9387.0f, 0.0f, 8022.0f), new Vector3(9411.0f, 0.0f, 7412.7f) };
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
            showPoint(terrain_board_points, "board");

            List<Vector3> point_cloud_list = new List<Vector3>();

            // W8D
            for (int point_index = 0; point_index < terrain_board_points.Length; point_index++)
            {
                List<List<Vector3>> w8d = W8D(terrain_board_points, terrain_board_points[point_index], point_cloud_list); // need to limit boundary
                foreach (List<Vector3> points in w8d)
                {
                    point_cloud_list.AddRange(points);
                }
            }

            point_cloud = point_cloud_list.ToArray();
            point_cloud_valid = new bool[point_cloud_list.Count];
            point_cloud_inside = new bool[point_cloud_list.Count];
            for (int point_cloud_valid_index = 0; point_cloud_valid_index < point_cloud_valid.Length; point_cloud_valid_index++)
                point_cloud_valid[point_cloud_valid_index] = true;
            for (int point_cloud_inside_index = 0; point_cloud_inside_index < point_cloud_inside.Length; point_cloud_inside_index++)
                point_cloud_inside[point_cloud_inside_index] = false;
            showPoint(point_cloud, "feature");

            Vector4[] point_cloud_w = new Vector4[point_cloud.Length];
            for (int i = 0; i < point_cloud.Length; i++)
            {
                point_cloud_w[i] = new Vector4(point_cloud[i].x, point_cloud[i].y, point_cloud[i].z, 1);
            }
            generateIDWTerrain(point_cloud_w);
            //generateTINTerrain(point_cloud);
        }
    }

    void showPoint(List<Vector3> path_points_dp, string tag)
    {
        showPoint(path_points_dp.ToArray(), tag);
    }

    void showPoint(Vector3[] path_points_dp, string tag)
    {
        for (int point_index = 0; point_index < path_points_dp.Length; point_index++)
        {
            GameObject ball = Instantiate(test_ball, path_points_dp[point_index], Quaternion.identity);
            ball.transform.localScale = new Vector3(10, 10, 10);
            ball.name = tag + "_" + point_index.ToString();
        }
    }

    void generateRoadBoards(List<Vector3> path_points_dp) // generate pieces of road
    {

    }

    List<List<Vector3>> W8D(Vector3[] terrain_board_points, Vector3 center, List<Vector3> point_cloud_list)
    {
        Vector3[] directions = new Vector3[8] { new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.707f, 0.0f, 0.707f), new Vector3(0.0f, 0.0f, 1.0f), new Vector3(-0.707f, 0.0f, 0.707f), new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(-0.707f, 0.0f, -0.707f), new Vector3(0.0f, 0.0f, -1.0f), new Vector3(0.707f, 0.0f, -0.707f) };
        List<List<Vector3>> terrain_feature_points = new List<List<Vector3>>();

        for (int dir = 0; dir < 8; dir++)
        {
            List<Vector3> terrain_feature_lonlats = new List<Vector3>();
            terrain_feature_points.Add(new List<Vector3>());
            for (float d = 0.0f; d < 1300.0f; d += 50.0f)
            {
                Vector3 terrain_feature_point = center + d * directions[dir];
                if (!isInBoard(terrain_board_points, terrain_feature_point))
                    break;
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
                bool is_too_near = false;
                for (int point_cloud_index = 0; point_cloud_index < point_cloud_list.Count; point_cloud_index++)
                {
                    if (distance2D(point_cloud_list[point_cloud_index].x, point_cloud_list[point_cloud_index].z, unity_x, unity_z) < 25.0f)
                    {
                        is_too_near = true;
                        break;
                    }
                }
                if (!is_too_near)
                    terrain_feature_points[dir].Add(new Vector3(unity_x, all_elevations[point_index], unity_z));
            }

            terrain_feature_points[dir] = DouglasPeuckerAlgorithm.DouglasPeucker(terrain_feature_points[dir], 4.0f);
        }

        return terrain_feature_points;
    }

    List<List<int>> DeWall(List<int> point_index_list, List<KeyValuePair<int, int>> afl, ref List<KeyValuePair<int, int>> afl_remain, ref int[] afl_point_connect_count, bool is_vertical_alpha, int growth_dir, int p)
    {
        List<List<int>> simplex_list = new List<List<int>>();
        //if (p >= 3) return simplex_list;
        float alpha = 0.0f;
        List<int> point1_index_list = new List<int>();
        List<int> point2_index_list = new List<int>();
        List<KeyValuePair<int, int>> afl1 = new List<KeyValuePair<int, int>>();
        List<KeyValuePair<int, int>> afl2 = new List<KeyValuePair<int, int>>();
        if (afl.Count == 0)
        {
            pointsetPartition(ref point_index_list, ref alpha, is_vertical_alpha, ref point1_index_list, ref point2_index_list);
            Debug.Log("alpha: " + alpha.ToString());
            List<List<int>> simplex_list_t = makeSimplex(point_index_list, alpha, is_vertical_alpha, ref afl_remain, ref afl_point_connect_count, ref afl1, ref afl2, 0, new KeyValuePair<int, int>()); // MakeFirstSimplex
            simplex_list.AddRange(simplex_list_t);
        }
        else
        {
            KeyValuePair<int, int> middle_afl = pointsetPartition(ref point_index_list, ref alpha, is_vertical_alpha, ref point1_index_list, ref point2_index_list, afl);
            Debug.Log("alpha: " + alpha.ToString() + "afl: " + middle_afl.ToString());
            List<List<int>> simplex_list_t = makeSimplex(point_index_list, alpha, is_vertical_alpha, ref afl_remain, ref afl_point_connect_count, ref afl1, ref afl2, growth_dir, middle_afl); // MakeFirstSimplex
            simplex_list.AddRange(simplex_list_t);
        }

        if (afl1.Count > 0)
        {
            if (is_vertical_alpha)
                simplex_list.AddRange(DeWall(point1_index_list, afl1, ref afl_remain, ref afl_point_connect_count, !is_vertical_alpha, 1, p + 1));
            else
                simplex_list.AddRange(DeWall(point1_index_list, afl1, ref afl_remain, ref afl_point_connect_count, !is_vertical_alpha, -1, p + 1));
        }
        if (afl2.Count > 0)
        {
            if (is_vertical_alpha)
                simplex_list.AddRange(DeWall(point2_index_list, afl2, ref afl_remain, ref afl_point_connect_count, !is_vertical_alpha, -1, p + 1));
            else
                simplex_list.AddRange(DeWall(point2_index_list, afl2, ref afl_remain, ref afl_point_connect_count, !is_vertical_alpha, 1, p + 1));
        }

        return simplex_list;
    }

    void pointsetPartition(ref List<int> point_index_list, ref float alpha_pxz, bool is_vertical_alpha, ref List<int> point1_index_list, ref List<int> point2_index_list)
    {
        mergeSortForPointCloud(ref point_index_list, 0, point_index_list.Count, new List<int>(point_index_list), is_vertical_alpha);
        if (is_vertical_alpha)
        {
            alpha_pxz = (point_cloud[point_index_list[point_index_list.Count / 2 - 1]].x + point_cloud[point_index_list[point_index_list.Count / 2]].x) / 2;

            for (int point_index_index = 0; point_index_index < point_index_list.Count; point_index_index++)
            {
                if (point_cloud[point_index_list[point_index_index]].x < alpha_pxz)
                    point1_index_list.Add(point_index_list[point_index_index]);
                else
                    point2_index_list.Add(point_index_list[point_index_index]);
            }
        }
        else
        {
            alpha_pxz = (point_cloud[point_index_list[point_index_list.Count / 2 - 1]].z + point_cloud[point_index_list[point_index_list.Count / 2]].z) / 2;

            for (int point_index_index = 0; point_index_index < point_index_list.Count; point_index_index++)
            {
                if (point_cloud[point_index_list[point_index_index]].z > alpha_pxz)
                    point1_index_list.Add(point_index_list[point_index_index]);
                else
                    point2_index_list.Add(point_index_list[point_index_index]);
            }
        }
    }

    KeyValuePair<int, int> pointsetPartition(ref List<int> point_index_list, ref float alpha_pxz, bool is_vertical_alpha, ref List<int> point1_index_list, ref List<int> point2_index_list, List<KeyValuePair<int, int>> afl)
    {
        //if (point_index_list.Count < 2)
        //    return afl[0];
        //mergeSortForPointCloud(ref point_index_list, 0, point_index_list.Count, new List<int>(point_index_list), is_vertical_alpha);
        //int select_middle = -1;
        //if (is_vertical_alpha)
        //{
        //    alpha_pxz = (point_cloud[point_index_list[point_index_list.Count / 2 - 1]].x + point_cloud[point_index_list[point_index_list.Count / 2]].x) / 2;

        //    for (int afl_index = 0; afl_index < afl.Count; afl_index++)
        //    {
        //        if ((point_cloud[afl[afl_index].Key].x < alpha_pxz && alpha_pxz < point_cloud[afl[afl_index].Value].x) ||
        //            (point_cloud[afl[afl_index].Value].x < alpha_pxz && alpha_pxz < point_cloud[afl[afl_index].Key].x))
        //        {
        //            select_middle = afl_index; // may two
        //            Debug.Log("--" + afl[afl_index].Key + ", " + afl[afl_index].Value);
        //        }

        //        if (point_cloud[afl[afl_index].Key].x < alpha_pxz && !point1_index_list.Contains(afl[afl_index].Key))
        //        {
        //            point1_index_list.Add(afl[afl_index].Key);
        //        }
        //        if (point_cloud[afl[afl_index].Value].x < alpha_pxz && !point1_index_list.Contains(afl[afl_index].Value))
        //        {
        //            point1_index_list.Add(afl[afl_index].Value);
        //        }
        //        if (point_cloud[afl[afl_index].Key].x > alpha_pxz && !point2_index_list.Contains(afl[afl_index].Key))
        //        {
        //            point2_index_list.Add(afl[afl_index].Key);
        //        }
        //        if (point_cloud[afl[afl_index].Value].x > alpha_pxz && !point2_index_list.Contains(afl[afl_index].Value))
        //        {
        //            point2_index_list.Add(afl[afl_index].Value);
        //        }
        //    }
        //}
        //else
        //{
        //    alpha_pxz = (point_cloud[point_index_list[point_index_list.Count / 2 - 1]].z + point_cloud[point_index_list[point_index_list.Count / 2]].z) / 2;

        //    for (int afl_index = 0; afl_index < afl.Count; afl_index++)
        //    {
        //        if ((point_cloud[afl[afl_index].Key].z < alpha_pxz && alpha_pxz < point_cloud[afl[afl_index].Value].z) ||
        //            (point_cloud[afl[afl_index].Value].z < alpha_pxz && alpha_pxz < point_cloud[afl[afl_index].Key].z))
        //        {
        //            select_middle = afl_index; // may two
        //            Debug.Log("--" + afl[afl_index].Key + ", " + afl[afl_index].Value);
        //        }

        //        if (point_cloud[afl[afl_index].Key].z > alpha_pxz && !point1_index_list.Contains(afl[afl_index].Key))
        //        {
        //            point1_index_list.Add(afl[afl_index].Key);
        //        }
        //        if (point_cloud[afl[afl_index].Value].z > alpha_pxz && !point1_index_list.Contains(afl[afl_index].Value))
        //        {
        //            point1_index_list.Add(afl[afl_index].Value);
        //        }
        //        if (point_cloud[afl[afl_index].Key].z < alpha_pxz && !point2_index_list.Contains(afl[afl_index].Key))
        //        {
        //            point2_index_list.Add(afl[afl_index].Key);
        //        }
        //        if (point_cloud[afl[afl_index].Value].z < alpha_pxz && !point2_index_list.Contains(afl[afl_index].Value))
        //        {
        //            point2_index_list.Add(afl[afl_index].Value);
        //        }
        //    }
        //}

        //if (select_middle == -1)
        //    Debug.Log("--qq");

        mergeSortForPointCloud(ref point_index_list, 0, point_index_list.Count, new List<int>(point_index_list), is_vertical_alpha);
        List<KeyValuePair<float, int>> middle_xz = new List<KeyValuePair<float, int>>();
        for (int afl_index = 0; afl_index < afl.Count; afl_index++)
        {
            if (is_vertical_alpha)
            {
                middle_xz.Add(new KeyValuePair<float, int>((point_cloud[afl[afl_index].Key].x + point_cloud[afl[afl_index].Value].x) / 2, afl_index));
            }
            else
            {
                middle_xz.Add(new KeyValuePair<float, int>((point_cloud[afl[afl_index].Key].z + point_cloud[afl[afl_index].Value].z) / 2, afl_index));
            }
        }
        mergeSortForAFL(ref middle_xz, 0, middle_xz.Count, new List<KeyValuePair<float, int>>(middle_xz), is_vertical_alpha);
        alpha_pxz = middle_xz[middle_xz.Count / 2].Key;
        if (is_vertical_alpha)
        {
            for (int point_index_index = 0; point_index_index < point_index_list.Count; point_index_index++)
            {
                if (point_cloud[point_index_list[point_index_index]].x < alpha_pxz)
                    point1_index_list.Add(point_index_list[point_index_index]);
                else
                    point2_index_list.Add(point_index_list[point_index_index]);
            }
        }
        else
        {
            for (int point_index_index = 0; point_index_index < point_index_list.Count; point_index_index++)
            {
                if (point_cloud[point_index_list[point_index_index]].z > alpha_pxz)
                    point1_index_list.Add(point_index_list[point_index_index]);
                else
                    point2_index_list.Add(point_index_list[point_index_index]);
            }
        }
        return afl[middle_xz[middle_xz.Count / 2].Value];
        //return afl[select_middle];
    }

    List<List<int>> makeSimplex(List<int> point_index_list, float alpha_pxz, bool is_vertical_alpha, ref List<KeyValuePair<int, int>> afl_remain, ref int[] afl_point_connect_count, ref List<KeyValuePair<int, int>> afl1, ref List<KeyValuePair<int, int>> afl2, int growth_dir, KeyValuePair<int, int> middle_afl)
    {
        List<List<int>> simplex_list_t = new List<List<int>>();

        int p1_index; //  = point_index_list[point_index_list.Count / 2 - 1]
        int p2_index; // = point_index_list[point_index_list.Count / 2]
        float dist_min; // = distance2D(point_cloud[p1_index], point_cloud[p2_index])
        Queue<KeyValuePair<int, int>> line_q = new Queue<KeyValuePair<int, int>>();
        Queue<int> side_q = new Queue<int>(); // 1 need left, 2 need right, 3 need all
        HashSet<KeyValuePair<int, int>> line_q_lib = new HashSet<KeyValuePair<int, int>>();
        if (growth_dir == 0)
        {
            p1_index = point_index_list[point_index_list.Count / 2 - 1];
            p2_index = point_index_list[point_index_list.Count / 2];
            dist_min = distance2D(point_cloud[p1_index], point_cloud[p2_index]);
            for (int point_index_index = point_index_list.Count / 2 + 1; point_index_index < point_index_list.Count; point_index_index++)
            {
                float dist_min_t = distance2D(point_cloud[p1_index], point_cloud[point_index_list[point_index_index]]);
                if (dist_min > dist_min_t)
                {
                    dist_min = dist_min_t;
                    p2_index = point_index_list[point_index_index];
                }
            }

            line_q.Enqueue(new KeyValuePair<int, int>(p1_index, p2_index));
            line_q.Enqueue(new KeyValuePair<int, int>(p1_index, p2_index));
            side_q.Enqueue(-1);
            side_q.Enqueue(1);
        }
        else
        {
            line_q.Enqueue(middle_afl);
            if (afl_remain.Contains(middle_afl))
            {
                afl_remain.Remove(middle_afl);
                afl_point_connect_count[middle_afl.Key]--;
                if (afl_point_connect_count[middle_afl.Key] == 0)
                    point_cloud_inside[middle_afl.Key] = true;
                afl_point_connect_count[middle_afl.Value]--;
                if (afl_point_connect_count[middle_afl.Value] == 0)
                    point_cloud_inside[middle_afl.Value] = true;
            }
            if (afl_remain.Contains(new KeyValuePair<int, int>(middle_afl.Value, middle_afl.Key)))
            {
                afl_remain.Remove(new KeyValuePair<int, int>(middle_afl.Value, middle_afl.Key));
                afl_point_connect_count[middle_afl.Key]--;
                if (afl_point_connect_count[middle_afl.Key] == 0)
                    point_cloud_inside[middle_afl.Key] = true;
                afl_point_connect_count[middle_afl.Value]--;
                if (afl_point_connect_count[middle_afl.Value] == 0)
                    point_cloud_inside[middle_afl.Value] = true;
            }
            side_q.Enqueue(growth_dir);
        }
        line_q_lib.Add(line_q.Peek());
        while (line_q.Count > 0)
        {
            p1_index = line_q.Peek().Key;
            p2_index = line_q.Peek().Value;
            if ((is_vertical_alpha && point_cloud[p1_index].x > point_cloud[p2_index].x) ||
                (!is_vertical_alpha && point_cloud[p1_index].z < point_cloud[p2_index].z))
            {
                int t = p1_index;
                p1_index = p2_index;
                p2_index = t;
            }
            Debug.Log(p1_index.ToString() + "~" + p2_index.ToString() + " isvertical: " + is_vertical_alpha + " alpha: " + alpha_pxz);
            int p3_index = -1; // only for initial 
            float r_min = float.MaxValue;
            bool is_afl1_point = false;
            bool counter_boundary = false;
            KeyValuePair<int, int> boundary_afl = new KeyValuePair<int, int>();
            for (int point_index_index = 0; point_index_index < point_index_list.Count; point_index_index++)
            {
                if (!point_cloud_valid[point_index_list[point_index_index]] || point_cloud_inside[point_index_list[point_index_index]])
                    continue;
                //if (p1_index == 330 && p2_index == 263)
                //    continue;
                bool is_afl1_point_t = ((is_vertical_alpha && point_cloud[point_index_list[point_index_index]].x < alpha_pxz) || (!is_vertical_alpha && point_cloud[point_index_list[point_index_index]].z > alpha_pxz));
                if (point_index_list[point_index_index] != p1_index && point_index_list[point_index_index] != p2_index &&
                    ((!is_afl1_point_t && !line_q_lib.Contains(new KeyValuePair<int, int>(p1_index, point_index_list[point_index_index])) && !line_q_lib.Contains(new KeyValuePair<int, int>(point_index_list[point_index_index], p1_index))) ||
                     (is_afl1_point_t && !line_q_lib.Contains(new KeyValuePair<int, int>(p2_index, point_index_list[point_index_index])) && !line_q_lib.Contains(new KeyValuePair<int, int>(point_index_list[point_index_index], p2_index)))))
                {
                    float r = circumCircleRadius(point_cloud[p1_index], point_cloud[p2_index], point_cloud[point_index_list[point_index_index]]);
                    //if (r_min > r && ((side_q.Peek() == 2 && pointSide(point_cloud[p1_index], point_cloud[p2_index], point_cloud[point_index_list[point_index_index]]) < 0) ||
                    //                  (side_q.Peek() == 1 && pointSide(point_cloud[p1_index], point_cloud[p2_index], point_cloud[point_index_list[point_index_index]]) > 0)))
                    if (r_min > r && side_q.Peek() == pointSide(point_cloud[p1_index], point_cloud[p2_index], point_cloud[point_index_list[point_index_index]]) && !isContainSimplex(ref simplex_list_t, clockwiseCorrect(new List<int>() { p1_index, p2_index, point_index_list[point_index_index] })))
                    {
                        bool connect_valid = true;
                        for (int point_check_index = 0; point_check_index < point_index_list.Count; point_check_index++)
                        {
                            if (!point_cloud_valid[point_index_list[point_check_index]] || point_cloud_inside[point_index_list[point_index_index]] || point_index_list[point_check_index] == p1_index || point_index_list[point_check_index] == p2_index || point_index_list[point_check_index] == point_index_list[point_index_index])
                                continue;
                            if (pointInTriangle(point_cloud[point_index_list[point_check_index]], point_cloud[p1_index], point_cloud[p2_index], point_cloud[point_index_list[point_index_index]]))
                            {
                                connect_valid = false;
                                break;
                            }
                        }

                        if (!connect_valid)
                            continue;

                        for (int afl_remain_index = 0; afl_remain_index < afl_remain.Count; afl_remain_index++)
                        {
                            //if (p1_index == 316 && p2_index == 236)
                            //    if ((afl_remain[afl_remain_index].Key == 238 && afl_remain[afl_remain_index].Value == 316) || (afl_remain[afl_remain_index].Key == 316 && afl_remain[afl_remain_index].Value == 238))
                            //        Debug.Log("Hello");

                            float cross_a = crossParam(point_cloud[afl_remain[afl_remain_index].Key], point_cloud[afl_remain[afl_remain_index].Value], point_cloud[p1_index], point_cloud[point_index_list[point_index_index]]);

                            if ((afl_remain[afl_remain_index].Key == p1_index && afl_remain[afl_remain_index].Value == point_index_list[point_index_index]) ||
                                (afl_remain[afl_remain_index].Value == p1_index && afl_remain[afl_remain_index].Key == point_index_list[point_index_index]))
                                ;
                            //else if (afl_remain[afl_remain_index].Key == p1_index || afl_remain[afl_remain_index].Value == p1_index || afl_remain[afl_remain_index].Key == point_index_list[point_index_index] || afl_remain[afl_remain_index].Value == point_index_list[point_index_index])
                            //    ;
                            //else if (isIntersect(point_cloud[afl_remain[afl_remain_index].Key], point_cloud[afl_remain[afl_remain_index].Value], point_cloud[p1_index], point_cloud[point_index_list[point_index_index]]))
                            else if (cross_a > 1e-1 && cross_a < 1 - 1e-1 && isIntersect(point_cloud[afl_remain[afl_remain_index].Key], point_cloud[afl_remain[afl_remain_index].Value], point_cloud[p1_index], point_cloud[point_index_list[point_index_index]]))
                            //else if (cross_a > 1e-1 && cross_a < 1 - 1e-1)
                            {
                                connect_valid = false;
                                Debug.Log(afl_remain[afl_remain_index].Key.ToString() + ", " + afl_remain[afl_remain_index].Value.ToString() + " = " + p1_index.ToString() + ", " + point_index_list[point_index_index].ToString() + " -> " + cross_a.ToString());
                                break;
                            }

                            //if (p1_index == 271 && p2_index == 338)
                            //    if ((afl_remain[afl_remain_index].Key == 17 || afl_remain[afl_remain_index].Value == 17))
                            //        Debug.Log("Hello 271 338 17 = " + cross_a.ToString());

                            cross_a = crossParam(point_cloud[afl_remain[afl_remain_index].Key], point_cloud[afl_remain[afl_remain_index].Value], point_cloud[p2_index], point_cloud[point_index_list[point_index_index]]);

                            if ((afl_remain[afl_remain_index].Key == p2_index && afl_remain[afl_remain_index].Value == point_index_list[point_index_index]) ||
                                (afl_remain[afl_remain_index].Value == p2_index && afl_remain[afl_remain_index].Key == point_index_list[point_index_index]))
                                ;
                            //else if (afl_remain[afl_remain_index].Key == p2_index || afl_remain[afl_remain_index].Value == p2_index || afl_remain[afl_remain_index].Key == point_index_list[point_index_index] || afl_remain[afl_remain_index].Value == point_index_list[point_index_index])
                            //    ;
                            //else if (isIntersect(point_cloud[afl_remain[afl_remain_index].Key], point_cloud[afl_remain[afl_remain_index].Value], point_cloud[p2_index], point_cloud[point_index_list[point_index_index]]))
                            else if (cross_a > 1e-1 && cross_a < 1 - 1e-1 && isIntersect(point_cloud[afl_remain[afl_remain_index].Key], point_cloud[afl_remain[afl_remain_index].Value], point_cloud[p2_index], point_cloud[point_index_list[point_index_index]]))
                            //else if (cross_a > 1e-1 && cross_a < 1 - 1e-1)
                            {
                                connect_valid = false;
                                Debug.Log(afl_remain[afl_remain_index].Key.ToString() + ", " + afl_remain[afl_remain_index].Value.ToString() + " = " + p2_index.ToString() + ", " + point_index_list[point_index_index].ToString() + " -> " + cross_a.ToString());
                                break;
                            }

                            //if (p1_index == 271 && p2_index == 338)
                            //    if ((afl_remain[afl_remain_index].Key == 17 || afl_remain[afl_remain_index].Value == 17))
                            //        Debug.Log("Hello 271 338 17 = " + cross_a.ToString());
                        }

                        if (!connect_valid)
                            continue;

                        //counter_boundary = ((is_afl1_point_t && (afl_lib.Contains(new KeyValuePair<int, int>(p2_index, point_index_list[point_index_index])) || afl_lib.Contains(new KeyValuePair<int, int>(point_index_list[point_index_index], p2_index)))) ||
                        //                    (!is_afl1_point_t && (afl_lib.Contains(new KeyValuePair<int, int>(p1_index, point_index_list[point_index_index])) || afl_lib.Contains(new KeyValuePair<int, int>(point_index_list[point_index_index], p1_index)))));

                        if (is_afl1_point_t)
                        {
                            if (afl_remain.Contains(new KeyValuePair<int, int>(p2_index, point_index_list[point_index_index])))
                            {
                                boundary_afl = new KeyValuePair<int, int>(p2_index, point_index_list[point_index_index]);
                                counter_boundary = true;
                            }
                            else if (afl_remain.Contains(new KeyValuePair<int, int>(point_index_list[point_index_index], p2_index)))
                            {
                                boundary_afl = new KeyValuePair<int, int>(point_index_list[point_index_index], p2_index);
                                counter_boundary = true;
                            }
                            else
                            {
                                counter_boundary = false;
                            }
                        }
                        else
                        {
                            if (afl_remain.Contains(new KeyValuePair<int, int>(p1_index, point_index_list[point_index_index])))
                            {
                                boundary_afl = new KeyValuePair<int, int>(p1_index, point_index_list[point_index_index]);
                                counter_boundary = true;
                            }
                            else if (afl_remain.Contains(new KeyValuePair<int, int>(point_index_list[point_index_index], p1_index)))
                            {
                                boundary_afl = new KeyValuePair<int, int>(point_index_list[point_index_index], p1_index);
                                counter_boundary = true;
                            }
                            else
                            {
                                counter_boundary = false;
                            }
                        }

                        r_min = r;
                        p3_index = point_index_list[point_index_index];
                        is_afl1_point = is_afl1_point_t;
                    }
                }
            }

            //if (p3_index > -1)
            //{
            //    KeyValuePair<int, int> simplex_line = new KeyValuePair<int, int>(p1_index, p3_index);
            //    List<int> simplex = clockwiseCorrect(new List<int>() { p1_index, p2_index, p3_index });
            //    if (!simplex_list_t.Contains(simplex))
            //    {
            //        simplex_list_t.Add(simplex);
            //        line_q.Enqueue(simplex_line);
            //        side_q.Enqueue(!side_q.Peek()); // need left (vertical alpha)
            //        Debug.Log(simplex[0].ToString() + ", " + simplex[1].ToString() + ", " + simplex[2].ToString());
            //    }
            //}

            //if (found_afl1_point)
            //{
            //    line_q.Dequeue();
            //    side_q.Dequeue();
            //    continue;
            //}

            //for (int point_index_index = point_index_list.Count / 2 - 2; point_index_index >= 0; point_index_index--)
            //{
            //    if (point_index_list[point_index_index] != p1_index && point_index_list[point_index_index] != p2_index && !line_q_lib.Contains(new KeyValuePair<int, int>(p2_index, point_index_list[point_index_index])) && !line_q_lib.Contains(new KeyValuePair<int, int>(point_index_list[point_index_index], p2_index)))
            //    {
            //        float r = circumCircleRadius(point_cloud[p1_index], point_cloud[p2_index], point_cloud[point_index_list[point_index_index]]);
            //        if (r_min > r && side_q.Peek() == pointSide(point_cloud[p1_index], point_cloud[p2_index], point_cloud[point_index_list[point_index_index]]) && !isContainSimplex(ref simplex_list_t, clockwiseCorrect(new List<int>() { p1_index, p2_index, point_index_list[point_index_index] })))
            //        //if (r_min > r && ((side_q.Peek() == 2 && pointSide(point_cloud[p1_index], point_cloud[p2_index], point_cloud[point_index_list[point_index_index]]) < 0) ||
            //        //                  (side_q.Peek() == 1 && pointSide(point_cloud[p1_index], point_cloud[p2_index], point_cloud[point_index_list[point_index_index]]) > 0)))
            //        {
            //            r_min = r;
            //            p3_index = point_index_list[point_index_index];
            //            is_afl1_point = true;
            //        }
            //    }
            //}

            if (p3_index > -1)
            {
                KeyValuePair<int, int> simplex_line;
                if (is_afl1_point)
                {
                    simplex_line = new KeyValuePair<int, int>(p2_index, p3_index);
                    bool remain_connect_1_3 = afl_remain.Contains(new KeyValuePair<int, int>(p1_index, p3_index));
                    bool remain_connect_3_1 = afl_remain.Contains(new KeyValuePair<int, int>(p3_index, p1_index));
                    if (!remain_connect_1_3 && !remain_connect_3_1)
                    {
                        afl1.Add(new KeyValuePair<int, int>(p1_index, p3_index));
                        afl_point_connect_count[p1_index]++;
                        afl_point_connect_count[p3_index]++;
                    }
                    else if (remain_connect_1_3)
                    {
                        afl_remain.Remove(new KeyValuePair<int, int>(p1_index, p3_index));
                        afl_point_connect_count[p1_index]--;
                        if (afl_point_connect_count[p1_index] == 0)
                            point_cloud_inside[p1_index] = true;
                        afl_point_connect_count[p3_index]--;
                        if (afl_point_connect_count[p3_index] == 0)
                            point_cloud_inside[p3_index] = true;
                    }
                    else if (remain_connect_3_1)
                    {
                        afl_remain.Remove(new KeyValuePair<int, int>(p3_index, p1_index));
                        afl_point_connect_count[p1_index]--;
                        if (afl_point_connect_count[p1_index] == 0)
                            point_cloud_inside[p1_index] = true;
                        afl_point_connect_count[p3_index]--;
                        if (afl_point_connect_count[p3_index] == 0)
                            point_cloud_inside[p3_index] = true;
                    }

                    //bool remain_connect_2_3 = afl_remain.Contains(new KeyValuePair<int, int>(p2_index, p3_index));
                    //bool remain_connect_3_2 = afl_remain.Contains(new KeyValuePair<int, int>(p3_index, p2_index));
                    //if (remain_connect_2_3)
                    //    afl_remain.Remove(new KeyValuePair<int, int>(p2_index, p3_index));
                    //else if (remain_connect_3_2)
                    //    afl_remain.Remove(new KeyValuePair<int, int>(p3_index, p2_index));
                }
                else
                {
                    simplex_line = new KeyValuePair<int, int>(p1_index, p3_index);
                    bool remain_connect_2_3 = afl_remain.Contains(new KeyValuePair<int, int>(p2_index, p3_index));
                    bool remain_connect_3_2 = afl_remain.Contains(new KeyValuePair<int, int>(p3_index, p2_index));
                    if (!remain_connect_2_3 && !remain_connect_3_2)
                    {
                        afl2.Add(new KeyValuePair<int, int>(p2_index, p3_index));
                        afl_point_connect_count[p2_index]++;
                        afl_point_connect_count[p3_index]++;
                    }
                    else if (remain_connect_2_3)
                    {
                        afl_remain.Remove(new KeyValuePair<int, int>(p2_index, p3_index));
                        afl_point_connect_count[p2_index]--;
                        if (afl_point_connect_count[p2_index] == 0)
                            point_cloud_inside[p2_index] = true;
                        afl_point_connect_count[p3_index]--;
                        if (afl_point_connect_count[p3_index] == 0)
                            point_cloud_inside[p3_index] = true;
                    }
                    else if (remain_connect_3_2)
                    {
                        afl_remain.Remove(new KeyValuePair<int, int>(p3_index, p2_index));
                        afl_point_connect_count[p2_index]--;
                        if (afl_point_connect_count[p2_index] == 0)
                            point_cloud_inside[p2_index] = true;
                        afl_point_connect_count[p3_index]--;
                        if (afl_point_connect_count[p3_index] == 0)
                            point_cloud_inside[p3_index] = true;
                    }

                    //bool remain_connect_1_3 = afl_remain.Contains(new KeyValuePair<int, int>(p1_index, p3_index));
                    //bool remain_connect_3_1 = afl_remain.Contains(new KeyValuePair<int, int>(p3_index, p1_index));
                    //if (remain_connect_1_3)
                    //    afl_remain.Remove(new KeyValuePair<int, int>(p1_index, p3_index));
                    //else if (remain_connect_3_1)
                    //    afl_remain.Remove(new KeyValuePair<int, int>(p3_index, p1_index));
                }
                List<int> simplex = clockwiseCorrect(new List<int>() { p1_index, p2_index, p3_index });
                if (!isContainSimplex(ref simplex_list_t, simplex))
                {
                    simplex_list_t.Add(simplex);

                    for (int point_cloude_index = 0; point_cloude_index < point_cloud.Length; point_cloude_index++)
                    {
                        if (point_cloud_valid[point_cloude_index] && point_cloude_index != p1_index && point_cloude_index != p2_index && point_cloude_index != p3_index && pointInTriangle(point_cloud[point_cloude_index], point_cloud[p1_index], point_cloud[p2_index], point_cloud[p3_index]))
                        {
                            point_cloud_valid[point_cloude_index] = false;
                        }
                    }

                    if (!counter_boundary)
                    {
                        line_q.Enqueue(simplex_line);
                        //if (is_afl1_point)
                        //    side_q.Enqueue(1); // need right (vertical alpha)
                        //else
                        //    side_q.Enqueue(-1); // need right (vertical alpha)
                        side_q.Enqueue(side_q.Peek());
                        Debug.Log(simplex[0].ToString() + ", " + simplex[1].ToString() + ", " + simplex[2].ToString() + " is afl1:" + is_afl1_point.ToString());
                    }
                    else
                    {
                        afl_remain.Remove(boundary_afl);
                        afl_point_connect_count[boundary_afl.Key]--;
                        if (afl_point_connect_count[boundary_afl.Key] == 0)
                            point_cloud_inside[boundary_afl.Key] = true;
                        afl_point_connect_count[boundary_afl.Value]--;
                        if (afl_point_connect_count[boundary_afl.Value] == 0)
                            point_cloud_inside[boundary_afl.Value] = true;
                    }
                }
            }
            else
            {
                afl_remain.Add(new KeyValuePair<int, int>(p1_index, p2_index));
                afl_point_connect_count[p1_index]++;
                afl_point_connect_count[p2_index]++;
                //if (afl_remain.Contains(new KeyValuePair<int, int>(271, 16)) || afl_remain.Contains(new KeyValuePair<int, int>(16, 271)))
                //    Debug.Log("271 16");
            }

            line_q.Dequeue();
            side_q.Dequeue();
        }
        //int first_point_index = 0;
        //float point_xz_min;

        //point_xz_min = point_cloud[first_point_index].z;
        //foreach (int point_index in point_index_list) // find bottom
        //{
        //    if (point_xz_min > point_cloud[first_point_index].z)
        //    {
        //        point_xz_min = point_cloud[first_point_index].z;
        //        first_point_index = point_index;
        //    }
        //}


        //point_xz_min = point_cloud[first_point_index].x;
        //foreach (int point_index in point_index_list) // find bottom
        //{
        //if (point_xz_min > point_cloud[first_point_index].x)
        //{
        //    point_xz_min = point_cloud[first_point_index].x;
        //    first_point_index = point_index;
        //}
        //}
        afl_remain.AddRange(afl1);
        afl_remain.AddRange(afl2);
        return simplex_list_t;
    }

    float circumCircleRadius(Vector3 P1, Vector3 P2, Vector3 P3)
    {
        float a = distance2D(P1, P2);
        float b = distance2D(P2, P3);
        float c = distance2D(P1, P3);
        float s = (a + b + c) / 2;
        float A = Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
        return a * b * c / (4 * A);
    }

    // https://www.geeksforgeeks.org/orientation-3-ordered-points/
    List<int> clockwiseCorrect(List<int> simplex)
    {
        //float m1 = (point_cloud[simplex[1]].z - point_cloud[simplex[0]].z) / (point_cloud[simplex[1]].x - point_cloud[simplex[0]].x);
        //float m2 = (point_cloud[simplex[2]].z - point_cloud[simplex[1]].z) / (point_cloud[simplex[2]].x - point_cloud[simplex[1]].x);
        //Debug.Log(simplex[0] + "~" + simplex[1] + "~" + simplex[2]);
        //Debug.Log("m: " + m1.ToString() + " " + m2.ToString());
        if ((point_cloud[simplex[1]].z - point_cloud[simplex[0]].z) * (point_cloud[simplex[2]].x - point_cloud[simplex[1]].x) - (point_cloud[simplex[2]].z - point_cloud[simplex[1]].z) * (point_cloud[simplex[1]].x - point_cloud[simplex[0]].x) < 0) // left
        {
            int t = simplex[2];
            simplex[2] = simplex[0];
            simplex[0] = t;
        }
        while (simplex[1] < simplex[0])
        {
            simplex.Add(simplex[0]);
            simplex.RemoveAt(0);
        }
        return simplex;
    }

    //***********************************************************************
    //
    // * Returns which side of the edge the line (x,y) is on. The return value
    //   is one of the constants defined above (LEFT, RIGHT, ON). See above
    //   for a discussion of which side is left and which is right.
    //=======================================================================
    int pointSide(Vector3 side_p1, Vector3 side_p2, Vector3 p)
    {
        // Compute the determinant: | xs ys 1 |
        //                          | xe ye 1 |
        //                          | x  y  1 |
        // Use its sign to get the answer.

        float det;

        det = side_p1.x *
                (side_p2.z - p.z) -
                side_p1.z *
                (side_p2.x - p.x) +
                side_p2.x * p.z -
                side_p2.z * p.x;

        if (det == 0.0)
            return 0;
        else if (det > 0.0)
            return -1;
        else
            return 1;

        //return sign((Bx - Ax) * (Y - Ay) - (By - Ay) * (X - Ax));
    }

    bool isLeft(Vector3 a, Vector3 b, Vector3 c)
    {
        return ((b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x)) > 0;
    }

    bool isContainSimplex(ref List<List<int>> simplex_list, List<int> simplex)
    {
        for (int simplex_list_index = 0; simplex_list_index < simplex_list.Count; simplex_list_index++)
        {
            if (simplex_list[simplex_list_index].SequenceEqual(simplex))
                return true;
        }
        return false;
    }

    void generateIDWTerrain(Vector4[] point_cloud)
    {
        Mesh mesh = new Mesh();
        int resolution = 64;
        float[,,] terrain_points = new float[resolution + 1, resolution + 1, 3];
        Vector3[] vertice = new Vector3[(resolution + 1) * (resolution + 1)];
        int[] indices = new int[6 * resolution * resolution];
        int indices_index = 0;
        float mse = 0.0f;
        List<float> all_elevations = new List<float>();
        List<EarthCoord> all_coords = new List<EarthCoord>();
        for (int i = 0; i < resolution + 1; i++)
        {
            for (int j = 0; j < resolution + 1; j++)
            {
                terrain_points[i, j, 0] = 8070.0f + i * 28;
                terrain_points[i, j, 2] = 6560.0f + j * 28;
                terrain_points[i, j, 1] = IDW.inverseDistanceWeighting(point_cloud, terrain_points[i, j, 0], terrain_points[i, j, 2]);
                all_coords.Add(new EarthCoord((float)MercatorProjection.xToLon(terrain_points[i, j, 0]), (float)MercatorProjection.yToLat(terrain_points[i, j, 2])));
                vertice[i * (resolution + 1) + j] = new Vector3(terrain_points[i, j, 0], terrain_points[i, j, 1], terrain_points[i, j, 2]);
            }
        }
        all_elevations = HgtReader.getElevations(all_coords);
        for (int i = 0; i < all_elevations.Count; i++)
        {
            mse += Mathf.Pow(all_elevations[i] - vertice[i].y, 2) / all_elevations.Count;
        }
        Debug.Log("mse: " + mse);
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                // counter-clockwise
                indices[indices_index++] = i * (resolution + 1) + j;
                indices[indices_index++] = (i + 1) * (resolution + 1) + j + 1;
                indices[indices_index++] = (i + 1) * (resolution + 1) + j;
                indices[indices_index++] = i * (resolution + 1) + j;
                indices[indices_index++] = i * (resolution + 1) + j + 1;
                indices[indices_index++] = (i + 1) * (resolution + 1) + j + 1;
            }
        }

        mesh.vertices = vertice;
        mesh.triangles = indices;
        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        //Name the mesh
        mesh.name = "terrain_mesh";
        GameObject terrain = new GameObject("terrain");
        MeshFilter mf = terrain.AddComponent<MeshFilter>();
        MeshRenderer mr = terrain.AddComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = terrain_mat;
    }

    void generateTINTerrain(Vector3[] point_cloud)
    {
        List<int> point_index_list = new List<int>();
        for (int point_index = 0; point_index < point_cloud.Length; point_index++)
        {
            point_index_list.Add(point_index);
        }

        List<KeyValuePair<int, int>> afl_remain = new List<KeyValuePair<int, int>>();
        int[] afl_point_connect_count = new int[point_cloud.Length];
        List<List<int>> simplex_list = DeWall(point_index_list, new List<KeyValuePair<int, int>>(), ref afl_remain, ref afl_point_connect_count, true, 0, 1);

        Mesh mesh = new Mesh();
        Vector3[] vertice = new Vector3[point_cloud.Length];
        int[] indices = new int[simplex_list.Count * 3];

        List<int>[] simplex_list_array = new List<int>[simplex_list.Count];
        simplex_list.CopyTo(simplex_list_array);
        for (int simplex_index = 0; simplex_index < simplex_list_array.Length; simplex_index++)
        {
            indices[simplex_index * 3] = simplex_list_array[simplex_index][0];
            indices[simplex_index * 3 + 1] = simplex_list_array[simplex_index][1];
            indices[simplex_index * 3 + 2] = simplex_list_array[simplex_index][2];
        }
        Debug.Log("030" + simplex_list.Count);
        mesh.vertices = point_cloud;
        mesh.triangles = indices;
        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        //Name the mesh
        mesh.name = "terrain_compress_mesh";
        GameObject terrain = new GameObject("terrain_compress");
        MeshFilter mf = terrain.AddComponent<MeshFilter>();
        MeshRenderer mr = terrain.AddComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = terrain_mat;

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
            mergeSortForPointCloud(ref point_index_list, x, m, point_index_list_t, is_vertical_alpha);
            mergeSortForPointCloud(ref point_index_list, m, y, point_index_list_t, is_vertical_alpha);
            int p = x, q = m;
            int index = x;
            while (p < m && q < y)
            {
                if ((is_vertical_alpha && point_cloud[point_index_list[p]].x < point_cloud[point_index_list[q]].x) ||
                    (!is_vertical_alpha && point_cloud[point_index_list[p]].z > point_cloud[point_index_list[q]].z))
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

    float distance2D(Vector3 p1, Vector3 p2)
    {
        return Mathf.Sqrt(Mathf.Pow(p1.x - p2.x, 2) + Mathf.Pow(p1.z - p2.z, 2));
    }

    float distance2D(float x1, float z1, float x2, float z2)
    {
        return Mathf.Sqrt(Mathf.Pow(x1 - x2, 2) + Mathf.Pow(z1 - z2, 2));
    }

    void mergeSortForAFL(ref List<KeyValuePair<float, int>> afl_middle_index_list, int x, int y, List<KeyValuePair<float, int>> afl_middle_index_list_t, bool is_vertical_alpha)
    {
        if (y - x > 1)
        {
            int m = x + (y - x) / 2;
            mergeSortForAFL(ref afl_middle_index_list, x, m, afl_middle_index_list_t, is_vertical_alpha);
            mergeSortForAFL(ref afl_middle_index_list, m, y, afl_middle_index_list_t, is_vertical_alpha);
            int p = x, q = m;
            int index = x;
            while (p < m && q < y)
            {
                if ((is_vertical_alpha && afl_middle_index_list[p].Key < afl_middle_index_list[q].Key) ||
                    (!is_vertical_alpha && afl_middle_index_list[p].Key > afl_middle_index_list[q].Key))
                    afl_middle_index_list_t[index++] = afl_middle_index_list[p++];
                else
                    afl_middle_index_list_t[index++] = afl_middle_index_list[q++];
            }
            while (p < m)
                afl_middle_index_list_t[index++] = afl_middle_index_list[p++];
            while (q < y)
                afl_middle_index_list_t[index++] = afl_middle_index_list[q++];
            for (int i = x; i < y; i++)
                afl_middle_index_list[i] = afl_middle_index_list_t[i];
        }
    }

    // https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
    float sign(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
    }

    bool pointInTriangle(Vector3 pt, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = sign(pt, v1, v2);
        d2 = sign(pt, v2, v3);
        d3 = sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }
    int orientation(Vector3 p, Vector3 q, Vector3 r)
    {
        // See https://www.geeksforgeeks.org/orientation-3-ordered-points/
        // for details of below formula.
        float val = (q.z - p.z) * (r.x - q.x) - (q.x - p.x) * (r.z - q.z);

        if (Mathf.Abs(val) < 1e-6) return 0; // collinear

        return (val > 0) ? 1 : 2; // clock or counterclock wise
    }

    bool isIntersect(Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2)
    {
        // Find the four orientations needed for general and
        // special cases
        int o1 = orientation(p1, q1, p2);
        int o2 = orientation(p1, q1, q2);
        int o3 = orientation(p2, q2, p1);
        int o4 = orientation(p2, q2, q1);

        // General case
        if (o1 != o2 && o3 != o4)
            return true;

        return false;
    }

    //**********************************************************************
    //
    // * Return the parameter value at which this segment crosses the given
    //   segment. This will return parameter values outside the range 0,1
    //   THIS FUNCTION IS EXTREMELY USEFUL FOR CLIPPING, but it 
    //   DOES NOT tell you whether the edge is "entering" or "leaving".
    //   But you can use tests like Edge::Point_Side() to figure that out.
    //======================================================================
    float crossParam(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        float dx1, dy1, dx2, dy2;
        float denom, s;

        // This computation comes from writing each segment in parametric form,
        // and solving a simulataneous equation to determine the parameter
        // value of the intersection point on this LineSeg.
        dx1 = d.x - c.x;
        dy1 = d.z - c.z;
        dx2 = b.x - a.x;
        dy2 = b.z - a.z;

        if ((denom = dx2 * dy1 - dy2 * dx1) == 0.0)
            // Parallel segments.
            return 1.0e20f;

        s = (c.x - a.x) * dy1 - (c.z - a.z) * dx1;

        return s / denom;
    }

    bool isInBoard(Vector3[] terrain_board_points, Vector3 point)
    {
        for (int board_index = 0; board_index < terrain_board_points.Length; board_index++)
        {
            if (pointSide(terrain_board_points[board_index], terrain_board_points[(board_index + 1) % terrain_board_points.Length], point) < 0)
                return false;
        }
        return true;
    }
}