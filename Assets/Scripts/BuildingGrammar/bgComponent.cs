using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgComponent
{
    public bgGrammarLocation source_code;

    public string name;
    public string type;

    public List<string> input_parameter;
    public List<string> component_parameter;

    public List<string> commands;
    public List<List<string>> commands_parameter;

    public Vector3 center;
    public Quaternion rotate;

    public GameObject go;

    public bgBuilder builder;

    public bgComponent()
    {
        
    }

    public bgComponent(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter)
    {
        input_parameter = _input_parameter;
        component_parameter = _component_parameter;
        commands = _commands;
        commands_parameter = _commands_parameter;
    }
    public virtual GameObject build()
    {
        if (this.type == "Asset")
        {
            return ((bgAsset)this).build();
        }
        else if (this.type == "Wall")
        {
            return ((bgWall)this).build();
        }
        else if (this.type == "Facade")
        {
            return ((bgFacade)this).build();
        }
        else if (this.type == "Base")
        {
            return ((bgBase)this).build();
        }
        else if (this.type == "Building")
        {
            return ((bgBuilding)this).build();
        }
        return null;
    }

    public virtual void parse()
    {

    }
}
