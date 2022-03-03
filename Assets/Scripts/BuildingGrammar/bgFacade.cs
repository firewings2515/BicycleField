using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgFacade : bgComponent
{
    public float width = 4.0f; //come from upper level

    public bgFacade(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter) : base(_input_parameter, _component_parameter, _commands, _commands_parameter)
    {
        
    }

    public override GameObject build()
    {
        go = new GameObject("Facade:" + name);
        Debug.Log("type: Facade");
        float height = 0.0f;
        for (int i = 0; i < commands.Count; i++) {
            bgWall wall = builder.get_wall(commands[i]);
            wall.width = this.width;
            GameObject obj = wall.build();
            height += wall.height;
            obj.transform.localPosition = new Vector3(0, height, 0);
            obj.transform.parent = go.transform;
            
        }

        return go;
    }
}
