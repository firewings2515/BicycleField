using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyclistFollow : MonoBehaviour
{
    float distanceTravelled = 0;
    int current_segment = 0;
    float speed = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distanceTravelled += speed;
        if (distanceTravelled >= OverallManager.segments[current_segment].GetComponent<PathCreation.PathCreator>().path.length)
        {
            distanceTravelled -= OverallManager.segments[current_segment].GetComponent<PathCreation.PathCreator>().path.length;

            GameObject temp = OverallManager.segments[0];
            OverallManager.segments.RemoveAt(0);
            Destroy(temp);
        }
        transform.position = Vector3.up + OverallManager.segments[current_segment].GetComponent<PathCreation.PathCreator>().path.GetPointAtDistance(distanceTravelled, PathCreation.EndOfPathInstruction.Stop);
        Vector3 here = OverallManager.segments[current_segment].GetComponent<PathCreation.PathCreator>().path.GetPointAtDistance(distanceTravelled, PathCreation.EndOfPathInstruction.Stop);
        Vector3 there = (distanceTravelled + 1f < OverallManager.segments[current_segment].GetComponent<PathCreation.PathCreator>().path.length) ? 
            OverallManager.segments[current_segment].GetComponent<PathCreation.PathCreator>().path.GetPointAtDistance(distanceTravelled + 1f, PathCreation.EndOfPathInstruction.Stop)
            : OverallManager.segments[current_segment + 1].GetComponent<PathCreation.PathCreator>().path.GetPointAtDistance(distanceTravelled + 1f - OverallManager.segments[current_segment].GetComponent<PathCreation.PathCreator>().path.length, PathCreation.EndOfPathInstruction.Stop);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(((there - here).magnitude < 0.000001f) ? Vector3.forward : there - here, Vector3.up), 0.2f);

    }
}
