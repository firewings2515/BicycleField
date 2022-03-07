using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgTest : MonoBehaviour
{
    public string component_name;
    public string[] grammar_files_path;
    private bgBuilder builder;
    private GameObject building;
    // Start is called before the first frame update
    void Start()
    {
        builder = new bgBuilder();
        builder.compile_code(grammar_files_path);
        building = builder.build(component_name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
