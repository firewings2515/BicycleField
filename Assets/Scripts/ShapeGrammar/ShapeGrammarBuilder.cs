using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using Dummiesman;
using System.Runtime.InteropServices;

[System.Serializable]
public class ParameterPair
{
    public string name = "";
    public string value = "";
}

public static class ShapeGrammarBuilder
{
    [DllImport("Dll1")]
    private static extern void Init();
    [DllImport("Dll1")]
    private static extern int getFreeIndex();
    [DllImport("Dll1")]
    private static extern void releaseContext(int index);
    [DllImport("Dll1")]
    private static extern int getFreeSize();

    [DllImport("Dll1")]
    private static extern bool testMulti();
    [DllImport("Dll1")]
    private static extern void InitContext(int index);
    [DllImport("Dll1")]
    private static extern bool setMapTableIdLenth(int len, int index);
    [DllImport("Dll1")]
    private static extern bool loadFile([MarshalAs(UnmanagedType.LPStr)]string fliename, int index);
    [DllImport("Dll1")]
    private static extern bool buildShapeGrammar(int index);
    [DllImport("Dll1")]
    private static extern bool setParameter([MarshalAs(UnmanagedType.LPStr)]string name, [MarshalAs(UnmanagedType.LPStr)]string value, int index);
    [DllImport("Dll1")]
    private static extern int getParamslen(int index);
    [DllImport("Dll1")]
    private static extern bool passParams(StringBuilder str, int index);

    [DllImport("Dll1")]
    private static extern int getObjlen(int index);
    [DllImport("Dll1")]
    private static extern int getMtllen(int index);
    [DllImport("Dll1")]
    private static extern int passObj(StringBuilder str, int index);
    [DllImport("Dll1")]
    private static extern int passMtl(StringBuilder str, int index);
    [DllImport("Dll1")]
    private static extern int getErrorlen(int index);
    [DllImport("Dll1")]
    private static extern int passError(StringBuilder str, int index);

    [DllImport("Dll1")]
    private static extern int getGrammarlen(int index);
    [DllImport("Dll1")]
    private static extern void passGrammar(StringBuilder str, int index);

    [DllImport("Dll1")]
    private static extern bool setPolygon([MarshalAs(UnmanagedType.LPStr)]string house_id, float[] point, int size, int index);
    [DllImport("Dll1")]
    private static extern float getPolygon([MarshalAs(UnmanagedType.LPStr)]string house_id, int index);

    [DllImport("Dll1")]
    private static extern bool setTexture([MarshalAs(UnmanagedType.LPStr)]string texture_id, [MarshalAs(UnmanagedType.LPStr)]string texture_path, int index);
    [DllImport("Dll1")]
    private static extern bool setMesh([MarshalAs(UnmanagedType.LPStr)]string mesh_id, [MarshalAs(UnmanagedType.LPStr)]string mesh_path, int index);

    private static Dictionary<int, bool> init_flag = new Dictionary<int, bool>();
    private static Dictionary<int, bool> load_file_flag = new Dictionary<int, bool>();

    public static void InitClass()
    {
        Init();
    }

    public static int getFreeStack()
    {
        return getFreeIndex();
    }

    public static void destroyContext(int index)
    {
        releaseContext(index);
    }

    public static int getStackSize()
    {
        return getFreeSize();
    }

    public static bool test_multi()
    {
        return testMulti();
    }

    // Use init to delete old object and build new one.
    // Init must be called before processing these functions. 
    public static void InitObject(int index)
    {
        InitContext(index);
        load_file_flag[index] = false;
        init_flag[index] = true;
    }

    public static bool setIdLength(int len, int index)
    {
        if (len < 1)
            return false;
        setMapTableIdLenth(len, index);
        return true;
    }

    // Set custom polygon. 
    // You have to call before loading file
    public static bool AddPolygon(string house_id, List<Vector2> points, int index)
    {
        if (!init_flag.ContainsKey(index) || !init_flag[index])
        {
            Debug.Log("---------Class is not initialized---------");
            return false;
        }
        float[] points_arr = new float[points.Count * 2];
        for (int i = 0; i < points.Count; i++) 
        {
            points_arr[i * 2] = points[i].x;
            points_arr[i * 2 + 1] = points[i].y;
        }
        setPolygon(house_id, points_arr, points.Count * 2, index);
        //for (int i = 0; i < points.Count * 2; i++)
        //{
        //    Debug.Log("get points[" + i.ToString() + "] = " + getPolygon(house_id, i).ToString());
        //}
        return true;
    }

    // Set custom polygon. 
    // You have to call before loading file
    public static bool AddPolygon(string house_id, float[] points, int index)
    {
        if (!init_flag.ContainsKey(index) || !init_flag[index])
        {
            Debug.Log("---------Class is not initialized---------");
            return false;
        }
        setPolygon(house_id, points, points.Length, index);
        return true;
    }

    // Set custom texture. 
    // You have to call before loading file
    public static bool AddTexture(string texture_id, string texture_path, int index)
    {
        if (!init_flag.ContainsKey(index) || !init_flag[index])
        {
            Debug.Log("---------Class is not initialized---------");
            return false;
        }
        setTexture(texture_id, texture_path, index);
        return true;
    }

    // Set custom mesh. 
    // You have to call before loading file
    public static bool AddMesh(string mesh_id, string mesh_path, int index)
    {
        if (!init_flag.ContainsKey(index) || !init_flag[index])
        {
            Debug.Log("---------Class is not initialized---------");
            return false;
        }
        setMesh(mesh_id, mesh_path, index);
        return true;
    }

