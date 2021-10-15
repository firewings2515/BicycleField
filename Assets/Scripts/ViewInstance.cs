using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewInstance : MonoBehaviour
{
    public GameObject prefab;
    public GameObject instance;
    bool in_view = false;
    public GameObject cam;
    public Vector3[] points;
    public bool finish_instance = false;
    // Start is called before the first frame update
    void Start()
    {
        //InvokeRepeating("viewerUpdate", 0f, 0.02f);
    }

    public void setup(bool setup_prefab)
    {
        if (setup_prefab)
            instance = Instantiate(prefab, gameObject.transform);

        instance.SetActive(false);

        finish_instance = true;
    }

    private void Update()
    {
        if (finish_instance)
        {
            Vector2 f = new Vector2(cam.transform.forward.x, cam.transform.forward.z).normalized;

            foreach (Vector3 point in points)
            {
                //yield return new WaitForEndOfFrame();

                Vector2 v = new Vector2(point.x, point.z) - new Vector2(cam.transform.position.x, cam.transform.position.z);
                float dist = v.magnitude;
                float angle = Vector2.Angle(f, v);

                if ((angle > 65 && dist > 150) || dist > 100)
                {
                    in_view = false;
                }
                else if (angle <= 65 && dist <= 150)
                {
                    in_view = true;
                    break;
                }
            }

            instance.SetActive(in_view);
        }
    }
}