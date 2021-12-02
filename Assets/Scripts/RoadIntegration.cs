using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RoadIntegration : MonoBehaviour
{
    public Material roads_selected_mat;
    List<string> bicycle_way_list;
    List<string> bicycle_points_list;

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

    public void selectPath(string road_id)
    {
        if (edit_mode)
        {
            int new_road_index = roadCanLinked(road_id);
            if (new_road_index != -1)
            {
                bicycle_way_list.Add(road_id);

                List<GameObject> path_objects = GetComponent<OSMRoadRender>().pathes_objects[road_id];
                for (int index = 0; index < path_objects.Count; index++)
                {
                    path_objects[index].GetComponent<ViewInstance>().instance.GetComponent<MeshRenderer>().material = roads_selected_mat;
                }

                if (bicycle_way_list.Count == 1)
                {
                    bicycle_points_list.Clear();
                    bicycle_points_list = new List<string>(GetComponent<OSMRoadRender>().osm_reader.pathes[new_road_index].ref_node);
                }
                else
                {
                    int bucycle_points_index;
                    for (bucycle_points_index = bicycle_points_list.Count - 1; bucycle_points_index >= 0; bucycle_points_index--)
                    {
                        if (GetComponent<OSMRoadRender>().osm_reader.points_lib[bicycle_points_list[bucycle_points_index]].connect_way.Contains(road_id))
                        {
                            break;
                        }
                        else
                        {
                            bicycle_points_list.RemoveAt(bucycle_points_index);
                        }
                    }

                    if (GetComponent<OSMRoadRender>().osm_reader.pathes[new_road_index].head_node == bicycle_points_list[bucycle_points_index]) // head to tail
                    {
                        for (int new_road_ref_index = 0; new_road_ref_index < GetComponent<OSMRoadRender>().osm_reader.pathes[new_road_index].ref_node.Count; new_road_ref_index++)
                        {
                            bicycle_points_list.Add(GetComponent<OSMRoadRender>().osm_reader.pathes[new_road_index].ref_node[new_road_ref_index]);
                        }
                    }
                    else
                    {
                        for (int new_road_ref_index = GetComponent<OSMRoadRender>().osm_reader.pathes[new_road_index].ref_node.Count - 1; new_road_ref_index >= 0; new_road_ref_index--)
                        {
                            bicycle_points_list.Add(GetComponent<OSMRoadRender>().osm_reader.pathes[new_road_index].ref_node[new_road_ref_index]);
                        }
                    }
                }

                Debug.Log("Road " + road_id + " Linked Successfully!");
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
            foreach (string ref_id in bicycle_points_list)
            {
                Vector3 pos = GetComponent<OSMRoadRender>().osm_reader.points_lib[ref_id].position;
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