using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnchorCheckpoint2 : MonoBehaviour
{
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
        if (other.gameObject.tag == "Player")
        {
            GameObject.Find("EventSystem").GetComponent<RoadManager2>().incrementCurrentSegment();
            Destroy(this.gameObject);
        }
    }
}
