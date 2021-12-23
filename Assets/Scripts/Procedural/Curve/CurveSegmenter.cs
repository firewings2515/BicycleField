using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveSegmenter : MonoBehaviour
{
    Vector3[] points = new Vector3[] { new Vector3(0.0f, 0.0f, 0.0f), new Vector3(100.0f, 0.0f, 0.0f), new Vector3(100.0f, 0.0f, 100.0f), new Vector3(200.0f, 0.0f, 100.0f), new Vector3(200.0f, 0.0f, 200.0f), new Vector3(300.0f, 0.0f, 200.0f), new Vector3(300.0f, 0.0f, 300.0f), new Vector3(400.0f, 0.0f, 300.0f), new Vector3(400.0f, 0.0f, 400.0f) };
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
        
    }
}
