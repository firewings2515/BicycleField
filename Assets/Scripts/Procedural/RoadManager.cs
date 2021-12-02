using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using PathCreation;

public class RoadManager : MonoBehaviour
{
    private int current_segment = -1;
    private int loaded_segment = -1;
    private long loading_pointer = 0;
    public PathCreator path_creator;

    private void Start()
    {
        //remove all (two) default segments
        removeEarliestRoad();
        removeEarliestRoad();

        while (path_creator.bezierPath.NumSegments < Info.MAX_LOADED_SEGMENT)
        {
            getAndSetNextSegment();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Info.MAX_LOADED_SEGMENT - GameObject.Find("Cyclist").GetComponent<PathCreation.Examples.PathFollower>().getCurrentSegment() < Info.PRELOAD_SEGMENT)
        {
            getAndSetNextSegment();
        }
        if (Input.GetKeyDown(KeyCode.Z))
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
    }
}
