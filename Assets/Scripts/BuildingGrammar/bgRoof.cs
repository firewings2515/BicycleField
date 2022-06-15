using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralToolkit.Buildings;

public class bgRoof : bgComponent
{
    private RoofPlanner roofPlanner = null;
    private RoofConstructor roofConstructor = null;
    //private PolygonAsset foundationPolygon = null;
    private BuildingGenerator.Config config = new BuildingGenerator.Config();

    public List<Vector3> vertexs;
    public List<Vector2> vertexs2D;
    RoofType roof_type = RoofType.Flat;
    float thickness = 0.2f;
    float overhang = 0.2f;

    public Vector3 model_pos = new Vector3(0, 0, 0);
    public bgRoof(List<string> _input_parameter, List<string> _component_parameter, List<string> _commands, List<List<string>> _commands_parameter) : base(_input_parameter, _component_parameter, _commands, _commands_parameter)
    {
        roofPlanner = Resources.Load<RoofPlanner>("ProceduralRoofPlanner");
        roofConstructor = Resources.Load<RoofConstructor>("ProceduralRoofConstructor");
        if (component_parameter.Count > 0)
        {
            thickness = float.Parse(component_parameter[0]);
            if (component_parameter.Count > 1)
            {
                overhang = float.Parse(component_parameter[1]);
                if (component_parameter.Count > 2) {
                    if (component_parameter[2] == "Flat") {
                        roof_type = RoofType.Flat;
                    }
                    else if (component_parameter[2] == "Hipped")
                    {
                        roof_type = RoofType.Hipped;
                    }
                    else if (component_parameter[2] == "Gabled")
                    {
                        roof_type = RoofType.Gabled;
                    }
                }
            }
        }
    }
    public override GameObject build()
    {
        go = new GameObject("Roof:" + name);

        config.roofConfig.type = roof_type;
        config.roofConfig.thickness = thickness;
        config.roofConfig.overhang = overhang;

        if (vertexs2D == null) vertexs2D = new List<Vector2>();
        else vertexs2D.Clear();

        for (int i = 0; i < vertexs.Count; i++) {
            vertexs2D.Add(new Vector2(vertexs[i].x, vertexs[i].z));
        }

        if (roofPlanner != null && roofConstructor != null)
        {
            var roofConstructible = roofPlanner.Plan(vertexs2D, config);

            var trans = go.transform;
            trans.localRotation = Quaternion.identity;
            roofConstructor.Construct(roofConstructible, trans);
        }

        for (int i = 0; i < commands.Count; i++)
        {
            if (commands[i] == "pos")
            {
                model_pos = new Vector3(float.Parse(commands_parameter[i][0]), float.Parse(commands_parameter[i][1]), float.Parse(commands_parameter[i][2]));
                continue;
            }
            else { 
                bgAsset model = builder.get_asset(commands[i]);
                GameObject obj = model.build();
                obj.transform.parent = go.transform;
                obj.transform.localPosition = model_pos;
            }

        }

        return go;
    }



}
