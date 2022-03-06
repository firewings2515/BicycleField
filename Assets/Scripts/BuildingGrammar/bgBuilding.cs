using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgBuilding : bgComponent
{
    public List<bgBase> bases;

    public bgBuilding(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter) : base(_input_parameter, _component_parameter, _commands, _commands_parameter)
    {

    }
    public override GameObject build()
    {
        Debug.Log("type: Building");

        return go;
    }
}
