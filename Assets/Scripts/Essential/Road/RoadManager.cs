using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using PathCreation;
using UnityEditor;
using UnityEngine.UI;

public class RoadManager : MonoBehaviour
{
    private int current_segment = 2;
    private int current_loaded_segment = 0;
    private int last_segment = 0;
    private int house_id = 0;

    private StreamReader reader;

    public string file_name;
    public PathCreator path_creator;
    public bool path_loop = false;
    private bool update_mesh = false;
    private bool check_terrain_loaded = false;

    private bool is_started = false;
    private bool is_initial = false;

    private string last_data = null;
    public GameObject finish_flag;
    private bool finished = false;
    GameObject trigger_manager;
    int trigger_index = 0;
    Queue<GameObject> trigger_wait_queue = new Queue<GameObject>();

    private void Start()
    {
        trigger_manager = new GameObject("TriggerManager");
    }

    // Update is called once per frame
    void Update()
    {
        if (TerrainGenerator.is_initial && !is_initial) 
        {
            is_initial = true;
            reader = new StreamReader(Application.dataPath + "/StreamingAssets/" + file_name);

            string end_point_data = reader.ReadLine();
            Info.end_point = Functions.StrToVec3(end_point_data);

            reader.ReadLine(); // throw away terrain anchor point (0, 0, 0)

            //remove first default segment
            removeEarliestRoad(false);

            while (path_creator.bezierPath.NumSegments < Info.MAX_LOADED_SEGMENT)
            {
                getAndSetNextSegment();
            }

            //remove second default segment
            getAndSetNextSegment();

            //remove last default segment
            removeEarliestRoad(false);

            path_creator.bezierPath.NotifyPathModified();
            check_terrain_loaded = true;
        }
        if (is_initial)
        {
            if (TerrainGenerator.checkTerrainLoaded())
                is_started = true;
        }
        if (!is_started) return;

        if (Info.MAX_LOADED_SEGMENT - current_segment <= Info.PRELOAD_SEGMENT)
        {
            getAndSetNextSegment();

            path_creator.bezierPath = path_creator.bezierPath; //force update
            check_terrain_loaded = true;
        }
        else if (check_terrain_loaded)
        {
            if (TerrainGenerator.checkTerrainLoaded())
            {
                check_terrain_loaded = false;
                update_mesh = true;
            }
        }
        else if (update_mesh)
        {
            update_mesh = false;
            GetComponent<PathCreation.Examples.MyRoadMeshCreator>().CreateRoadMesh();
            while (trigger_wait_queue.Count > 0)
            {
                GameObject trigger = trigger_wait_queue.Dequeue();
                if (trigger)
                {
                    trigger.transform.position = new Vector3(trigger.transform.position.x, TerrainGenerator.getHeightWithBais(trigger.transform.position.x, trigger.transform.position.z), trigger.transform.position.z);
                }
            }
        }
    }

    private void getAndSetNextSegment()
    {
        if (getNextSegment(out string str_point))
        {
            Vector3 vec3_point = Functions.StrToVec3(str_point);
            vec3_point.y = 0.0f;

            //queue_checkpoint_vec3.Enqueue(vec3_point);
            spawnAnchorCheckpoint(vec3_point);

            generateRoad(vec3_point);
            if (path_creator.bezierPath.NumSegments > Info.MAX_LOADED_SEGMENT) removeEarliestRoad();
        }
    }

    private bool getNextSegment(out string point_data)
    {
        current_loaded_segment++;
        house_id = 0;
        //
        List<int> segment_id_list = new List<int>();
        List< int > house_id_list = new List<int>();
        List<string> info_list = new List<string>();
        //
        string new_data = reader.ReadLine();
        if (new_data == null && last_data != null && !finished)
        {
            string[] infos = last_data.Split(' ');
            Vector3 point = new Vector3(float.Parse(infos[0]), float.Parse(infos[1]) + 20, float.Parse(infos[2]));
            GameObject.Instantiate(finish_flag, point, Quaternion.identity);
            finished = true;
        }
        last_data = new_data;
        point_data = new_data;

        if (path_loop)
        {
            if (point_data == null)
            {
                reader.Close();
                reader = new StreamReader(Application.dataPath + "/StreamingAssets/" + file_name);
                reader.ReadLine(); // last point repeats -> delete
                point_data = reader.ReadLine();
            }
        }

        while (point_data != null && point_data[0] == 'H')
        {
            segment_id_list.Add(current_loaded_segment);
            house_id_list.Add(house_id);
            info_list.Add(point_data);
            
            house_id++;

            //GetComponent<HouseManager>().addToBuffer(point_data);
            point_data = reader.ReadLine();
        }
        HouseGenerator.generateHouses(segment_id_list, house_id_list, info_list);
        return point_data != null;
    }

    private void generateRoad(Vector3 road)
    {
        //terrain
        TerrainGenerator.generateTerrain(road);
        //BezierPath new_bezier = new BezierPath(path_creator.bezierPath[0]);
        //new_bezier = path_creator.bezierPath;
        //path_creator.bezierPath = new_bezier;

        path_creator.bezierPath.AddSegmentToEnd(road);
        //force display
        Selection.activeGameObject = this.gameObject;
    }

    private void removeEarliestRoad(bool destroy = true)
    {
        if (destroy) HouseGenerator.destroySegment(last_segment++);
        Vector3 remove_pos = path_creator.bezierPath.GetPoint(0);
        StartCoroutine(TerrainGenerator.removeAreaTerrain(remove_pos.x, remove_pos.z));
        path_creator.bezierPath.DeleteSegment(0);
        current_segment--;
    }

    private void spawnAnchorCheckpoint(Vector3 position)
    {
        GameObject trigger = new GameObject("Trigger" + (trigger_index++).ToString());
        trigger.transform.position = position;
        trigger.transform.parent = trigger_manager.transform;
        trigger_wait_queue.Enqueue(trigger);
        trigger.AddComponent<SphereCollider>();
        trigger.GetComponent<SphereCollider>().isTrigger = true;
        trigger.GetComponent<SphereCollider>().transform.localScale *= Info.CHECKPOINT_SIZE;
        trigger.AddComponent<AnchorCheckpoint>();
        trigger.layer = 6; //only collide with cyclist
    }

    public void incrementCurrentSegment()
    {
        current_segment++;
    }
}
