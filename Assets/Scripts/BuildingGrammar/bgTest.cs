using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgTest : MonoBehaviour
{
    public string component_name;
    public string[] grammar_files_path;
    private bgBuilder builder;
    private GameObject building;
    Material default_;
    Queue<GameObject> houses;
    Camera camera;
    Transform cam;
    // Start is called before the first frame update
    void Start()
    {
        default_ = GetComponent<MeshRenderer>().sharedMaterial;
        builder = new bgBuilder();
        houses = new Queue<GameObject>();
        float start = Time.realtimeSinceStartup;
        builder.clear();
        builder.compile_code(grammar_files_path);
        float end = Time.realtimeSinceStartup;
        Debug.Log("compile time:" + (end - start).ToString());
        camera = Camera.main;
        cam = camera.transform;
        reBuild();
    }
    public void reBuild() {
        float start = Time.realtimeSinceStartup;

        //Debug.Log(Time.realtimeSinceStartup - start);
        //building = builder.build(component_name);


        //GameObject obj = new GameObject();
        //obj.AddComponent<MeshFilter>().sharedMesh = builder.build_mesh(component_name);
        //obj.AddComponent<MeshRenderer>().sharedMaterial = default_;
        float range = 100.0f;
        GameObject obj = builder.build(component_name);
        obj.transform.position = new Vector3(cam.position.x + Random.Range(-range, range),0, cam.position.z + Random.Range(-range, range));
        obj.transform.rotation = Quaternion.Euler(0, Random.Range(0.0f,360.0f), 0);
        houses.Enqueue(obj);
        if (houses.Count > 100) {
            houses.Dequeue();
        }
        float end = Time.realtimeSinceStartup;
        Debug.Log("process time:" + (end - start).ToString());
    }
    int count = 0;
    // Update is called once per frame
    void Update()
    {
        //if (count == 5)
        //{
        //    reBuild();
        //    count = 0;
        //}
        //else
        //    count++;

        reBuild();
    }
}
