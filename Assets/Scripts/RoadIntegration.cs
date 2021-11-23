using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadIntegration : MonoBehaviour
{
    public Material roads_selected_mat;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void selectPath(string road_id)
    {
        List<GameObject> path_objects = GetComponent<OSMReaderManager>().pathes_objects[road_id];
        for (int index = 0; index < path_objects.Count; index++)
        {
            path_objects[index].GetComponent<ViewInstance>().instance.GetComponent<MeshRenderer>().material = roads_selected_mat;
        }
    }
}