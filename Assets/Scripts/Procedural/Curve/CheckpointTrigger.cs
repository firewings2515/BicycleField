using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CheckpointTrigger : MonoBehaviour
{
    public float cp_radius = 50.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        EventSystem.current.GetComponent<CurveSegmenter>().setNextSegment();
        Destroy(gameObject);
    }
}
