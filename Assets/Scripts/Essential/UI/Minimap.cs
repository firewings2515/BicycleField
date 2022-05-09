using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public Transform player;
    public GameObject minimap;
    private float height = 20.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) minimap.SetActive(!minimap.activeSelf);
        if (Input.GetKey(KeyCode.Alpha1)) height += 1f;
        if (Input.GetKey(KeyCode.Alpha2)) height -= 1f;

        if (height < 1f) height = 1;
        if (height > 1000f) height = 1000f;
    }

    private void LateUpdate()
    {
        Vector3 newPosition = player.position;
        newPosition.y = player.position.y + height;
        transform.position = newPosition;
    }
}
