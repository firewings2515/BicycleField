using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;
using PathCreation.Examples;

public class ObjectManager : MonoBehaviour
{
    private List<GameObject> segments = new List<GameObject>() { };
    private int current_segment = 0;
    private GameObject current_cyclist;

    public string filename;
    public Material road;
    public Material underside;
    public GameObject cyclist;
    public Camera cam;

    public bool need_update = false;

    // Start is called before the first frame update
    void Start()
    {
        Data.loadFile(filename);
        loadNextSegment();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            need_update = true;
        }

        if (need_update)
        {
            loadNextSegment();
            unloadLastSegment();

            current_segment += 1;
            need_update = false;
        }
    }

    private void loadNextSegment()
    {
        createCheckpoint();
        createRoadMesh();

        createCyclist();
    }

    private void unloadLastSegment()
    {
        Destroy(segments[0]);
        segments.RemoveAt(0);
    }

    private void createRoadMesh()
    {
        GameObject segment = new GameObject();
        segment.AddComponent<PathCreator>();
        PathCreator pc = segment.GetComponent<PathCreator>();
        pc.bezierPath = new BezierPath(Data.points[current_segment]);
        for (int segment_id = current_segment + 1; segment_id < current_segment + Data.POINTS_PER_SEGMENT; segment_id++)
        {
            pc.bezierPath.AddSegmentToEnd(Data.points[segment_id]);
        }

        segment.AddComponent<RoadMeshCreator>();
        RoadMeshCreator rmc = segment.GetComponent<RoadMeshCreator>();
        rmc.pathCreator = pc;
        rmc.roadMaterial = road;
        rmc.undersideMaterial = underside;

        //force display
        Selection.activeGameObject = segment;

        segments.Add(segment);
    }

    private void createCheckpoint()
    {
        GameObject prefab = new GameObject();
        prefab.transform.position = Data.points[current_segment];
        prefab.AddComponent<SphereCollider>();
        prefab.GetComponent<SphereCollider>().isTrigger = true;
        prefab.GetComponent<SphereCollider>().transform.localScale *= Data.CHECKPOINT_RADIUS;
        prefab.AddComponent<Checkpoint>();
    }

    private void createCyclist()
    {
        current_cyclist = Instantiate(cyclist, Data.points[current_segment], Quaternion.identity);
        current_cyclist.GetComponent<PathFollower>().pathCreator = segments[current_segment].GetComponent<PathCreator>();
    }
}
