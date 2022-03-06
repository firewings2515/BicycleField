using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SmallTerrainGenerator : MonoBehaviour
{
    OSMEditor osm_editor;
    RoadIntegration road_integration;
    float piece_length = 32.0f; //128
    public bool generate;
    public Material heightmap_mat;
    public Texture2D heightmap;
    public GameObject blue_ball;
    public GameObject red_ball;
    // Start is called before the first frame update
    void Start()
    {
        osm_editor = GetComponent<OSMEditor>();
        road_integration = GetComponent<RoadIntegration>();
    }

    // Update is called once per frame
    void Update()
    {
        if (generate && osm_editor.is_initial)
        {
            generate = false;
            generatePlane(road_integration.view_max_x, road_integration.view_max_z, road_integration.view_min_x, road_integration.view_min_z);
        }
    }

    void generatePlane(float max_x, float max_z, float min_x, float min_z)
    {
        float max_lon;
        float max_lat;
        float min_lon;
        float min_lat;
        osm_editor.osm_reader.toLonAndLat(max_x, max_z, out max_lon, out max_lat);
        osm_editor.osm_reader.toLonAndLat(min_x, min_z, out min_lon, out min_lat);
        Debug.Log("lon: " + min_lon + " lat: " + min_lat + " ~ lon: " + max_lon + " lat: " + max_lat);
        //float max_u = max_lon - (int)max_lon;
        //float max_v = max_lat - (int)max_lat;
        //float min_u = min_lon - (int)min_lon;
        //float min_v = min_lat - (int)min_lat;

        Mesh mesh = new Mesh();
        int x_length = Mathf.CeilToInt((max_x - min_x) / piece_length);
        int z_length = Mathf.CeilToInt((max_z - min_z) / piece_length);
        double[,,] terrain_points = new double[x_length, z_length, 3];
        Vector3[] vertice = new Vector3[x_length * z_length];
        Vector2[] uv = new Vector2[x_length * z_length];
        int[] indices = new int[6 * (x_length - 1) * (z_length - 1)];
        int indices_index = 0;
        List<EarthCoord> all_coords = new List<EarthCoord>();
        for (int i = 0; i < x_length; i++)
        {
            for (int j = 0; j < z_length; j++)
            {
                float terrain_lon, terrain_lat;
                //osm_editor.osm_reader.toUnityLocation(terrain_points[i, j].x, terrain_points[i, j].z, out pos_x, out pos_z);
                terrain_points[i, j, 0] = min_x + i * piece_length;
                terrain_points[i, j, 2] = min_z + j * piece_length;
                osm_editor.osm_reader.toLonAndLat((float)terrain_points[i, j, 0], (float)terrain_points[i, j, 2], out terrain_lon, out terrain_lat);
                all_coords.Add(new EarthCoord(terrain_lon, terrain_lat));
                //terrain_points[i, j, 1] = heightmap.GetPixel(Mathf.FloorToInt((min_u + i * du) * 2048), Mathf.FloorToInt((min_v + j * dv) * 2048)).r * 100.0f;

                //uv[i * z_length + j] = new Vector2(min_u + i * du, min_v + j * dv);
                uv[i * z_length + j] = new Vector2((float)i / x_length, (float)j / z_length);
            }
        }
        //////////////////////////////get elevations/////////////////////////////////////////
        List<float> all_elevations = HgtReader.getElevations(all_coords);
        /////////////////////////////////////////////////////////////////////////////////////
        float max_height = float.MinValue;
        for (int i = 0; i < x_length; i++)
        {
            for (int j = 0; j < z_length; j++)
            {
                vertice[i * z_length + j] = new Vector3((float)terrain_points[i, j, 0], all_elevations[i * z_length + j], (float)terrain_points[i, j, 2]);
                max_height = Mathf.Max(max_height, all_elevations[i * z_length + j]);
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

        Texture2D texture = exportSmallTexture(x_length, z_length, vertice, max_height);
        heightmap_mat.SetTexture("Texture2D", texture);
        float[] edges = getTerrainEdgeDetection(vertice, x_length, z_length);
        mr.material = heightmap_mat;
        terrain.AddComponent<ExportPNG>();
        terrain.AddComponent<HeightmapCompress>();
        terrain.GetComponent<HeightmapCompress>().heightmap = texture;
        terrain.GetComponent<HeightmapCompress>().vertice = vertice;
        terrain.GetComponent<HeightmapCompress>().edges = edges;
        terrain.GetComponent<HeightmapCompress>().x_length = x_length;
        terrain.GetComponent<HeightmapCompress>().z_length = z_length;
        terrain.GetComponent<HeightmapCompress>().min_x = min_x;
        terrain.GetComponent<HeightmapCompress>().min_z = min_z;
        terrain.GetComponent<HeightmapCompress>().map_size_width = max_x - min_x;
        terrain.GetComponent<HeightmapCompress>().map_size_height = max_z - min_z;
        terrain.GetComponent<HeightmapCompress>().gray_height = max_height;
        terrain.GetComponent<HeightmapCompress>().blue_ball = blue_ball;
        terrain.GetComponent<HeightmapCompress>().red_ball = red_ball;
    }

    Texture2D exportSmallTexture(int x_length, int z_length, Vector3[] vertice, float max_height)
    {
        //first Make sure you're using RGB24 as your texture format
        Texture2D texture2D = new Texture2D(x_length, z_length, TextureFormat.RGBA32, false);

        for (int i = 0; i < x_length; i++)
        {
            for (int j = 0; j < z_length; j++)
            {
                float gray = vertice[i * z_length + j].y / max_height;
                texture2D.SetPixel(i, j, new Color(gray, gray, gray));
            }
        }

        //then Save To Disk as PNG
        byte[] bytes = texture2D.EncodeToPNG();
        var dirPath = Application.dataPath + "/Resources/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        File.WriteAllBytes(dirPath + "smallImage" + ".png", bytes);

        return texture2D;
    }

    float[] getTerrainEdgeDetection(Vector3[] vertice, int x_length, int z_length)
    {
        float[] edges = new float[vertice.Length];
        float[] value = new float[8];
        int[] dx = new int[8] { 1, 1, 1, 0, -1, -1, -1, 0 };
        int[] dz = new int[8] { 1, 0, -1, -1, -1, 0, 1, 1 };
        for (int x = 0; x < x_length; x++)
        {
            for (int z = 0; z < z_length; z++)
            {
                for (int dir = 0; dir < 8; dir++)
                {
                    int get_x = x + dx[dir];
                    int get_z = z + dz[dir];
                    if (get_x < 0) get_x = 0;
                    if (get_x >= x_length) get_x = x_length - 1;
                    if (get_z < 0) get_z = 0;
                    if (get_z >= z_length) get_z = z_length - 1;
                    value[dir] = vertice[get_x * z_length + get_z].y;
                }

                float colorX =
                    value[6] * 1.0f +
                    value[7] * 2.0f +
                    value[0] * 1.0f +
                    value[2] * -1.0f +
                    value[3] * -2.0f +
                    value[4] * -1.0f;

                float colorZ =
                    value[0] * 1.0f +
                    value[1] * 2.0f +
                    value[2] * 1.0f +
                    value[4] * -1.0f +
                    value[5] * -2.0f +
                    value[6] * -1.0f;

                edges[x * z_length + z] = Mathf.Sqrt(colorX * colorX + colorZ * colorZ);
            }
        }
        return edges;
    }
}