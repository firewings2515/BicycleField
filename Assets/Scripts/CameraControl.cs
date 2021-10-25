using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            this.gameObject.transform.position += this.gameObject.transform.forward * 0.2f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            this.gameObject.transform.position -= this.gameObject.transform.forward * 0.2f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            this.gameObject.transform.position -= this.gameObject.transform.right * 0.2f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            this.gameObject.transform.position += this.gameObject.transform.right * 0.2f;
        }



        if (Input.GetKey(KeyCode.LeftArrow))
        {
            this.gameObject.transform.Rotate(Vector3.up, -1.0f);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            this.gameObject.transform.Rotate(Vector3.up, 1.0f);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            this.gameObject.transform.Rotate(Vector3.right, -1.0f);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            this.gameObject.transform.Rotate(Vector3.right, 1.0f);
        }
    }
}