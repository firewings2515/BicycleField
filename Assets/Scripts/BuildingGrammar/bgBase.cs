using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class bgBase : bgComponent
{
    public List<Vector3> vertexs;
    public List<List<bgFacade>> facades;
    public float height = float.MinValue;
    float floor_height = float.MaxValue;
    int vertex_read = 0;
    bool runtime_vertex = false;
    public bgBase(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter) : base(_input_parameter, _component_parameter, _commands, _commands_parameter)
    {

    }
    public override GameObject build()
    {
        height = float.MinValue;
        floor_height = float.MaxValue;
        random_background = Random.Range(0, 15);
        //if (go != null)
        //{
        //    go = GameObject.Instantiate(go);
        //    go.name = "Base: " + name;
        //    return go;
        //}
       
        go = new GameObject("Base:" + name);
        //return go;

        if (component_parameter.Count > 0)
        {
            if (component_parameter[0] == "runtime_vertex")
            {
                runtime_vertex = true;
            }
        }

        if (facades == null)
        {
            if (runtime_vertex == false)
            {
                vertexs = new List<Vector3>();
            }
            facades = new List<List<bgFacade>>();

            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i] == "vertex")
                {
                    if (runtime_vertex == false)
                    {
                        Vector3 vertex = new Vector3(
                            float.Parse(commands_parameter[i][0]),
                            float.Parse(commands_parameter[i][1]),
                            float.Parse(commands_parameter[i][2]));
                        vertexs.Add(vertex);
                    }
                    vertex_read++;
                }
                else
                {
                    if (vertex_read > facades.Count)
                    {
                        facades.Add(new List<bgFacade>());
                    }
                    bgFacade facade = builder.get_facade(commands[i]);
                    facades.Last().Add(facade);
                }
            }
        }

        if (!determine_clock_wise(vertexs)) {
            vertexs.Reverse();
            facades.Reverse();
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

        //create floor
        List<Vector2> floor_vertex2D = new List<Vector2>();
        float floor_scale = 1.0f;
        for (int i = 0; i < vertexs.Count; i++)
        {
            floor_vertex2D.Add(new Vector2(vertexs[i].x * floor_scale, vertexs[i].z * floor_scale));
            if (floor_height > vertexs[i].y) floor_height = vertexs[i].y;
        }
        GameObject floor = PolygonPlane.create(floor_vertex2D);
        floor.transform.parent = go.transform;
        floor.transform.localPosition = new Vector3(0, floor_height, 0);

        //create roof
        List<Vector2> roof_vertex2D= new List<Vector2>();
        float roof_scale = 1.2f;
        for (int i = 0; i < vertexs.Count; i++) {
            roof_vertex2D.Add(new Vector2(vertexs[i].x * roof_scale, vertexs[i].z * roof_scale));
        }
        GameObject roof = PolygonPlane.create(roof_vertex2D);
        roof.transform.parent = go.transform;
        roof.transform.localPosition = new Vector3(0,height,0);
        return go;
    }

    bool determine_clock_wise(List<Vector3> points) { //�P�_���ɰwOR�f�ɰw
        //https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
        int coords_size = points.Count;
        float result = 0.0f;
        for (int index = 0; index < coords_size; index++) {
            Vector3 current = points[index];
            Vector3 next = points[(index + 1) % coords_size];
            result += (next.x - current.x) * (next.z + current.z);
        }
        return result >= 0.0f;
    }
}
