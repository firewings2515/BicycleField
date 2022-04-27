using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSMEditor : MonoBehaviour
{
    public OSMReader osm_reader;
    public HierarchyControl hierarchy_c;
    public string osm3d_file_name = "YangJin3D.osm";
    public bool is_initial = false;
    public GameObject cam;
    public string initial_point = "45263678_226830312+24";
    public string initial_road = "";
    public bool open_limit = false;
    public float lon_max;
    public float lon_min;
    public float lat_max;
    public float lat_min;
    public GameObject view_instance;
    // Start is called before the first frame update
    void Start()
    {
        osm_reader = new OSMReader();
    }

    // Update is called once per frame
    void Update()
    {
        if (!osm_reader.read_finish)
        {
            osm_reader.readOSM(Application.streamingAssetsPath + "//" + osm3d_file_name, false, Application.streamingAssetsPath + "//" + osm3d_file_name, open_limit, lon_max, lon_min, lat_max, lat_min);

            // each hierarchy manage range is 200m x 200m
            hierarchy_c = new HierarchyControl();
            hierarchy_c.setup((int)(osm_reader.boundary_max.x - osm_reader.boundary_min.x) / 200, (int)(osm_reader.boundary_max.y - osm_reader.boundary_min.y) / 200, osm_reader.boundary_max.x, osm_reader.boundary_max.y);

            // set camera to begin
            setCam();
        }
        else // OSMReader is initail
        {
            // OSMEditor is initail
            is_initial = true;
        }
    }

    void setCam()
    {
        if (!osm_reader.points_lib.ContainsKey(initial_point))
        {
            int path_index = osm_reader.findPathNameIndex(initial_road);
            if (path_index == -1)
            {
                Debug.Log(initial_road + " not found");
                path_index = 0;
            }
            Debug.Log(osm_reader.pathes.Count);
            initial_point = osm_reader.pathes[path_index].ref_node[0];
            Debug.Log("need to set exist initial_point");
        }
        cam.transform.position = osm_reader.points_lib[initial_point].position + new Vector3(0, 800.0f, 0);
        cam.transform.rotation = Quaternion.Euler(90, 0, 0);
    }
}