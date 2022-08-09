using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using PathCreation;
using UnityEditor;
using UnityEngine.UI;

public class RoadManager : MonoBehaviour
{
    //road
    public PathCreator path_creator;
    public bool path_loop = false;

    //data
    public string file_name;
    private StreamReader reader;
    private string[] data_lines;
    private int current_line = 0;

    //segment
    private int rendered_segments = 2; //because 2 exist by default
    private int current_segment = 0;
    private bool update_mesh = false;
    private List<List<GameObject>> landmarks = new List<List<GameObject>>() { new List<GameObject>(), new List<GameObject>() };
    
    //trigger
    private GameObject trigger_manager;
    private int trigger_index = 0;

    public string calc_path = null;
    private bool calc_path_check = false;

    public HouseManager house_manager;

    //debug
    private bool using_house = true;
    private bool using_terrain = false;

    private void Start()
    {
        if (calc_path_check)
        {
            StreamReader reader1 = new StreamReader(Application.dataPath + "/StreamingAssets/" + calc_path);
            string data = reader1.ReadToEnd();
            data_lines = data.Split('\n');

            List<Vector3> vec3_lines = new List<Vector3>();
            for (int id = 0; id < data_lines.Length; id++)
            {
                vec3_lines.Add(Functions.StrToVec3(data_lines[id]));
            }
            Vector3 old_check = vec3_lines[0];
            for (int id = 1; id < vec3_lines.Count; id++)
            {
                if ((vec3_lines[id] - old_check).magnitude < 100)
                {
                    vec3_lines.RemoveAt(id);
                    id--;
                }
                else old_check = vec3_lines[id];
            }

            Debug.Log(vec3_lines[vec3_lines.Count - 1]);

            StreamWriter writer = new StreamWriter(Application.dataPath + "/StreamingAssets/new.bpf", true);
            for (int id = 0; id < vec3_lines.Count; id++)
            {
                writer.WriteLine(vec3_lines[id].x + " " + vec3_lines[id].y + " " + vec3_lines[id].z);
            }
            writer.Close();
        }

        trigger_manager = new GameObject("TriggerManager");
        readRoadData();
        initializeRoad();

        preCalc(); //need to store in memory and not calc every execution
    }

    private void readRoadData()
    {
        reader = new StreamReader(Application.dataPath + "/StreamingAssets/" + file_name + "/" + file_name + ".bpf");

        if (using_terrain) TerrainGenerator.file_path = file_name + "/features.f";
        string data = reader.ReadToEnd();
        data_lines = data.Split('\n');
    }

    private void preCalc()
    {
        calcTotalRoadLength();
        //there should be more
    }

    private void calcTotalRoadLength()
    {
        for (int id = 1; id < data_lines.Length; id++)
        {
            Info.total_length += (Functions.StrToVec3(data_lines[id]) - Functions.StrToVec3(data_lines[id - 1])).magnitude;
        }
    }

    // Update is called once per frame
    void Update()
    {
        generateRoad();
    }

    private void initializeRoad()
    {
        
        while (path_creator.bezierPath.NumSegments < Info.MAX_SEGMENTS) generateNextSegment();

        //remove both default segment
        removeFirstSegment(false);
        removeFirstSegment(false);
    }

    private void generateRoad()
    {
        if (Info.MAX_SEGMENTS - rendered_segments <= Info.PRELOAD_SEGMENT)
        {
            generateNextSegment();
            path_creator.bezierPath = path_creator.bezierPath; //force update

            update_mesh = true;
        }
        else if (update_mesh)
        {
            GetComponent<PathCreation.Examples.MyRoadMeshCreator>().CreateRoadMesh();
            update_mesh = false;
        }
    }

    private void generateNextSegment()
    {
        if (getNextSegment(out string str_point))
        {
            landmarks.Add(new List<GameObject>());

            string[] split = str_point.Split(' ');
            Vector3 vec3_point = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
            if (split.Length > 3) adjustRoad(int.Parse(split[3]), vec3_point);

            //terrain
            if (using_terrain) TerrainGenerator.generateTerrain(vec3_point);

            //checkpoint
            spawnCheckpoint(vec3_point);

            //road
            addRoadSegment(vec3_point);
            if (path_creator.bezierPath.NumSegments > Info.MAX_SEGMENTS) removeFirstSegment();
        }
    }

    private void adjustRoad(int variation, Vector3 point)
    {
        switch (variation)
        {
            case 0:
                createBridge(point);
                break;
            case 1:
                Debug.Log("Intersection");
                break;
            case 2:
                Debug.Log("Merge");
                break;
            case 3:
                Debug.Log("Split");
                break;
        }
    }

