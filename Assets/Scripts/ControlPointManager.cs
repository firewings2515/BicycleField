using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPointManager : MonoBehaviour
{
    bool is_initial = false;
    OSMReader osm_reader;
    public GameObject sphere_prefab;
    public Dictionary<string, GameObject> controlpoints_lib = new Dictionary<string, GameObject>();
    string last_select_sphere_id;
    public Material spheres_select_mat;
    public Material spheres_unselect_mat;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!is_initial && GetComponent<OSMRoadRender>().is_initial)
        {
            is_initial = true;
            osm_reader = GetComponent<OSMEditor>().osm_reader;
            GameObject point_manager = new GameObject("Point Manager");
            foreach (KeyValuePair<string, Node> nn in osm_reader.points_lib)
            {
                GameObject bb = Instantiate(sphere_prefab, nn.Value.position, Quaternion.identity);
                bb.transform.localScale *= 5.0f;
                string ans = "connectway";
                for (int i = 0; i < nn.Value.connect_way.Count; i++)
                    ans += " & " + nn.Value.connect_way[i];
                bb.name = nn.Key + ans;
                bb.transform.parent = point_manager.transform;
                bb.GetComponent<ControlPoint>().setControlPoint(nn.Key, nn.Value, this);
                controlpoints_lib.Add(nn.Key, bb);
                last_select_sphere_id = nn.Key;
            }
            controlpoints_lib[GetComponent<OSMEditor>().initial_point].GetComponent<ControlPoint>().select();
        }
    }

    public void selectPoint(string point_id)
    {
        controlpoints_lib[last_select_sphere_id].GetComponent<ControlPoint>().unselect();
        last_select_sphere_id = point_id;
    }
}