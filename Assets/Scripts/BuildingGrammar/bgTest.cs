using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgTest : MonoBehaviour
{
    public string component_name;
    public string[] grammar_files_path;
    public bool random_pos = false;
    public int max_house_count = 1;
    private bgBuilder builder;
    private GameObject building;
    Material default_;
    MeshRenderer mr;
    GameObject obj;
    // Start is called before the first frame update
    void Start()
    {
        default_ = GetComponent<MeshRenderer>().sharedMaterial;
        builder = new bgBuilder();
        reCompile();
        reBuild();
    }
    public void reBuild() {
        float start = Time.realtimeSinceStartup;

        //Debug.Log(Time.realtimeSinceStartup - start);
        //building = builder.build(component_name);


        //GameObject obj = new GameObject();
        //obj.AddComponent<MeshFilter>().sharedMesh = builder.build_mesh(component_name);
        //obj.AddComponent<MeshRenderer>().sharedMaterial = default_;

        obj = builder.build(component_name);

        float end = Time.realtimeSinceStartup;
        //Debug.Log("process time:" + (end - start).ToString());
    }
    public void reCompile()
    {
        float start = Time.realtimeSinceStartup;
        builder.clear();
        builder.compile_code(grammar_files_path);
        float end = Time.realtimeSinceStartup;
        Debug.Log("compile time:" + (end - start).ToString());
    }
    public void random_pos_on(bool val) {
        random_pos = val;
    }

    public void max_house(float val)
    {
        max_house_count = (int)val;
    }
    int count = 0;
    // Update is called once per frame
    void Update()
    {
        if (count == 5)
        {
            reBuild();
            count = 0;
        }
        else
            count++;

        //reBuild();
    }
}
