using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadIntegration : MonoBehaviour
{
    public Material roads_selected_mat;
    List<string> bicycle_road_list;

    [Header("Edit Bicycle Road List")]
    public bool edit_mode = true;

    // Start is called before the first frame update
    void Start()
    {
        bicycle_road_list = new List<string>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool roadCanLinked(string road_id)
    {
        if (bicycle_road_list.Count == 0)
            return true;

        int pathes_index = GetComponent<OSMReaderManager>().osm_reader.getPathIndex(bicycle_road_list[bicycle_road_list.Count - 1]);
        List<string> ref_node = GetComponent<OSMReaderManager>().osm_reader.pathes[pathes_index].ref_node;

        for (int road_point_index = 0; road_point_index < ref_node.Count; road_point_index++)
        {
            if (GetComponent<OSMReaderManager>().osm_reader.points_lib[ref_node[road_point_index]].connect_way.Contains(road_id))
            {
                return true;
            }
        }

        return false;
    }

    public void selectPath(string road_id)
    {
        if (edit_mode)
        {
            if (roadCanLinked(road_id))
            {
                bicycle_road_list.Add(road_id);

                List<GameObject> path_objects = GetComponent<OSMReaderManager>().pathes_objects[road_id];
                for (int index = 0; index < path_objects.Count; index++)
                {
                    path_objects[index].GetComponent<ViewInstance>().instance.GetComponent<MeshRenderer>().material = roads_selected_mat;
                }

                Debug.Log("Road " + road_id + " Linked Successfully!");
            }
            else
            {
                Debug.Log("Far away~");
            }
        }
    }
}