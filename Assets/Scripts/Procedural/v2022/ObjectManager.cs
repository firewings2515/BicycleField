using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;
using PathCreation.Examples;

public class ObjectManager : MonoBehaviour
{
    private GameObject current_segment = null;
    private int current_segment_id = 0;
    private GameObject current_cyclist;

    public string filename;
    public Material road;
    public Material underside;
    public GameObject cyclist;
    public float cyclist_speed;
    public Camera cam;

    public bool need_update = false;

    // Start is called before the first frame update
    void Start()
    {
        Data.loadFile(filename);
        loadNextSegment();
        HouseGenerator.init();
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
            need_update = false;
        }
    }

    private void loadNextSegment()
    {
        if (current_segment_id == 0 || current_segment_id > Data.POINTS_PER_SEGMENT / 2)
        {
            createRoadMesh();
            createCyclist();
        }

        current_segment_id += 1;
        createCheckpoint();

    }

    private void createRoadMesh()
    {
        GameObject segment = new GameObject();
        segment.AddComponent<PathCreator>();
        PathCreator pc = segment.GetComponent<PathCreator>();
        pc.bezierPath = new BezierPath(Data.points[(current_segment_id - Data.POINTS_PER_SEGMENT / 2 < 0) ? 0 : current_segment_id - Data.POINTS_PER_SEGMENT / 2]);
        for (int segment_id = current_segment_id - ((current_segment_id - Data.POINTS_PER_SEGMENT / 2 < 0) ? 0 : current_segment_id - Data.POINTS_PER_SEGMENT / 2) + 1; segment_id < current_segment_id + Data.POINTS_PER_SEGMENT; segment_id++)
        {
            pc.bezierPath.AddSegmentToEnd(Data.points[segment_id]);
        }
        //remove default segment
        pc.bezierPath.DeleteSegment(0);
        pc.bezierPath.DeleteSegment(0);

        segment.AddComponent<RoadMeshCreator>();
        RoadMeshCreator rmc = segment.GetComponent<RoadMeshCreator>();
        rmc.pathCreator = pc;
        rmc.roadMaterial = road;
        rmc.undersideMaterial = underside;

        //force display
        Selection.activeGameObject = segment;

        Destroy(current_segment);
        current_segment = segment;
    }

    private void createCheckpoint()
    {
        GameObject prefab = new GameObject();
        prefab.transform.position = Data.points[current_segment_id];
        prefab.AddComponent<SphereCollider>();
        prefab.GetComponent<SphereCollider>().isTrigger = true;
        prefab.GetComponent<SphereCollider>().transform.localScale *= Data.CHECKPOINT_RADIUS;
        prefab.AddComponent<Checkpoint>();
    }

    private void createCyclist()
    {
        GameObject new_cyclist = Instantiate(cyclist, Data.points[0], Quaternion.identity);

        cam.transform.parent = new_cyclist.transform;
        cam.transform.position = new Vector3(-1, 0, 0);
        cam.transform.eulerAngles = new Vector3(0,0,90);

        if (current_cyclist != null)
        {
            new_cyclist.transform.position = current_cyclist.transform.position;
            Destroy(current_cyclist);
        }
        current_cyclist = new_cyclist;
        current_cyclist.GetComponent<PathFollower>().pathCreator = current_segment.GetComponent<PathCreator>();
        current_cyclist.GetComponent<PathFollower>().speed = cyclist_speed;
        current_cyclist.GetComponent<PathFollower>().setDistance(current_cyclist.GetComponent<PathFollower>().nearestDistance());
        cam.transform.parent = current_cyclist.transform;
    }
}
