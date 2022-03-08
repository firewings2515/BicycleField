using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using PathCreation;
using PathCreation.Examples;
using UnityEditor;

public class RoadManager2 : MonoBehaviour
{
    private int current_segment = 2;
    private int current_loaded_segment = 0;
    private int current_running_segment = 0;

    private StreamReader reader;

    public string file_name;

    List<GameObject> segments = new List<GameObject>();
    private int current_seg = 0;
    //private const int segments_per_load = 4;
    //private const int number_of_loads = 4;

    public Material road;
    public Material underside;

    List<Vector3> points = new List<Vector3> {  };

    public GameObject cyclist;

    private void Start()
    {
        reader = new StreamReader(Application.dataPath + "/StreamingAssets/" + file_name);

        for (int seg_id = 0; seg_id < Info.MAX_LOADED_SEGMENT; seg_id++)
        {
            getAndSetNextSegment();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Info.MAX_LOADED_SEGMENT - current_running_segment <= Info.PRELOAD_SEGMENT)
        {
            getAndSetNextSegment();
        }
    }

    private void getAndSetNextSegment()
    {
        if (getNextSegment(out string str_point))
        {
            Vector3 vec3_point = new Vector3();
            for (int seg_id = 1; seg_id < Info.MAX_LOADED_SEGMENT; seg_id++)
            {
                vec3_point = Functions.StrToVec3(str_point);
                points.Add(vec3_point);
            }

            spawnAnchorCheckpoint(vec3_point);
            current_loaded_segment += 1;

            if (points.Count >= current_seg + Info.MAX_LOADED_SEGMENT)
            {
                createNextMesh();
            }
        }
    }

    private bool getNextSegment(out string point_data)
    {
        point_data = reader.ReadLine();
        while (point_data != null && point_data[0] == 'H')
        {
            //GetComponent<HouseManager>().addToBuffer(point_data);
            point_data = reader.ReadLine();
        }
        return point_data != null;
    }

    private void spawnAnchorCheckpoint(Vector3 position)
    {
        GameObject prefab = new GameObject();
        prefab.transform.position = position;
        prefab.AddComponent<SphereCollider>();
        prefab.GetComponent<SphereCollider>().isTrigger = true;
        prefab.GetComponent<SphereCollider>().transform.localScale *= Info.CHECKPOINT_SIZE;
        prefab.AddComponent<AnchorCheckpoint2>();

    }

    public void incrementCurrentSegment()
    {
        current_segment++;
        current_running_segment++;
    }

    private void createNextMesh()
    {
        GameObject segment = new GameObject();
        segments.Add(segment);

        while (segments.Count > Info.MAX_LOADED_SEGMENT)
        {
            Destroy(segments[0]);
            segments.RemoveAt(0);
            current_running_segment--;
        }

        segment.AddComponent<PathCreator>();
        PathCreator pc = segment.GetComponent<PathCreator>();
        pc.bezierPath = new BezierPath(points[current_seg]);
        for (int segment_id = current_seg + 1; segment_id < current_seg + Info.MAX_LOADED_SEGMENT; segment_id++)
        {
            pc.bezierPath.AddSegmentToEnd(points[segment_id]);
        }
        current_seg += Info.MAX_LOADED_SEGMENT;

        segment.AddComponent<RoadMeshCreator>();
        RoadMeshCreator rmc = segment.GetComponent<RoadMeshCreator>();
        rmc.pathCreator = pc;
        rmc.roadMaterial = road;
        rmc.undersideMaterial = underside;

        //force display
        Selection.activeGameObject = segment;

        cyclist.GetComponent<PathFollower>().pathCreator = segments[0].GetComponent<PathCreator>();
    }
}