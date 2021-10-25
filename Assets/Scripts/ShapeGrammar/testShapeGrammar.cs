using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
public class testShapeGrammar : MonoBehaviour
{

    public string filename = ".\\grammars\\hello_house.shp";
    [Header("build with parameters")]
    public bool load_file_n_parameters = false;
    public bool build_model_with_parameters = false;
    public List<ParameterPair> parameters = new List<ParameterPair>();
    [Header("build with default")]
    public bool build = false;

    List<GameObject> buildlist = new List<GameObject>();
    int index1 = -1;
    int index2 = -1;
    bool tag1 = false;
    bool tag2 = false;
    Thread[] threads = new Thread[2];
    // Start is called before the first frame update
    void Start()
    {
        ShapeGrammarBuilder.InitClass();
        bool flag = false;
        bool flag2 = false;
        index1 = ShapeGrammarBuilder.getFreeStack();
        if (index1 != -1)
        {
            ShapeGrammarBuilder.InitObject(index1);

            flag = ShapeGrammarBuilder.loadShape(filename, index1);
        }
        Debug.Log("Get Index: " + index1.ToString());
        
        
        index2 = ShapeGrammarBuilder.getFreeStack();
        Debug.Log("Get Index2: " + index2.ToString());
        if (index2 != -1)
        {
            ShapeGrammarBuilder.InitObject(index2);
            flag2 = ShapeGrammarBuilder.loadShape(filename, index2);
        }
        

        
        
        if (flag)
        {
            threads[0] = new Thread(() =>
            {
                ShapeGrammarBuilder.buildShape(index1);
                tag1 = true;
            });
        }
        if (flag2)
        {
            threads[1] = new Thread(() =>
            {
                ShapeGrammarBuilder.buildShape(index2);
                tag2 = true;
                
            });
        }
        Debug.Log("Context Free Size: " + ShapeGrammarBuilder.getStackSize().ToString());
        int index3 = ShapeGrammarBuilder.getFreeStack();
        Debug.Log("Get Index3: " + index3.ToString());
        //ShapeGrammarBuilder.InitObject(index3);
        //bool flag3 = ShapeGrammarBuilder.loadShape(filename, index3);
        //ShapeGrammarBuilder.buildShape(index3);
        //buildlist.Add(ShapeGrammarBuilder.buildMesh(index3));
        //ShapeGrammarBuilder.destroyContext(index3);
        StartCoroutine(createTest());
        threads[0].Start();
        threads[1].Start();

    }

    IEnumerator createTest()
    {
        int context_index = ShapeGrammarBuilder.getFreeStack();
        while (context_index == -1)
        {
            context_index = ShapeGrammarBuilder.getFreeStack();
            Debug.Log("Wait: " + context_index + " context free Size: " + ShapeGrammarBuilder.getStackSize().ToString());
            yield return null;
        }
        ShapeGrammarBuilder.InitObject(context_index);
        bool flag3 = ShapeGrammarBuilder.loadShape(filename, context_index);
        ShapeGrammarBuilder.buildShape(context_index);
        buildlist.Add(ShapeGrammarBuilder.buildMesh(context_index));
        ShapeGrammarBuilder.destroyContext(context_index);
    }

