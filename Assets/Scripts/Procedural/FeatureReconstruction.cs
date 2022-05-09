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
            TerrainGenerator.file_path = file_path;
            TerrainGenerator.terrain_idw_mat = terrain_mat;
            TerrainGenerator.readFeatureFile(Application.streamingAssetsPath + "//" + file_path);
            TerrainGenerator.generateTerrain(new Vector3());
            //generateIDWTerrain(features);
            //getAreaTerrain(0.0f, 0.0f);
        }
    }

    // Only for whole small terrain. Small terrain is put into TerrainGenerator.cs
    void generateIDWTerrain(Vector4[] features)
    {
        Mesh mesh = new Mesh();
        float[,,] terrain_points = new float[x_length, z_length, 3];
        Vector3[] vertice = new Vector3[x_length * z_length];
        Vector2[] uv = new Vector2[x_length * z_length];
        int[] indices = new int[6 * (x_length - 1) * (z_length - 1)];
        int indices_index = 0;
        for (int i = 0; i < x_length; i++)
        {
            for (int j = 0; j < z_length; j++)
            {
                terrain_points[i, j, 0] = min_x + i * PublicOutputInfo.piece_length;
                terrain_points[i, j, 2] = min_z + j * PublicOutputInfo.piece_length;
                terrain_points[i, j, 1] = IDW.inverseDistanceWeighting(features, terrain_points[i, j, 0], terrain_points[i, j, 2]);
                vertice[i * z_length + j] = new Vector3(terrain_points[i, j, 0], terrain_points[i, j, 1], terrain_points[i, j, 2]);
                uv[i * z_length + j] = new Vector2((float)i / x_length, (float)j / z_length);
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
        mesh.uv = uv;
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
        Debug.Log("Generate IDW Successfully");
    }
}