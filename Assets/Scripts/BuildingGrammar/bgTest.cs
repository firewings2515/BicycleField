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
        builder.clear();
        builder.compile_code(grammar_files_path);
        building = builder.build(component_name);


        //GameObject obj = new GameObject();
        //obj.AddComponent<MeshFilter>().sharedMesh = builder.build_mesh(component_name);
        //obj.AddComponent<MeshRenderer>().sharedMaterial = default_;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
