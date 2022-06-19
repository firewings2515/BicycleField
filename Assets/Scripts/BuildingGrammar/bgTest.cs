using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgTest : MonoBehaviour
{
    public string component_name;
    public string[] grammar_files_path;
    public bool random_pos = false;
    public int max_house_count = 1;
    public bgBuilder builder;
    private GameObject building;
    MeshRenderer mr;
    GameObject obj;
    // Start is called before the first frame update
    void Start()
    {
        builder = new bgBuilder();
        reCompile();
        //reBuild();
        //build_polygon_house(new List<Vector3>
        //{ new Vector3(2,0,8),new Vector3(2,0,2),new Vector3(8,0,2),new Vector3(8,0,-2),
        // new Vector3(2,0,-2),new Vector3(2,0,-8),new Vector3(-2,0,-8),new Vector3(-2,0,-2),
        // new Vector3(-8,0,-2),new Vector3(-8,0,2),new Vector3(-2,0,2),new Vector3(-2,0,8)
        //});
    }
    public void reBuild() {
        float start = Time.realtimeSinceStartup;

        //Debug.Log(Time.realtimeSinceStartup - start);
        //building = builder.build(component_name);


        //GameObject obj = new GameObject();
        //obj.AddComponent<MeshFilter>().sharedMesh = builder.build_mesh(component_name);
        //obj.AddComponent<MeshRenderer>().sharedMaterial = default_;

        obj = builder.build(component_name);
        obj.transform.Translate(-30,0,0);
        float end = Time.realtimeSinceStartup;
        //Debug.Log("process time:" + (end - start).ToString());
    }

    public void load_polygon(List<Vector3> vertexs)
    {
        builder.load_base_coords(component_name, vertexs);
    }
    public GameObject build_polygon()
    {
        return  builder.build(component_name);
    }

    public void build_polygon_house(List<Vector3> vertexs)
    {
        List<List<string>> component_names = new List<List<string>>();
        for (int i = 0; i < vertexs.Count; i++) {
            component_names.Add(new List<string>());
            //component_names[i].Add("hello_facade");
            component_names[i].Add("nothing_facade");
        }
        builder.load_base_coords("runtime_base", vertexs);
        builder.load_base_facades("runtime_base", component_names);
        obj = builder.build("runtime_base");
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
            //reBuild();
            count = 0;
        }
        else
            count++;


        //reBuild();
    }
}