    // Update is called once per frame
    void Update()
    {
        if (tag1)
        {
            buildlist.Add(ShapeGrammarBuilder.buildMesh(index1));
            ShapeGrammarBuilder.destroyContext(index1);
            Debug.Log("tag1: Context Free Size: " + ShapeGrammarBuilder.getStackSize().ToString());
            tag1 = false;
            int index4 = ShapeGrammarBuilder.getFreeStack();
            Debug.Log("Get Index4: " + index4.ToString());
            if (index4 != -1)
            {
                ShapeGrammarBuilder.InitObject(index4);
                bool flag3 = ShapeGrammarBuilder.loadShape(filename, index4);
                ShapeGrammarBuilder.buildShape(index4);
                buildlist.Add(ShapeGrammarBuilder.buildMesh(index4));
                ShapeGrammarBuilder.destroyContext(index4);
            }
           
            Debug.Log("tag1-1: Context Free Size: " + ShapeGrammarBuilder.getStackSize().ToString());
        }
        if (tag2)
        {
            buildlist.Add(ShapeGrammarBuilder.buildMesh(index2));
            ShapeGrammarBuilder.destroyContext(index2);
            Debug.Log("tag2: Context Free Size: " + ShapeGrammarBuilder.getStackSize().ToString());

            int index5 = ShapeGrammarBuilder.getFreeStack();
            Debug.Log("Get Index5: " + index5.ToString());
            if (index5 != -1)
            {
                ShapeGrammarBuilder.InitObject(index5);
                bool flag3 = ShapeGrammarBuilder.loadShape(filename, index5);
                ShapeGrammarBuilder.buildShape(index5);
                buildlist.Add(ShapeGrammarBuilder.buildMesh(index5));
                ShapeGrammarBuilder.destroyContext(index5);
            }

            Debug.Log("tag2-1: Context Free Size: " + ShapeGrammarBuilder.getStackSize().ToString());
            tag2 = false;
        }
        // test for default parameters
        if (build)
        {
            build = false;
            buildModel(0);
        }

        // test for custom parameters in unity
        // First, Use setFileTag() to load the file and parameters.
        //        If you want to set polygon, texture and mesh,
        //        you have to do it before loading file.
        // Second, Set each parameter with custom value.
        if (load_file_n_parameters)
        {
            setFileTag();
        }
        if (build_model_with_parameters)
        {
            buildWithParameters();
        }
    }

    // build model with default parameters
    bool buildModel(int index)
    {
        build = false;

        ShapeGrammarBuilder.InitObject(index);

        bool flag = ShapeGrammarBuilder.loadShape(filename, index);
        if (flag)
        {
            bool temp = ShapeGrammarBuilder.buildShape(index);
            if (temp)
            {
                buildlist.Add(ShapeGrammarBuilder.buildMesh(index));
                return true;
            }
        }
        return false;
    }

    // build model with custom parameters
    void setFileTag()
    {
        load_file_n_parameters = false;

        // Init the builder
        ShapeGrammarBuilder.InitObject(0);

        // set custom polygon, texture and mesh
        float[] points = { 0.0f, 0.0f, 5.0f, 0.0f, 5.0f, 5.0f, -5.0f, 5.0f, -5.0f, 2.5f, 0.0f, 2.5f };
        string house_id = "001";
        string roof_id = "002";
        string wood_path = "assets/farm_wood_planks.jpg";
        string texture_path = "assets/castle_brick_wall.jpg";
        string mesh_path = "assets/hello_house_window_frame.obj";
        ShapeGrammarBuilder.AddPolygon(house_id, points, 0);
        ShapeGrammarBuilder.AddTexture(house_id, texture_path, 0);
        ShapeGrammarBuilder.AddTexture(roof_id, wood_path, 0);
        ShapeGrammarBuilder.AddMesh(house_id, mesh_path, 0);

        // load shape grammar file
        ShapeGrammarBuilder.loadShape(filename, 0);

        // get the name and the value of parameters 
        parameters = ShapeGrammarBuilder.GetParameterPairs(0);
    }

    // build the model with parameters
    bool buildWithParameters()
    {
        build_model_with_parameters = false;

        // set custom parameters
        bool param_flag = false;
        for (int i = 0; i < parameters.Count; i++)
        {
            param_flag = ShapeGrammarBuilder.setParam(parameters[i].name, parameters[i].value, 0);
            if (!param_flag)
                return false;
        }

        // build the mesh
        bool temp = ShapeGrammarBuilder.buildShape(0);
        if (temp)
        {
            buildlist.Add(ShapeGrammarBuilder.buildMesh(0));
            return true;
        }
        return false;
    }

    private void OnDisable()
    {
        threads[0].Abort();
        threads[1].Abort();
    }
}
