using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgBuilding : bgComponent
{
    public List<bgBase> bases;
    //public float height = 0.0f;
    public Vector3 center = new Vector3(0,0,0);
    public bgBuilding(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter) : base(_input_parameter, _component_parameter, _commands, _commands_parameter)
    {

    }
    public override GameObject build()
    {
        //height = 0.0f;
        random_background = Random.Range(0, 15);
        go = new GameObject("Building:" + name);
        for (int i = 0; i < commands.Count; i++) {
            if (commands[i] == "center")
            {
                center = new Vector3(float.Parse(commands_parameter[i][0]), float.Parse(commands_parameter[i][1]), float.Parse(commands_parameter[i][2]));

                continue;
            }
            else
            {
                bgBase _base = builder.get_base(commands[i]);
                _base.random_background = random_background;
                GameObject obj = _base.build();
                obj.transform.parent = go.transform;
                obj.transform.localPosition = center;
                center.y += _base.height;
            }
        }
        return go;
    }
}
