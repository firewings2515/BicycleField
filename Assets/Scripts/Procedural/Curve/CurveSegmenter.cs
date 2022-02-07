using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;
using UnityEditor;

public class CurveSegmenter : MonoBehaviour
{
    List<GameObject> segments = new List<GameObject>();
    private int current_segment = 0;
    private const int segments_per_load = 10;
    private const int number_of_loads = 1;

    public Material road;
    public Material underside;
    public GameObject cyclist;

    List<Vector3> points = new List<Vector3> { new Vector3(0.0f, 0.0f, 0.0f) };
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            setNextSegment();
        }
    }

    private void createNextMesh()
    {
        GameObject segment = new GameObject();
        segments.Add(segment);

        while (segments.Count > number_of_loads)
        {
            Destroy(segments[0]);
            segments.RemoveAt(0);
        }

        segment.AddComponent<PathCreator>();
        PathCreator pc = segment.GetComponent<PathCreator>();
        pc.bezierPath = new BezierPath(points[current_segment]);
        for (int segment_id = current_segment + 1; segment_id < current_segment + segments_per_load; segment_id++)
        {
            pc.bezierPath.AddSegmentToEnd(points[segment_id]);
        }
        current_segment += segments_per_load - 1;

        segment.AddComponent<RoadMeshCreator>();
        RoadMeshCreator rmc = segment.GetComponent<RoadMeshCreator>();
        rmc.pathCreator = pc;
        rmc.roadMaterial = road;
        rmc.undersideMaterial = underside;

        //force display
        Selection.activeGameObject = segment;

        followPath();
    }

    private void followPath()
    {
        Debug.Log(0);
        if (cyclist.GetComponent<PathFollower>().pathCreator != null)
        {
            Destroy(cyclist.GetComponent<PathFollower>());
            PathFollower pf = cyclist.AddComponent<PathFollower>();
            pf.pathCreator = segments[0].GetComponent<PathCreator>();
            pf.endOfPathInstruction = EndOfPathInstruction.Stop;
            pf.speed = 50;
        }
        else cyclist.GetComponent<PathFollower>().pathCreator = segments[0].GetComponent<PathCreator>();
    }

    private void createCheckpoint()
    {
        GameObject cp = new GameObject();
        cp.AddComponent<SphereCollider>();
        cp.AddComponent<CheckpointTrigger>();
        cp.GetComponent<SphereCollider>().radius = cp.GetComponent<CheckpointTrigger>().cp_radius;
        cp.GetComponent<SphereCollider>().isTrigger = true;
        cp.transform.position = points[current_segment];
    }

    public void setNextSegment()
    {
        //DEBUG perpetual addage
        while (!(current_segment <= points.Count - segments_per_load))
        {
            float x = points[points.Count - 1].x, z = points[points.Count - 1].z;
            if (points.Count % 2 == 0) z += 100;
            else x += 100;
            points.Add(new Vector3(x, 0, z));
        }
        createNextMesh();
        createCheckpoint();
    }

    public List<Vector3> getPoints() 
    {
        return points;
    }
}
