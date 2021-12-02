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

    [Header("Edit Bicycle Road List")]
    public bool edit_mode = true;

    [Header("Write Bicycle Pathes File")]
    public string file_path = "NTUSTCG.bpf";
    public bool write_file = false;

    string last_way_id;
    bool from_tail;

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
        if (write_file)
        {
            write_file = false;
            writeBPF(Application.streamingAssetsPath + "//" + file_path);
        }
    }

    // return the index of road_id in Pathes
    int roadCanLinked(string road_id)
    {
        if (bicycle_way_list.Count == 0)
            return GetComponent<OSMRoadRender>().osm_reader.getPathIndex(road_id);

        int pathes_index = GetComponent<OSMRoadRender>().osm_reader.getPathIndex(bicycle_way_list[bicycle_way_list.Count - 1]);
        List<string> ref_node = GetComponent<OSMRoadRender>().osm_reader.pathes[pathes_index].ref_node;

        for (int road_point_index = 0; road_point_index < ref_node.Count; road_point_index++)
        {
            if (GetComponent<OSMRoadRender>().osm_reader.points_lib[ref_node[road_point_index]].connect_way.Contains(road_id))
            {
                return GetComponent<OSMRoadRender>().osm_reader.getPathIndex(road_id);
            }
        }

        return -1;
    }

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
                    bicycle_points_list = new List<string>(GetComponent<OSMRoadRender>().osm_reader.pathes[new_road_index].ref_node);
                    bicycle_roads_list = path_objects;
                }
                else
                {
                    int bicycle_points_index;
                    //int target_road_ref_index;
                    //List<GameObject> path_objects_target = GetComponent<OSMRoadRender>().pathes_objects[last_way_id];
                    //List<string> path_target = new List<string>(GetComponent<OSMRoadRender>().osm_reader.pathes[GetComponent<OSMRoadRender>().osm_reader.getPathIndex(last_way_id)].ref_node);
                    //if (from_tail) target_road_ref_index = 1;
                    //else target_road_ref_index = path_target.Count - 2;
                    //Debug.Log("Hello" + bicycle_points_list[bicycle_points_list.Count - 1]);
                    bicycle_points_list.Reverse();
                    for (bicycle_points_index = bicycle_points_list.Count - 1; bicycle_points_index >= 0; bicycle_points_index--)
                    {
                        if (GetComponent<OSMRoadRender>().osm_reader.points_lib[bicycle_points_list[bicycle_points_index]].connect_way.Contains(new_road_id))
                        {
                            break;
                        }
                        else
                        {
                            Debug.Log(bicycle_points_index);
                            bicycle_points_list.RemoveAt(bicycle_points_index);
                            bicycle_roads_list[bicycle_roads_list.Count - 1].GetComponent<ViewInstance>().instance.GetComponent<MeshRenderer>().material = roads_unselected_mat;
                            bicycle_roads_list.RemoveAt(bicycle_roads_list.Count - 1);
                            //if (from_tail)
                            //{
                            //    for (; target_road_ref_index < path_target.Count; target_road_ref_index++)
                            //    {
                            //        if (path_target[target_road_ref_index] == bicycle_points_list[bicycle_points_index])
                            //        {
                            //            path_objects_target[target_road_ref_index].GetComponent<ViewInstance>().instance.GetComponent<MeshRenderer>().material = roads_unselected_mat;
                            //        }
                            //        else
                            //        {
                            //            break;
                            //        }
                            //    }
                            //}
                            //else
                            //{
                            //    for (; target_road_ref_index > 0; target_road_ref_index--)
                            //    {
                            //        if (path_target[target_road_ref_index] == bicycle_points_list[bicycle_points_index])
                            //        {
                            //            path_objects_target[target_road_ref_index - 1].GetComponent<ViewInstance>().instance.GetComponent<MeshRenderer>().material = roads_unselected_mat;
                            //        }
                            //        else
                            //        {
                            //            break;
                            //        }
                            //    }
                            //}
                        }
                        //Debug.Log("Hello~" + bicycle_points_index);
                    }

                    //Debug.Log("Hi");
                    if (GetComponent<OSMRoadRender>().osm_reader.pathes[new_road_index].head_node == bicycle_points_list[bicycle_points_index]) // head to tail
                    {
                        for (int new_road_ref_index = 1; new_road_ref_index < GetComponent<OSMRoadRender>().osm_reader.pathes[new_road_index].ref_node.Count; new_road_ref_index++)
                        {
                            bicycle_points_list.Add(GetComponent<OSMRoadRender>().osm_reader.pathes[new_road_index].ref_node[new_road_ref_index]);
                            bicycle_roads_list.Add(path_objects[new_road_ref_index]);
                        }
                        //from_tail = false;
                    }
                    else
                    {
                        for (int new_road_ref_index = GetComponent<OSMRoadRender>().osm_reader.pathes[new_road_index].ref_node.Count - 2; new_road_ref_index >= 0; new_road_ref_index--)
                        {
                            bicycle_points_list.Add(GetComponent<OSMRoadRender>().osm_reader.pathes[new_road_index].ref_node[new_road_ref_index]);
                            bicycle_roads_list.Add(path_objects[new_road_ref_index]);
                        }
                        //from_tail = true;
                    }
                    //Debug.Log(from_tail);
                }

                last_way_id = new_road_id;
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
            Vector3 origin_pos = GetComponent<OSMRoadRender>().osm_reader.points_lib[bicycle_points_list[0]].position;
            foreach (string ref_id in bicycle_points_list)
            {
                Vector3 pos = GetComponent<OSMRoadRender>().osm_reader.points_lib[ref_id].position - origin_pos;
                sw.WriteLine($"{pos.x} {pos.y} {pos.z}");
            }

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