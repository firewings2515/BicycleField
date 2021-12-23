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
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<OSMEditor>().osm_reader.read_finish)
        {
            osm_reader = GetComponent<OSMEditor>().osm_reader;
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
        using (StreamWriter sw = new StreamWriter(file_path))
        {
            // move first point to origin because of pathCreator
            Vector3 origin_pos = osm_reader.points_lib[bicycle_points_list[0]].position;
            double begin_lon = MercatorProjection.xToLon(osm_reader.points_lib[bicycle_points_list[0]].position.x + osm_reader.boundary_min.x);
            double begin_ele = origin_pos.y;
            double begin_lat = MercatorProjection.yToLat(osm_reader.points_lib[bicycle_points_list[0]].position.z + osm_reader.boundary_min.y);
            sw.WriteLine($"{begin_lon} {begin_ele} {begin_lat}");
            for (int bicycle_points_list_index = 0; bicycle_points_list_index < bicycle_points_list.Count; bicycle_points_list_index++)
            {
                Vector3 pos = osm_reader.points_lib[bicycle_points_list[bicycle_points_list_index]].position - origin_pos;
                sw.WriteLine($"{pos.x} {pos.y} {pos.z}");
                if (bicycle_points_list_index + 1 == bicycle_points_list.Count)
                    break;
                List<string> house_polygon_ids = HouseIntegration.house_polygons_object_index[bicycle_points_list_index];
                foreach (string house_polygon_id in house_polygon_ids)
                {
                    Vector3[] vertice = HouseIntegration.house_polygons_view_instances[house_polygon_id].points;
                    sw.Write($"H {vertice.Length + 1} ");
                    foreach (Vector3 origin_vertex in vertice)
                    {
                        Vector3 vertex = origin_vertex - origin_pos;
                        sw.Write($"{vertex.x} {vertex.y} {vertex.z} ");
                    }
                }
            }

            // old method
            //sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            //sw.WriteLine("<osm version=\"0.6\" generator=\"CGImap0.8.5(3066139spike-06.openstreetmap.org)\" copyright=\"OpenStreetMapandcontributors\" attribution=\"http://www.openstreetmap.org/copyright\" license=\"http://opendatacommons.org/licenses/odbl/1-0/\">");
            //foreach (KeyValuePair<string, Node> point in GetComponent<OSMRoadRender>().osm_reader.points_lib)
            //{
            //    if (point.Value.tag_k.Count == 0)
            //    {
            //        sw.WriteLine($" <node id=\"{point.Key}\" x=\"{point.Value.position.x}\" ele=\"{point.Value.position.y}\" z=\"{point.Value.position.z}\"/>");
            //    }
            //    else
            //    {
            //        sw.WriteLine($" <node id=\"{point.Key}\" x=\"{point.Value.position.x}\" ele=\"{point.Value.position.y}\" z=\"{point.Value.position.z}\">");
            //        for (int k_index = 0; k_index < point.Value.tag_k.Count; k_index++)
            //        {
            //            sw.WriteLine($"  <tag k=\"{point.Value.tag_k[k_index]}\" v=\"{point.Value.tag_v[k_index]}\"/>");
            //        }
            //        sw.WriteLine(" </node>");
            //    }
            //}

            //sw.WriteLine($" <way id=\"NTUSTCSIE\">");
            //foreach (string ref_id in bicycle_points_list)
            //{
            //    sw.WriteLine($"  <nd x=\"{pos.x}\"/>");
            //}
            //sw.WriteLine(" </way>");
        }
        Debug.Log("Write Successfully!");
    }
}