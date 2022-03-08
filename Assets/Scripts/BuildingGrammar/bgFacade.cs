using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgFacade : bgComponent
{
    public float width = 4.0f; //come from upper level
    List<float> widths;
    List<Vector3> positions;

    public bgFacade(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter) : base(_input_parameter, _component_parameter, _commands, _commands_parameter)
    {
        positions = new List<Vector3>();
        widths = new List<float>();

    }

    public override GameObject build()
    {
        go = new GameObject("Facade:" + name);
        Debug.Log("type: Facade");
        float height = 0.0f;
        for (int i = 0; i < commands.Count; i++)
        {
            bgWall wall = builder.get_wall(commands[i]);
            wall.width = this.width;
            widths.Add(width);
            GameObject obj = wall.build();

            obj.transform.localPosition = new Vector3(0, height, 0);
            positions.Add(new Vector3(0, height, 0));
            height += wall.height;
            obj.transform.parent = go.transform;

        }



        return go;
    }
}
