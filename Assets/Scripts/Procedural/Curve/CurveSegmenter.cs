using System.Collections;
using System.Collections.Generic;
using UnityEngine;
<<<<<<< Updated upstream
using UnityEditor;

using PathCreation.Examples;
using PathCreation;

public class CurveSegmenter : MonoBehaviour
{
    private const int POINTS_PER_SEGMENT = 4;

    Vector3[] points = new Vector3[] { new Vector3(0.0f, 0.0f, 0.0f), new Vector3(100.0f, 0.0f, 0.0f), new Vector3(100.0f, 0.0f, 100.0f), new Vector3(200.0f, 0.0f, 100.0f), new Vector3(200.0f, 0.0f, 200.0f), new Vector3(300.0f, 0.0f, 200.0f), new Vector3(300.0f, 0.0f, 300.0f), new Vector3(400.0f, 0.0f, 300.0f), new Vector3(400.0f, 0.0f, 400.0f) };
    private int current_point = 0;

    public Material road_material;
    public Material road_underside_material;
    
=======
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
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
        if (current_point + 3 < points.Length)
        {
            GameObject go = new GameObject();
            Selection.activeGameObject = go;
            go.AddComponent<PathCreator>();
            go.GetComponent<PathCreator>().bezierPath = new BezierPath(points[current_point * (POINTS_PER_SEGMENT - 1)]);
            go.AddComponent<RoadMeshCreator>();

            go.GetComponent<RoadMeshCreator>().roadMaterial = road_material;
            go.GetComponent<RoadMeshCreator>().undersideMaterial = road_underside_material;

            go.GetComponent<RoadMeshCreator>().PathUpdated();
            go.GetComponent<RoadMeshCreator>().pathCreator = go.GetComponent<PathCreator>();
            for (int point = 1; point < POINTS_PER_SEGMENT; point++)
            {
                go.GetComponent<PathCreator>().bezierPath.AddSegmentToEnd(points[current_point * (POINTS_PER_SEGMENT - 1) + point]);
            }
            current_point++;

            while (go.GetComponent<PathCreator>().bezierPath.NumSegments > POINTS_PER_SEGMENT)
            {
                go.GetComponent<PathCreator>().bezierPath.DeleteSegment(0);
            }

            //for (int i = 0; i < go.GetComponent<PathCreator>().bezierPath.NumSegments; i++)
            //{
            //   Debug.Log(go.GetComponent<PathCreator>().bezierPath.GetPoint(i));
            //}

            go.GetComponent<PathCreator>().bezierPath = go.GetComponent<PathCreator>().bezierPath;
        }
=======
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
>>>>>>> Stashed changes
    }
}
