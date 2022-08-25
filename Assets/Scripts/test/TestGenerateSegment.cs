using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TestGenerateSegment : MonoBehaviour
{
    public Material bottom = null;
    public Material top = null;

    private List<Vector3> test_list = new List<Vector3>();

    private Vector3 undefined_vector = new Vector3(-9999, -9999, -9999);
    private Vector3 connect_recall = new Vector3(-9999, -9999, -9999);

    private int num_rendered = 4;
    private int render_pointer = 0;

    // Start is called before the first frame update
    void Start()
    {
        /*
        test_list.Add(new Vector3(0, 0, 0));
        test_list.Add(new Vector3(10, 0, 0));
        test_list.Add(new Vector3(90, 0, 0));
        test_list.Add(new Vector3(100, 0, 0));
        test_list.Add(new Vector3(110, 0, -1));
        test_list.Add(new Vector3(190, 0, -9));
        test_list.Add(new Vector3(200, 0, 0));
        test_list.Add(new Vector3(200, 0, 10));
        test_list.Add(new Vector3(200, 0, 90));
        test_list.Add(new Vector3(200, 0, 100));
        test_list.Add(new Vector3(200, 0, 110));
        test_list.Add(new Vector3(200, 0, 190));
        test_list.Add(new Vector3(200, 0, 200));
        test_list.Add(new Vector3(199, 0, 210));
        test_list.Add(new Vector3(190, 0, 290));
        test_list.Add(new Vector3(200, 0, 300));
        test_list.Add(new Vector3(210, 0, 300));
        test_list.Add(new Vector3(290, 0, 300));
        test_list.Add(new Vector3(300, 0, 300));
        test_list.Add(new Vector3(310, 0, 300));
        test_list.Add(new Vector3(390, 0, 300));
        test_list.Add(new Vector3(400, 0, 300));
        test_list.Add(new Vector3(410, 0, 300));
        test_list.Add(new Vector3(490, 0, 300));
        */
        test_list.Add(new Vector3(0, 0, 0));
        test_list.Add(new Vector3(100, 0, 0));
        test_list.Add(new Vector3(200, 0, 0));
        test_list.Add(new Vector3(200, 0, 100));
        test_list.Add(new Vector3(200, 0, 200));
        test_list.Add(new Vector3(200, 0, 300));
        test_list.Add(new Vector3(300, 0, 300));
        test_list.Add(new Vector3(400, 0, 300));

        for (int id = 0; id < num_rendered; id++) createNewSegment();
    }

    // Update is called once per frame
    void Update()
    {
        if (OverallManager.segments.Count < num_rendered) createNewSegment();
    }

    private void createNewSegment()
    {
        List<Vector3> points = new List<Vector3>() { test_list[(render_pointer++) % test_list.Count], test_list[(render_pointer) % test_list.Count] };

        //object
        GameObject segment = new GameObject();
        OverallManager.segments.Add(segment);
        segment.transform.eulerAngles = new Vector3(90.0f, 0.0f, 0.0f);

        //line
        segment.AddComponent<PathCreation.Examples.MyRoadMeshCreator>();
        segment.AddComponent<PathCreation.PathCreator>();
        segment.GetComponent<PathCreation.Examples.MyRoadMeshCreator>().pathCreator = segment.GetComponent<PathCreation.PathCreator>();

        //material
        segment.GetComponent<PathCreation.Examples.MyRoadMeshCreator>().roadMaterial = top;
        segment.GetComponent<PathCreation.Examples.MyRoadMeshCreator>().undersideMaterial = bottom;

        for (int id = 0; id < points.Count; id++)
        {
            segment.GetComponent<PathCreation.PathCreator>().bezierPath.AddSegmentToEnd(points[id]);
        }

        segment.GetComponent<PathCreation.PathCreator>().bezierPath.DeleteSegment(0);
        segment.GetComponent<PathCreation.PathCreator>().bezierPath.DeleteSegment(0);

        if (connect_recall != undefined_vector) segment.GetComponent<PathCreation.PathCreator>().bezierPath.points[1] = segment.GetComponent<PathCreation.PathCreator>().bezierPath.points[0] + connect_recall.normalized * (segment.GetComponent<PathCreation.PathCreator>().bezierPath.points[1] - segment.GetComponent<PathCreation.PathCreator>().bezierPath.points[0]).magnitude;
        else segment.GetComponent<PathCreation.PathCreator>().bezierPath.points[1] = segment.GetComponent<PathCreation.PathCreator>().bezierPath.points[0] + segment.GetComponent<PathCreation.PathCreator>().bezierPath.points[0] - segment.GetComponent<PathCreation.PathCreator>().bezierPath.points[1];
        Debug.Log(segment.GetComponent<PathCreation.PathCreator>().bezierPath.points[1]);
        connect_recall = segment.GetComponent<PathCreation.PathCreator>().bezierPath.points[segment.GetComponent<PathCreation.PathCreator>().bezierPath.points.Count - 1] - segment.GetComponent<PathCreation.PathCreator>().bezierPath.points[segment.GetComponent<PathCreation.PathCreator>().bezierPath.points.Count - 2];
        
        //mesh
        segment.GetComponent<PathCreation.Examples.MyRoadMeshCreator>().PathUpdated();
    }
}
