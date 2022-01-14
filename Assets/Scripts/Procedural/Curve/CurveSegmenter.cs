using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            createNextMesh();
        }
    }

    private void createNextMesh()
    {
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
    }
}
