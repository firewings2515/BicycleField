using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ExecuteInEditMode]
public class FeatureReconstruction : MonoBehaviour
{
    public string file_path = "features.f";
    int x_length;
    int z_length;
    float min_x;
    float min_z;
    Vector3[] features;
    public Material terrain_mat;
    public bool generate;
    public float piece_length = 32.0f; //2048
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (generate)
        {
            generate = false;
            readFeatureFile(Application.streamingAssetsPath + "//" + file_path);
            generateIDWTerrain(features);
        }
    }

    void readFeatureFile(string file_path)
    {
        using (StreamReader sr = new StreamReader(file_path))
        {
            string[] inputs = sr.ReadLine().Split(' ');
            x_length = int.Parse(inputs[0]);
            z_length = int.Parse(inputs[1]);
            inputs = sr.ReadLine().Split(' ');
            min_x = float.Parse(inputs[0]);
            min_z = float.Parse(inputs[1]);
            int n = int.Parse(sr.ReadLine());
            features = new Vector3[n];
            for (int f_i = 0; f_i < n; f_i++)
            {
                inputs = sr.ReadLine().Split(' ');
                float x = float.Parse(inputs[0]);
                float y = float.Parse(inputs[1]);
                float z = float.Parse(inputs[2]);
                features[f_i] = new Vector3(x, y, z);
            }
            Debug.Log("Read Successfully");
        }
    }

    void generateIDWTerrain(Vector3[] features)
    {
        Mesh mesh = new Mesh();
        float[,,] terrain_points = new float[x_length, z_length, 3];
        Vector3[] vertice = new Vector3[x_length * z_length];
        int[] indices = new int[6 * (x_length - 1) * (z_length - 1)];
        int indices_index = 0;
        for (int i = 0; i < x_length; i++)
        {
            for (int j = 0; j < z_length; j++)
            {
                terrain_points[i, j, 0] = min_x + i * piece_length;
                terrain_points[i, j, 2] = min_z + j * piece_length;
                terrain_points[i, j, 1] = IDW.inverseDistanceWeighting(features, terrain_points[i, j, 0], terrain_points[i, j, 2]);
                vertice[i * z_length + j] = new Vector3(terrain_points[i, j, 0], terrain_points[i, j, 1], terrain_points[i, j, 2]);
            }
        }

        for (int i = 0; i < x_length - 1; i++)
        {
            for (int j = 0; j < z_length - 1; j++)
            {
                // counter-clockwise
                indices[indices_index++] = i * z_length + j;
                indices[indices_index++] = (i + 1) * z_length + j + 1;
                indices[indices_index++] = (i + 1) * z_length + j;
                indices[indices_index++] = i * z_length + j;
                indices[indices_index++] = i * z_length + j + 1;
                indices[indices_index++] = (i + 1) * z_length + j + 1;
            }
        }

        mesh.vertices = vertice;
        mesh.triangles = indices;
        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        //Name the mesh
        mesh.name = "terrain_mesh";
        GameObject terrain = new GameObject("terrain_IDW");
        MeshFilter mf = terrain.AddComponent<MeshFilter>();
        MeshRenderer mr = terrain.AddComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = terrain_mat;
        Debug.Log("Generate Successfully");
    }
}