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
    
    //trigger
    private GameObject trigger_manager;
    private int trigger_index = 0;

    private void Start()
    {
        trigger_manager = new GameObject("TriggerManager");
        readRoadData();
        initializeRoad();

        preCalc(); //need to store in memory and not calc every execution
    }

    private void readRoadData()
    {
        reader = new StreamReader(Application.dataPath + "/StreamingAssets/" + file_name);
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
            Vector3 vec3_point = Functions.StrToVec3(str_point);

            //terrain
            TerrainGenerator.generateTerrain(vec3_point);

            //checkpoint
            spawnCheckpoint(vec3_point);

            //road
            addRoadSegment(vec3_point);
            if (path_creator.bezierPath.NumSegments > Info.MAX_SEGMENTS) removeFirstSegment();
        }
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
        //terrain
        Vector3 remove_pos = path_creator.bezierPath.GetPoint(0);
        StartCoroutine(TerrainGenerator.removeAreaTerrain(remove_pos.x, remove_pos.z));

        //road
        path_creator.bezierPath.DeleteSegment(0);
        rendered_segments--;
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

        Info.total_length -= (Functions.StrToVec3(data_lines[current_segment]) - Functions.StrToVec3(data_lines[current_segment - 1])).magnitude;
    }
}