    // Loading shape grammar file
    public static bool loadShape(string filename, int index)
    {
        if (!init_flag.ContainsKey(index) || !init_flag[index])
        {
            Debug.Log("---------Class is not initialized---------");
            return false;
        }
        bool flag = loadFile(filename, index);
        if (!flag)
        {
            Debug.Log("---------failed to load files---------");
            Debug.Log(flag);
            int size = getErrorlen(index);
            StringBuilder error = new StringBuilder(size);
            passError(error, index);
            Debug.Log(error);
            return false;
        }
        load_file_flag[index] = true;
        return true;
    }

    // Set custom mesh. 
    // You have to call it after loading file and before building mesh
    public static bool setParam(string name, string value, int index)
    {
        if (!load_file_flag.ContainsKey(index) || !load_file_flag[index])
        {
            Debug.Log("---------You have not load file yet---------");
            return false;
        }
        bool flag = setParameter(name, value, index);
        if (!flag)
        {
            Debug.Log("---------failed to set parameter---------");
            Debug.Log(flag);
            int size = getErrorlen(index);
            StringBuilder error = new StringBuilder(size);
            passError(error, index);
            Debug.Log(error);
            return false;
        }
        return true;
    }

    // Use grammar file to build Mesh
    // You have to call after loading file
    public static bool buildShape(int index)
    {
        if (!load_file_flag.ContainsKey(index) || !load_file_flag[index])
        {
            Debug.Log("---------You have not load file yet---------");
            return false;
        }

        // build the mesh
        bool flag = buildShapeGrammar(index);
        return flag;
        //if (flag)
        //{
        //    // get obj file and mtl file
        //    StringBuilder objs = new StringBuilder(getObjlen());
        //    passObj(objs);
        //    Debug.Log(objs);
        //    StringBuilder mtls = new StringBuilder(getMtllen());
        //    passMtl(mtls);
        //    Debug.Log(mtls);

        //    // convert string to stream
        //    byte[] objbytes = Encoding.ASCII.GetBytes(objs.ToString());
        //    MemoryStream objStream = new MemoryStream(objbytes);

        //    byte[] mtlbytes = Encoding.ASCII.GetBytes(mtls.ToString());
        //    MemoryStream mtlStream = new MemoryStream(mtlbytes);

        //    // build GameObject with obj and mtl
        //    return new OBJLoader().Load(objStream, mtlStream);
        //}
        //else
        //{

        //    Debug.Log("---------failed to build files---------");
        //    int size = getGrammarlen();
        //    StringBuilder grammar = new StringBuilder(size);
        //    passGrammar(grammar);
        //    Debug.Log(grammar);
        //    size = getErrorlen();
        //    StringBuilder error = new StringBuilder(size);
        //    passError(error);
        //    Debug.Log(error);
        //    return null;
        //}
    }

    public static GameObject buildMesh(int index)
    {
        // get obj file and mtl file
        StringBuilder objs = new StringBuilder(getObjlen(index));
        passObj(objs, index);
        //Debug.Log(objs);
        StringBuilder mtls = new StringBuilder(getMtllen(index));
        passMtl(mtls, index);
        //Debug.Log(mtls);

        // convert string to stream
        byte[] objbytes = Encoding.ASCII.GetBytes(objs.ToString());
        MemoryStream objStream = new MemoryStream(objbytes);

        byte[] mtlbytes = Encoding.ASCII.GetBytes(mtls.ToString());
        MemoryStream mtlStream = new MemoryStream(mtlbytes);

        // build GameObject with obj and mtl
        return new OBJLoader().Load(objStream, mtlStream);
    }

    public static void buildMesh(ref string obj, ref string mtl, int index)
    {
        // get obj file and mtl file
        StringBuilder objs = new StringBuilder(getObjlen(index));
        passObj(objs, index);
        //Debug.Log(objs);
        StringBuilder mtls = new StringBuilder(getMtllen(index));
        passMtl(mtls, index);
        //Debug.Log(mtls);

        obj = objs.ToString();
        mtl = mtls.ToString();
    }

    public static GameObject StringToGameobject(ref string obj, ref string mtl)
    {
        // convert string to stream
        byte[] objbytes = Encoding.ASCII.GetBytes(obj);
        MemoryStream objStream = new MemoryStream(objbytes);

        byte[] mtlbytes = Encoding.ASCII.GetBytes(mtl);
        MemoryStream mtlStream = new MemoryStream(mtlbytes);

        // build GameObject with obj and mtl
        return new OBJLoader().Load(objStream, mtlStream);
    }

    // Get the name and the value of parameters
    // You have to call after loading file
    public static List<ParameterPair> GetParameterPairs(int index)
    {
        if (!load_file_flag.ContainsKey(index) || !load_file_flag[index])
        {
            Debug.Log("---------You have not load file yet---------");
            return new List<ParameterPair>();
        }

        List<ParameterPair> pairs = new List<ParameterPair>();

        // get parameters string
        int size = getParamslen(index);
        StringBuilder str = new StringBuilder(size);
        passParams(str, index);

        // parse parameters string
        string input = str.ToString();
        char[] splitchar = { '\n' };
        string[] lines = input.Split(char.Parse("\n"));

        int mode = 0;
        int count = 0;
        
        // read each parameter
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == "")
                continue;
            if (lines[i] == "param_name")
            {
                mode = 1;
            }
            else if (lines[i] == "param_value")
            {
                mode = 2;
            }
            else if (mode == 1)
            {
                pairs.Add(new ParameterPair());
                pairs[pairs.Count - 1].name = lines[i];
            }
            else if (mode == 2)
            {
                pairs[count].value = lines[i];
                count++;
            }
        }
        return pairs;
    }
  
}
