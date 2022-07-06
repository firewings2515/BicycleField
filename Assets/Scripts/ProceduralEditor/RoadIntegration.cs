using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RoadIntegration : MonoBehaviour
{
    public Material roads_unselected_mat;
    public Material roads_selected_mat;
    List<string> bicycle_way_list;
    List<string> bicycle_points_list;
    List<GameObject> bicycle_roads_list;
    OSMReader osm_reader;
    float origin_piece_x;
    float origin_piece_z;
    public float terrain_max_x;
    public float terrain_max_z;
    public float terrain_min_x;
    public float terrain_min_z;
    bool is_initial;

    [Header("Edit Bicycle Road List")]
    public bool edit_mode = true;

    [Header("Write Bicycle Pathes File")]
    public string file_path = "NTUSTCG.bpf";
    public bool write_file = false;

    // Start is called before the first frame update
    void Start()
    {
        bicycle_way_list = new List<string>();
        bicycle_points_list = new List<string>();
        bicycle_roads_list = new List<GameObject>();

        terrain_max_x = float.MinValue;
        terrain_max_z = float.MinValue;
        terrain_min_x = float.MaxValue;
        terrain_min_z = float.MaxValue;
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<OSMEditor>().osm_reader.read_finish && !is_initial)
        {
            is_initial = true;
            osm_reader = GetComponent<OSMEditor>().osm_reader;

            PublicOutputInfo.origin_pos = osm_reader.points_lib[GetComponent<OSMEditor>().initial_point].position;
            PublicOutputInfo.origin_pos.y = 0.0f;
            origin_piece_x = PublicOutputInfo.origin_pos.x - PublicOutputInfo.piece_length / 2;
            origin_piece_z = PublicOutputInfo.origin_pos.z - PublicOutputInfo.piece_length / 2;
            terrain_min_x = origin_piece_x;
            terrain_min_z = origin_piece_z;
            terrain_max_x = origin_piece_x + PublicOutputInfo.piece_length;
            terrain_max_z = origin_piece_z + PublicOutputInfo.piece_length;
        }
        if (write_file)
        {
            write_file = false;

            Debug.Log("Binding houses to roads...");
            int house_amount = HouseIntegration.bindHouses(osm_reader, bicycle_points_list, GetComponent<OSMEditor>().hierarchy_c, GetComponent<OSMHousePolygonRender>().house_polygons_objects);
            Debug.Log($"Bind {house_amount} houses Successfully!");

            writeBPF(Application.streamingAssetsPath + "//" + file_path);
        }
    }

    // return the index of road_id in Pathes
    int roadCanLinked(string road_id)
    {
        if (bicycle_way_list.Count == 0)
            return osm_reader.getPathIndex(road_id);

        int pathes_index = osm_reader.getPathIndex(bicycle_way_list[bicycle_way_list.Count - 1]);
        List<string> ref_node = osm_reader.pathes[pathes_index].ref_node;

        for (int road_point_index = 0; road_point_index < ref_node.Count; road_point_index++)
        {
            if (osm_reader.points_lib[ref_node[road_point_index]].connect_way.Contains(road_id))
            {
                return osm_reader.getPathIndex(road_id);
            }
        }

        return -1;
    }

    // click a road and check near road
    public void selectPath(string new_road_id)
    {
        if (edit_mode)
        {
            int new_road_index = roadCanLinked(new_road_id);
            if (new_road_index != -1)
            {
                bicycle_way_list.Add(new_road_id);

                List<GameObject> path_objects = GetComponent<OSMRoadRender>().pathes_objects[new_road_id];
                for (int index = 0; index < path_objects.Count; index++)
                {
                    path_objects[index].GetComponent<ViewInstance>().instance.GetComponent<MeshRenderer>().material = roads_selected_mat;
                }

                if (bicycle_way_list.Count == 1)
                {
                    bicycle_points_list.Clear();
                    bicycle_points_list = new List<string>(osm_reader.pathes[new_road_index].ref_node);
                    bicycle_roads_list = path_objects;
                    if (GetComponent<OSMEditor>().initial_point != bicycle_points_list[0])
                    {
                        bicycle_points_list.Reverse();
                        bicycle_roads_list.Reverse();
                    }
                }
                else
                {
                    int bicycle_points_index;
                    bool read_reverse = false;
                    for (bicycle_points_index = bicycle_points_list.Count - 1; bicycle_points_index >= 0; bicycle_points_index--)
                    {
                        if (bicycle_points_list[bicycle_points_index] == osm_reader.pathes[new_road_index].head_node)
                        {
                            break;
                        }
                        else if (bicycle_points_list[bicycle_points_index] == osm_reader.pathes[new_road_index].tail_node)
                        {
                            read_reverse = true;
                            break;
                        }
                        else
                        {
                            bicycle_points_list.RemoveAt(bicycle_points_index);
                            bicycle_roads_list[bicycle_roads_list.Count - 1].GetComponent<ViewInstance>().instance.GetComponent<MeshRenderer>().material = roads_unselected_mat;
                            bicycle_roads_list.RemoveAt(bicycle_roads_list.Count - 1);
                            bicycle_roads_list[bicycle_roads_list.Count - 1].GetComponent<ViewInstance>().instance.GetComponent<MeshRenderer>().material = roads_unselected_mat;
                            bicycle_roads_list.RemoveAt(bicycle_roads_list.Count - 1);
                        }
                    }

                    if (!read_reverse) // head to tail
                    {
                        for (int new_road_ref_index = 1; new_road_ref_index < osm_reader.pathes[new_road_index].ref_node.Count; new_road_ref_index++)
                        {
                            bicycle_points_list.Add(osm_reader.pathes[new_road_index].ref_node[new_road_ref_index]);
                            bicycle_roads_list.Add(path_objects[(new_road_ref_index - 1) * 2]);
                            bicycle_roads_list.Add(path_objects[(new_road_ref_index - 1) * 2 + 1]);
                        }
                    }
                    else
                    {
                        for (int new_road_ref_index = osm_reader.pathes[new_road_index].ref_node.Count - 2; new_road_ref_index >= 0; new_road_ref_index--)
                        {
                            bicycle_points_list.Add(osm_reader.pathes[new_road_index].ref_node[new_road_ref_index]);
                            bicycle_roads_list.Add(path_objects[new_road_ref_index * 2]);
                            bicycle_roads_list.Add(path_objects[new_road_ref_index * 2 + 1]);
                        }
                    }
                }

                largeVision(new_road_index);

                Debug.Log("Road " + new_road_id + " Linked Successfully!");
            }
            else
            {
                Debug.Log("Far away~");
            }
        }
    }

    void writeBPF(string file_path)
    {
        Debug.Log("Writing " + file_path);
        string[] bicycle_points_array = bicycle_points_list.ToArray();
        using (StreamWriter sw = new StreamWriter(file_path))
        {
            // move first point to origin because of pathCreator
            PublicOutputInfo.boundary_min = osm_reader.boundary_min;
            //double begin_lon = MercatorProjection.xToLon(osm_reader.points_lib[bicycle_points_list[0]].position.x + osm_reader.boundary_min.x);
            //double begin_ele = PublicOutputInfo.origin_pos.y;
            //double begin_lat = MercatorProjection.yToLat(osm_reader.points_lib[bicycle_points_list[0]].position.z + osm_reader.boundary_min.y);
            //sw.WriteLine($"{begin_lon} {begin_ele} {begin_lat}");
            Vector3 end_pos = osm_reader.points_lib[bicycle_points_array[bicycle_points_array.Length - 1]].position - PublicOutputInfo.origin_pos;
            sw.WriteLine($"{end_pos.x} {end_pos.y} {end_pos.z}");
            for (int bicycle_points_list_index = 0; bicycle_points_list_index < bicycle_points_array.Length; bicycle_points_list_index++)
            {
                Vector3 pos = osm_reader.points_lib[bicycle_points_array[bicycle_points_list_index]].position - PublicOutputInfo.origin_pos;
                sw.WriteLine($"{pos.x} {pos.y} {pos.z}");
                if (bicycle_points_list_index + 1 == bicycle_points_array.Length)
                    break;
                List<string> house_polygon_ids = HouseIntegration.house_polygons_object_index[bicycle_points_list_index];
                for (int house_polygon_ids_index = 0; house_polygon_ids_index < house_polygon_ids.Count; house_polygon_ids_index++)
                {
                    Vector3[] vertices = HouseIntegration.house_polygons_view_instances[house_polygon_ids[house_polygon_ids_index]].points;
                    sw.Write($"H {vertices.Length} ");
                    for (int vertices_index = 0; vertices_index < vertices.Length; vertices_index++)
                    {
                        Vector3 vertex = vertices[vertices_index] - PublicOutputInfo.origin_pos;
                        sw.Write($"{vertex.x} {vertex.y} {vertex.z} ");
                    }
                    sw.Write("\n");
                }
            }
        }
        Debug.Log("Write " + file_path + " Successfully!");
    }

    void largeVision(int new_road_index)
    {
        for (int new_road_ref_index = 0; new_road_ref_index < osm_reader.pathes[new_road_index].ref_node.Count; new_road_ref_index++)
        {
            Vector3 point = osm_reader.points_lib[osm_reader.pathes[new_road_index].ref_node[new_road_ref_index]].position;
            float expanded_x = (Mathf.CeilToInt(Mathf.Abs(point.x - origin_piece_x) / PublicOutputInfo.piece_length) + 1 + TerrainGenerator.vision_patch_num) * PublicOutputInfo.piece_length; // 2 is a adjust value
            float expanded_z = (Mathf.CeilToInt(Mathf.Abs(point.z - origin_piece_z) / PublicOutputInfo.piece_length) + 1 + TerrainGenerator.vision_patch_num) * PublicOutputInfo.piece_length; // 2 is a adjust value
            if (point.x + expanded_x >= origin_piece_x)
                terrain_max_x = Mathf.Max(terrain_max_x, origin_piece_x + expanded_x);
            if (point.z + expanded_z >= origin_piece_z)
                terrain_max_z = Mathf.Max(terrain_max_z, origin_piece_z + expanded_z);
            if (point.x - expanded_x < origin_piece_x)
                terrain_min_x = Mathf.Min(terrain_min_x, origin_piece_x - expanded_x);
            if (point.z - expanded_z < origin_piece_z)
                terrain_min_z = Mathf.Min(terrain_min_z, origin_piece_z - expanded_z);
        }
    }

    public List<Vector3> bicyclePointsListToVec3()
    {
        List<Vector3> output = new List<Vector3>();
        for (int index = 0; index < bicycle_points_list.Count; index++)
        {
            output.Add(osm_reader.points_lib[bicycle_points_list[index]].position);
        }
        return output;
    }
}