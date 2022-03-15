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
    // Start is called before the first frame update
    void Start()
    {
        default_ = GetComponent<MeshRenderer>().sharedMaterial;
        builder = new bgBuilder();        
        reBuild();
    }
    public void reBuild() {
        float start = Time.realtimeSinceStartup;
        builder.clear();
        Debug.Log(Time.realtimeSinceStartup - start);
        builder.compile_code(grammar_files_path);
        Debug.Log(Time.realtimeSinceStartup - start);
        //building = builder.build(component_name);


        //GameObject obj = new GameObject();
        //obj.AddComponent<MeshFilter>().sharedMesh = builder.build_mesh(component_name);
        //obj.AddComponent<MeshRenderer>().sharedMaterial = default_;

        GameObject obj = builder.build(component_name);

        float end = Time.realtimeSinceStartup;
        Debug.Log("process time:" + (end - start).ToString());
    }
    int count = 0;
    // Update is called once per frame
    void Update()
    {
        if (count == 5)
        {
            //reBuild();
            count = 0;
        }
        else
            count++;
    }
}
