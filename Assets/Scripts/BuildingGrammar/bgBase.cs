using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class bgBase : bgComponent
{
    List<Vector3> vertexs;
    List<List<bgFacade>> facades;

    public bgBase(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter) : base(_input_parameter, _component_parameter, _commands, _commands_parameter)
    {

    }
    public override GameObject build()
    {
        Debug.Log("type: Base");
        go = new GameObject("Base:" + name);
        List<Vector3> vertexs = new List<Vector3>();
        List<List<bgFacade>> facades = new List<List<bgFacade>>();
        for (int i = 0; i < commands.Count; i++)
        {
            if (commands[i] == "vertex")
            {
                Vector3 vertex = new Vector3(
                    float.Parse(commands_parameter[i][0]),
                    float.Parse(commands_parameter[i][1]),
                    float.Parse(commands_parameter[i][2]));
                vertexs.Add(vertex);
            }
            else {
                if (vertexs.Count > facades.Count) {
                    facades.Add(new List<bgFacade>());
                }
                bgFacade facade = builder.get_facade(commands[i]);
                facades.Last().Add(facade);                
            }
        }

        for (int i = 0; i < vertexs.Count; i++) {
            Vector3 v1 = vertexs[i];
            Vector3 v2 = vertexs[(i+1)% vertexs.Count];
            float t = 1.0f / facades[i].Count;
            float total_t = t / 2.0f;
            float length = Vector3.Distance(v1, v2) * t;
            for (int j = 0; j < facades[i].Count; j++) {
                facades[i][j].width = length;
                Vector3 facade_pos = Vector3.Lerp(v1, v2, total_t);
                GameObject obj = facades[i][j].build();
                obj.transform.parent = go.transform;
                obj.transform.position = facade_pos;
                obj.transform.rotation = Quaternion.LookRotation(v2-v1, Vector3.up);
                obj.transform.Rotate(new Vector3(0,1,0),90);
                total_t += t;
            }
            

        }

        return go;
    }
}
