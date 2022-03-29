using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class bgBase : bgComponent
{
    List<Vector3> vertexs;
    List<List<bgFacade>> facades;
    public float height = float.MinValue;
    public bgBase(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter) : base(_input_parameter, _component_parameter, _commands, _commands_parameter)
    {

    }
    public override GameObject build()
    {
        height = float.MinValue;
        //random_background = Random.Range(0, 15);
        //if (go != null)
        //{
        //    go = GameObject.Instantiate(go);
        //    go.name = "Base: " + name;
        //    return go;
        //}
        go = new GameObject("Base:" + name);
        //return go;
        if (vertexs == null)
        {
            vertexs = new List<Vector3>();
            facades = new List<List<bgFacade>>();
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
                else
                {
                    if (vertexs.Count > facades.Count)
                    {
                        facades.Add(new List<bgFacade>());
                    }
                    bgFacade facade = builder.get_facade(commands[i]);
                    facades.Last().Add(facade);
                }
            }
        }

        for (int i = 0; i < vertexs.Count; i++) {
            Vector3 v1 = vertexs[i];
            Vector3 v2 = vertexs[(i + 1) % vertexs.Count];
            float t = 1.0f / facades[i].Count;
            float total_t = t / 2.0f;
            float length = Vector3.Distance(v1, v2) * t;
            for (int j = 0; j < facades[i].Count; j++) {
                facades[i][j].width = length;
                facades[i][j].random_background = this.random_background;
                Vector3 facade_pos = Vector3.Lerp(v1, v2, total_t);
                GameObject obj = facades[i][j].build();

                //GameObject obj = new GameObject("facade");
                //
                //obj.AddComponent<MeshFilter>().mesh = facades[i][j].build_mesh();
                //obj.AddComponent<MeshRenderer>().sharedMaterial = Resources.Load<Material>("Material/tillable");
                //
                obj.transform.parent = go.transform;
                obj.transform.position = facade_pos;
                obj.transform.rotation = Quaternion.LookRotation(v2 - v1, Vector3.up);
                obj.transform.Rotate(new Vector3(0, 1, 0), 90);

                if (j == 0) {
                    float vert_height = facades[i][0].height;

                    GameObject vert_split = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    GameObject.Destroy(vert_split.GetComponent<BoxCollider>());
                    vert_split.transform.parent = go.transform;
                    vert_split.transform.localScale = new Vector3(0.5f, vert_height, 0.5f);
                    vert_split.transform.localPosition = v1 + new Vector3(0, vert_height/2.0f, 0);
                    vert_split.transform.rotation = Quaternion.LookRotation(v2 - v1, Vector3.up);
                    vert_split.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/gray");
                    //vert_height = facades[(i + 1) % vertexs.Count][0].height;
                    //GameObject vert_split2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    //vert_split2.transform.parent = go.transform;
                    //vert_split2.transform.localScale = new Vector3(0.1f, vert_height, 1.0f);
                    //vert_split2.transform.localPosition = v2 + new Vector3(0, vert_height / 2.0f, 0);
                    //vert_split2.transform.rotation = Quaternion.LookRotation(v2 - v1, Vector3.up);
                    if (vert_height > height) height = vert_height;
                }

                total_t += t;
            }

        }

        return go;
    }
}
