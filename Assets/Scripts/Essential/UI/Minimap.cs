using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public Transform player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Minus)) Info.mapview_height += 1f;
        if (Input.GetKey(KeyCode.Equals)) Info.mapview_height -= 1f;

        if (Info.mapview_height < 1f) Info.mapview_height = 1;
        if (Info.mapview_height > 1000f) Info.mapview_height = 1000f;
    }

    private void LateUpdate()
    {
        Vector3 newPosition = player.position;
        newPosition.y = player.position.y + Info.mapview_height;
        transform.position = newPosition;
    }
}
