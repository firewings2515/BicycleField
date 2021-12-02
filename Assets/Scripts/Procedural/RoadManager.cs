using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using PathCreation;

public class RoadManager : MonoBehaviour
{
    private int current_segment = 2;
    private int loaded_segment = -1;
    private long loading_pointer = 0;
    public PathCreator path_creator;

    private void Start()
    {
        //remove first default segment
        removeEarliestRoad();

        while (path_creator.bezierPath.NumSegments < Info.MAX_LOADED_SEGMENT)
        {
            getAndSetNextSegment();
        }

        //remove second default segment
        getAndSetNextSegment();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(current_segment);
        if (Info.MAX_LOADED_SEGMENT - current_segment <= Info.PRELOAD_SEGMENT)
        {
            getAndSetNextSegment();
        }

        path_creator.bezierPath = path_creator.bezierPath; //force update
    }

    private void getAndSetNextSegment()
    {
        if (getNextSegment(out string str_point))
        {
            Vector3 vec3_point = Functions.StrToVec3(str_point);

            spawnAnchorCheckpoint(vec3_point);

            generateRoad(vec3_point);
            if (path_creator.bezierPath.NumSegments > Info.MAX_LOADED_SEGMENT) removeEarliestRoad();
        }
    }

    private bool getNextSegment(out string point_data)
    {
        StreamReader reader = new StreamReader(Application.dataPath + "/StreamingAssets/procedural_test.txt");

        if (loading_pointer < reader.BaseStream.Length)
        {
            reader.BaseStream.Position = loading_pointer;
            point_data = reader.ReadLine();
            loading_pointer += point_data.Length + 2; // + 2 skips newline (seen as 2 characters)
        }
        else point_data = "";

        reader.Close();
        return point_data != "";
    }

    private void generateRoad(Vector3 road)
    {
        path_creator.bezierPath.AddSegmentToEnd(road);
    }

    private void removeEarliestRoad()
    {
        path_creator.bezierPath.DeleteSegment(0);
        current_segment--;
    }

    private void spawnAnchorCheckpoint(Vector3 position)
    {
        GameObject prefab = new GameObject();
        prefab.transform.position = position;
        prefab.AddComponent<SphereCollider>();
        prefab.GetComponent<SphereCollider>().isTrigger = true;
        prefab.AddComponent<AnchorCheckpoint>();

    }

    public void incrementCurrentSegment()
    {
        current_segment++;
    }
}