    private void createBridge(Vector3 point)
    {
        //placeholder
        float rail_length = 10;

        Vector3 direction = Functions.StrToVec3(data_lines[current_line + 2]) - point;
        int rail_segment = (int)(direction.magnitude / rail_length);

        for (int id = 1; id < rail_segment; id++)
        {
            //left
            GameObject left_rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left_rail.transform.position = point + (((Functions.StrToVec3(data_lines[current_line + 2]) - point) / rail_segment) * id);
            left_rail.transform.position -= (Vector3.Cross(Vector3.up, direction)).normalized * Info.road_width;
            left_rail.transform.localScale = new Vector3(rail_length, 5, 1);
            left_rail.GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0, 255) / 255.0f, Random.Range(0, 255) / 255.0f, Random.Range(0, 255) / 255.0f);

            //rotation
            left_rail.transform.RotateAround(left_rail.transform.position, new Vector3(0, 1, 0), Vector3.Angle(new Vector3(1, 0, 0), direction));

            landmarks[landmarks.Count - 1].Add(left_rail);

            //right
            GameObject right_rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right_rail.transform.position = point + (((Functions.StrToVec3(data_lines[current_line + 2]) - point) / rail_segment) * id);
            right_rail.transform.position += (Vector3.Cross(Vector3.up, direction)).normalized * Info.road_width;
            right_rail.transform.localScale = new Vector3(rail_length, 5, 1);
            right_rail.GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0, 255) / 255.0f, Random.Range(0, 255) / 255.0f, Random.Range(0, 255) / 255.0f);

            //rotation
            right_rail.transform.RotateAround(right_rail.transform.position, new Vector3(0, 1, 0), Vector3.Angle(new Vector3(1, 0, 0), direction));

            landmarks[landmarks.Count - 1].Add(right_rail);
        }

        /*
         GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.transform.position = (point + Functions.StrToVec3(data_lines[current_line + 2])) / 2;
            rail.transform.position += (Vector3.Cross(Vector3.up, direction)).normalized * Info.road_width;
            rail.transform.localScale = new Vector3(rail_length, 5, 1);
            rail.GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0, 255), Random.Range(0, 255), Random.Range(0, 255));

            //rotation
            rail.transform.RotateAround(rail.transform.position, new Vector3(0, 1, 0), Vector3.Angle(new Vector3(1, 0, 0), direction));

            landmarks[landmarks.Count - 1].Add(rail);
         */
    }

    private bool getNextSegment(out string point_data)
    {
        point_data = null;

        //end reached
        if (current_line == data_lines.Length)
        {
            if (path_loop) current_line = 0;
            else return false;
        }

        point_data = data_lines[current_line++];
        return point_data != null;
    }

    private void addRoadSegment(Vector3 road)
    {
        path_creator.bezierPath.AddSegmentToEnd(road);
        Selection.activeGameObject = this.gameObject; //force display
    }

    private void removeFirstSegment(bool destroy = true)
    {
        removeLandmarks();

        //terrain
        Vector3 remove_pos = path_creator.bezierPath.GetPoint(0);
        if (using_terrain) StartCoroutine(TerrainGenerator.removeAreaTerrain(remove_pos.x, remove_pos.z));

        //road
        path_creator.bezierPath.DeleteSegment(0);
        rendered_segments--;
    }
    
    private void removeLandmarks()
    {
        //remove landmarks
        for (int id = landmarks[0].Count - 1; id >= 0; id--)
        {
            Destroy(landmarks[0][id]);
        }
        landmarks.RemoveAt(0);
    }

    private void spawnCheckpoint(Vector3 position)
    {
        GameObject trigger = new GameObject("Trigger" + (trigger_index++).ToString());
        trigger.transform.position = position;
        trigger.transform.parent = trigger_manager.transform;
        trigger.AddComponent<SphereCollider>();
        trigger.GetComponent<SphereCollider>().isTrigger = true;
        trigger.GetComponent<SphereCollider>().transform.localScale *= Info.CHECKPOINT_SIZE;
        trigger.AddComponent<AnchorCheckpoint>();
        trigger.layer = 6; //only collide with cyclist
    }

    public void incrementCurrentSegment()
    {
        rendered_segments++;
        current_segment++;
        if (using_house) StartCoroutine(house_manager.go_next_segment());
        Info.total_length -= (Functions.StrToVec3(data_lines[current_segment]) - Functions.StrToVec3(data_lines[current_segment - 1])).magnitude;
    }
}
