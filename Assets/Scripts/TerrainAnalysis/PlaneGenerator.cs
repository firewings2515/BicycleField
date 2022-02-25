using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlaneGenerator : MonoBehaviour
{
    int resolution = 180;
    public Material heightmap_mat;
    public bool generate;
    public bool export;
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
            generatePlane();
        }
    }

    void generatePlane()
    {
        Mesh mesh = new Mesh();
        double[,,] terrain_points = new double[resolution + 1, resolution + 1, 3];
        Vector3[] vertice = new Vector3[(resolution + 1) * (resolution + 1)];
        Vector2[] uv = new Vector2[(resolution + 1) * (resolution + 1)];
        int[] indices = new int[6 * resolution * resolution];
        int indices_index = 0;
        for (int i = 0; i < resolution + 1; i++)
        {
            for (int j = 0; j < resolution + 1; j++)
            {
                //float pos_x, pos_z;
                //osm_editor.osm_reader.toUnityLocation(terrain_points[i, j].x, terrain_points[i, j].z, out pos_x, out pos_z);
                terrain_points[i, j, 0] = i / 8.0;
                terrain_points[i, j, 1] = 0.0;
                terrain_points[i, j, 2] = j / 8.0;
                vertice[i * (resolution + 1) + j] = new Vector3((float)terrain_points[i, j, 0], (float)terrain_points[i, j, 1], (float)terrain_points[i, j, 2]);
                uv[i * (resolution + 1) + j] = new Vector2((float)i / resolution, (float)j / resolution);
            }
        }
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                // counter-clockwise
                indices[indices_index++] = i * (resolution + 1) + j;
                indices[indices_index++] = (i + 1) * (resolution + 1) + j + 1;
                indices[indices_index++] = (i + 1) * (resolution + 1) + j;
                indices[indices_index++] = i * (resolution + 1) + j;
                indices[indices_index++] = i * (resolution + 1) + j + 1;
                indices[indices_index++] = (i + 1) * (resolution + 1) + j + 1;
            }
        }

        //Assign data to mesh
        mesh.vertices = vertice;
        mesh.uv = uv;
        mesh.triangles = indices;
        //Recalculations
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
        //Name the mesh
        mesh.name = "terrain_mesh";
        GameObject terrain = new GameObject("terrain");
        MeshFilter mf = terrain.AddComponent<MeshFilter>();
        MeshRenderer mr = terrain.AddComponent<MeshRenderer>();
        mf.mesh = mesh;
        mr.material = heightmap_mat;
    }
}